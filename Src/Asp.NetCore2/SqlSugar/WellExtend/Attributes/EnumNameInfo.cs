using System;

namespace SqlSugar
{
    public class EnumNameInfo
    {
        public EnumNameInfo(ColumnProperty attributeProperty, EnumName enumNameAttr, TableType tableType)
        {
            AttributeProperty = attributeProperty;
            ValueProperty = tableType.GetProperty(enumNameAttr.ValueColumn);
            if (ValueProperty is null)
                throw new Exception($"Unknow value property [{enumNameAttr.ValueColumn}] in table [{tableType.Name}]");
        }

        public ColumnProperty AttributeProperty { get; set; }

        public ColumnProperty ValueProperty { get; set; }
    }

}
