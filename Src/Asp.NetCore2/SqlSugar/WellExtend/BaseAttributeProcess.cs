using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlSugar
{
    public partial class QueryableProvider<T>
    {
        /// <summary>
        /// 获取枚举、字典、外键描述
        /// </summary>
        private List<T> ToWellList<T>(List<T> list) => list == null ? null : BaseAttributeProcess(list);

        private async Task<List<T>> ToWellListAsync<T>(Task<List<T>> list) => BaseAttributeProcess(await list);
    }
}