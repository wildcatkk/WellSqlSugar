using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记字典项的值
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DictItemValue : Attribute
    {
        public readonly string Code;
        public readonly string ValueProp;

        public DictItemValue(string code, string valueProp)
        {
            Code = code;
            ValueProp = valueProp;
        }
    }
}