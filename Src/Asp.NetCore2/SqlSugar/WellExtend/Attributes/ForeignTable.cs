using System;

namespace SqlSugar
{
    /// <summary>
    /// 外键表
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class ForeignTable : Attribute
    {
        /// <summary>
        /// 外键表
        /// </summary>
        public string Table { get; }

        public string ResultColumn { get; }

        /// <summary>
        /// 工厂值
        /// </summary>
        public object FactoryId { get; set; } = null;

        /// <summary>
        /// 工厂值所在列
        /// </summary>
        public string FactoryIdColumn { get; set; }

        public ForeignTable(string table)
        {
            this.Table = table;
            this.ResultColumn = "Name";
        }

        public ForeignTable(string table, string resultColumn)
        {
            this.Table = table;
            this.ResultColumn = resultColumn;
        }
    }

    /// <summary>
    /// 外键条件
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ForeignCondition : Attribute
    {
        /// <summary>
        /// 条件列
        /// </summary>
        public string Column { get; }

        /// <summary>
        /// 条件值
        /// </summary>
        public object Value { get; set; } = null;

        /// <summary>
        /// 条件值所在列
        /// </summary>
        public string ValueColumn { get; }

        public bool IsMultiValue { get; set; }

        public ForeignCondition(string column)
        {
            this.Column = column;
            this.ValueColumn = "";
        }

        public ForeignCondition(string column, string valueColumn)
        {
            this.Column = column;
            this.ValueColumn = valueColumn;
        }

        public ForeignCondition(string column, string valueColumn, object value, bool isMultiValue)
        {
            this.Column = column;
            this.ValueColumn = valueColumn;
            this.Value = value;
            this.IsMultiValue = isMultiValue;
        }
    }

}
