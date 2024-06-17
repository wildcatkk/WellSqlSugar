using System;
using System.Collections.Generic;
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
                return Tables.Exists(p => p.Type.Name.ToLower().Equals(tableName.Trim().ToLower()));
            }

            return false;
        }

        public static TableType GetTable(this string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var item = Tables.FirstOrDefault(p => p.Type.Name.ToLower().Equals(name.Trim().ToLower()));
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
                var item = Tables.FirstOrDefault(p => p.Type.Name.Equals(tableType.Name));
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
            var props = type.GetProperties();
            if (props.Length > 0)
            {
                Properties = props.Select(p => new ColumnProperty(p)).ToList();
            }
            else
                Properties = new List<ColumnProperty>();
        }

        public Type Type { get; set; }

        public List<ColumnProperty> Properties { get; set; }

        public ColumnProperty GetProperty(string name)
        {
            if (!string.IsNullOrEmpty(name) && Properties.Count > 0)
            {
                return Properties.FirstOrDefault(p => p.Info.Name.ToLower().Equals(name.Trim().ToLower()));
            }

            return default;
        }
    }

    public class ColumnProperty
    {
        public ColumnProperty(PropertyInfo property)
        {
            Info = property;
            Type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (Type.IsEnum)
            {
                EnumValueType = Enum.GetUnderlyingType(Type);
            }
        }

        public PropertyInfo Info { get; set; }

        public Type Type { get; set; }

        public Type EnumValueType { get; set; }
    }
}
