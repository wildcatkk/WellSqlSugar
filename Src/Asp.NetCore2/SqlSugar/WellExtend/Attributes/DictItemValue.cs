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
        public readonly string CodeColumn;
        public readonly string ResultColumn;

        /// <summary>
        /// parentCode + SysDictItem.Code => "Name"
        /// </summary>
        /// <param name="parentCode">SysDictType的Code值</param>
        /// <param name="codeColumn">当前表存储Code的列</param>
        public DictItemValue(string parentCode, string codeColumn)
        {
            ParentCode = parentCode;
            CodeColumn = codeColumn;
            ResultColumn = "Name";
        }

        /// <summary>
        /// parentCode + SysDictItem.Code => SysDictType.[ResultColumn]
        /// </summary>
        /// <param name="parentCode">SysDictType的Code值</param>
        /// <param name="codeColumn">当前表存储Code的列</param>
        /// <param name="resultColumn">字典类型表结果列</param>
        public DictItemValue(string parentCode, string codeColumn, string resultColumn)
        {
            ParentCode = parentCode;
            CodeColumn = codeColumn;
            ResultColumn = resultColumn;
        }
    }
}