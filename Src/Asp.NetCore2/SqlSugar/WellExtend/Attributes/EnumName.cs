using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记枚举的名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class EnumName : Attribute
    {
        public EnumName(string property)
        {
            Property = property;
        }

        public string Property { get; set; }
    }
}