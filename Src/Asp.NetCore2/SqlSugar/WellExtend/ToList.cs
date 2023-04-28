using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar
{
    public partial class QueryableProvider<T> : QueryableAccessory, ISugarQueryable<T>
    {
        public List<T> ToList(long factoryId, bool isDeleted = false)
        {
            WellFilter(factoryId, isDeleted);

            //TODO 处理自定义特性

            return this.ToSugarList();
        }

        public Task<List<T>> ToListAsync(long factoryId, bool isDeleted = false)
        {
            WellFilter(factoryId, isDeleted);

            //TODO 处理自定义特性

            return this.ToSugarListAsync();
        }
    }
}
