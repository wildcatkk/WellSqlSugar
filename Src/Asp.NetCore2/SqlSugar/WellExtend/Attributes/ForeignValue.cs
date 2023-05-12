using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记外键表字段（单主键）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ForeignValue : Attribute
    {
        public readonly string TableName;
        public readonly string TableColumn;
        public readonly string TargetColumn;
        public readonly string Property;
        public readonly bool IsId;

        /// <summary>
        /// "Id" => "Name"
        /// </summary>
        /// <param name="tableName">外表名称</param>
        /// <param name="property">当前表查询值字段</param>
        public ForeignValue(string tableName, string property)
        {
            TableName = tableName;
            TableColumn = "Id";
            TargetColumn = "Name";
            IsId = true;
            Property = property;
        }

        /// <summary>
        /// tableColumn => "Name"
        /// </summary>
        /// <param name="tableName">外表名称</param>
        /// <param name="tableColumn">外键表(逻辑)主键列</param>
        /// <param name="property">当前表查询值字段</param>
        public ForeignValue(string tableName, string tableColumn, string property)
        {
            TableName = tableName;
            TableColumn = tableColumn;
            IsId = "Id".Equals(tableColumn);
            TargetColumn = "Name";
            Property = property;
        }

        /// <summary>
        /// tableColumn => targetColumn
        /// </summary>
        /// <param name="tableName">外表名称</param>
        /// <param name="tableColumn">外键表(逻辑)主键列</param>
        /// <param name="property">当前表查询值字段</param>
        /// <param name="targetColumn">外键表目标字段</param>
        public ForeignValue(string tableName, string tableColumn, string property, string targetColumn)
        {
            TableName = tableName;
            TableColumn = tableColumn;
            IsId = "Id".Equals(tableColumn);
            TargetColumn = targetColumn;
            Property = property;
        }
    }
}