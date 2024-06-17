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
            var tableType = type.GetTable();
            List<EnumNameInfo> enumInfoes = new List<EnumNameInfo>();
            List<ForeignValueInfo> foreignInfoes = new List<ForeignValueInfo>();
            List<SubForeignValueInfo> subForeignInfoes = new List<SubForeignValueInfo>();
            List<ForeignListValueInfo> foreignListInfoes = new List<ForeignListValueInfo>();
            foreach (var prop in tableType.Properties)
            {
                //枚举特性
                if (prop.Info.TryGetAtrribute(out EnumName enumAttr))
                {
                    if (prop.Type != typeof(string))
                    {
                        throw new Exception($"特性EnumName({tableType.Type.Name}.{prop.Info.Name})仅支持string类型的属性。");
                    }
                    EnumNameInfo enumInfo = new EnumNameInfo(prop, enumAttr, tableType);

                    if (enumInfo.ValueProperty != null) enumInfoes.Add(enumInfo);
                }
                //字典类型特性（转换为ForeignValue处理）
                else if (prop.Info.TryGetAtrribute(out DictTypeValue dictTypeAttr))
                {
                    ForeignValue foreignAttr = new ForeignValue("SysDictType", "Code", dictTypeAttr.CodeColumn, dictTypeAttr.ResultColumn);
                    var foreignInfo = new ForeignValueInfo(prop, foreignAttr, tableType);

                    if (foreignInfo.ValueProperty != null) foreignInfoes.Add(foreignInfo);
                }
                //字典项特性（转换为SubForeignValue处理）
                else if (prop.Info.TryGetAtrribute(out DictItemValue dictItemAttr))
                {
                    SubForeignValue foreignAttr = new SubForeignValue("SysDictItem", "ParentCode", dictItemAttr.ParentCode, "Code", dictItemAttr.CodeColumn, dictItemAttr.ResultColumn);
                    var subForeignInfo = new SubForeignValueInfo(prop, foreignAttr, tableType);

                    if (subForeignInfo.Value2Property != null) subForeignInfoes.Add(subForeignInfo);
                }
                //外键表特性（单主键）
                else if (prop.Info.TryGetAtrribute(out ForeignValue foreignAttr))
                {
                    if (foreignAttr.IsId && prop.Type != typeof(string))
                    {
                        throw new Exception($"特性ForeignValue({tableType.Type.Name}.{prop.Info.Name})仅支持string类型的属性。");
                    }

                    var foreignInfo = new ForeignValueInfo(prop, foreignAttr, tableType);

                    if (foreignInfo.ValueProperty != null) foreignInfoes.Add(foreignInfo);
                }
                //外键表特性（复合主键）
                else if (prop.Info.TryGetAtrribute(out SubForeignValue subForeignAttr))
                {
                    var subForeignInfo = new SubForeignValueInfo(prop, subForeignAttr, tableType);

                    if (subForeignInfo.Value2Property != null) subForeignInfoes.Add(subForeignInfo);
                }
                //外键表特性（单主键）
                else if (prop.Info.TryGetAtrribute(out ForeignListValue foreignListAttr))
                {
                    if (foreignListAttr.IsId && prop.Type != typeof(string))
                    {
                        throw new Exception($"特性ForeignListValue({tableType.Type.Name} . {prop.Info.Name})仅支持string类型的属性。");
                    }

                    var foreignInfo = new ForeignListValueInfo(prop, foreignListAttr, tableType);

                    if (foreignInfo.ValueProperty != null) foreignListInfoes.Add(foreignInfo);
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
                foreach (var t in list)
                {
                    var enumValue = info.ValueProperty.Info.GetValue(t);

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
                        if (info.ValueProperty.Type.GetField(enumValue.ToString())?.TryGetAtrribute(out descrip) ?? false
                            && !string.IsNullOrWhiteSpace(descrip?.Description))
                        {
                            enumStr = descrip.Description;
                        }
                        else
                        {
                            enumStr = Enum.GetName(info.ValueProperty.Type, enumValue) ?? "";
                        }

                        enumCache.Add(enumValue, enumStr);
                    }

                    info.AttributeProperty.Info.SetValue(t, enumStr);

                }

            }
        }

        private static void ForeignValueProcess(ISqlSugarClient db, ICollection list, List<ForeignValueInfo> foreignInfoes)
        {
            // 获取所有表名
            var tableNames = new List<string>();
            foreach (ForeignValueInfo info in foreignInfoes)
            {
                if (string.IsNullOrEmpty(info.Attribute.ForeignTable) || tableNames.Contains(info.Attribute.ForeignTable)) continue;

                tableNames.Add(info.Attribute.ForeignTable);
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
                    var dataSet = tableInfoes.FirstOrDefault(x => x.TableName == item.Attribute.ForeignTable)?.TableList;
                    if (dataSet is null || dataSet.Count == 0) continue;

                    //当前行数据
                    var infoValue = item.ValueProperty.Info.GetValue(t);
                    if (infoValue is null) continue;

                    string infoValueStr;
                    if (infoValue is Enum)
                        infoValueStr = ((int)infoValue).ToString();
                    else
                        infoValueStr = infoValue.ToString();

                    dynamic firstObj = null;
                    foreach (var data in dataSet)
                    {
                        if (DynamicExtensions.TryGetDynamicValue(data, item.Attribute.ForeignColumn, out object columnValue)
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
                    if (DynamicExtensions.TryGetDynamicValue(firstObj, item.Attribute.ResultColumn, out object targetValue)
                        && targetValue != null)
                    {
                        object value;
                        if (targetValue.GetType() != item.AttributeProperty.Type)
                            value = Convert.ChangeType(targetValue, item.AttributeProperty.Type);
                        else
                            value = targetValue;

                        item.AttributeProperty.Info.SetValue(t, value);
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
                if (info.Attribute.ForeignTable == tableName)
                {
                    foreach (var t in list)
                    {
                        // 查询条件去重
                        object propValue = info.ValueProperty.Info.GetValue(t);
                        if (propValue is null || propValues.Exists(p => p.KeyColumn == info.Attribute.ForeignColumn && p.Key.ToString() == propValue.ToString())) continue;

                        //对于Id，需要做额外的判断
                        if (info.Attribute.IsId)
                        {
                            string idStr = propValue.ToString();
                            
                            if (string.IsNullOrEmpty(idStr) || !long.TryParse(idStr, out var id)) continue;
                        }

                        propValues.Add(new SingleKey(info.Attribute.ForeignColumn, propValue));

                        // 组装查询条件
                        string propValueStr;
                        if (propValue is Enum)
                            propValueStr = ((int)propValue).ToString();
                        else
                            propValueStr = propValue.ToString();

                        //对于非bool类型条件，可以将多个Or合并为一个In
                        if (info.ValueProperty.Type != typeof(bool))
                        {
                            ConditionMerge merge = merges.FirstOrDefault(p => p.Column == info.Attribute.ForeignColumn);
                            if (merge != null)
                            {
                                //二次匹配，表示有多个，从Equal=>In
                                string columnValue = merge.Value;

                                merge.Value += "," + propValueStr;
                                merge.CondiType = ConditionalType.In;
                            }
                            else
                            {
                                merges.Add(new ConditionMerge(ConditionalType.Equal, info.Attribute.ForeignColumn, propValueStr, info.ForeignProperty.Type));
                            }
                        }
                        else
                        {
                            condiModels.Add(WhereType.Or, info.Attribute.ForeignColumn, propValueStr, info.ForeignProperty.Type);
                        }
                    }

                    // 组装Select条件
                    if (!fieldNames.Contains(info.Attribute.ForeignColumn))
                    {
                        fieldNames.Add(info.Attribute.ForeignColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.Attribute.ForeignColumn, AsName = info.Attribute.ForeignColumn });
                    }
                    if (!fieldNames.Contains(info.Attribute.ResultColumn))
                    {
                        fieldNames.Add(info.Attribute.ResultColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.Attribute.ResultColumn, AsName = info.Attribute.ResultColumn });
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
                if (string.IsNullOrEmpty(info.Attribute.ForeignTable) || tableNames.Contains(info.Attribute.ForeignTable)) continue;

                tableNames.Add(info.Attribute.ForeignTable);
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
                    var dataSet = tableInfoes.FirstOrDefault(x => x.TableName == info.Attribute.ForeignTable)?.TableList;
                    if (dataSet is null || dataSet.Count == 0) continue;

                    //当前行数据
                    var infoValue = info.Value2Property.Info.GetValue(t);
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
                        if (DynamicExtensions.TryGetDynamicValue(data, info.Attribute.ForeignColumn2, out object columnValue)
                            && columnValue != null
                            && columnValue.ToString() == infoValueStr
                         )
                        {
                            if (DynamicExtensions.TryGetDynamicValue(data, info.Attribute.ForeignColumn1, out object parentValue)
                                && parentValue != null
                                && parentValue.ToString() == info.Attribute.ForeignValue1
                             )
                            {
                                firstObj = data;
                                break;
                            }
                        }
                    }
                    if (firstObj is null) continue;

                    // 给当前属性赋值
                    if (DynamicExtensions.TryGetDynamicValue(firstObj, info.Attribute.ResultColumn, out object targetValue)
                        && targetValue != null)
                    {
                        object value;
                        if (targetValue.GetType() != info.AttributeProperty.Type)
                            value = Convert.ChangeType(targetValue, info.AttributeProperty.Type);
                        else
                            value = targetValue;

                        info.AttributeProperty.Info.SetValue(t, value);
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
                if (info.Attribute.ForeignTable == tableName)
                {
                    foreach (var t in list)
                    {
                        // 查询条件去重
                        object propValue = info.Value2Property.Info.GetValue(t);
                        if (propValue is null || propValues.Exists(p => p.ParentColumn == info.Attribute.ForeignColumn1 && p.ParentKey == info.Attribute.ForeignValue1 && p.KeyColumn == info.Attribute.ForeignColumn2 && p.Key.ToString() == propValue.ToString())) continue;

                        propValues.Add(new DoubleKey(info.Attribute.ForeignColumn1, info.Attribute.ForeignValue1, info.Attribute.ForeignColumn2, propValue));

                        string propValueStr;
                        if (propValue is Enum)
                            propValueStr = ((int)propValue).ToString();
                        else
                            propValueStr = propValue.ToString();

                        // 组装复合查询条件
                        var condiModels = new List<KeyValuePair<WhereType, ConditionalModel>>();
                        condiModels.Add(WhereType.Or, info.Attribute.ForeignColumn1, info.Attribute.ForeignValue1, info.ForeignProperty1.Type);
                        condiModels.Add(WhereType.And, info.Attribute.ForeignColumn2, propValueStr, info.ForeignProperty2.Type);

                        tableInfo.ConditionalModels.Add(SugarConditional.CreateList(condiModels));
                    }

                    // 组装复合Select条件
                    if (!fieldNames.Contains(info.Attribute.ForeignColumn1))
                    {
                        fieldNames.Add(info.Attribute.ForeignColumn1);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.Attribute.ForeignColumn1, AsName = info.Attribute.ForeignColumn1 });
                    }
                    if (!fieldNames.Contains(info.Attribute.ForeignColumn2))
                    {
                        fieldNames.Add(info.Attribute.ForeignColumn2);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.Attribute.ForeignColumn2, AsName = info.Attribute.ForeignColumn2 });
                    }
                    if (!fieldNames.Contains(info.Attribute.ResultColumn))
                    {
                        fieldNames.Add(info.Attribute.ResultColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.Attribute.ResultColumn, AsName = info.Attribute.ResultColumn });
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
                if (string.IsNullOrEmpty(info.Attribute.ForeignTable) || tableNames.Contains(info.Attribute.ForeignTable)) continue;

                tableNames.Add(info.Attribute.ForeignTable);
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
                    var dataSet = tableInfoes.FirstOrDefault(x => x.TableName == info.Attribute.ForeignTable)?.TableList;
                    if (dataSet is null || dataSet.Count == 0) continue;

                    //当前行数据
                    //原始条件将被拆分为多个单独的条件
                    object rowPropValue = info.ValueProperty.Info.GetValue(t);
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
                            if (DynamicExtensions.TryGetDynamicValue(data, info.Attribute.ForeignColumn, out object columnValue)
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
                        if (DynamicExtensions.TryGetDynamicValue(firstObj, info.Attribute.ResultColumn, out object targetValue)
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
                        info.AttributeProperty.Info.SetValue(t, targetValues);
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
                if (info.Attribute.ForeignTable == tableName)
                {
                    foreach (var t in list)
                    {
                        //原始条件将被拆分为多个单独的条件
                        object rowPropValue = info.ValueProperty.Info.GetValue(t);
                        if (rowPropValue is null) continue;

                        string? rowPropValueStr = rowPropValue.ToString();
                        if (string.IsNullOrEmpty(rowPropValueStr)) continue;

                        var keyPropValues = rowPropValueStr.Trim().Split(',').ToList();
                        if (keyPropValues is null || keyPropValues.Count == 0) continue;

                        foreach (var propValue in keyPropValues)
                        {
                            // 查询条件去重
                            if (string.IsNullOrEmpty(propValue) || propValues.Exists(p => p.KeyColumn == info.Attribute.ForeignColumn && p.Key.ToString() == propValue)) continue;

                            //对于Id，需要做额外的判断
                            if (info.Attribute.IsId)
                            {
                                if (!long.TryParse(propValue, out var id)) continue;
                            }

                            propValues.Add(new SingleKey(info.Attribute.ForeignColumn, propValue));

                            // 组装查询条件
                            //对于非bool类型条件，可以将多个Or合并为一个In
                            if (info.ValueProperty.Type != typeof(bool))
                            {
                                ConditionMerge merge = merges.FirstOrDefault(p => p.Column == info.Attribute.ForeignColumn);
                                if (merge != null)
                                {
                                    //二次匹配，表示有多个，从Equal=>In
                                    string columnValue = merge.Value;

                                    merge.Value += "," + propValue;
                                    merge.CondiType = ConditionalType.In;
                                }
                                else
                                {
                                    merges.Add(new ConditionMerge(ConditionalType.Equal, info.Attribute.ForeignColumn, propValue, info.ForeignProperty.Type));
                                }
                            }
                            else
                            {
                                condiModels.Add(WhereType.Or, info.Attribute.ForeignColumn, propValue, info.ForeignProperty.Type);
                            }
                        }
                    }

                    // 组装Select条件
                    if (!fieldNames.Contains(info.Attribute.ForeignColumn))
                    {
                        fieldNames.Add(info.Attribute.ForeignColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.Attribute.ForeignColumn, AsName = info.Attribute.ForeignColumn });
                    }
                    if (!fieldNames.Contains(info.Attribute.ResultColumn))
                    {
                        fieldNames.Add(info.Attribute.ResultColumn);
                        tableInfo.SelectModels.Add(new SelectModel() { FiledName = info.Attribute.ResultColumn, AsName = info.Attribute.ResultColumn });
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
        public EnumNameInfo(ColumnProperty attributeProperty, EnumName attribute, TableType tableType)
        {
            AttributeProperty = attributeProperty;
            Attribute = attribute;
            ValueProperty = tableType.GetProperty(attribute.ValueColumn);
            if (ValueProperty is null)
                throw new Exception($"Unknow value property [{attribute.ValueColumn}] in table [{tableType.Type.Name}]");
        }

        public ColumnProperty AttributeProperty { get; set; }

        public EnumName Attribute { get; set; }

        public ColumnProperty ValueProperty { get; set; }
    }

    internal class ForeignValueInfo
    {
        public ForeignValueInfo(ColumnProperty attributeProperty, ForeignValue attribute, TableType tableType)
        {
            AttributeProperty = attributeProperty;
            Attribute = attribute;
            ForeignTableType = attribute.ForeignTable.GetTable();
            if (ForeignTableType is null)
                throw new Exception($"Unknow foreign table [{attribute.ForeignTable}]");

            ForeignProperty = ForeignTableType.GetProperty(attribute.ForeignColumn);
            if (ForeignProperty is null)
                throw new Exception($"Unknow foreign property [{attribute.ForeignColumn}] in table [{ForeignTableType.Type.Name}]");
            ValueProperty = tableType.GetProperty(attribute.ValueColumn);
            if (ValueProperty is null)
                throw new Exception($"Unknow value property [{attribute.ValueColumn}] in table [{tableType.Type.Name}]");
        }

        public ColumnProperty AttributeProperty { get; set; }

        public ForeignValue Attribute { get; set; }

        public TableType ForeignTableType { get; set; }

        public ColumnProperty ForeignProperty { get; set; }

        public ColumnProperty ValueProperty { get; set; }
    }

    internal class ForeignListValueInfo
    {
        public ForeignListValueInfo(ColumnProperty attributeProperty, ForeignListValue attribute, TableType tableType)
        {
            AttributeProperty = attributeProperty;
            Attribute = attribute;
            ForeignTableType = attribute.ForeignTable.GetTable();
            if (ForeignTableType is null)
                throw new Exception($"Unknow foreign table [{attribute.ForeignTable}]");

            ForeignProperty = ForeignTableType.GetProperty(attribute.ForeignColumn);
            if (ForeignProperty is null)
                throw new Exception($"Unknow foreign property [{attribute.ForeignColumn}] in table [{ForeignTableType.Type.Name}]");
            ValueProperty = tableType.GetProperty(attribute.ValueColumn);
            if (ValueProperty is null)
                throw new Exception($"Unknow value property [{attribute.ValueColumn}] in table [{tableType.Type.Name}]");
        }

        public ColumnProperty AttributeProperty { get; set; }

        public ForeignListValue Attribute { get; set; }

        public TableType ForeignTableType { get; set; }

        public ColumnProperty ForeignProperty { get; set; }

        public ColumnProperty ValueProperty { get; set; }
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
        public SubForeignValueInfo(ColumnProperty attributeProperty, SubForeignValue attribute, TableType tableType)
        {
            AttributeProperty = attributeProperty;
            Attribute = attribute;
            ForeignTableType = attribute.ForeignTable.GetTable();
            if (ForeignTableType is null)
                throw new Exception($"Unknow foreign table [{attribute.ForeignTable}]");

            ForeignProperty1 = ForeignTableType.GetProperty(attribute.ForeignColumn1);
            if (ForeignProperty1 is null)
                throw new Exception($"Unknow foreign property1 [{attribute.ForeignColumn1}] in table [{ForeignTableType.Type.Name}]");
            ForeignProperty2 = ForeignTableType.GetProperty(attribute.ForeignColumn2);
            if (ForeignProperty2 is null)
                throw new Exception($"Unknow foreign property2 [{attribute.ForeignColumn2}] in table [{ForeignTableType.Type.Name}]");
            Value2Property = tableType.GetProperty(attribute.Value2Column);
            if (Value2Property is null)
                throw new Exception($"Unknow value property [{attribute.Value2Column}] in table [{tableType.Type.Name}]");
        }

        public ColumnProperty AttributeProperty { get; set; }

        public SubForeignValue Attribute { get; set; }

        public TableType ForeignTableType { get; set; }

        public ColumnProperty ForeignProperty1 { get; set; }

        public ColumnProperty ForeignProperty2 { get; set; }

        public ColumnProperty Value2Property { get; set; }
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