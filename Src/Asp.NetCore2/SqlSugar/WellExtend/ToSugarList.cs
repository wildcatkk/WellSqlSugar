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
            var list = await _ToListAsync<T>();

            return AttributeProvider.Process(Context, list);
        }

    }

    public partial interface ISugarQueryable<T>
    {
        List<T> ToSugarList();

        Task<List<T>> ToSugarListAsync();
    }
}
