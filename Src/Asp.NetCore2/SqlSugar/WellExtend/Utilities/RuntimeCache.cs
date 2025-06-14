using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
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

        private static Dictionary<string, bool> _tableConfigs = null;
        private static object _tableConfigsLock = new object();
        public static void InitTableConfig(Dictionary<string, bool> configs)
        {
            if (_tableConfigs is null)
            {
                lock (_tableConfigsLock)
                {
                    if (_tableConfigs is null)
                    {
                        _tableConfigs = configs is null ? new Dictionary<string, bool>() : new Dictionary<string, bool>(configs);
                    }
                }
            }
        }

        public static bool? GetIGroupCoFromTableConfig(this Type type)
        {
            lock (_tableConfigsLock)
            {
                if (_tableConfigs is null || _tableConfigs.Count == 0)
                    return null;

                if (_tableConfigs.ContainsKey(type.FullName))
                    return _tableConfigs[type.FullName];

                if (_tableConfigs.ContainsKey(type.Name))
                    return _tableConfigs[type.Name];

                return null;
            }
        }

        public static bool IsSugarTable(this string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return false;

            tableName = tableName.Trim();
            var type = GetTable(tableName);
            if (type is null)
                return false;

            return type.Type.IsSugarTable();
        }

        public static bool IsSugarTable(this TableType tableType)
            => tableType.Type.IsSugarTable();


        public static TableType GetTable(this string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                tableName = tableName.Trim();
                var item = Tables.FirstOrDefault(p => p.Name.Equals(tableName));
                if (item is null)
                {
                    var items = RuntimeUtil.GetTypes(u => !u.IsInterface && u is { IsAbstract: false, IsClass: true } && u.Name.Equals(tableName));
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


        public static bool TryGetTable([NotNullWhen(true)] this string tableName, out TableType tableType)
        {
            tableType = GetTable(tableName);

            return tableType != null;
        }

        public static bool TryGetTable([NotNullWhen(true)] this Type type, out TableType tableType)
        {
            tableType = GetTable(type);

            return tableType != null;
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

            var iGroupCoConfig = type.GetIGroupCoFromTableConfig();
            if (iGroupCoConfig != null)
                IGroupCo = iGroupCoConfig.Value;

            if (type.TryGetAtrribute(out TenantAttribute tenantAttr))
                ConfigId = tenantAttr.configId;

            if (type.TryGetObjectAtrribute(out SugarTable sugarTableAttr))
            {
                SugarTable = sugarTableAttr;
                DbTable = this;
            }
            else
            {
                var sugarTableType = GetSugarTableType(type);
                if (sugarTableType != null)
                {
                    DbTable = sugarTableType.GetTable();
                }
            }

            IsDiffLog = type.IsDefined(typeof(DiffLog));
        }

        private Type GetSugarTableType(Type type)
        {
            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                if (type.BaseType.TryGetObjectAtrribute(out SugarTable sugarTableAttr))
                    return type.BaseType;
                else
                    return GetSugarTableType(type.BaseType);
            }

            return null;
        }

        public string Name { get; set; }

        public Type Type { get; set; }

        public TableType DbTable { get; set; }

        public bool ILogicalDelete { get; set; }
        public bool IFactory { get; set; }
        public bool IGroupCo { get; set; }

        public bool IsDiffLog { get; set; }

        public object ConfigId { get; set; }

        public SugarTable SugarTable { get; set; }

        public List<ColumnProperty> Properties { get; set; }

        public ColumnProperty GetProperty(string name)
        {
            if (!string.IsNullOrEmpty(name) && Properties.Count > 0)
            {
                name = name.Trim();
                return Properties.FirstOrDefault(p => p.Name.Equals(name));
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

            if (Info.TryGetAtrribute(out ForeignTable foreignTableAttr))
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
