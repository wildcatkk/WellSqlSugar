using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace SqlSugar
{
    public static class DynamicExtensions
    {
        public static bool TryGetType(this string typeName, out Type? type)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new Exception("请提供有效的类型名");
            }

            typeName = typeName.Trim();
            type = Type.GetType(typeName);
            if (type != null)
            {
                return true;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in assemblies)
            {
                type = ass.GetType(typeName);
                if (type != null)
                {
                    return true;
                }

                Type[] types = ass.GetTypes();
                foreach (Type st in types)
                {
                    if (st.Name.Equals(typeName))
                    {
                        type = st;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryGetDbType(this string typeName, out Type? type)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new Exception("请提供有效的类型名");
            }

            typeName = typeName.Trim();
            type = Type.GetType(typeName);
            if (type != null)
            {
                return true;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(p => (p.GetName().Name ?? "").EndsWith(".DbModels"));
            foreach (Assembly ass in assemblies)
            {
                type = ass.GetType(typeName);
                if (type != null)
                {
                    return true;
                }

                Type[] types = ass.GetTypes();
                foreach (Type st in types)
                {
                    if (st.Name.Equals(typeName))
                    {
                        type = st;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryGetDynamicValue<T>(ExpandoObject obj, string propName, out T propValue) 
        {
            propValue = default;
            bool re = false;

            foreach (KeyValuePair<string, object> prop in obj)
            {
                if (prop.Key == propName)
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
        /// 判断并返回属性的指定自定义特性
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