using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlSugar
{
    public partial class QueryableProvider<T> : QueryableAccessory, ISugarQueryable<T>
    {
        public List<T> ToList(long factoryId, bool isDeleted = false)
        {
            // WellFilter(factoryId, isDeleted);
            return ToWellList(ToSugarList());
        }

        public Task<List<T>> ToListAsync(long factoryId, bool isDeleted = false)
        {
            //WellFilter(factoryId, isDeleted);
            return ToWellListAsync(ToSugarListAsync());
        }
    }

    public partial interface ISugarQueryable<T>
    {
        List<T> ToList(long factoryId, bool isDeleted = false);

        Task<List<T>> ToListAsync(long factoryId, bool isDeleted = false);
    }
}