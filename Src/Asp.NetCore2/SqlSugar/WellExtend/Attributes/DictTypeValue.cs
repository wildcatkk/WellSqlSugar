using System;

namespace SqlSugar
{
    /// <summary>
    /// 自定义特性，用于标记字典类型的值
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DictTypeValue : Attribute
    {
        public readonly string CodeProperty;
        public readonly string TargetColumn;

        /// <summary>
        /// SysDictType.Code => SysDictType.Name
        /// </summary>
        /// <param name="codeProperty">当前表存储Code的字段</param>
        public DictTypeValue(string codeProperty)
        {
            CodeProperty = codeProperty;
            TargetColumn = "Name";
        }

        /// <summary>
        /// SysDictType.Code => SysDictType.targetColumn
        /// </summary>
        /// <param name="codeProperty">当前表存储Code的字段</param>
        /// <param name="targetColumn">字典类型表目标字段</param>
        public DictTypeValue(string codeProperty, string targetColumn)
        {
            CodeProperty = codeProperty;
            TargetColumn = targetColumn;
        }
    }
}