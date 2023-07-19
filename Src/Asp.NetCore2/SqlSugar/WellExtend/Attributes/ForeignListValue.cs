using System;

namespace SqlSugar
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ForeignListValue : Attribute
    {
        public readonly string TableName;

        public readonly string TableColumn;

        public readonly string TargetColumn;

        public readonly string Property;

        public readonly bool IsId;

        public ForeignListValue(string tableName, string property)
        {
            TableName = tableName;
            TableColumn = "Id";
            TargetColumn = "Name";
            IsId = true;
            Property = property;
        }

        public ForeignListValue(string tableName, string tableColumn, string property)
        {
            TableName = tableName;
            TableColumn = tableColumn;
            IsId = "Id".Equals(tableColumn);
            TargetColumn = "Name";
            Property = property;
        }

        public ForeignListValue(string tableName, string tableColumn, string property, string targetColumn)
        {
            TableName = tableName;
            TableColumn = tableColumn;
            IsId = "Id".Equals(tableColumn);
            TargetColumn = targetColumn;
            Property = property;
        }
    }
}
