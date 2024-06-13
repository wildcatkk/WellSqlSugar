using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记外键表指定列（双主键）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SubForeignValue : Attribute
    {
        public readonly string ForeignTable;
        public readonly string ForeignColumn1;
        public readonly string ForeignValue1;
        public readonly string ForeignColumn2;
        public readonly string ResultColumn;
        public readonly string Value2Column;

        /// <summary>
        /// ForeignColumn1 + ForeignColumn2 => "Name"
        /// </summary>
        /// <param name="foreignTable">外键表名称</param>
        /// <param name="foreignColumn1">外键表主键列1</param>
        /// <param name="foreignValue1">外键表主键列1的值</param>
        /// <param name="foreignColumn2">外键表主键列2</param>
        /// <param name="values2Column">外键表主键列2的值 —— 当前表某个列</param>
        public SubForeignValue(string foreignTable, string foreignColumn1, string foreignValue1, string foreignColumn2, string values2Column)
        {
            ForeignTable = foreignTable;
            ForeignColumn1 = foreignColumn1;
            ForeignValue1 = foreignValue1;
            ForeignColumn2 = foreignColumn2;
            Value2Column = values2Column;
            ResultColumn = "Name";
        }

        /// <summary>
        /// ForeignColumn1 + ForeignColumn2 => ResultColumn
        /// </summary>
        /// <param name="foreignTable">外键表名称</param>
        /// <param name="foreignColumn1">外键表主键列1</param>
        /// <param name="foreignValue1">外键表主键列1的值</param>
        /// <param name="foreignColumn2">外键表主键列2</param>
        /// <param name="values2Column">外键表主键列2的值 —— 当前表某个列</param>
        /// <param name="resultColumn">外键表结果列</param>
        public SubForeignValue(string foreignTable, string foreignColumn1, string foreignValue1, string foreignColumn2, string values2Column, string resultColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn1 = foreignColumn1;
            ForeignValue1 = foreignValue1;
            ForeignColumn2 = foreignColumn2;
            Value2Column = values2Column;
            ResultColumn = resultColumn;
        }
    }
}