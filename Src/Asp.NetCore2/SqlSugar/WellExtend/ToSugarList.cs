using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar
{
    public partial class QueryableProvider<T> : QueryableAccessory, ISugarQueryable<T>
    {

        public List<T> ToSugarList()
        {
            InitMapping();
            return _ToList<T>();
        }

        public async Task<List<T>> ToSugarListAsync()
        {
            InitMapping();
            return await _ToListAsync<T>();
        }

    }

    public partial interface ISugarQueryable<T>
    {
        List<T> ToSugarList();

        Task<List<T>> ToSugarListAsync();
    }
}
