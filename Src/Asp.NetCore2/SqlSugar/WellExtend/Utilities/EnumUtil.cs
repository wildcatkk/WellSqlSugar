using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlSugar
{
    public static class EnumUtil
    {
        private static ConcurrentDictionary<Type, List<EnumInfo>> EnumCache { get; set; } = new ConcurrentDictionary<Type, List<EnumInfo>>();
        private static object EnumLock = new object();

        /// <summary>
        /// 获取枚举信息（缓存）
        /// </summary>
        public static List<EnumInfo> GetEnumInfoes<T>() where T : Enum
            => GetEnumInfoes(typeof(T));

        /// <summary>
        /// 获取枚举信息（缓存）
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static List<EnumInfo> GetEnumInfoes(this Type enumType)
        {
            if (!enumType.IsEnum)
                throw new Exception($"类型({enumType.FullName ?? enumType.Name})不是枚举类型");

            // 查询缓存
            var infoes = EnumCache.GetOrAdd(enumType, new List<EnumInfo>());
            if (infoes.Count == 0)
            {
                lock (EnumLock)
                {
                    infoes = EnumCache.GetOrAdd(enumType, new List<EnumInfo>());
                    if (infoes.Count == 0)
                    {
                        infoes = InnerGetEnumInfoes(enumType);
                        // 缓存
                        EnumCache[enumType] = infoes;
                    }
                }
            }

            return infoes;
        }

        /// <summary>
        /// 获取枚举信息
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        private static List<EnumInfo> InnerGetEnumInfoes(Type enumType)
        {
            var infoes = new List<EnumInfo>();

            var values = Enum.GetValues(enumType);

            foreach (var value in values)
            {
                if (value is null) continue;
                var name = value.ToString();
                if (string.IsNullOrEmpty(name)) continue;

                string descrip;
                if (value is Enum)
                {
                    var fi = enumType.GetField(name);
                    if (fi is null)
                    {
                        descrip = name;
                    }
                    else
                    {
                        var attr = fi.GetCustomAttribute<DescriptionAttribute>();
                        if (string.IsNullOrWhiteSpace(attr?.Description))
                        {
                            descrip = name;
                        }
                        else
                            descrip = attr.Description;
                    }
                }
                else
                    descrip = name;

                infoes.Add(new EnumInfo(name, value, descrip));
            }

            return infoes;
        }

        /// <summary>
        /// 获取枚举项描述，不存在则取name（缓存）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            var enumType = value.GetType();
            
            return GetDescription(value, enumType);
        }

        /// <summary>
        /// 获取枚举项描述，不存在则取name（缓存）
        /// </summary>
        /// <param name="value"></param>
        public static string GetDescription(this Enum value, Type enumType)
        {
            var infoes = GetEnumInfoes(enumType);

            var name = value.ToString();
            var info = infoes.FirstOrDefault(p => p.Name == name);
            if (info is null)
            {
                var attr = enumType.GetField(name)?.GetCustomAttribute<DescriptionAttribute>();
                if (!string.IsNullOrWhiteSpace(attr?.Description))
                {
                    return attr.Description;
                }
                else
                {
                    return name;
                }
            }
            else
                return info.Description;
        }
    }

    public class EnumInfo
    {
        public EnumInfo(string name, object value, string description)
        {
            Name = name;
            Value = value;
            Description = description;
        }

        public string Name { get; set; }

        public virtual object Value { get; set; }

        public string Description { get; set; }
    }

}
