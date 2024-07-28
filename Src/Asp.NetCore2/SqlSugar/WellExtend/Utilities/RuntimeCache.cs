using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;

namespace SqlSugar
{
    public static class RuntimeCache
    {
        private static List<TableType>? _tables = null;
        private static object _tablesLock = new object();
        public static List<TableType> Tables
        {
            get
            {
                if (_tables is null)
                {
                    lock (_tablesLock)
                    {
                        if (_tables is null)
                            _tables = RuntimeUtil.GetSugarTables().Select(p => new TableType(p)).ToList();
                    }
                }

                return _tables;
            }
        }

        public static bool IsSugarTable(this string tableName)
        {
            if (!string.IsNullOrEmpty(tableName) && Tables.Count > 0)
            {
                return Tables.Exists(p => p.Name.ToLower().Equals(tableName.Trim().ToLower()));
            }

            return false;
        }

        public static TableType GetTable(this string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var item = Tables.FirstOrDefault(p => p.Name.ToLower().Equals(name.Trim().ToLower()));
                if (item is null)
                {
                    var items = RuntimeUtil.GetTypes(u => !u.IsInterface && u is { IsAbstract: false, IsClass: true } && u.Name.ToLower().Equals(name.Trim().ToLower()));
                    if (items.Any())
                    {
                        item = new TableType(items.First());
                        lock (_tablesLock)
                            _tables.Add(item);
                    }
                }

                return item;
            }

            return default;
        }

        public static TableType GetTable(this Type tableType)
        {
            if (tableType != null)
            {
                var item = Tables.FirstOrDefault(p => p.Name.Equals(tableType.Name));
                if (item is null)
                {
                    item = new TableType(tableType);
                    lock (_tablesLock)
                        _tables.Add(item);
                }

                return item;
            }

            return default;
        }
    }

    public class TableType
    {
        public TableType(Type type)
        {
            Type = type;
            Name = type.Name;
            var props = type.GetProperties();
            if (props.Length > 0)
            {
                Properties = props.Select(p => new ColumnProperty(p)).ToList();
            }
            else
                Properties = new List<ColumnProperty>();

            var interfaces = type.GetInterfaces();
            if (interfaces.Length > 0)
            {
                ILogicalDelete = interfaces.Contains(typeof(ILogicalDelete));
                IFactory = interfaces.Contains(typeof(IFactory));
                IGroupCo = interfaces.Contains(typeof(IGroupCo));
            }


            IsDiffLog = type.IsDefined(typeof(DiffLog));
        }

        public string Name { get; set; }

        public Type Type { get; set; }

        public bool ILogicalDelete { get; set; }
        public bool IFactory { get; set; }
        public bool IGroupCo { get; set; }

        public bool IsDiffLog { get; set; }

        public List<ColumnProperty> Properties { get; set; }

        public ColumnProperty GetProperty(string name)
        {
            if (!string.IsNullOrEmpty(name) && Properties.Count > 0)
            {
                return Properties.FirstOrDefault(p => p.Name.ToLower().Equals(name.Trim().ToLower()));
            }

            return default;
        }
    }

    public class ColumnProperty
    {
        public ColumnProperty(PropertyInfo property)
        {
            Info = property;
            Name = property.Name;
            Type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (Type.IsEnum)
            {
                EnumValueType = Enum.GetUnderlyingType(Type);
            }

            // 枚举特性
            if (Info.TryGetAtrribute(out EnumName enumNameAttr))
                EnumName = enumNameAttr;

            if(Info.TryGetAtrribute(out ForeignTable foreignTableAttr))
            {
                if (Info.TryGetAtrributes(out List<ForeignCondition> conditions))
                {
                    ForeignTable = foreignTableAttr;
                    ForeignConditions = conditions;

                    if (foreignTableAttr.FactoryId != null || !string.IsNullOrEmpty(foreignTableAttr.FactoryIdColumn))
                    {
                        ForeignConditions.Add(new ForeignCondition(nameof(IFactory.FactoryId), foreignTableAttr.FactoryIdColumn, foreignTableAttr.FactoryId, false));
                    }
                }
            }

            if (foreignTableAttr is null)
            {
                if (Info.TryGetAtrribute(out DictItemValue dictItemValueAttr))
                {
                    ForeignTable = new ForeignTable("SysDictItem", dictItemValueAttr.ResultColumn);
                    ForeignConditions = new List<ForeignCondition>
                    {
                        new ForeignCondition("ParentCode", "", dictItemValueAttr.ParentCode, false),
                        new ForeignCondition("Code", dictItemValueAttr.CodeColumn, null, false)
                    };
                }
                else if (Info.TryGetAtrribute(out DictTypeValue dictTypeValueAttr))
                {
                    ForeignTable = new ForeignTable("SysDictType", dictTypeValueAttr.ResultColumn);
                    ForeignConditions = new List<ForeignCondition>
                    {
                        new ForeignCondition("Code", dictTypeValueAttr.CodeColumn, null, false)
                    };
                }
                else if (Info.TryGetAtrribute(out ForeignValue foreignValueAttr))
                {
                    ForeignTable = new ForeignTable(foreignValueAttr.ForeignTable, foreignValueAttr.ResultColumn);
                    ForeignConditions = new List<ForeignCondition>
                    {
                        new ForeignCondition(foreignValueAttr.ForeignColumn, foreignValueAttr.ValueColumn, null, false)
                    };

                    if (foreignValueAttr.FactoryId != null || !string.IsNullOrEmpty(foreignValueAttr.FactoryIdColumn))
                    {
                        ForeignConditions.Add(new ForeignCondition(nameof(IFactory.FactoryId), foreignValueAttr.FactoryIdColumn, foreignValueAttr.FactoryId, false));
                    }
                }
                else if (Info.TryGetAtrribute(out ForeignListValue foreignListValueAttr))
                {
                    ForeignTable = new ForeignTable(foreignListValueAttr.ForeignTable, foreignListValueAttr.ResultColumn);
                    ForeignConditions = new List<ForeignCondition>
                    {
                        new ForeignCondition(foreignListValueAttr.ForeignColumn, foreignListValueAttr.ValueColumn, null, true)
                    };

                    if (foreignListValueAttr.FactoryId != null || !string.IsNullOrEmpty(foreignListValueAttr.FactoryIdColumn))
                    {
                        ForeignConditions.Add(new ForeignCondition(nameof(IFactory.FactoryId), foreignListValueAttr.FactoryIdColumn, foreignListValueAttr.FactoryId, false));
                    }
                }
            }
        }

        public string Name { get; set; }

        public PropertyInfo Info { get; set; }

        public Type Type { get; set; }

        public Type EnumValueType { get; set; }


        public EnumName EnumName { get; set; } = null;
        public EnumNameInfo EnumNameInfo { get; set; } = null;

        public ForeignTable ForeignTable { get; set; } = null;
        public List<ForeignCondition> ForeignConditions { get; set; } = null;

        public DictItemValue DictItemValue { get; set; } = null;
        public DictTypeValue DictTypeValue { get; set; } = null;
        public ForeignValue ForeignValue { get; set; } = null;
        public ForeignListValue ForeignListValue { get; set; } = null;

        public ForeignTableInfo ForeignTableInfo { get; set; } = null;
    }
}
