using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记字典类型的值
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DictTypeValue : Attribute
    {
        public readonly string CodeColumn;
        public readonly string ResultColumn;

        /// <summary>
        /// SysDictType.Code => SysDictType.Name
        /// </summary>
        /// <param name="codeColumn">当前表存储Code的列</param>
        public DictTypeValue(string codeColumn)
        {
            CodeColumn = codeColumn;
            ResultColumn = "Name";
        }

        /// <summary>
        /// SysDictType.Code => SysDictType.[ResultColumn]
        /// </summary>
        /// <param name="codeColumn">当前表存储Code的列</param>
        /// <param name="resultColumn">字典类型表结果列</param>
        public DictTypeValue(string codeColumn, string resultColumn)
        {
            CodeColumn = codeColumn;
            ResultColumn = resultColumn;
        }
    }
}