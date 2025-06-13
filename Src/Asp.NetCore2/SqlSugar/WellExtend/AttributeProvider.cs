using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性处理
    /// </summary>
    public class AttributeProvider
    {
        public static event Func<ISqlSugarClient, TableType, long, List<string>> GetMaskColumns;

        public static T Process<T>(ISqlSugarClient db, T obj, long? userId = null)
        {
            Process(db, new List<T> { obj }, typeof(T), userId);

            return obj;
        }

        public static object Process(ISqlSugarClient db, object obj, long? userId = null)
        {
            if (obj is IEnumerable)
                throw new Exception("类型异常，函数AttributeProvider.Process(ISqlSugarClient db, object obj)，参数obj不支持IEnumerable类型。");

            Process(db, new List<object> { obj }, obj.GetType(), userId);

            return obj;
        }

        public static object Process(ISqlSugarClient db, object obj, Type objType, long? userId = null)
        {
            if (obj is IEnumerable)
                throw new Exception("类型异常，函数AttributeProvider.Process(ISqlSugarClient db, object obj, Type objType)，参数obj不支持IEnumerable类型。");

            Process(db, new List<object> { obj }, objType, userId);

            return obj;
        }

        public static List<T> Process<T>(ISqlSugarClient db, List<T> list, long? userId = null)
        {
            Process(db, list, typeof(T), userId);

            return list;
        }

        public static ICollection Process(ISqlSugarClient db, ICollection list, Type type, long? userId = null)
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
            var enumNameInfoes = new List<EnumNameInfo>();
            var foreignTabelInfoes = new List<ForeignTableInfo>();
            foreach (var prop in tableType.Properties)
            {
                //枚举特性
                if (prop.EnumName != null)
                {
                    if (prop.Type != typeof(string))
                    {
                        throw new Exception($"特性EnumName({tableType.Name}.{prop.Name})仅支持string类型的属性。");
                    }
                    if (prop.EnumNameInfo is null)
                        prop.EnumNameInfo = new EnumNameInfo(prop, prop.EnumName, tableType);

                    if (prop.EnumNameInfo.ValueProperty != null)
                        enumNameInfoes.Add(prop.EnumNameInfo);
                }
                //外键表特性
                else if (prop.ForeignTable != null)
                {
                    if (prop.ForeignTableInfo is null)
                        prop.ForeignTableInfo = new ForeignTableInfo(prop, prop.ForeignTable, prop.ForeignConditions, tableType);

                    foreignTabelInfoes.Add(prop.ForeignTableInfo);
                }
            }

            // 2、[EnumName] 处理
            if (enumNameInfoes.Count > 0)
            {
                EnumNameProcess(list, enumNameInfoes);
            }

            // 3、[ForeignTable] 处理
            if (foreignTabelInfoes.Count > 0)
            {
                ForeignTableProcess(db, list, foreignTabelInfoes, userId);
            }

            MaskProcess(db, list, tableType, userId);

            return list;
        }

        private static void EnumNameProcess(ICollection list, List<EnumNameInfo> enumNameInfoes)
        {
            Dictionary<object, string> enumCache = new Dictionary<object, string>();
            foreach (var info in enumNameInfoes)
            {
                foreach (var t in list)
                {
                    var enumValue = info.ValueProperty.Info.GetValue(t);

                    if (enumValue is null) continue;

                    if (enumValue is Enum)
                    {
                        var enumStr = (enumValue as Enum).GetDescription(info.ValueProperty.Type);
                        info.AttributeProperty.Info.SetValue(t, enumStr);
                    }
                }

            }
        }

        private static object GetValue(object originalVal, ColumnProperty property)
        {
            Type originalValType = originalVal.GetType();
            if (originalValType != property.Type)
            {
                if (property.Type.IsEnum)
                {
                    if (originalValType != property.EnumValueType)
                        return Convert.ChangeType(originalVal, property.EnumValueType);
                    else
                        return originalVal;
                }
                else
                    return Convert.ChangeType(originalVal, property.Type);
            }
            else
                return originalVal;
        }

        private static bool Equals(List<ForeignConditionValueInfo> l1, List<ForeignConditionValueInfo> l2)
        {
            if (l1.Count != l2.Count) return false;

            foreach (var item1 in l1)
            {
                var item2 = l2.FirstOrDefault(p => p.Name == item1.Name);
                if (item2 is null) return false;
                if (item1.Value != item2.Value) return false;
                if (item1.ConditionalType != item2.ConditionalType) return false;
            }

            return true;
        }
        private static TableQueryInfo GetForeignTableQueryInfo(ICollection list, TableType tableType, List<ForeignTableInfo> foreignTabelInfoes)
        {
            var tableName = tableType.Name;
            var tableInfo = new TableQueryInfo(tableName);

            var tableCondition = new TableQueryConditionInfo();
            if (tableType.ILogicalDelete)
                tableCondition.ConditionalModels.Add(nameof(ILogicalDelete.IsDeleted), "0", typeof(long));
            var isFirst = true;
            ConditionalTree condiTree = new ConditionalTree
            {
                ConditionalList = new List<KeyValuePair<WhereType, IConditionalModel>>()
            };
            int condiIndex = 1;
            List<string> fieldNames = new List<string>();

            var foreignKeysGroup = new List<List<ForeignConditionValueInfo>>();
            foreach (var info in foreignTabelInfoes)
            {
                // 分组条件
                if (info.ForeignTableType.Name == tableName)
                {
                    foreach (var t in list)
                    {
                        var foreignKeys = new List<ForeignConditionValueInfo>();
                        foreach (var foreignCondi in info.ForeignConditions)
                        {
                            var condiType = foreignCondi.IsMultiValue ? ConditionalType.In : ConditionalType.Equal;
                            object propValue;
                            if (foreignCondi.ValueProperty != null)
                            {
                                propValue = foreignCondi.ValueProperty.Info.GetValue(t);
                            }
                            else
                            {
                                propValue = foreignCondi.Value;
                            }

                            string propValueStr;
                            if (propValue is null)
                            {
                                if (foreignCondi.Property.Type == typeof(string))
                                    condiType = ConditionalType.IsNullOrEmpty;
                                else
                                    condiType = ConditionalType.EqualNull;
                                propValueStr = null;
                            }
                            else
                            {
                                if (propValue is Enum)
                                    propValueStr = Convert.ToInt64(propValue).ToString();
                                else
                                    propValueStr = propValue.ToString();
                            }

                            foreignKeys.Add(new ForeignConditionValueInfo(foreignCondi.Property.Name, foreignCondi.Property.Type, propValueStr, condiType, foreignCondi.IsMultiValue));
                        }

                        // 查询条件去重
                        bool isExist = false;
                        foreach (var item in foreignKeysGroup)
                        {
                            if (Equals(foreignKeys, item))
                            {
                                isExist = true;
                                break;
                            }
                        }
                        if (isExist)
                            continue;
                        else
                            foreignKeysGroup.Add(foreignKeys);

                        // 组装复合查询条件
                        var condiModels = new List<KeyValuePair<WhereType, IConditionalModel>>();
                        bool isCellFirst = true;
                        var firstWhere = WhereType.And;
                        foreach (var foreignKey in foreignKeys)
                        {
                            var where = WhereType.And;
                            if (isCellFirst)
                            {
                                if (isFirst)
                                {
                                    where = WhereType.And;
                                    isFirst = false;
                                    isCellFirst = false;
                                }
                                else
                                {
                                    firstWhere = WhereType.Or;
                                    where = WhereType.Or;
                                    isCellFirst = false;
                                }
                            }

                            condiModels.Add(where, foreignKey.Name, foreignKey.Value, foreignKey.Type, foreignKey.ConditionalType);
                            condiIndex++;
                        }

                        condiTree.ConditionalList.Add(new KeyValuePair<WhereType, IConditionalModel>(firstWhere, SugarConditional.CreateTree(condiModels)));
                        if (condiIndex > 1000)
                        {
                            // 组装复合Select条件
                            foreach (var foreignCondi in info.ForeignConditions)
                            {
                                if (!fieldNames.Contains(foreignCondi.Property.Name))
                                {
                                    fieldNames.Add(foreignCondi.Property.Name);
                                    tableCondition.SelectModels.Add(new SelectModel() { FieldName = foreignCondi.Property.Name, AsName = foreignCondi.Property.Name });
                                }
                            }
                            if (!fieldNames.Contains(info.ResultProperty.Name))
                            {
                                fieldNames.Add(info.ResultProperty.Name);
                                tableCondition.SelectModels.Add(new SelectModel() { FieldName = info.ResultProperty.Name, AsName = info.ResultProperty.Name });
                            }
                            tableCondition.ConditionalModels.Add(condiTree);
                            tableInfo.Conditions.Add(tableCondition);

                            //重新开始
                            tableCondition = new TableQueryConditionInfo();
                            if (tableType.ILogicalDelete)
                                tableCondition.ConditionalModels.Add(nameof(ILogicalDelete.IsDeleted), "0", typeof(long));
                            isFirst = true;
                            condiTree = new ConditionalTree
                            {
                                ConditionalList = new List<KeyValuePair<WhereType, IConditionalModel>>()
                            };
                            condiIndex = 1;
                            fieldNames = new List<string>();
                        }
                    }

                    if (!isFirst)
                    {
                        // 组装复合Select条件
                        foreach (var foreignCondi in info.ForeignConditions)
                        {
                            if (!fieldNames.Contains(foreignCondi.Property.Name))
                            {
                                fieldNames.Add(foreignCondi.Property.Name);
                                tableCondition.SelectModels.Add(new SelectModel() { FieldName = foreignCondi.Property.Name, AsName = foreignCondi.Property.Name });
                            }
                        }
                        if (!fieldNames.Contains(info.ResultProperty.Name))
                        {
                            fieldNames.Add(info.ResultProperty.Name);
                            tableCondition.SelectModels.Add(new SelectModel() { FieldName = info.ResultProperty.Name, AsName = info.ResultProperty.Name });
                        }
                    }
                }
            }
            if (!isFirst)
            {
                tableCondition.ConditionalModels.Add(condiTree);
                tableInfo.Conditions.Add(tableCondition);
            }

            return tableInfo;
        }

        private static Type StringType = typeof(string);
        private static void ForeignTableProcess(ISqlSugarClient db, ICollection list, List<ForeignTableInfo> foreignTabelInfoes, long? userId)
        {
            bool isMaskColumns = (userId >= 0 && GetMaskColumns != null);
            // 根据表名分组聚合查询
            var tableQueryInfoes = new List<TableQueryInfo>();
            var tableNames = new List<string>();
            foreach (var info in foreignTabelInfoes)
            {
                var tableName = info.ForeignTableType.Name;
                if (tableNames.Contains(tableName))
                    continue;
                tableNames.Add(tableName);

                //根据数据集组装条件
                var tableQueryInfo = GetForeignTableQueryInfo(list, info.ForeignTableType, foreignTabelInfoes);

                //查询数据库获取结果
                foreach (var condition in tableQueryInfo.Conditions)
                {
                    var dataRows = db.Queryable<dynamic>().AS(tableName).Where(condition.ConditionalModels).Select(condition.SelectModels).ToSugarList();
                    if (dataRows.Count > 0)
                        tableQueryInfo.DataTable.AddRange(dataRows);
                }
                
                //处理掩码加密列
                if (isMaskColumns && info.ForeignTableType.DbTable != null)
                {
                    tableQueryInfo.MaskColumns = GetMaskColumns.Invoke(db, info.ForeignTableType.DbTable, userId.Value);
                }

                tableQueryInfoes.Add(tableQueryInfo);
            }

            //按表名和主键条件回填数据
            foreach (var t in list)
            {
                foreach (var info in foreignTabelInfoes)
                {
                    //外键表数据及信息收集对象
                    var tableQueryInfo = tableQueryInfoes.FirstOrDefault(x => x.TableName == info.ForeignTableType.Name);
                    if (tableQueryInfo is null) continue;

                    //掩码加密列
                    var maskColumns = tableQueryInfo.MaskColumns;
                    if (maskColumns.Count > 0 && info.AttributeProperty.Type == StringType && maskColumns.Contains(info.AttributeProperty.ForeignTable.ResultColumn))
                    {
                        info.AttributeProperty.Info.SetValue(t, "******");
                        continue;
                    }

                    var dataTable = tableQueryInfo.DataTable;
                    if (dataTable is null || dataTable.Count == 0) continue;

                    var foreignKeys = new List<ForeignCompareValueInfo>();
                    bool isMultiValue = false;
                    foreach (var foreignCondi in info.ForeignConditions)
                    {
                        var propName = foreignCondi.Property.Name;
                        object propValue;
                        if (foreignCondi.ValueProperty != null)
                        {
                            propValue = foreignCondi.ValueProperty.Info.GetValue(t);
                        }
                        else
                        {
                            propValue = foreignCondi.Value;
                        }

                        string propValueStr;
                        if (propValue is null)
                            propValueStr = null;
                        else
                        {
                            if (propValue is Enum)
                                propValueStr = Convert.ToInt64(propValue).ToString();
                            else
                                propValueStr = propValue.ToString();
                        }

                        foreignKeys.Add(new ForeignCompareValueInfo(foreignCondi.Property.Name, foreignCondi.Property.Type, propValueStr, foreignCondi.IsMultiValue));

                        isMultiValue |= foreignCondi.IsMultiValue;
                    }

                    if (isMultiValue)
                    {
                        var dataResult = new List<DataResultSorted>();
                        foreach (var dataRow in dataTable)
                        {
                            //比较复合主键以查找返回的数据，这里匹配所有可能的数据
                            bool isSuccess = true;
                            int index = -1;
                            foreach (var foreignKey in foreignKeys)
                            {
                                if (DynamicExtensions.TryGetDynamicValue(dataRow, foreignKey.Name, out object columnValue))
                                {
                                    if (columnValue is null && foreignKey.Value is null)
                                        continue;
                                    else if (columnValue != null && (columnValue.ToString().Equals(foreignKey.Value) || foreignKey.Values.Contains(columnValue.ToString())))
                                    {
                                        if (foreignKey.Values.Contains(columnValue.ToString()))
                                        {
                                            var fIndex = foreignKey.Values.IndexOf(columnValue.ToString());
                                            if (index == -1)
                                                index = fIndex;
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        isSuccess = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    isSuccess = false;
                                    break;
                                }
                            }

                            if (isSuccess)
                                dataResult.Add(new DataResultSorted { Index = index, Value = dataRow });
                        }

                        if (dataResult.Count > 0)
                        {
                            var results = dataResult.OrderBy(p => p.Index);
                            var resultMultiValue = "";
                            foreach (var dataRow in results)
                            {
                                // 给当前属性赋值
                                if (DynamicExtensions.TryGetDynamicValue(dataRow.Value, info.ResultProperty.Name, out object resultValue) && resultValue != null)
                                {
                                    resultMultiValue += GetValue(resultValue, info.AttributeProperty).ToString() + ",";
                                }
                            }
                            resultMultiValue = resultMultiValue.TrimEnd(',');

                            info.AttributeProperty.Info.SetValue(t, resultMultiValue);
                        }
                    }
                    else
                    {
                        foreach (var dataRow in dataTable)
                        {
                            //比较复合主键以查找返回的数据，这里仅匹配第一个
                            bool isSuccess = true;
                            foreach (var foreignKey in foreignKeys)
                            {
                                if (DynamicExtensions.TryGetDynamicValue(dataRow, foreignKey.Name, out object columnValue))
                                {
                                    if (columnValue is null && foreignKey.Value is null)
                                        continue;
                                    else if (columnValue != null && columnValue.ToString().Equals(foreignKey.Value))
                                        continue;
                                    else
                                    {
                                        isSuccess = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    isSuccess = false;
                                    break;
                                }
                            }

                            if (isSuccess)
                            {
                                // 给当前属性赋值
                                if (DynamicExtensions.TryGetDynamicValue(dataRow, info.ResultProperty.Name, out object resultValue) && resultValue != null)
                                {
                                    info.AttributeProperty.Info.SetValue(t, GetValue(resultValue, info.AttributeProperty));
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        private static void MaskProcess(ISqlSugarClient db, ICollection list, TableType tableType, long? userId)
        {
            if (userId >= 0 && GetMaskColumns != null && tableType.DbTable != null)
            {
                var maskColumns = GetMaskColumns.Invoke(db, tableType.DbTable, userId.Value);
                var maskProps = tableType.Properties.Where(p => maskColumns.Contains(p.Name) && p.Type == typeof(string));

                if (maskProps.Any())
                {
                    foreach (var t in list)
                    {
                        foreach (var prop in maskProps)
                        {
                            prop.Info.SetValue(t, "******");
                        }
                    }
                }
            }
        }
    }

    public class TableQueryInfo
    {
        public TableQueryInfo(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }

        public List<dynamic> DataTable { get; set; } = new List<dynamic>();

        public List<TableQueryConditionInfo> Conditions { get; set; } = new List<TableQueryConditionInfo>();

        public List<string> MaskColumns { get; set; } = new List<string>();
    }

    public class TableQueryConditionInfo
    {
        public List<IConditionalModel> ConditionalModels { get; set; } = new List<IConditionalModel>();

        public List<SelectModel> SelectModels { get; set; } = new List<SelectModel>();

    }

    public class ForeignCompareValueInfo
    {
        public ForeignCompareValueInfo(string name, Type type, string value, bool isMultiValue)
        {
            Name = name;
            Type = type;
            Value = value;
            IsMultiValue = isMultiValue;

            if (IsMultiValue && !string.IsNullOrEmpty(value))
                Values = value.Split(',').ToList();
            else
                Values = new List<string>();
        }

        public string Name { get; set; }

        public Type Type { get; set; }

        public string Value { get; set; }

        public List<string> Values { get; set; }

        public bool IsMultiValue { get; set; }
    }

    public class ForeignConditionValueInfo
    {
        public ForeignConditionValueInfo(string name, Type type, string value, ConditionalType condiType, bool isMultiValue)
        {
            Name = name;
            Type = type;
            Value = value;
            ConditionalType = condiType;
            IsMultiValue = isMultiValue;
        }

        public string Name { get; set; }

        public Type Type { get; set; }

        public string Value { get; set; }

        public ConditionalType ConditionalType { get; set; }

        public bool IsMultiValue { get; set; }
    }

    public class DataResultSorted
    {
        public int Index { get; set; }

        public dynamic Value { get; set; }
    }
}