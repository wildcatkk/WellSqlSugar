using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性处理
    /// </summary>
    public class AttributeProvider
    {

        public static T Process<T>(ISqlSugarClient db, T obj)
        {
            Process(db, new List<T> { obj }, typeof(T));

            return obj;
        }

        public static object Process(ISqlSugarClient db, object obj)
        {
            if (obj is IEnumerable)
                throw new Exception("类型异常，函数AttributeProvider.Process(ISqlSugarClient db, object obj)，参数obj不支持IEnumerable类型。");

            Process(db, new List<object> { obj }, obj.GetType());

            return obj;
        }

        public static object Process(ISqlSugarClient db, object obj, Type objType)
        {
            if (obj is IEnumerable)
                throw new Exception("类型异常，函数AttributeProvider.Process(ISqlSugarClient db, object obj, Type objType)，参数obj不支持IEnumerable类型。");

            Process(db, new List<object> { obj }, objType);

            return obj;
        }

        public static List<T> Process<T>(ISqlSugarClient db, List<T> list)
        {
            Process(db, list, typeof(T));

            return list;
        }

        public static ICollection Process(ISqlSugarClient db, ICollection list, Type type)
        {
            if (list is null || list.Count == 0)
            {
                return list;
            }

            //if (!type.IsDefined(typeof(SugarTable)))
            //{
            //    return list;
            //}

            //1、反射收集特性
            PropertyInfo[] props = type.GetProperties();
            List<EnumNameInfo> enumInfoes = new List<EnumNameInfo>();
            List<ForeignValueInfo> foreignInfoes = new List<ForeignValueInfo>();
            List<SubForeignValueInfo> subForeignInfoes = new List<SubForeignValueInfo>();
            List<ForeignListValueInfo> foreignListInfoes = new List<ForeignListValueInfo>();
            foreach (PropertyInfo prop in props)
            {
                //枚举特性
                if (prop.TryGetAtrribute(out EnumName enumAttr))
                {
                    if (prop.PropertyType != typeof(string))
                    {
                        throw new Exception($"特性EnumName({type.Name}.{prop.Name})仅支持string类型的属性。");
                    }
                    EnumNameInfo enumInfo = new EnumNameInfo(prop, enumAttr, type);

                    if (enumInfo.KeyPropInfo != null) enumInfoes.Add(enumInfo);
                }
                //字典类型特性（转换为ForeignValue处理）
                else if (prop.TryGetAtrribute(out DictTypeValue dictTypeAttr))
                {
                    ForeignValue foreignAttr = new ForeignValue("SysDictType", "Code", dictTypeAttr.CodeProperty, dictTypeAttr.TargetColumn);
                    var foreignInfo = new ForeignValueInfo(prop, foreignAttr, type);

                    if (foreignInfo.KeyPropInfo != null) foreignInfoes.Add(foreignInfo);
                }
                //字典项特性（转换为SubForeignValue处理）
                else if (prop.TryGetAtrribute(out DictItemValue dictItemAttr))
                {
                    SubForeignValue foreignAttr = new SubForeignValue("SysDictItem", "ParentCode", dictItemAttr.ParentCode, "Code", dictItemAttr.CodeProperty, dictItemAttr.TargetColumn);
                    var subForeignInfo = new SubForeignValueInfo(prop, foreignAttr, type);

                    if (subForeignInfo.KeyPropInfo != null) subForeignInfoes.Add(subForeignInfo);
                }
                //外键表特性（单主键）
                else if (prop.TryGetAtrribute(out ForeignValue foreignAttr))
                {
                    if (foreignAttr.IsId && prop.PropertyType != typeof(string))
                    {
                        throw new Exception($"特性ForeignValue({type.Name}.{prop.Name})仅支持string类型的属性。");
                    }

                    var foreignInfo = new ForeignValueInfo(prop, foreignAttr, type);

                    if (foreignInfo.KeyPropInfo != null) foreignInfoes.Add(foreignInfo);
                }
                //外键表特性（复合主键）
                else if (prop.TryGetAtrribute(out SubForeignValue subForeignAttr))
                {
                    var subForeignInfo = new SubForeignValueInfo(prop, subForeignAttr, type);

                    if (subForeignInfo.KeyPropInfo != null) subForeignInfoes.Add(subForeignInfo);
                }
                //外键表特性（单主键）
                else if (prop.TryGetAtrribute(out ForeignListValue foreignListAttr))
                {
                    if (foreignListAttr.IsId && prop.PropertyType != typeof(string))
                    {
                        throw new Exception($"特性ForeignListValue({type.Name}.{prop.Name})仅支持string类型的属性。");
                    }

                    var foreignInfo = new ForeignListValueInfo(prop, foreignListAttr, type);

                    if (foreignInfo.KeyPropInfo != null) foreignListInfoes.Add(foreignInfo);
                }
            }

            // 2、[EnumName] 处理
            if (enumInfoes.Count > 0)
            {
                EnumNameProcess(list, enumInfoes);
            }

            // 3、[ForeignValue] 处理
            if (foreignInfoes.Count > 0)
            {
                ForeignValueProcess(db, list,foreignInfoes);
            }

            // 4、[SubForeignValue] 处理
            if (subForeignInfoes.Count > 0)
            {
                SubForeignValueProcess(db, list, subForeignInfoes);
            }

            // 5、[ForeignListValue] 处理
            if (foreignListInfoes.Count > 0)
            {
                ForeignListValueProcess(db, list, foreignListInfoes);
            }

            return list;
        }

        private static void EnumNameProcess(ICollection list, List<EnumNameInfo> enumInfoes)
        {
            Dictionary<object, string> enumCache = new Dictionary<object, string>();
            foreach (var info in enumInfoes)
            {
                Type enumType = info.KeyPropType;
                //if (info.KeyPropInfo.PropertyType.IsGenericType)
                //{
                //    if (info.KeyPropInfo.PropertyType.GenericTypeArguments.Length > 0)
                //    {
                //        enumType = info.KeyPropInfo.PropertyType.GenericTypeArguments[0];
                //    }
                //}

                foreach (var t in list)
                {
                    var enumValue = info.KeyPropInfo.GetValue(t);

                    if (enumValue is null) continue;

                    string enumStr;
                    //增加缓存，以减少反复解析枚举
                    if (enumCache.ContainsKey(enumValue))
                    {
                        enumStr = enumCache[enumValue];
                    }
                    else
                    {
                        //先尝试获取Description
                        DescriptionAttribute descrip = null;
                        if (enumType.GetField(enumValue.ToString())?.TryGetAtrribute(out descrip) ?? false
                            && !string.IsNullOrWhiteSpace(descrip?.Description))
                        {
                            enumStr = descrip.Description;
                        }
                        else
                        {
                            enumStr = Enum.GetName(enumType, enumValue) ?? "";
                        }

                        enumCache.Add(enumValue, enumStr);
                    }

                    info.PropInfo.SetValue(t, enumStr);

                }

            }
        }

        private static void ForeignValueProcess(ISqlSugarClient db, ICollection list, List<ForeignValueInfo> foreignInfoes)
        {
            // 获取所有表名
            var tableNames = new List<string>();
            foreach (ForeignValueInfo info in foreignInfoes)
            {
                if (string.IsNullOrEmpty(info.ForeignValue.TableName) || tableNames.Contains(info.ForeignValue.TableName)) continue;

                tableNames.Add(info.ForeignValue.TableName);
            }

            // 根据表名分组查询
            var tableInfoes = new List<EntityTableInfo>();
            foreach (var tableName in tableNames)
            {
                //根据数据集组装id条件
                var tableInfo = GetForeignCondModel(list, tableName, foreignInfoes);

                //查询数据库获取结果
                tableInfo.TableList = db.Queryable<dynamic>().AS(tableName).Where(tableInfo.ConditionalModels).Select(tableInfo.SelectModels).ToSugarList();

                tableInfoes.Add(tableInfo);
            }

            // 设置对象的属性值
            foreach (var t in list)
            {
                foreach (var item in foreignInfoes)
                {
                    //数据库数据集
                    var dataSet = tableInfoes.FirstOrDefault(x => x.TableName == item.ForeignValue.TableName)?.TableList;
                    if (dataSet is null || dataSet.Count == 0) continue;

                    //当前行数据
                    var infoValue = item.KeyPropInfo.GetValue(t);
                    if (infoValue is null) continue;

                    string infoValueStr;
                    if (infoValue is Enum)
                        infoValueStr = ((int)infoValue).ToString();
                    else
                        infoValueStr = infoValue.ToString();

                    dynamic firstObj = null;
                    foreach (var data in dataSet)
                    {
                        if (DynamicExtensions.TryGetDynamicValue(data, item.ForeignValue.TableColumn, out object columnValue)
                            && columnValue != null
                            && columnValue.ToString() == infoValueStr
                         )
                        {
                            firstObj = data;
                            break;
                        }
                    }
                    if (firstObj is null) continue;

                    // 给当前属性赋值
                    if (DynamicExtensions.TryGetDynamicValue(firstObj, item.ForeignValue.TargetColumn, out object targetValue)
                        && targetValue != null)
                    {
                        item.PropInfo.SetValue(t, targetValue);
                    }
                }
            }
        }

        /// <summary>
        /// 分组获取外键查询条件
        /// </summary>
        /// <param name="list"></param>
        /// <param name="tableInfo"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static EntityTableInfo GetForeignCondModel(ICollection list, string tableName, List<ForeignValueInfo> foreignInfoes)
        {
            EntityTableInfo tableInfo = new EntityTableInfo(tableName);

            var propValues = new List<SingleKey>();
            List<KeyValuePair<WhereType, ConditionalModel>> condiModels = new List<KeyValuePair<WhereType, ConditionalModel>>();
            List<string> fieldNames = new List<string>();
            //对于非bool类型条件，可以将多个Or合并为一个In
            List<ConditionMerge> merges = new List<ConditionMerge>();
            foreach (var info in foreignInfoes)
            {
                // 分组条件
                if (info.ForeignValue.TableName == tableName)
                {
                    foreach (var t in list)
                    {
                        // 查询条件去重
                        object propValue = info.KeyPropInfo.GetValue(t);
                        if (propValue is null || propValues.Exists(p => p.KeyColumn == info.ForeignValue.TableColumn && p.Key.ToString() == propValue.ToString())) continue;

                        //对于Id，需要做额外的判断
                        if (info.ForeignValue.IsId)
                        {
                            string idStr = propValue.ToString();
                            
                            if (string.IsNullOrEmpty(idStr) || !long.TryParse(idStr, out var id)) continue;
                        }

                        propValues.Add(new SingleKey(info.ForeignValue.TableColumn, propValue));

                        // 组装查询条件
                        string propValueStr;
                        if (propValue is Enum)
                            propValueStr = ((int)propValue).ToString();
                        else
                            propValueStr = propValue.ToString();

                        //对于非bool类型条件，可以将多个Or合并为一个In
                        if (info.KeyPropType != typeof(bool))
                        {
                            ConditionMerge merge = merges.FirstOrDefault(p => p.Column == info.ForeignValue.TableColumn);
                            if (merge != null)
                            {
                                //二次匹配，表示有多个，从Equal=>In
                                string columnValue = merge.Value;

                                merge.Value += "," + propValueStr;
                                merge.CondiType = ConditionalType.In;
                            }
                            else
                            {
                                merges.Add(new ConditionMerge(ConditionalType.Equal, info.ForeignValue.TableColumn, propValueStr, info.KeyPropType));
                            }
                        }
                        else
                        {
                            condiModels.Add(WhereType.Or, info.ForeignValue.TableColumn, propValueStr, info.KeyPropType);
                        }
                    }

                    // 组装Select条件
                    if (!fieldNames.Contains(info.ForeignValue.TableColumn))
                    {
                        fieldNames.Add(info.ForeignValue.TableColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.ForeignValue.TableColumn, AsName = info.ForeignValue.TableColumn });
                    }
                    if (!fieldNames.Contains(info.ForeignValue.TargetColumn))
                    {
                        fieldNames.Add(info.ForeignValue.TargetColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.ForeignValue.TargetColumn, AsName = info.ForeignValue.TargetColumn });
                    }
                }
            }

            foreach (var merge in merges)
            {
                condiModels.Add(WhereType.Or, merge.Column, merge.Value, merge.ValueType, merge.CondiType);
            }

            tableInfo.ConditionalModels = SugarConditional.Create(condiModels);

            return tableInfo;
        }

        private static void SubForeignValueProcess(ISqlSugarClient db, ICollection list, List<SubForeignValueInfo> subForeignInfoes)
        {
            // 获取所有表名
            var tableNames = new List<string>();
            foreach (var info in subForeignInfoes)
            {
                if (string.IsNullOrEmpty(info.SubForeignValue.TableName) || tableNames.Contains(info.SubForeignValue.TableName)) continue;

                tableNames.Add(info.SubForeignValue.TableName);
            }

            // 根据表名分组查询
            var tableInfoes = new List<EntityTableInfo>();
            foreach (var tableName in tableNames)
            {
                //根据数据集组装id条件
                var tableInfo = GetSubForeignCondModel(list, tableName, subForeignInfoes);

                //查询数据库获取结果
                tableInfo.TableList = db.Queryable<dynamic>().AS(tableName).Where(tableInfo.ConditionalModels).Select(tableInfo.SelectModels).ToSugarList();

                tableInfoes.Add(tableInfo);
            }

            // 设置对象的属性值
            foreach (var t in list)
            {
                foreach (var info in subForeignInfoes)
                {
                    //数据库数据集
                    var dataSet = tableInfoes.FirstOrDefault(x => x.TableName == info.SubForeignValue.TableName)?.TableList;
                    if (dataSet is null || dataSet.Count == 0) continue;

                    //当前行数据
                    var infoValue = info.KeyPropInfo.GetValue(t);
                    if (infoValue is null) continue;

                    string infoValueStr;
                    if (infoValue is Enum)
                        infoValueStr = ((int)infoValue).ToString();
                    else
                        infoValueStr = infoValue.ToString();

                    dynamic firstObj = null;
                    foreach (var data in dataSet)
                    {
                        //比较复合主键以查找返回的数据，这里仅匹配第一个
                        if (DynamicExtensions.TryGetDynamicValue(data, info.SubForeignValue.TableColumn, out object columnValue)
                            && columnValue != null
                            && columnValue.ToString() == infoValueStr
                         )
                        {
                            if (DynamicExtensions.TryGetDynamicValue(data, info.SubForeignValue.ParentColumn, out object parentValue)
                                && parentValue != null
                                && parentValue.ToString() == info.SubForeignValue.ParentKey
                             )
                            {
                                firstObj = data;
                                break;
                            }
                        }
                    }
                    if (firstObj is null) continue;

                    // 给当前属性赋值
                    if (DynamicExtensions.TryGetDynamicValue(firstObj, info.SubForeignValue.TargetColumn, out object targetValue)
                        && targetValue != null)
                    {
                        info.PropInfo.SetValue(t, targetValue);
                    }
                }
            }
        }

        private static EntityTableInfo GetSubForeignCondModel(ICollection list, string tableName, List<SubForeignValueInfo> subForeignInfoes)
        {
            EntityTableInfo tableInfo = new EntityTableInfo(tableName);

            List<DoubleKey> propValues = new List<DoubleKey>();
            List<string> fieldNames = new List<string>();
            foreach (var info in subForeignInfoes)
            {
                // 分组条件
                if (info.SubForeignValue.TableName == tableName)
                {
                    foreach (var t in list)
                    {
                        // 查询条件去重
                        object propValue = info.KeyPropInfo.GetValue(t);
                        if (propValue is null || propValues.Exists(p => p.ParentColumn == info.SubForeignValue.ParentColumn && p.ParentKey == info.SubForeignValue.ParentKey && p.KeyColumn == info.SubForeignValue.TableColumn && p.Key.ToString() == propValue.ToString())) continue;

                        propValues.Add(new DoubleKey(info.SubForeignValue.ParentColumn, info.SubForeignValue.ParentKey, info.SubForeignValue.TableColumn, propValue));

                        string propValueStr;
                        if (propValue is Enum)
                            propValueStr = ((int)propValue).ToString();
                        else
                            propValueStr = propValue.ToString();

                        // 组装复合查询条件
                        var condiModels = new List<KeyValuePair<WhereType, ConditionalModel>>();
                        condiModels.Add(WhereType.Or, info.SubForeignValue.ParentColumn, info.SubForeignValue.ParentKey, info.KeyPropType);
                        condiModels.Add(WhereType.And, info.SubForeignValue.TableColumn, propValueStr, info.KeyPropType);

                        tableInfo.ConditionalModels.Add(SugarConditional.CreateList(condiModels));
                    }

                    // 组装复合Select条件
                    if (!fieldNames.Contains(info.SubForeignValue.ParentColumn))
                    {
                        fieldNames.Add(info.SubForeignValue.ParentColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.SubForeignValue.ParentColumn, AsName = info.SubForeignValue.ParentColumn });
                    }
                    if (!fieldNames.Contains(info.SubForeignValue.TableColumn))
                    {
                        fieldNames.Add(info.SubForeignValue.TableColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.SubForeignValue.TableColumn, AsName = info.SubForeignValue.TableColumn });
                    }
                    if (!fieldNames.Contains(info.SubForeignValue.TargetColumn))
                    {
                        fieldNames.Add(info.SubForeignValue.TargetColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.SubForeignValue.TargetColumn, AsName = info.SubForeignValue.TargetColumn });
                    }

                }
            }

            return tableInfo;
        }

        private static void ForeignListValueProcess(ISqlSugarClient db, ICollection list, List<ForeignListValueInfo> foreignInfoes)
        {
            // 获取所有表名
            var tableNames = new List<string>();
            foreach (ForeignListValueInfo info in foreignInfoes)
            {
                if (string.IsNullOrEmpty(info.ForeignListValue.TableName) || tableNames.Contains(info.ForeignListValue.TableName)) continue;

                tableNames.Add(info.ForeignListValue.TableName);
            }

            // 根据表名分组查询
            var tableInfoes = new List<EntityTableInfo>();
            foreach (var tableName in tableNames)
            {
                //根据数据集组装id条件
                var tableInfo = GetForeignListCondModel(list, tableName, foreignInfoes);

                //查询数据库获取结果
                tableInfo.TableList = db.Queryable<dynamic>().AS(tableName).Where(tableInfo.ConditionalModels).Select(tableInfo.SelectModels).ToSugarList();

                tableInfoes.Add(tableInfo);
            }

            // 设置对象的属性值
            foreach (var t in list)
            {
                foreach (var info in foreignInfoes)
                {
                    //数据库数据集
                    var dataSet = tableInfoes.FirstOrDefault(x => x.TableName == info.ForeignListValue.TableName)?.TableList;
                    if (dataSet is null || dataSet.Count == 0) continue;

                    //当前行数据
                    //原始条件将被拆分为多个单独的条件
                    object rowPropValue = info.KeyPropInfo.GetValue(t);
                    if (rowPropValue is null) continue;

                    string? rowPropValueStr = rowPropValue.ToString();
                    if (string.IsNullOrEmpty(rowPropValueStr)) continue;

                    var keyPropValues = rowPropValueStr.Trim().Split(',').ToList();
                    if (keyPropValues is null || keyPropValues.Count == 0) continue;

                    string targetValues = "";
                    foreach (var propValue in keyPropValues)
                    {
                        dynamic firstObj = null;
                        foreach (var data in dataSet)
                        {
                            if (DynamicExtensions.TryGetDynamicValue(data, info.ForeignListValue.TableColumn, out object columnValue)
                                && columnValue != null
                                && columnValue.ToString() == propValue
                             )
                            {
                                firstObj = data;
                                break;
                            }
                        }
                        if (firstObj is null) continue;

                        // 给当前属性赋值
                        if (DynamicExtensions.TryGetDynamicValue(firstObj, info.ForeignListValue.TargetColumn, out object targetValue)
                            && targetValue != null)
                        {
                            var targetValueStr = targetValue.ToString();
                            if (string.IsNullOrEmpty(targetValueStr)) continue;

                            targetValues += targetValueStr + ",";
                        }
                    }

                    if (!string.IsNullOrEmpty(targetValues))
                    {
                        targetValues = targetValues.TrimEnd(',');
                        info.PropInfo.SetValue(t, targetValues);
                    }
                }
            }
        }

        private static EntityTableInfo GetForeignListCondModel(ICollection list, string tableName, List<ForeignListValueInfo> foreignInfoes)
        {
            EntityTableInfo tableInfo = new EntityTableInfo(tableName);

            var propValues = new List<SingleKey>();
            List<KeyValuePair<WhereType, ConditionalModel>> condiModels = new List<KeyValuePair<WhereType, ConditionalModel>>();
            List<string> fieldNames = new List<string>();
            //对于非bool类型条件，可以将多个Or合并为一个In
            List<ConditionMerge> merges = new List<ConditionMerge>();
            foreach (var info in foreignInfoes)
            {
                // 分组条件
                if (info.ForeignListValue.TableName == tableName)
                {
                    foreach (var t in list)
                    {
                        //原始条件将被拆分为多个单独的条件
                        object rowPropValue = info.KeyPropInfo.GetValue(t);
                        if (rowPropValue is null) continue;

                        string? rowPropValueStr = rowPropValue.ToString();
                        if (string.IsNullOrEmpty(rowPropValueStr)) continue;

                        var keyPropValues = rowPropValueStr.Trim().Split(',').ToList();
                        if (keyPropValues is null || keyPropValues.Count == 0) continue;

                        foreach (var propValue in keyPropValues)
                        {
                            // 查询条件去重
                            if (string.IsNullOrEmpty(propValue) || propValues.Exists(p => p.KeyColumn == info.ForeignListValue.TableColumn && p.Key.ToString() == propValue)) continue;

                            //对于Id，需要做额外的判断
                            if (info.ForeignListValue.IsId)
                            {
                                if (!long.TryParse(propValue, out var id)) continue;
                            }

                            propValues.Add(new SingleKey(info.ForeignListValue.TableColumn, propValue));

                            // 组装查询条件
                            //对于非bool类型条件，可以将多个Or合并为一个In
                            if (info.KeyPropType != typeof(bool))
                            {
                                ConditionMerge merge = merges.FirstOrDefault(p => p.Column == info.ForeignListValue.TableColumn);
                                if (merge != null)
                                {
                                    //二次匹配，表示有多个，从Equal=>In
                                    string columnValue = merge.Value;

                                    merge.Value += "," + propValue;
                                    merge.CondiType = ConditionalType.In;
                                }
                                else
                                {
                                    merges.Add(new ConditionMerge(ConditionalType.Equal, info.ForeignListValue.TableColumn, propValue, info.KeyPropType));
                                }
                            }
                            else
                            {
                                condiModels.Add(WhereType.Or, info.ForeignListValue.TableColumn, propValue, info.KeyPropType);
                            }
                        }
                    }

                    // 组装Select条件
                    if (!fieldNames.Contains(info.ForeignListValue.TableColumn))
                    {
                        fieldNames.Add(info.ForeignListValue.TableColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.ForeignListValue.TableColumn, AsName = info.ForeignListValue.TableColumn });
                    }
                    if (!fieldNames.Contains(info.ForeignListValue.TargetColumn))
                    {
                        fieldNames.Add(info.ForeignListValue.TargetColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.ForeignListValue.TargetColumn, AsName = info.ForeignListValue.TargetColumn });
                    }
                }
            }

            foreach (var merge in merges)
            {
                condiModels.Add(WhereType.Or, merge.Column, merge.Value, merge.ValueType, merge.CondiType);
            }

            tableInfo.ConditionalModels = SugarConditional.Create(condiModels);

            return tableInfo;
        }

    }

    internal class EnumNameInfo
    {
        public EnumNameInfo(PropertyInfo propInfo, EnumName enumName, Type type)
        {
            PropInfo = propInfo;
            EnumName = enumName;
            KeyPropInfo = type.GetProperty(enumName.Property);
            if (KeyPropInfo != null)
                KeyPropType = Nullable.GetUnderlyingType(KeyPropInfo.PropertyType) ?? KeyPropInfo.PropertyType;
        }

        public PropertyInfo PropInfo { get; set; }

        public PropertyInfo KeyPropInfo { get; set; }

        public Type KeyPropType { get; set; }

        public EnumName EnumName { get; set; }
    }

    internal class ForeignValueInfo
    {
        public ForeignValueInfo(PropertyInfo propInfo, ForeignValue foreign, Type type)
        {
            PropInfo = propInfo;
            ForeignValue = foreign;
            KeyPropInfo = type.GetProperty(foreign.Property);
            if (KeyPropInfo != null)
                KeyPropType = Nullable.GetUnderlyingType(KeyPropInfo.PropertyType) ?? KeyPropInfo.PropertyType;
        }

        public PropertyInfo PropInfo { get; set; }

        public PropertyInfo KeyPropInfo { get; set; }

        public Type KeyPropType { get; set; }

        public ForeignValue ForeignValue { get; set; }
    }

    internal class ForeignListValueInfo
    {
        public ForeignListValueInfo(PropertyInfo propInfo, ForeignListValue foreign, Type type)
        {
            PropInfo = propInfo;
            ForeignListValue = foreign;
            KeyPropInfo = type.GetProperty(foreign.Property);
            if (KeyPropInfo != null)
                KeyPropType = Nullable.GetUnderlyingType(KeyPropInfo.PropertyType) ?? KeyPropInfo.PropertyType;
        }

        public PropertyInfo PropInfo { get; set; }

        public PropertyInfo KeyPropInfo { get; set; }

        public Type KeyPropType { get; set; }

        public ForeignListValue ForeignListValue { get; set; }
    }

    internal class ConditionMerge
    {
        public ConditionMerge(ConditionalType condiType, string column, string value, Type valueType)
        {
            CondiType = condiType;
            Column = column;
            Value = value;
            ValueType = valueType;
        }

        public ConditionalType CondiType { get; set; }

        public string Column { get; set; }

        public string Value { get; set; }

        public Type ValueType { get; set; }
    }

    internal class SubForeignValueInfo
    {
        public SubForeignValueInfo(PropertyInfo propInfo, SubForeignValue foreign, Type type)
        {
            PropInfo = propInfo;
            SubForeignValue = foreign;
            KeyPropInfo = type.GetProperty(foreign.Property);
            if (KeyPropInfo != null)
                KeyPropType = Nullable.GetUnderlyingType(KeyPropInfo.PropertyType) ?? KeyPropInfo.PropertyType;
        }

        public PropertyInfo PropInfo { get; set; }

        public PropertyInfo KeyPropInfo { get; set; }

        public Type KeyPropType { get; set; }

        public SubForeignValue SubForeignValue { get; set; }
    }

    internal class SingleKey
    {
        public SingleKey(string keyColumn, object key)
        {
            KeyColumn = keyColumn;
            Key = key;
        }

        public string KeyColumn { get; set; }

        public object Key { get; set; }
    }

    internal class DoubleKey
    {
        public DoubleKey(string parentColumn, string parentKey, string keyColumn, object key)
        {
            ParentColumn = parentColumn;
            ParentKey = parentKey;
            KeyColumn = keyColumn;
            Key = key;
        }

        public string ParentColumn { get; set; }

        public string ParentKey { get; set; }

        public string KeyColumn { get; set; }

        public object Key { get; set; }
    }

    /// <summary>
    /// 表，对象对应关系
    /// </summary>
    internal class EntityTableInfo
    {
        public EntityTableInfo(string tableName)
        {
            TableName = tableName;
            TableList = new List<dynamic>();
            ConditionalModels = new List<IConditionalModel>();
            SelectModels = new List<SelectModel>();
        }

        public string TableName { get; set; }

        public List<dynamic> TableList { get; set; }

        public List<IConditionalModel> ConditionalModels { get; set; }

        public List<SelectModel> SelectModels { get; set; }
    }
}