using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NetTaste;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性处理
    /// </summary>
    public partial class QueryableProvider<T>
    {
        private List<T> AttributeProcess<T>(List<T> list)
        {
            if (list is null || list.Count == 0)
            {
                return list;
            }

            Type type = typeof(T);
            if (!type.IsDefined(typeof(SugarTable)))
            {
                return list;
            }

            //1、反射收集特性
            PropertyInfo[] props = type.GetProperties();
            List<EnumNameInfo> enumInfoes = new List<EnumNameInfo>();
            List<DictTypeInfo> dictTypeInfoes = new List<DictTypeInfo>();
            List<DictItemInfo> dictItemInfoes = new List<DictItemInfo>();
            List<ForeignValueInfo> foreignInfoes = new List<ForeignValueInfo>();
            foreach (PropertyInfo prop in props)
            {
                if (prop.TryGetAtrribute(out EnumName enumAttr))
                {
                    if (prop.PropertyType != typeof(string))
                    {
                        throw new Exception($"特性EnumName({type.Name}.{prop.Name})仅支持string类型的属性。");
                    }
                    EnumNameInfo enumNameInfo = new EnumNameInfo(prop, enumAttr, type);

                    if (enumNameInfo.TargetPropInfo != null) enumInfoes.Add(enumNameInfo);
                }
                else if (prop.TryGetAtrribute(out DictTypeValue dictTypeAttr))
                {
                    dictTypeInfoes.Add(new DictTypeInfo(prop, dictTypeAttr));
                }
                else if (prop.TryGetAtrribute(out DictItemValue dictItemAttr))
                {
                    dictItemInfoes.Add(new DictItemInfo(prop, dictItemAttr));
                }
                else if (prop.TryGetAtrribute(out ForeignValue foreignAttr))
                {
                    if (foreignAttr.IsId && prop.PropertyType != typeof(string))
                    {
                        throw new Exception($"特性ForeignValue({type.Name}.{prop.Name})仅支持string类型的属性。");
                    }

                    var foreignNameInfo = new ForeignValueInfo(prop, foreignAttr, type);

                    if (foreignNameInfo.TargetPropInfo != null) foreignInfoes.Add(foreignNameInfo);
                }
            }

            // 2、[EnumName] 处理
            if (enumInfoes.Count > 0)
            {
                EnumNameProcess(list, type, enumInfoes);
            }

            // 3、[DictTypeValue] 处理
            if (dictTypeInfoes.Count > 0)
            {
                DictTypeProcess(list, type, dictTypeInfoes);
            }

            // 4、[DictItemValue] 处理
            if (dictItemInfoes.Count > 0)
            {
                DictItemProcess(list, type, dictItemInfoes);
            }

            // 5、[ForeignValue] 处理
            if (foreignInfoes.Count > 0)
            {
                ForeignValueProcess(list, type, foreignInfoes);
            }

            return list;
        }

        private void EnumNameProcess<T>(List<T> list, Type type, List<EnumNameInfo> enumInfoes)
        {
            Dictionary<object, string> enumCache = new Dictionary<object, string>();
            foreach (var info in enumInfoes)
            {
                foreach (var t in list)
                {
                    var enumValue = info.TargetPropInfo.GetValue(t);

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
                        if (info.TargetPropInfo.PropertyType.TryGetAtrribute(out DescriptionAttribute descrip)
                            && !string.IsNullOrWhiteSpace(descrip.Description))
                        {
                            enumStr = descrip.Description;
                        }
                        else
                        {
                            enumStr = Enum.GetName(info.TargetPropInfo.PropertyType, enumValue) ?? "";
                        }

                        enumCache.Add(enumValue, enumStr);
                    }

                    info.PropInfo.SetValue(t, enumStr);
                        
                }
                
            }
        }

        private void DictTypeProcess<T>(List<T> list, Type type, List<DictTypeInfo> dictTypeInfoes)
        {
            var typeCons = new List<KeyValuePair<WhereType, ConditionalModel>>();

            
            foreach (var cons in list.Select(t=> GetDictTypeCondModel(t, dictTypeInfoes)))
            {
                typeCons.AddRange(cons);
            }

            typeCons = typeCons.Distinct().ToList();

            if (typeCons.Count > 0)
            {
                var typeModels = new List<IConditionalModel> { new ConditionalCollections { ConditionalList = typeCons } };

                var typeList = Context.Queryable<dynamic>().AS("SysDictType").Where(typeModels).ToSugarList();

                foreach (var item in list)
                {
                    foreach (var prop in dictTypeInfoes)
                    {
                        if (prop.DictType != null)
                        {
                            var infoValue = item.GetType().GetProperty(prop.DictType.Code)?.GetValue(item)?.ToString();

                            var firstObj = typeList.FirstOrDefault(x =>
                                x.TryGetDynamicValue("Code", out string code) && code == infoValue);

                            // 获取当前数据指定列的值
                            var val = firstObj?.GetType().GetProperty(prop.DictType.ValueProp)?.GetValue(firstObj)?.ToString();

                            // 给当前属性赋值
                            prop.PropInfo?.SetValue(item, val ?? "");
                        }
                    }
                }
            }

            var itemCons = new List<KeyValuePair<WhereType, ConditionalModel>>();
        }

        private void DictItemProcess<T>(List<T> list, Type type, List<DictItemInfo> dictItemInfoes)
        {
            List<KeyValuePair<WhereType, ConditionalModel>> itemCons = new List<KeyValuePair<WhereType, ConditionalModel>>();
            foreach (List<KeyValuePair<WhereType, ConditionalModel>> cons in list.Select(t=>GetDictItemCondModel(t, dictItemInfoes)))
            {
                itemCons.AddRange(cons);
            }

            itemCons = itemCons.Distinct().ToList();

            if (itemCons.Count > 0)
            {
                var itemModels = new List<IConditionalModel> { new ConditionalCollections { ConditionalList = itemCons } };

                var itemList = Context.Queryable("SysDictItem", "SysDictItem").Where(itemModels).ToSugarList();

                foreach (var item in list)
                {
                    foreach (var prop in dictItemInfoes)
                    {
                        var infoValue = item.GetType().GetProperty(prop.DictItem.Code)?.GetValue(item)?.ToString();

                        //var firstObj = itemList.FirstOrDefault(x => x.Code == infoValue);

                        var firstObj = itemList.FirstOrDefault(x =>
                            x.TryGetDynamicValue("Code", out string code) && code == infoValue);

                        // 获取当前数据指定列的值
                        var val = firstObj?.GetType().GetProperty(prop.DictItem.ValueProp)?.GetValue(firstObj)
                            ?.ToString();
                        // 给当前属性赋值
                        prop.PropInfo?.SetValue(item, val ?? "");
                    }
                }
            }
        }

        private void ForeignValueProcess<T>(List<T> list, Type type, List<ForeignValueInfo> foreignInfoes)
        {
            // 获取所有表数据
            var tableNames = new List<string>();
            var tableInfoes = new List<EntityTableInfo>();
            foreach (ForeignValueInfo info in foreignInfoes)
            {
                if (string.IsNullOrEmpty(info.ForeignValue.TableName) || tableNames.Contains(info.ForeignValue.TableName)) continue;
                
                tableNames.Add(info.ForeignValue.TableName);

                //根据数据集组装id条件
                var tableInfo = GetForeignCondModel(list, type, info.ForeignValue.TableName, foreignInfoes);

                var select = new List<SelectModel>()
                {
                    new SelectModel(){ FiledName = info.ForeignValue.TableColumn, AsName = info.ForeignValue.TableColumn },
                    new SelectModel(){ FiledName = info.ForeignValue.TargetColumn, AsName = info.ForeignValue.TargetColumn },
                };

                //查询数据库获取结果
                tableInfo.TableList = Context.Queryable<dynamic>().AS(info.ForeignValue.TableName).Where(tableInfo.ConditionalModels).Select(select).ToSugarList();

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
                    var infoValue = item.TargetPropInfo.GetValue(t);
                    if (infoValue is null) continue;

                    dynamic firstObj = null;
                    foreach (var data in dataSet)
                    {
                        if (DynamicExtensions.TryGetDynamicValue(data, item.ForeignValue.TableColumn, out object columnValue)
                            && columnValue != null
                            && columnValue.ToString() == infoValue.ToString()
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
        /// 获取DictType查询条件
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private  List<KeyValuePair<WhereType, ConditionalModel>> GetDictTypeCondModel<T>(T t, List<DictTypeInfo> dictTypeInfoes)
        {
            // 生成条件表达式
            var conditionalList = (from item in dictTypeInfoes
                                   where item.DictType.Code != null
                                   select t.GetType().GetProperty(item.DictType.Code)?.GetValue(t)?.ToString()
                    into infoValue
                                   select new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or,
                                       new ConditionalModel { FieldName = "Code", FieldValue = infoValue }))
                .Distinct().ToList();

            return conditionalList;
        }

        /// <summary>
        /// 获取DictItem查询条件
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private  List<KeyValuePair<WhereType, ConditionalModel>> GetDictItemCondModel<T>(T t, List<DictItemInfo> dictItemInfoes)
        {
            // 生成条件表达式
            var conditionalList = (from item in dictItemInfoes
                                   where item.DictItem.Code != null
                    select t.GetType().GetProperty(item.DictItem.Code)?.GetValue(t)?.ToString()
                    into infoValue
                    select new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or,
                        new ConditionalModel { FieldName = "Code", FieldValue = infoValue }))
                .Distinct().ToList();
            return conditionalList;
        }

        /// <summary>
        /// 获取外键查询条件
        /// </summary>
        /// <param name="list"></param>
        /// <param name="tableInfo"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static EntityTableInfo GetForeignCondModel<T>(List<T> list, Type type, string tableName, List<ForeignValueInfo> foreignInfoes)
        {
            EntityTableInfo tableInfo = new EntityTableInfo(tableName);

            var propValues = new List<object>();
            List<KeyValuePair<WhereType, ConditionalModel>> condiModels = new List<KeyValuePair<WhereType, ConditionalModel>>();
            foreach (var item in foreignInfoes)
            {
                if (item.ForeignValue.TableName == tableName)
                {
                    foreach (var t in list)
                    {
                        object propValue = item.TargetPropInfo.GetValue(t);
                        if (propValue is null || propValues.Contains(propValue)) continue;

                        if (item.ForeignValue.IsId)
                        {
                            string idStr = propValue.ToString();
                            if (string.IsNullOrEmpty(idStr) || Convert.ToInt64(idStr) <= 0) continue;
                        }

                        propValues.Add(propValue);

                        condiModels.Add(WhereType.Or, item.ForeignValue.TableColumn, propValue);
                    }
                }
            }

            tableInfo.ConditionalModels = new List<IConditionalModel>
            {
                new ConditionalCollections
                {
                    ConditionalList = condiModels
                }
            };

            return tableInfo;
        }
    }

    internal class EnumNameInfo
    {
        public EnumNameInfo(PropertyInfo propInfo, EnumName enumName, Type type)
        {
            PropInfo = propInfo;
            EnumName = enumName;
            TargetPropInfo = type.GetProperty(enumName.Property);
        }

        public PropertyInfo PropInfo { get; set; }

        public PropertyInfo TargetPropInfo { get; set; }

        public EnumName EnumName { get; set; }
    }

    internal class DictTypeInfo
    {
        public DictTypeInfo(PropertyInfo propInfo, DictTypeValue dictType)
        {
            PropInfo = propInfo;
            DictType = dictType;
        }

        public PropertyInfo PropInfo { get; set; }

        public DictTypeValue DictType { get; set; }

    }

    internal class DictItemInfo
    {
        public DictItemInfo(PropertyInfo propInfo, DictItemValue dictItem)
        {
            PropInfo = propInfo;
            DictItem = dictItem;
        }

        public PropertyInfo PropInfo { get; set; }

        public DictItemValue DictItem { get; set; }

    }

    internal class ForeignValueInfo
    {
        public ForeignValueInfo(PropertyInfo propInfo, ForeignValue foreign, Type type)
        {
            PropInfo = propInfo;
            ForeignValue = foreign;
            TargetPropInfo = type.GetProperty(foreign.Property);
        }

        public PropertyInfo PropInfo { get; set; }

        public PropertyInfo TargetPropInfo { get; set; }

        public ForeignValue ForeignValue { get; set; }
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
        }

        public string TableName { get; set; }

        public List<dynamic> TableList { get; set; }

        public List<IConditionalModel> ConditionalModels { get; set; }
    }
}