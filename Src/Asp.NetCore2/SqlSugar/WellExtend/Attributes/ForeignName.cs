using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记外键的Name
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ForeignName : Attribute
    {
        public readonly string TableName;
        public readonly string PropName;

        public ForeignName(string tableName, string propName)
        {
            TableName = tableName;
            PropName = propName;
        }
    }
}