﻿using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmTest
{
    internal class UnitTreaaafasa
    {
        public static void Init() 
        {
            var db = NewUnitTest.Db;
            var pageIndex = 1;
            var pageSize = 10;
            RefAsync<int> total = 0;
            db.CodeFirst.InitTables<TableA, TableB, TableC, TableD>();
            var list =   db.Queryable<TableA>()
                .Where(t => t.Id == 1)
                .Select(t => new  
                {
                    Id = t.Id,
                    Name = t.AName,
                    Amount = SqlFunc.IsNull(SqlFunc.Subqueryable<TableB>()
                        .LeftJoin<TableC>((tableB, tableC) => tableB.TableCId == tableC.Id)
                        .LeftJoin<TableD>((tableB, tableC, tableD) => tableB.TableDId == tableD.Id)
                        .Where((tableB, tableC, tableD) => tableD.TableAId == t.Id)
                        .Sum((tableB, tableC, tableD) =>tableB.Quantity * tableB.Price), 0)
                }).ToPageListAsync(pageIndex, pageSize, total).GetAwaiter().GetResult();

        }
        public class BaseEntity
        {
            [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
            public int Id { get; set; }
        }
        [SugarTable("unitTableAdfa")]
        public class TableA : BaseEntity
        {
            public string AName { get; set; }
        }
        [SugarTable("unitTableAasdfafadfa")]
        public class TableC : BaseEntity
        {
            public string CName { get; set; }
        }
        [SugarTable("unitTableAadfaasdfafadfa")]
        public class TableB : BaseEntity
        {
            public string BName { get; set; }
            public int TableCId { get; set; }
            public int TableDId { get; set; }
            public decimal Quantity { get; set; }
            public decimal Price { get; set; }
        }
        [SugarTable("unitTableAdddfadfa")]
        public class TableD : BaseEntity
        {
            public string DName { get; set; }
            public int TableAId { get; set; }
        }
    }


}
