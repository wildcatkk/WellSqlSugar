using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SqlSugar
{
    public static class WellFilterExtend
    {
        public static ISugarQueryable<T> WellFilter<T>(this ISugarQueryable<T> queryable, long factoryId, bool isDeleted = false)
        {
            List<IConditionalModel> conditions = new List<IConditionalModel>();

            Type type = typeof(T);
            if (type.GetInterface(nameof(ILogicalDelete)) != null && !isDeleted)
            {
                conditions.Add(nameof(ILogicalDelete.IsDeleted), isDeleted);
            }

            if (typeof(T).GetInterface(nameof(IGroupCo)) is null && factoryId > 0 && type.GetInterface(nameof(IFactory)) != null)
            {
                conditions.Add(nameof(IFactory.FactoryId), factoryId);
            }

            if (conditions.Count > 0)
            {
                return queryable.Where(conditions);
            }
            else
            {
                return queryable;
            }
        }

        public static ISugarQueryable<T> WellFilter<T>(this ISugarQueryable<T> queryable, bool isDeleted = false)
        {
            List<IConditionalModel> conditions = new List<IConditionalModel>();

            Type type = typeof(T);
            if (type.GetInterface(nameof(ILogicalDelete)) != null && !isDeleted)
            {
                conditions.Add(nameof(ILogicalDelete.IsDeleted), isDeleted);
            
                return queryable.Where(conditions);
            }
            else
            {
                return queryable;
            }
        }


        public static IUpdateable<T> WellFilter<T>(this IUpdateable<T> updateable, long factoryId, bool isDeleted = false) where T: class, new()
        {
            List<IConditionalModel> conditions = new List<IConditionalModel>();

            Type type = typeof(T);
            if (type.GetInterface(nameof(ILogicalDelete)) != null && !isDeleted)
            {
                conditions.Add(nameof(ILogicalDelete.IsDeleted), isDeleted);
            }

            if (typeof(T).GetInterface(nameof(IGroupCo)) is null && factoryId > 0 && type.GetInterface(nameof(IFactory)) != null)
            {
                conditions.Add(nameof(IFactory.FactoryId), factoryId);
            }

            if (conditions.Count > 0)
            {
                return updateable.Where(conditions);
            }
            else
            {
                return updateable;
            }
        }

        public static IUpdateable<T> WellFilter<T>(this IUpdateable<T> updateable, bool isDeleted = false) where T : class, new()
        {
            List<IConditionalModel> conditions = new List<IConditionalModel>();

            Type type = typeof(T);
            if (type.GetInterface(nameof(ILogicalDelete)) != null && !isDeleted)
            {
                conditions.Add(nameof(ILogicalDelete.IsDeleted), isDeleted);

                return updateable.Where(conditions);
            }
            else
            {
                return updateable;
            }
        }

    }

}
