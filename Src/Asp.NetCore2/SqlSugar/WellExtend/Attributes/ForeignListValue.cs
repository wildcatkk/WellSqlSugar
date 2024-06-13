using System;

namespace SqlSugar
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ForeignListValue : Attribute
    {
        public readonly string ForeignTable;
        public readonly string ForeignColumn;
        public readonly string ResultColumn;
        public readonly string ValueColumn;
        public readonly bool IsId;

        public ForeignListValue(string foreignTable, string valueColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = "Id";
            ResultColumn = "Name";
            IsId = true;
            ValueColumn = valueColumn;
        }

        public ForeignListValue(string foreignTable, string foreignColumn, string valueColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
            IsId = "Id".Equals(foreignColumn);
            ResultColumn = "Name";
            ValueColumn = valueColumn;
        }

        public ForeignListValue(string foreignTable, string foreignColumn, string valueColumn, string resultColumn)
        {
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
            IsId = "Id".Equals(foreignColumn);
            ResultColumn = resultColumn;
            ValueColumn = valueColumn;
        }
    }
}
