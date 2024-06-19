using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar
{
    public static class SugarParameterExtension
    {
        public static List<SugarParameter> Copy(this List<SugarParameter> list)
        {
            if (list == null)
                return null;

            var newList = new List<SugarParameter>();
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    newList.Add(new SugarParameter(item.ParameterName, item.Value, item.DbType, item.Direction, item.Size)
                    {
                        Precision = item.Precision,
                        Scale = item.Scale,
                        IsRefCursor = item.IsRefCursor,
                        IsClob = item.IsClob,
                        IsNClob = item.IsNClob,
                        IsNvarchar2 = item.IsNvarchar2,
                        IsNullable = item.IsNullable,
                        SourceColumn = item.SourceColumn,
                        SourceColumnNullMapping = item.SourceColumnNullMapping,
                        UdtTypeName = item.UdtTypeName,
                        TempDate = item.TempDate,
                        SourceVersion = item.SourceVersion,
                        TypeName = item.TypeName,
                        IsJson = item.IsJson,
                        IsArray = item.IsArray,
                        CustomDbType = item.CustomDbType
                    });
                }
            }

            return newList;
        }
        public static SugarParameter[] CopyToArray(this List<SugarParameter> list)
        {
            if (list == null)
                return null;

            var newList = new List<SugarParameter>();
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    newList.Add(new SugarParameter(item.ParameterName, item.Value, item.DbType, item.Direction, item.Size)
                    {
                        Precision = item.Precision,
                        Scale = item.Scale,
                        IsRefCursor = item.IsRefCursor,
                        IsClob = item.IsClob,
                        IsNClob = item.IsNClob,
                        IsNvarchar2 = item.IsNvarchar2,
                        IsNullable = item.IsNullable,
                        SourceColumn = item.SourceColumn,
                        SourceColumnNullMapping = item.SourceColumnNullMapping,
                        UdtTypeName = item.UdtTypeName,
                        TempDate = item.TempDate,
                        SourceVersion = item.SourceVersion,
                        TypeName = item.TypeName,
                        IsJson = item.IsJson,
                        IsArray = item.IsArray,
                        CustomDbType = item.CustomDbType
                    });
                }
            }

            return newList.ToArray();
        }
    }
}
