﻿using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrmTest
{
    public class DemoE_CodeFirst
    {
        public static void Init()
        {
            Console.WriteLine("");
            Console.WriteLine("#### CodeFirst Start ####");
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                DbType = DbType.MySql,
                ConnectionString = Config.ConnectionString3,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true
            });
            db.Aop.OnLogExecuting=(s,p)=>Console.WriteLine(s);
            db.DbMaintenance.CreateDatabase();
            db.CodeFirst.InitTables(typeof(CodeFirstTable1));//Create CodeFirstTable1 
            db.Insertable(new CodeFirstTable1() { Name = "a", Text = "a" }).ExecuteCommand();
            var list = db.Queryable<CodeFirstTable1>().ToList();
            Console.WriteLine("#### CodeFirst end ####");
            db.CodeFirst.InitTables<UnitByte1>();
            db.CodeFirst.InitTables<CodeFirstGuid>();
            db.Insertable(new CodeFirstGuid() { Id = Guid.NewGuid() }).ExecuteCommand();
            db.Insertable(new CodeFirstGuid() { Id = null }).ExecuteCommand();
            var ids=db.Queryable<CodeFirstGuid>().Select(x => x.Id).ToList();
            var ids2 = db.Queryable<CodeFirstGuid>()
                .Select(x =>new
                {
                   ids= SqlFunc.Subqueryable<CodeFirstGuid>().ToList(x1 => x1.Id)
                }).ToList();
            Console.WriteLine("#### CodeFirst end ####");
        }
    }

    public class CodeFirstGuid 
    {
        [SugarColumn(IsNullable =true)]
        public Guid? Id { get; set; }
    }

    public class UnitByte1
    {
        public byte[] bytes{ get; set; }
 
    }


    public class CodeFirstTable1
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Name { get; set; }
        [SugarColumn(ColumnDataType = "Nvarchar(255)")]//custom
        public string Text { get; set; }
        [SugarColumn(IsNullable = true)]
        public DateTime CreateTime { get; set; }
    }
}
