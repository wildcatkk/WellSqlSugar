using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SqlSugar
{
    public partial class QueryableProvider<T> : QueryableAccessory, ISugarQueryable<T>
    {
        public ISugarQueryable<T> WellFilter(long factoryId, bool isDeleted = false)
        {
            List<IConditionalModel> conditions = new List<IConditionalModel>();

            Type type = typeof(T);
            if (type.GetInterface(nameof(ILogicalDelete)) != null)
            {
                conditions.Add(nameof(ILogicalDelete.IsDeleted), isDeleted);
            }

            if (typeof(T).GetInterface(nameof(IGroupCo)) is null && factoryId > 0 && type.GetInterface(nameof(IFactory)) != null)
            {
                conditions.Add(nameof(IFactory.FactoryId), factoryId);
            }

            if (conditions.Count > 0)
            {
                return this.Where(conditions);
            }
            else
            {
                return this;
            }
        }

        public ISugarQueryable<T> WellFilter(bool isDeleted = false)
        {
            List<IConditionalModel> conditions = new List<IConditionalModel>();

            Type type = typeof(T);
            if (type.GetInterface(nameof(ILogicalDelete)) != null)
            {
                conditions.Add(nameof(ILogicalDelete.IsDeleted), isDeleted);
            
                return this.Where(conditions);
            }
            else
            {
                return this;
            }
        }

    }


    public partial interface ISugarQueryable<T>
    {
        ISugarQueryable<T> WellFilter(long factoryId, bool isDeleted = false);

        ISugarQueryable<T> WellFilter(bool isDeleted = false);
    }
}
