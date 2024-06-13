using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记外键表指定列（单主键）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ForeignValue : Attribute
    {
        public readonly string ForeignTable;
        public readonly string ForeignColumn;
        public readonly string ResultColumn;
        public readonly string ValueColumn;
        public readonly bool IsId;

        /// <summary>
        /// "Id" => "Name"
        /// </summary>
        /// <param name="foreignTable">外键表名称</param>
        /// <param name="valueColumn">外键表主键列的值 —— 当前表某个列</param>
        public ForeignValue(string foreignTable, string valueColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = "Id";
            ResultColumn = "Name";
            IsId = true;
            ValueColumn = valueColumn;
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
            IsId = "Id".Equals(foreignColumn);
            ResultColumn = "Name";
            ValueColumn = valueColumn;
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
            IsId = "Id".Equals(foreignColumn);
            ResultColumn = resultColumn;
            ValueColumn = valueColumn;
        }
    }
}