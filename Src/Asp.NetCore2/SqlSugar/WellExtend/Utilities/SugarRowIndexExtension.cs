using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar
{
    public static class SugarRowIndexExtension
    {
        static Dictionary<int, string> _rowIndexNames = new Dictionary<int, string>();
        static object _rowIndexNamesLock = new object();
        public static string GetRowIndexName(this int i)
        {
            if (!_rowIndexNames.ContainsKey(i))
                lock (_rowIndexNamesLock)
                {
                    if (!_rowIndexNames.ContainsKey(i))
                    {
                        var uid = "RI" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                        _rowIndexNames.Add(i, uid);
                    }
                }

            return _rowIndexNames[i];
        }
    }
}
