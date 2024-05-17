﻿using SqlSugar.DbConvert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tech.fuhong.mes.basis.entity;
using static OrmTest.UJsonsdafa;

namespace OrmTest
{
    internal class UnitSelectNASFDADSFA
    {
        public static void Init() 
        {
            var db = NewUnitTest.Db;
            db.CodeFirst.InitTables<Unitadadfafa>();
            db.Updateable(new Unitadadfafa() {Id=11, Name = "a" }).ExecuteCommand();
            var sqlobj=db.Updateable(new Unitadadfafa() { Id = 211, Name = "a" }).ToSqlString();
            if (sqlobj != @"UPDATE [Unitadadfafa]  SET
           [Name]=N'a'  WHERE [Id]=211") 
            {
                throw new Exception("UnitSelectNASFDADSFA error");
            }
            db.CodeFirst.InitTables<ProductProcessUnit, ProcessUnit>();
            db.DbMaintenance.TruncateTable<ProductProcessUnit, ProcessUnit>();
            db.Insertable(
                new ProcessUnit()
                {
                    ProcessUnitCode = "a",
                    //ProcessUnitName = "c",
                    //SyncRecordId = "a",
                    //ActiveFlag = true,
                    //SyncTime = DateTime.Now,
                    //SyncTimeBottom = DateTime.Now,
                    //SyncTimeTop = DateTime.Now
                })
                .ExecuteCommand();
            db.Insertable(
            new ProcessUnit()
            {
                ProcessUnitCode = "c",
                //ProcessUnitName = "c",
                //SyncRecordId = "a",
                //ActiveFlag = true,
                //SyncTime = DateTime.Now,
                //SyncTimeBottom = DateTime.Now,
                //SyncTimeTop = DateTime.Now
            })
            .ExecuteCommand();
            db.Insertable(
            new ProductProcessUnit()
            {
                 Id=1,
                  //InspectionStandardUrl="",
                  //  NextProcessUnitId=1,
                  //   ProcessId=1,
                  //    ProcessUnitId=1,
                  //     ProductId=1,
                  //      SortNumber=1,
                  //       SyncRecordId="",
                  //        WorkingInstructionUrl="",
                  //         SyncTime=DateTime.Now,
                  //          SyncTimeBottom=DateTime.Now,
                  //           SyncTimeTop=DateTime.Now
            })
            .ExecuteCommand();
            var list=db.Queryable<ProductProcessUnit>()
                .LeftJoin<ProcessUnit>((t1, t2) => t1.Id == t2.Id)
                .LeftJoin<ProcessUnit>((t1, t2, t3) => t2.Id+1 == t3.Id)
                .Select((t1,t2,t3) => new ProductProcessUnit()
                { 
                    InspectionStandardUrl =t3.ProcessUnitCode + t2.ProcessUnitCode,
                      ProcessUnit= new ProcessUnit() {  ProcessUnitCode=t2.ProcessUnitCode},
                      NextProcessUnit= new ProcessUnit() { ProcessUnitCode = t3.ProcessUnitCode },
                }).ToList();
            if (list.First().InspectionStandardUrl != "ca" && list.First().ProcessUnit.ProcessUnitCode != "a"
                && list.First().ProcessUnit.ProcessUnitCode != "c") 
            {
                throw new Exception("unit error");
            }

            var list2 = db.Reportable(new List<Unitguidsda>() {
               new Unitguidsda()
               }).ToQueryable().ToList();
        }
 

        public class Unitguidsda
        {
            public Guid Id { get; set; } = Guid.NewGuid();
        }
        public class Unitadadfafa 
        {
            [SqlSugar.SugarColumn(IsPrimaryKey =true,SqlParameterDbType =typeof(CommonPropertyConvert))]
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
