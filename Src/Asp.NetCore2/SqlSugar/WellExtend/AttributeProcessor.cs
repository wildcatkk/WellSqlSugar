using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using NetTaste;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性处理
    /// </summary>
    public partial class QueryableProvider<T>
    {
        private List<T> AttributeProcess<T>(List<T> t)
        {
            var type = typeof(T);
            if (!type.IsDefined(typeof(SugarTable)))
            {
                return t;
            }

            var typeCons = new List<KeyValuePair<WhereType, ConditionalModel>>();

            // [DictTypeValue] 处理
            foreach (var cons in t.Select(GetDictTypeCondModel))
            {
                typeCons.AddRange(cons);
            }

            typeCons = typeCons.Distinct().ToList();

            if (typeCons.Count > 0)
            {
                var typeModels = new List<IConditionalModel> { new ConditionalCollections { ConditionalList = typeCons } };

                var typeList = Context.Queryable<dynamic>().AS("SysDictType").Where(typeModels).ToSugarList();

                foreach (var item in t)
                {
                    // 获取具有[DictTypeValue]的属性
                    var dictTypeProps = type.GetProperties().Where(u => u.IsDefined(typeof(DictTypeValue), false)).Select(
                        x => new ValueInfo
                        {
                            Prop = x,
                            TypeValue = x.TryGetAtrribute(out DictTypeValue typeValue) ? typeValue : null,
                        }).ToList();
                    foreach (var prop in dictTypeProps)
                    {
                        if (prop.TypeValue != null)
                        {
                            var infoValue = item.GetType().GetProperty(prop.TypeValue.Code)?.GetValue(item)?.ToString();


                            var firstObj = typeList.FirstOrDefault(x =>
                                x.TryGetDynamicValue("Code", out string code) && code == infoValue);

                            // 获取当前数据指定列的值
                            var val = firstObj?.GetType().GetProperty(prop.TypeValue.ValueProp)?.GetValue(firstObj)
                                ?.ToString();
                            // 给当前属性赋值
                            prop.Prop?.SetValue(item, val ?? "");
                        }
                    }
                }
            }

            var itemCons = new List<KeyValuePair<WhereType, ConditionalModel>>();

            // [DictItemValue] 处理
            foreach (var cons in t.Select(GetDictItemCondModel))
            {
                itemCons.AddRange(cons);
            }

            itemCons = itemCons.Distinct().ToList();

            if (itemCons.Count > 0)
            {
                var itemModels = new List<IConditionalModel> { new ConditionalCollections { ConditionalList = itemCons } };

                var itemList = Context.Queryable("SysDictItem", "SysDictItem").Where(itemModels).ToSugarList();

                foreach (var item in t)
                {
                    // 获取具有[DictItemValue]的属性
                    var dictItemProps = type.GetProperties().Where(u => u.IsDefined(typeof(DictItemValue), false)).Select(
                        x =>
                            new ValueInfo
                            {
                                Prop = x,
                                ItemValue = x.TryGetAtrribute(out DictItemValue itemValue) ? itemValue : null
                            }).ToList();
                    foreach (var prop in dictItemProps)
                    {
                        if (prop.ItemValue == null) continue;
                        var infoValue = item.GetType().GetProperty(prop.ItemValue.Code)?.GetValue(item)?.ToString();

                        //var firstObj = itemList.FirstOrDefault(x => x.Code == infoValue);

                        var firstObj = itemList.FirstOrDefault(x =>
                            x.TryGetDynamicValue("Code", out string code) && code == infoValue);

                        // 获取当前数据指定列的值
                        var val = firstObj?.GetType().GetProperty(prop.ItemValue.ValueProp)?.GetValue(firstObj)
                            ?.ToString();
                        // 给当前属性赋值
                        prop.Prop?.SetValue(item, val ?? "");
                    }
                }
            }

            // [ForeignName] 处理
            // 获取所有表名
            var foreignProps = type.GetProperties().Where(u => u.IsDefined(typeof(ForeignName), false))
                .Select(x => new ValueInfo
                {
                    Prop = x,
                    ForeignName = x.TryGetAtrribute(out ForeignName foreignName) ? foreignName : null
                }).ToList();

            var tableList = foreignProps.Where(item => item.ForeignName != null)
                .Select(item => item.ForeignName!.TableName)
                .Distinct()
                .ToList();

            var tableInfo = new List<EntityTableInfo>();
            foreach (var tbName in tableList)
            {
                if (string.IsNullOrEmpty(tbName)) continue;
                var newInfo = GetDictForeignCondModel(t, new EntityTableInfo
                {
                    TableName = tbName,
                    Prop = null,
                    ConditionalModels = new List<IConditionalModel>()
                });

                newInfo.TableList = Context.Queryable<dynamic>().AS(tbName).Where(newInfo.ConditionalModels).ToSugarList();
                
                tableInfo.Add(newInfo);
            }

            // 设置对象的属性值
            foreach (var item in t)
            {
                foreach (var prop in foreignProps)
                {
                    if (prop.ForeignName == null) continue;
                    var infoValue = item.GetType().GetProperty(prop.ForeignName.PropName)?.GetValue(item)?.ToString();

                    var dataSet = tableInfo.FirstOrDefault(x => x.TableName == prop.ForeignName.TableName)?.TableList;

                    if (!int.TryParse(infoValue, out var propVal)) continue;
                    if (dataSet != null)
                    {
                        var firstObj = dataSet.FirstOrDefault(x => x.Id == propVal);
                        // 给当前属性赋值
                        prop.Prop?.SetValue(item, firstObj?.Name ?? "");
                    }
                }
            }

            // [EnumName] 处理
            var enumProps = type.GetProperties().Where(u => u.IsDefined(typeof(EnumName), false));
            foreach (var tModel in t)
            {
                foreach (var prop in enumProps)
                {
                    if (prop.TryGetAtrribute(out EnumName refEnum))
                    {
                        var enumProp = type.GetProperty(refEnum.Property);
                        if (enumProp != null)
                        {
                            var enumValue = enumProp.GetValue(tModel);

                            if (enumValue != null)
                            {
                                var enumType = enumProp.PropertyType;
                                string enumName;
                                //先尝试获取Description
                                if (enumType.TryGetAtrribute(out DescriptionAttribute description) &&
                                    !string.IsNullOrWhiteSpace(description.Description))
                                    enumName = description.Description;
                                else
                                    enumName = Enum.GetName(enumType, enumValue) ?? "";

                                prop.SetValue(tModel, enumName);
                            }
                        }
                    }
                }
            }

            return t;
        }

        /// <summary>
        /// 获取DictType查询条件
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private  List<KeyValuePair<WhereType, ConditionalModel>> GetDictTypeCondModel<T>( T t)
        {
            var type = typeof(T);

            // [DictTypeValue] 处理
            // 获取具有[DictTypeValue]的属性
            var dictTypeProps = type.GetProperties().Where(u => u.IsDefined(typeof(DictTypeValue), false))
                .Select(x => new ValueInfo
                {
                    Prop = x,
                    TypeValue = x.TryGetAtrribute(out DictTypeValue typeValue) ? typeValue : null
                }).ToList();

            // 处理属性值
            if (!dictTypeProps.Any()) return new List<KeyValuePair<WhereType, ConditionalModel>>();
            // 生成条件表达式
            var conditionalList = (from item in dictTypeProps
                    where item.TypeValue.Code != null
                    select t.GetType().GetProperty(item.TypeValue.Code)?.GetValue(t)?.ToString()
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
        private  List<KeyValuePair<WhereType, ConditionalModel>> GetDictItemCondModel<T>( T t)
        {
            var type = typeof(T);

            // [DictItemValue] 处理
            // 获取具有[DictItemValue]的属性
            var dictItemProps = type.GetProperties().Where(u => u.IsDefined(typeof(DictItemValue), false))
                .Select(x => new ValueInfo
                {
                    Prop = x,
                    ItemValue = x.TryGetAtrribute(out DictItemValue itemValue) ? itemValue : null,
                }).ToList();

            // 处理属性
            if (!dictItemProps.Any()) return new List<KeyValuePair<WhereType, ConditionalModel>>();
            // 生成条件表达式
            var conditionalList = (from item in dictItemProps
                    where item.ItemValue.Code != null
                    select t.GetType().GetProperty(item.ItemValue.Code)?.GetValue(t)?.ToString()
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
        private static EntityTableInfo GetDictForeignCondModel<T>(List<T> list, EntityTableInfo tableInfo)
        {
            var type = typeof(T);

            // 获取具有[ForeignName]的属性
            var foreignProps = type.GetProperties().Where(u => u.IsDefined(typeof(ForeignName), false))
                .Select(x => new ValueInfo
                {
                    Prop = x,
                    ForeignName = x.TryGetAtrribute(out ForeignName foreignName) ? foreignName : null
                }).ToList();

            if (!foreignProps.Any()) return new EntityTableInfo();

            var idList = new List<string>();

            foreach (var t in list)
            {
                // 处理属性

                var ids = foreignProps
                    .Where(item => item.ForeignName != null && item.ForeignName.TableName == tableInfo.TableName)
                    .Select(item => t.GetType().GetProperty(item.ForeignName!.PropName)?.GetValue(t)?.ToString())
                    .ToList();

                idList.AddRange(ids!);
            }

            var conModels = new List<IConditionalModel>
            {
                new ConditionalCollections
                {
                    ConditionalList =
                        idList.Distinct().Where(p => !string.IsNullOrWhiteSpace(p) && Convert.ToInt64(p) > 0)
                            .Select(infoValue => new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or,
                                new ConditionalModel { FieldName = "Id", FieldValue = infoValue }))
                            .ToList()
                }
            };


            tableInfo.ConditionalModels = conModels;
            return tableInfo;
        }
    }


    /// <summary>
    /// 枚举、字典、值对应关系
    /// </summary>
    internal class ValueInfo
    {
        public PropertyInfo Prop { get; set; }

        public DictItemValue ItemValue { get; set; }

        public DictTypeValue TypeValue { get; set; }

        public ForeignName ForeignName { get; set; }
    }

    /// <summary>
    /// 表，对象对应关系
    /// </summary>
    internal class EntityTableInfo
    {
        public string TableName { get; set; }

        public PropertyInfo Prop { get; set; }

        public List<dynamic> TableList { get; set; }

        public List<IConditionalModel> ConditionalModels { get; set; }
    }
}