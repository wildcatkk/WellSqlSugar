using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlSugar
{
    public static class RuntimeUtil
    {
        public static List<Type> GetSugarTables()
        {
            return GetTypes(u => !u.IsInterface && u is { IsAbstract: false, IsClass: true } && u.IsDefined(typeof(SugarTable), false));
        }

        public static bool IsSugarTable(this Type tableType)
        {
            return !tableType.IsInterface && tableType is { IsAbstract: false, IsClass: true } && tableType.IsDefined(typeof(SugarTable), false);
        }

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

    }
}
