using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记外键表指定列（单主键）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class ForeignValue : Attribute
    {
        public string ForeignTable { get; }
        public string ForeignColumn { get; }
        public string ValueColumn { get; }
        public string ResultColumn { get; }

        /// <summary>
        /// 工厂值
        /// </summary>
        public object FactoryId { get; set; } = null;

        /// <summary>
        /// 工厂值所在列
        /// </summary>
        public string FactoryIdColumn { get; set; }

        /// <summary>
        /// "Id" => "Name"
        /// </summary>
        /// <param name="foreignTable">外键表名称</param>
        /// <param name="valueColumn">外键表主键列的值 —— 当前表某个列</param>
        public ForeignValue(string foreignTable, string valueColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = "Id";
            ValueColumn = valueColumn;
            ResultColumn = "Name";
        }

        /// <summary>
        /// ForeignColumn => "Name"
        /// </summary>
        /// <param name="foreignTable">外键表名称</param>
        /// <param name="foreignColumn">外键表主键列</param>
        /// <param name="valueColumn">外键表主键列的值 —— 当前表某个列</param>
        public ForeignValue(string foreignTable, string foreignColumn, string valueColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
            ValueColumn = valueColumn;
            ResultColumn = "Name";
        }

        /// <summary>
        /// ForeignColumn => ResultColumn
        /// </summary>
        /// <param name="foreignTable">外键表名称</param>
        /// <param name="foreignColumn">外键表主键列</param>
        /// <param name="valueColumn">外键表主键列的值 —— 当前表某个列</param>
        /// <param name="resultColumn">外键表结果列</param>
        public ForeignValue(string foreignTable, string foreignColumn, string valueColumn, string resultColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
            ValueColumn = valueColumn;
            ResultColumn = resultColumn;
        }
    }
}