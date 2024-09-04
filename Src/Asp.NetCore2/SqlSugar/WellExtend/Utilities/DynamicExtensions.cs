using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace SqlSugar
{
    public static class DynamicExtensions
    {
        public static bool TryGetDynamicValue<T>(ExpandoObject obj, string propName, out T propValue) 
        {
            propValue = default;
            bool re = false;

            propName = propName.Trim().ToLower();
            foreach (KeyValuePair<string, object> prop in obj)
            {
                if (prop.Key.Trim().ToLower() == propName)
                {
                    if (prop.Value is T)
                    {
                        propValue = (T)prop.Value;
                    }

                    re = true;
                    break;
                }
            }

            return re;
        }

        /// <summary>
        /// 判断并返回属性的指定自定义特性(包含基类中的特性)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="t"></param>
        /// <returns>
        /// true：特性存在
        /// false：特性不存在
        /// </returns>
        public static bool TryGetAtrribute<T>(this Type type, out T t) where T : Attribute
        {
            // 读取自定义特性
            t = type.GetCustomAttribute<T>();
            return t != null;
        }

        /// <summary>
        /// 判断并返回属性的指定自定义特性(不包含基类中的特性)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="t"></param>
        /// <returns>
        /// true：特性存在
        /// false：特性不存在
        /// </returns>
        public static bool TryGetObjectAtrribute<T>(this Type type, out T t) where T : Attribute
        {
            if (type.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(T)) != null)
                // 读取自定义特性
                t = type.GetCustomAttribute<T>();
            else
                t = default;

            return t != null;
        }

        /// <summary>
        /// 判断并返回属性的指定自定义特性
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="t"></param>
        /// <returns>
        /// true：特性存在
        /// false：特性不存在
        /// </returns>
        public static bool TryGetAtrribute<T>(this PropertyInfo pi, out T t) where T : Attribute
        {
            // 读取自定义特性
            t = pi.GetCustomAttribute<T>();
            return t != null;
        }

        /// <summary>
        /// 判断并返回属性的指定自定义特性
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="t"></param>
        /// <returns>
        /// true：特性存在
        /// false：特性不存在
        /// </returns>
        public static bool TryGetAtrributes<T>(this PropertyInfo pi, out List<T> list) where T : Attribute
        {
            // 读取自定义特性
            list = pi.GetCustomAttributes<T>().ToList();
            return list?.Count > 0;
        }

        /// <summary>
        /// 判断并返回属性的指定自定义特性
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="t"></param>
        /// <returns>
        /// true：特性存在
        /// false：特性不存在
        /// </returns>
        public static bool TryGetAtrribute<T>(this FieldInfo fi, out T t) where T : Attribute
        {
            // 读取自定义特性
            t = fi.GetCustomAttribute<T>();
            return t != null;
        }
    }
}