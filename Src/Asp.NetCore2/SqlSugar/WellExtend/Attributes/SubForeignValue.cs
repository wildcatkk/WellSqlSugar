using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记外键表(子表)字段（双主键）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SubForeignValue : Attribute
    {
        public readonly string TableName;
        public readonly string ParentColumn;
        public readonly string ParentKey;
        public readonly string TableColumn;
        public readonly string TargetColumn;
        public readonly string Property;

        /// <summary>
        /// tableColumn => "Name"
        /// </summary>
        /// <param name="tableName">外表名称</param>
        /// <param name="parentKey">父表主键值</param>
        /// <param name="tableColumn">外键表(逻辑)复合主键列</param>
        /// <param name="property">当前表查询值字段</param>
        public SubForeignValue(string tableName, string parentColumn, string parentKey, string tableColumn, string property)
        {
            TableName = tableName;
            ParentColumn = parentColumn;
            ParentKey = parentKey;
            TableColumn = tableColumn;
            TargetColumn = "Name";
            Property = property;
        }

        /// <summary>
        /// tableColumn => targetColumn
        /// </summary>
        /// <param name="tableName">外表名称</param>
        /// <param name="parentKey">父表主键值</param>
        /// <param name="tableColumn">外键表(逻辑)复合主键列</param>
        /// <param name="property">当前表查询值字段</param>
        /// <param name="targetColumn">外键表目标字段</param>
        public SubForeignValue(string tableName, string parentColumn, string parentKey, string tableColumn, string property, string targetColumn)
        {
            TableName = tableName;
            ParentColumn = parentColumn;
            ParentKey = parentKey;
            TableColumn = tableColumn;
            TargetColumn = targetColumn;
            Property = property;
        }
    }
}