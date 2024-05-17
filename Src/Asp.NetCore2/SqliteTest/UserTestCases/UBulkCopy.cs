﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrmTest
{
    public partial class NewUnitTest
    {
        public static void Bulk()
        {
            if (Db.DbMaintenance.IsAnyTable("UnitIdentity1", false))
            {
                Db.DbMaintenance.DropTable("UnitIdentity1");
            }
            Db.CodeFirst.InitTables<UnitIdentity1>();
            Db.DbMaintenance.TruncateTable<UnitIdentity1>();
            var data = new UnitIdentity1()
            {
                Name = "jack"
            };
            Db.Fastest<UnitIdentity1>().BulkCopy(new List<UnitIdentity1>() {
              data
            });
            var list = Db.Queryable<UnitIdentity1>().ToSugarList();
            if (list.Count != 1 || data.Name != list.First().Name)
            {
                throw new Exception("unit Bulk");
            }
            data.Name = "2";
            Db.Fastest<UnitIdentity1>().BulkCopy(new List<UnitIdentity1>() {
              data,
              data
            });
            list = Db.Queryable<UnitIdentity1>().ToSugarList();
            if (list.Count != 3 || !list.Any(it => it.Name == "2"))
            {
                throw new Exception("unit Bulk");
            }
            Db.Fastest<UnitIdentity1>().BulkUpdate(new List<UnitIdentity1>() {
               new UnitIdentity1(){
                Id=1,
                 Name="222"
               },
                 new UnitIdentity1(){
                Id=2,
                 Name="111"
               }
            });
            list = Db.Queryable<UnitIdentity1>().ToSugarList();
            if (list.First(it => it.Id == 1).Name != "222")
            {
                throw new Exception("unit Bulk");
            }
            if (list.First(it => it.Id == 2).Name != "111")
            {
                throw new Exception("unit Bulk");
            }
            if (list.First(it => it.Id == 3).Name != "2")
            {
                throw new Exception("unit Bulk");
            }
            Db.CodeFirst.InitTables<UnitIdentity111>();
            Db.DbMaintenance.TruncateTable<UnitIdentity111>();
            var count = Db.Fastest<UnitIdentity111111111>().AS("UnitIdentity111").BulkCopy(new List<UnitIdentity111111111> {
              new UnitIdentity111111111(){ Id=1, Name="jack" }
            });
            if (count == 0)
            {
                throw new Exception("unit Bulk");
            }
            count = Db.Fastest<UnitIdentity111111111>().AS("UnitIdentity111").BulkUpdate(new List<UnitIdentity111111111> {
              new UnitIdentity111111111(){ Id=1, Name="jack" }
            });
            if (count == 0)
            {
                throw new Exception("unit Bulk");
            }
            Db.CodeFirst.InitTables<UnitTable001>();
            Db.Fastest<UnitTable001>().BulkUpdate(new List<UnitTable001> {
              new UnitTable001(){   Id=1, table="a" }
            });

            Db.CodeFirst.InitTables<UnitBulk23131>();
            Db.DbMaintenance.TruncateTable<UnitBulk23131>();
            Db.Fastest<UnitBulk23131>().BulkCopy(new List<UnitBulk23131> {
            new UnitBulk23131()
            {
                Id = 1,
                table = false
            }
            });
            var list1 = Db.Queryable<UnitBulk23131>().ToSugarList();
            SqlSugar.Check.Exception(list1.First().table == true, "unit error");
            Db.Fastest<UnitBulk23131>().BulkUpdate(new List<UnitBulk23131> {
            new UnitBulk23131()
            {
                Id = 1,
                table = true
            }
            });
            var list2 = Db.Queryable<UnitBulk23131>().ToSugarList();
            SqlSugar.Check.Exception(list2.First().table == false, "unit error");

            Db.DbMaintenance.TruncateTable<UnitBulk23131>();
            Db.Fastest<UnitBulk23131>().BulkCopy(new List<UnitBulk23131> {
            new UnitBulk23131()
            {
                Id = 1,
                table = true
            }
            });
            var list3 = Db.Queryable<UnitBulk23131>().ToSugarList();
            SqlSugar.Check.Exception(list3.First().table == false, "unit error");
            Db.Fastest<UnitBulk23131>().BulkUpdate(new List<UnitBulk23131> {
            new UnitBulk23131()
            {
                Id = 1,
                table = false
            }
            });
            list3 = Db.Queryable<UnitBulk23131>().ToSugarList();
            SqlSugar.Check.Exception(list3.First().table == true, "unit error");

            Db.DbMaintenance.TruncateTable<UnitBulk23131>();
            Db.Fastest<UnitBulk23131>().BulkCopy(new List<UnitBulk23131> {
            new UnitBulk23131()
            {
                Id = 1,
                table = null
            }
            });
            var list4 = Db.Queryable<UnitBulk23131>().ToSugarList();
            SqlSugar.Check.Exception(list4.First().table == true, "unit error");
            var db = Db;
            db.CodeFirst.InitTables<UnitTestoffset11>();
            db.Fastest<UnitTestoffset11>().BulkCopy(new List<UnitTestoffset11>() {
            new  UnitTestoffset11 { },
             new  UnitTestoffset11 {  DateTimeOffset= DateTimeOffset.Now}
            });
            var list5 = db.Queryable<UnitTestoffset11>().ToSugarList();

            Db.CodeFirst.InitTables<UnitIdentity111111111>();
            Db.DbMaintenance.TruncateTable<UnitIdentity111111111>();
            Db.Fastest<UnitIdentity111111111>().BulkCopy(new List<UnitIdentity111111111> {
              new UnitIdentity111111111(){ Id=1, Name="True" }
            });
            var list6 = db.Queryable<UnitIdentity111111111>().ToSugarList();
            if (list6.First().Name != "True")
            {
                throw new Exception("unit error");
            }
            db.CodeFirst.InitTables<UnitBool01>();
            db.DbMaintenance.TruncateTable<UnitBool01>();
            db.Insertable(new UnitBool01() { Bool = true }).ExecuteCommand();
            db.Insertable(new UnitBool01() { Bool = false }).ExecuteCommand();
            db.Fastest<UnitBool01>().BulkCopy(new List<UnitBool01>() { new UnitBool01() { Bool = true } });
            db.Fastest<UnitBool01>().BulkCopy(new List<UnitBool01>() { new UnitBool01() { Bool = false } });
            var list7 = db.Queryable<UnitBool01>().ToSugarList();
            var list8 = db.Queryable<UnitBool01>().ToDataTable();
            var json = db.Utilities.SerializeObject(db.Utilities.DataTableToDictionaryList(list8));
            if (json != "[{\"Bool\":1},{\"Bool\":0},{\"Bool\":1},{\"Bool\":0}]")
            {
                throw new Exception("unit error");
            }
            Console.WriteLine("用例跑完");
            Db.CodeFirst.InitTables<UnitDateOffsetTimex>();
            Db.DbMaintenance.TruncateTable<UnitDateOffsetTimex>();
            Db.Insertable(new UnitDateOffsetTimex() { offsetTime = DateTimeOffset.Now }).ExecuteCommand();
            Db.Insertable(new List<UnitDateOffsetTimex>() {
                new UnitDateOffsetTimex() { offsetTime = DateTimeOffset.Now },
                new UnitDateOffsetTimex() { offsetTime = DateTimeOffset.Now }}).ExecuteCommand();
            var dt = Db.Ado.GetDataTable("select * from UnitDateOffsetTimex");
            db.CodeFirst.InitTables<Unitadfasyyafda>();
            db.DbMaintenance.TruncateTable<Unitadfasyyafda>();
            db.Insertable(new Unitadfasyyafda() { Id = 1, Name = "a" }).ExecuteCommand();
            db.Insertable(new Unitadfasyyafda() { Id = 2, Name = "a2" }).ExecuteCommand();
            var list10 = db.Queryable<Unitadfasyyafda>().ToDataTable();
            db.DbMaintenance.TruncateTable<Unitadfasyyafda>();
            db.Fastest<System.Data.DataTable>().AS("Unitadfasyyafda").BulkCopy(list10);
            var list11 = db.Queryable<Unitadfasyyafda>().ToList();
            if (list11.First().Id != 1 || list11.Last().Name != "a2")
            {
                throw new Exception("unit error");
            }
            if (list11.First().Name != "a" || list11.Last().Id != 2)
            {
                throw new Exception("unit error");
            }
            TestIdentity();
        }

        private static void TestIdentity()
        {
            var db = NewUnitTest.Db;
            db.CodeFirst.InitTables<UnitadfasyyafdaIdentity>();
            db.DbMaintenance.TruncateTable<UnitadfasyyafdaIdentity>();
            db.Insertable(new UnitadfasyyafdaIdentity() { Id = 1, Name = "a", Name2 = "a11" }).ExecuteCommand();
            db.Insertable(new UnitadfasyyafdaIdentity() { Id = 2, Name = "a2", Name2 = "a22" }).ExecuteCommand();
            var list10 = db.Queryable<UnitadfasyyafdaIdentity>().ToDataTable();
            db.DbMaintenance.TruncateTable<UnitadfasyyafdaIdentity>();
            db.Fastest<System.Data.DataTable>().AS("UnitadfasyyafdaIdentity").BulkCopy(list10);
            var list11 = db.Queryable<UnitadfasyyafdaIdentity>().ToList();
            if (list11.First().Id != 1 || list11.Last().Name != "a2")
            {
                throw new Exception("unit error");
            }
            if (list11.First().Name != "a" || list11.Last().Id != 2)
            {
                throw new Exception("unit error");
            }
        }
    }
        public class UnitadfasyyafdaIdentity
        {
            [SqlSugar.SugarColumn(IsIdentity =true,IsPrimaryKey =true)]
            public int Id { get; set; }
            public string Name { get; set; }
            public string Name2 { get; set; }
        }
        public class Unitadfasyyafda
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class UnitDateOffsetTimex
    {
        public DateTimeOffset offsetTime { get; set; }
    }

    public class UnitBool01
    {
        public bool Bool { get; set; }
    }

    public class UnitTestoffset11
    {
        [SqlSugar.SugarColumn(IsNullable = true)]
        public DateTimeOffset? DateTimeOffset { get; set; }
    }
    public class UnitBulk23131
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        [SqlSugar.SugarColumn(ColumnDataType = "tinyint", Length = 1, IsNullable = true)]
        public bool? table { get; set; }
    }
    public class UnitTable001
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        public string table { get; set; }
    }

    public class UnitIdentity111
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class UnitIdentity111111111
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class UnitIdentity1
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
