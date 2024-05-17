﻿using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;
using System.Linq;
namespace OrmTest
{
    public class CrossDatabase03
    {
        public static void Init()
        {
            var db = new SqlSugarClient(new List<ConnectionConfig>()
            {
                new ConnectionConfig(){ConfigId="OrderDb",DbType=DbType.SqlServer,ConnectionString="server=.;uid=sa;pwd=sasa;database=SQLSUGAR4XTEST",IsAutoCloseConnection=true},
                new ConnectionConfig(){ConfigId="OrderItemDb",DbType=DbType.SqlServer,ConnectionString="server=.;uid=sa;pwd=sasa;database=SQLSUGAR4XTEST2",IsAutoCloseConnection=true  }
            });

            db.GetConnection("OrderDb").CodeFirst.InitTables<Order>();
            db.GetConnection("OrderItemDb").CodeFirst.InitTables<OrderItem>();

            db.GetConnection("OrderDb").DbMaintenance.TruncateTable<Order>();
            db.GetConnection("OrderItemDb").DbMaintenance.TruncateTable<OrderItem>();

            db.GetConnection("OrderDb").Insertable(new Order() { Id=1, CreateTime=DateTime.Now, Name="a", Price=10, CustomId=1 }).ExecuteCommand();
            db.GetConnection("OrderItemDb").Insertable(new OrderItem() { OrderId = 1, CreateTime = DateTime.Now , Price=10 }).ExecuteCommand();
 
            var list=db.QueryableWithAttr<OrderItem>()
                //.CrossQueryWithAttr()
                .Includes(z => z.Order)
                .ToList();


            var list2 = db.QueryableWithAttr<Order>()
                   // .CrossQueryWithAttr()
                    .Includes(z => z.Items)
                    .ToList();
            db.QueryableWithAttr<OrderItem>().IncludeLeftJoin(z => z.Order).ToList();


            if (list.First().Order == null) 
            {
                throw new Exception("unit error");
            }
            if (list2.First().Items.Count == 0)
            {
                throw new Exception("unit error");
            }
            Console.WriteLine("OrderDb");
            foreach (var item in db.GetConnection("OrderDb").DbMaintenance.GetTableInfoList(false))
            {
                Console.WriteLine(item.Name);
            }
            Console.WriteLine("OrderItemDb");
            foreach (var item in db.GetConnection("OrderItemDb").DbMaintenance.GetTableInfoList(false))
            {
                Console.WriteLine(item.Name);
            }
        }
        [Tenant("OrderDb")]
        [SqlSugar.SugarTable("Order8")]
        public class Order
        {
            [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
            public int Id { get; set; }

            public string Name { get; set; }
            public decimal Price { get; set; }
            [SugarColumn(IsNullable = true)]
            public DateTime CreateTime { get; set; }
            [SugarColumn(IsNullable = true)]
            public int CustomId { get; set; }
            [Navigate(NavigateType.OneToMany, nameof(OrderItem.OrderId))]
            public List<OrderItem> Items { get; set; }
        }
        [SqlSugar.SugarTable("OrderDetail8")]
        [Tenant("OrderItemDb")]
        public class OrderItem
        {
            [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
            public int ItemId { get; set; }
            public int OrderId { get; set; }
            public decimal? Price { get; set; }
            [SqlSugar.SugarColumn(IsNullable = true)]
            public DateTime? CreateTime { get; set; }
            [Navigate(NavigateType.OneToOne,nameof(OrderId))]
            public Order Order { get; set; }
        }
    }
}
