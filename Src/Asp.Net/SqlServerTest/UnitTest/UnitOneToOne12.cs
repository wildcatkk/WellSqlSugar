﻿using SqlSugarDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmTest
{
    internal class UnitOneToOne12
    {
        public static void Init()
        {
            var db = NewUnitTest.Db;
            db.CodeFirst.InitTables<user_name_simple, poetry_video_comment>();
            db.DbMaintenance.TruncateTable<user_name_simple, poetry_video_comment>();
            var userid = db.Insertable(new user_name_simple()
            {
                name = "jack",
                create_time = DateTime.Now
            }).ExecuteReturnIdentity();
            var userid2 = db.Insertable(new user_name_simple()
            {
                name = "hellid",
                create_time = DateTime.Now
            }).ExecuteReturnIdentity();
            db.Insertable(new poetry_video_comment()
            {
                first_reply_id = 0,
                reply_user_id = 0,
                user_id = 0
            }).ExecuteReturnIdentity();
            db.Insertable(new poetry_video_comment()
            {
                first_reply_id = 2,
                reply_user_id = userid,
                user_id = userid2
            }).ExecuteReturnIdentity();
            var x = db.Queryable<poetry_video_comment>().ToList();
            //db.Insertable(new poetry_video_comment()
            //{
            //    first_reply_id = 11,
            //    reply_user_id = 11,
            //    user_id = 11
            //}).ExecuteReturnIdentity();
            var list = db.Queryable<poetry_video_comment>()
                              .Includes(o => o.FirstReply, o => o.ReplyUser)
                            .Includes(o => o.FirstReply, o => o.UserInfo)
                             .ToList();
            db.Queryable<poetry_video_comment>().IncludeFullJoin(it => it.FirstReply)
                .ToList();
            db.Queryable<poetry_video_comment>().IncludeLeftJoin(it => it.FirstReply)
               .ToList();
            db.Queryable<poetry_video_comment>().IncludeRightJoin(it => it.FirstReply)
              .ToList();
            db.Queryable<poetry_video_comment>().IncludeInnerJoin(it => it.FirstReply)
             .ToList();
            if (list.Last().FirstReply.UserInfo == null) 
            {
                throw new Exception("unit error");
            }
            if (list.Last().FirstReply.ReplyUser == null)
            {
                throw new Exception("unit error");
            }
            var list2=db.Queryable<poetry_video_comment>()
                .Select(it => new
                {
                    y= it.UserInfo.create_time .Value.ToString("yyyy-MM-dd")
                }).ToList();
        }
    }
}
