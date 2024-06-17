using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace SqlSugar
{
    public static class OracleDateTimeOffsetSupport
    {
        public static Type GetWellFieldType(this IDataRecord reader, int i)
        {
            if (reader is OracleDataReader && "timestamptz".Equals(reader.GetDataTypeName(i).ToLower()))
            {
                return typeof(DateTimeOffset);
            }

            return reader.GetFieldType(i);
        }

        public static Type GetWellFieldType(this OracleDataReader reader, int i)
        {
            if ("timestamptz".Equals(reader.GetDataTypeName(i).ToLower()))
            {
                return typeof(DateTimeOffset);
            }

            return reader.GetFieldType(i);
        }

        public static object GetWellValue(this IDataRecord reader, int i, Type type)
        {
            if (reader is OracleDataReader)
            {
                return GetWellValue((OracleDataReader)reader, i, type);
            }

            return reader.GetValue(i);
        }

        public static object GetWellValue(this OracleDataReader reader, int i, Type type)
        {
            if (type == typeof(DateTimeOffset))
            {
                return reader.GetDateTimeOffset(i);
            }

            return reader.GetValue(i);
        }

        public static object GetWellValueOrDBNull(this IDataRecord reader, int i, Type type)
        {
            if (reader is OracleDataReader)
            {
                return GetWellValueOrDBNull((OracleDataReader)reader, i, type);
            }

            return reader.GetValue(i);
        }

        public static object GetWellValueOrDBNull(this OracleDataReader reader, int i, Type type)
        {
            if (type == typeof(DateTimeOffset) && !reader.IsDBNull(i))
            {
                return reader.GetDateTimeOffset(i);
            }

            return reader.GetValue(i);
        }

        public static object GetWellValue(this IDataRecord reader, int i)
        {
            if (reader is OracleDataReader)
            {
                return GetWellValue((OracleDataReader)reader, i);
            }

            return reader.GetValue(i);
        }

        public static object GetWellValue(this OracleDataReader reader, int i)
        {
            if ("timestamptz".Equals(reader.GetDataTypeName(i).ToLower()))
            {
                return reader.GetDateTimeOffset(i);
            }

            return reader.GetValue(i);
        }

        public static object GetWellValueOrDBNull(this IDataRecord reader, int i)
        {
            if (reader is OracleDataReader)
            {
                return GetWellValueOrDBNull((OracleDataReader)reader, i);
            }

            return reader.GetValue(i);
        }

        public static object GetWellValueOrDBNull(this OracleDataReader reader, int i)
        {
            if ("timestamptz".Equals(reader.GetDataTypeName(i).ToLower()) && !reader.IsDBNull(i))
            {
                return reader.GetDateTimeOffset(i);
            }

            return reader.GetValue(i);
        }

    }
}
