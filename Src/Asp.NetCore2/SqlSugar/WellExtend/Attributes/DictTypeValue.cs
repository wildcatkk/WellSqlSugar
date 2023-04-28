using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记字典类型的值
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DictTypeValue : Attribute
    {
        public readonly string Code;
        public readonly string ValueProp;

        public DictTypeValue(string code, string valueProp)
        {
            Code = code;
            ValueProp = valueProp;
        }
    }
}