using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar
{
    public class ForeignTableInfo
    {
        public ForeignTableInfo(ColumnProperty attributeProperty, ForeignTable foreignTableAttr, List<ForeignCondition> foreignConditionAttrs, TableType tableType)
        {
            AttributeProperty = attributeProperty;

            ForeignTableType = foreignTableAttr.Table.GetTable();
            if (ForeignTableType is null)
                throw new Exception($"Unknow foreign table [{foreignTableAttr.Table}]");

            ResultProperty = ForeignTableType.GetProperty(foreignTableAttr.ResultColumn);
            if (ResultProperty is null)
                throw new Exception($"Unknow foreign result property [{foreignTableAttr.ResultColumn}] in table [{foreignTableAttr.Table}]");

            foreach (var foreignConditionAttr in foreignConditionAttrs)
            {
                ForeignConditions.Add(new ForeignConditionInfo(foreignConditionAttr, ForeignTableType, tableType));
            }
        }


        public ColumnProperty AttributeProperty { get; set; }

        public TableType ForeignTableType { get; set; }
        public ColumnProperty ResultProperty { get; set; }


        public List<ForeignConditionInfo> ForeignConditions { get; set; } = new List<ForeignConditionInfo>();

    }

    public class ForeignConditionInfo
    {
        public ForeignConditionInfo(ForeignCondition foreignConditionAttr, TableType foreignTableType, TableType tableType)
        {
            Property = foreignTableType.GetProperty(foreignConditionAttr.Column);
            if (Property is null)
                throw new Exception($"Unknow foreign conditon property [{foreignConditionAttr.Column}] in table [{foreignTableType.Name}]");

            Value = foreignConditionAttr.Value;
            IsMultiValue = foreignConditionAttr.IsMultiValue;

            if (!string.IsNullOrEmpty(foreignConditionAttr.ValueColumn))
            {
                ValueProperty = tableType.GetProperty(foreignConditionAttr.ValueColumn);
                if (ValueProperty is null)
                    throw new Exception($"Unknow foreign conditon value property [{foreignConditionAttr.ValueColumn}] in table [{tableType.Name}]");
            }
        }

        /// <summary>
        /// 条件列
        /// </summary>
        public ColumnProperty Property { get; }

        /// <summary>
        /// 条件值
        /// </summary>
        public object Value { get; }

        public bool IsMultiValue { get; }

        /// <summary>
        /// 条件值所在列
        /// </summary>
        public ColumnProperty ValueProperty { get; }

    }
}
