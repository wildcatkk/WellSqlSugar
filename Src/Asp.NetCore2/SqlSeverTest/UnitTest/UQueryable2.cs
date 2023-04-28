using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmTest
{
    public partial class NewUnitTest
    {
        public static void Queryable2()
        {
            var list4 = Db.Queryable<ABMapping>()
                            .Mapper(it => it.A, it => it.AId)
                            .Where(it => it.A.Name == "a")
                            .ToSugarList();


            var list5 = Db.Queryable<ABMapping>()
                   .Mapper(it => it.A, it => it.AId, it => it.A.Id)
                   .Where(it => it.A.Name == "a")
                   .ToSugarList();


            var list3 = Db.Queryable<Order>()
                .Mapper(it => it.Items, it => it.Items.First().OrderId)
                .Where(it => it.Items.Count() > 0)
                .ToSugarList();

            var list6 = Db.Queryable<Order>()
               .Mapper(it => it.Items, it => it.Items.First().OrderId)
               .Where(it => it.Items.Any())
               .ToSugarList();
            var list7= Db.Queryable<Order>()
               .Mapper(it => it.Items, it => it.Items.First().OrderId)
               .Where(it => it.Items.Any(y => y.ItemId == 1))
               .ToSugarList();

            var sql=Db.Queryable<Order>().AS("[order]").ToSugarList();

            var sql1 = Db.Queryable<Order, OrderItem, Custom>((o, i, c) => new JoinQueryInfos(
             JoinType.Left, o.Id == i.OrderId,
             JoinType.Left, c.Id == o.CustomId
            ))
            .AS("[aa]")
            .AS<OrderItem>("[xx]")
            .AS<Custom>("[yy]")
           .Select<ViewOrder>().ToSql().Key;
            if (!sql1.Contains("[aa]") || !sql1.Contains("[xx]") || !sql1.Contains("[yy]"))
            {
                throw new Exception("unit queryable2 ");
            }
            var sql2 = Db.Queryable<OrderItem>().AS("[zz]").ToSql().Key;
            if (sql2 != "SELECT [ItemId],[OrderId],[Price],[CreateTime] FROM [zz] ")
            {
                throw new Exception("unit queryable2 ");
            }
            Db.Queryable<Order, OrderItem, Custom>((o, i, c) => new JoinQueryInfos(
             JoinType.Left, o.Id == i.OrderId,
             JoinType.Left, c.Id == o.CustomId
            ))
            .AS("[order]")
            .AS<OrderItem>("[orderdetail]")
            .AS<Custom>("[custom]")
           .Select<ViewOrder>().ToSugarList();

            Db.Queryable<object>().AS("[order]").Select("*").ToSugarList();

            var qu1=Db.Queryable<Order>().Select(it => new
            {
                id = it.Id
            }).MergeTable().Select<Order>();
            var qu2 = Db.Queryable<Order>().Select(it => new
            {
                id = it.Id,
                name=it.Name
            }).MergeTable().Select<Order>();
            var list=Db.Queryable(qu1, qu2,JoinType.Left, (x, y) => x.Id == y.Id).Select((x,y) => new
            {
                id1=x.Id,
                name=y.Name
            }).ToSugarList();

            var qu3 = Db.Queryable<Order>().Select(it => new
            {
                id = it.Id,
                name = it.Name
            })
                   .Where(it => 2 > it.id)
            .MergeTable()
         
            .Select(it=> new Order() {
                 Id=SqlFunc.IIF(2>it.id,1,2)
            }).ToSugarList();


            var qu4 = Db.Queryable<Order>().OrderBy(it=>it.Id+it.Id).ToSugarList();
        }
    }
}
