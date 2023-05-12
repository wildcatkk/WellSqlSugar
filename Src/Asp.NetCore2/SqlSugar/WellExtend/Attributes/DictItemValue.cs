using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记字典项的值
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DictItemValue : Attribute
    {
        public readonly string ParentCode;
        public readonly string CodeProperty;
        public readonly string TargetColumn;

        /// <summary>
        /// parentCode + SysDictItem.Code => "Name"
        /// </summary>
        /// <param name="parentCode">SysDictType的Code值</param>
        /// <param name="codeProperty">当前表存储Code的字段</param>
        public DictItemValue(string parentCode, string codeProperty)
        {
            ParentCode = parentCode;
            CodeProperty = codeProperty;
            TargetColumn = "Name";
        }

        /// <summary>
        /// parentCode + SysDictItem.Code => SysDictType.targetColumn
        /// </summary>
        /// <param name="parentCode">SysDictType的Code值</param>
        /// <param name="codeProperty">当前表存储Code的字段</param>
        /// <param name="targetColumn">字典类型表目标字段</param>
        public DictItemValue(string parentCode, string codeProperty, string targetColumn)
        {
            ParentCode = parentCode;
            CodeProperty = codeProperty;
            TargetColumn = targetColumn;
        }
    }
}