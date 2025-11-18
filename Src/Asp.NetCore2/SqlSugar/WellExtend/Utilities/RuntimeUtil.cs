using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlSugar
{
    public static class RuntimeUtil
    {
        /// <summary>
        /// 从所有程序集中收集SugarTable特性标注的类
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetSugarTables()
        {
            return GetTypes(u => !u.IsInterface && u is { IsAbstract: false, IsClass: true } && u.IsDefined(typeof(SugarTable), false));
        }

        /// <summary>
        /// 判断类型是否为SugarTable特性标注的类
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public static bool IsSugarTable(this Type tableType)
        {
            return !tableType.IsInterface && tableType is { IsAbstract: false, IsClass: true } && tableType.IsDefined(typeof(SugarTable), false);
        }

        /// <summary>
        /// 从所有程序集中收集类型（返回所有项）
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Type> GetTypes()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> list = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                if (types != null && types.Length > 0)
                {
                    list.AddRange(types);
                }
            }

            return list;
        }

        /// <summary>
        /// 从所有程序集中搜索类型（返回所有匹配项）
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<Type> GetTypes(Func<Type, bool> predicate)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var list = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                var types = assembly.GetTypes();
                if (types != null && types.Length > 0)
                {
                    var re = types.Where(predicate);
                    if (re.Any())
                        list.AddRange(re);
                }
            }

            return list;
        }
        
        /// <summary>
        /// 从所有程序集中搜索类型（返回所有匹配项）
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Type GetType(Func<Type, bool> predicate)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var list = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                var types = assembly.GetTypes();
                if (types != null && types.Length > 0)
                {
                    var re = types.Where(predicate);
                    if (re.Any())
                    {
                        return re.First();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 从所有程序集中搜索类型（返回第一个匹配项）
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetType(this string typeName)
        {
            Type type = null;
            if (string.IsNullOrEmpty(typeName))
            {
                return type;
            }

            typeName = typeName.Trim();
            type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in assemblies)
            {
                type = ass.GetType(typeName);
                if (type != null)
                {
                    return type;
                }

                Type[] types = ass.GetTypes();
                foreach (Type st in types)
                {
                    if (st.Name.Equals(typeName))
                    {
                        type = st;
                        return type;
                    }
                }
            }

            return type;
        }

        /// <summary>
        /// 从所有程序集中搜索类型（返回第一个匹配项）
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool TryGetType([NotNullWhen(true)] this string typeName, out Type type)
        {
            type = GetType(typeName);

            return type != null;
        }

        /// <summary>
        /// 从后缀为".DbModels"的程序集中搜索类型（返回第一个匹配项）
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetDbType(this string typeName)
        {
            Type type = null;

            if (string.IsNullOrEmpty(typeName))
            {
                return type;
            }

            typeName = typeName.Trim();
            type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(p => (p.GetName().Name ?? "").EndsWith(".DbModels"));
            foreach (Assembly ass in assemblies)
            {
                type = ass.GetType(typeName);
                if (type != null)
                {
                    return type;
                }

                Type[] types = ass.GetTypes();
                foreach (Type st in types)
                {
                    if (st.Name.Equals(typeName) || typeName.Equals(st.FullName))
                    {
                        type = st;
                        return type;
                    }
                }
            }

            return type;
        }

        /// <summary>
        /// 从后缀为".DbModels"的程序集中搜索类型（返回第一个匹配项）
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool TryGetDbType([NotNullWhen(true)] this string typeName, out Type type)
        {
            type = GetDbType(typeName);

            return type != null;
        }

    }
}
