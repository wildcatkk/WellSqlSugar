using System;

namespace SqlSugar
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class ForeignListValue : Attribute
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

        public ForeignListValue(string foreignTable, string valueColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = "Id";
            ValueColumn = valueColumn;
            ResultColumn = "Name";
        }

        public ForeignListValue(string foreignTable, string foreignColumn, string valueColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
            ValueColumn = valueColumn;
            ResultColumn = "Name";
        }

        public ForeignListValue(string foreignTable, string foreignColumn, string valueColumn, string resultColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
            ValueColumn = valueColumn;
            ResultColumn = resultColumn;
        }
    }
}
