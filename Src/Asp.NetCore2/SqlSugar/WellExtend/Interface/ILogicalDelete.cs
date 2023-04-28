using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar
{
    public interface ILogicalDelete
    {
        bool IsDeleted { get; set; }
    }
}
