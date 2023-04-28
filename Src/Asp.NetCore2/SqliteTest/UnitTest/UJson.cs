﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmTest
{
    public partial class NewUnitTest
    {

        public static void Json()
        {
            Db.CodeFirst.InitTables<UnitJsonTest>();
            Db.DbMaintenance.TruncateTable<UnitJsonTest>();
            var id= Db.Insertable(new UnitJsonTest() { Order = new Order { Id = 1, Name = "order1" } }).ExecuteReturnIdentity();
            var list = Db.Queryable<UnitJsonTest>().ToSugarList();
            UValidate.Check("order1", list.First().Order.Name, "Json");
            Db.Updateable(new UnitJsonTest() { Id = id, Order = new Order { Id = 2, Name = "order2" } }).ExecuteCommand();
            list= Db.Queryable<UnitJsonTest>().ToSugarList();
            UValidate.Check("order2", list.First().Order.Name, "Json");
            var list2 = Db.Queryable<UnitJsonTest>().ToSugarList();
        }
    }


    public class UnitJsonTest
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        [SqlSugar.SugarColumn(ColumnDataType = "varchar(4000)", IsJson = true)]
        public Order Order { get; set; }
    }
}
