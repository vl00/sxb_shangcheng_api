using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Linq;

namespace iSchool.Infrastructure.Repositories
{
    public class SchoolExtReposiory : BaseRepository<SchoolExtension>, ISchoolExtReposiory
    {

        public SchoolExtReposiory(IUnitOfWork IUnitOfWork) : base(IUnitOfWork)
        {
        }

        //public List<ExtMenuItem> GetMenuList(Guid extId)
        //{
        //    List<ExtMenuItem> list = new List<ExtMenuItem>();
        //    //学校概况
        //    var item1 = GetMenuItem("学校概况", "dbo.SchoolExtContent", extId);
        //    list.Add(item1);
        //    //招生简章
        //    var item2 = GetMenuItem("招生简章", "dbo.SchoolExtRecruit", extId);
        //    list.Add(item2);
        //    //课程体系
        //    var item3 = GetMenuItem("课程体系", "dbo.SchoolExtCourse", extId);
        //    list.Add(item3);
        //    //收费标准
        //    var item4 = GetMenuItem("收费标准", "dbo.SchoolExtCharge", extId);
        //    list.Add(item4);
        //    //师资力量及教学质量
        //    var item5 = GetMenuItem("师资力量及教学质量", "dbo.SchoolExtQuality", extId);
        //    list.Add(item5);
        //    //师资力量及教学质量
        //    var item6 = GetMenuItem("硬件设施及学生生活", "dbo.SchoolExtLife", extId);
        //    list.Add(item6);
        //    return list;
        //}
        public List<ExtMenuItem> GetMenuList(Guid extId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("学校概况", "dbo.SchoolExtContent");
            dic.Add("招生简章", "dbo.SchoolExtRecruit");
            dic.Add("课程体系", "dbo.SchoolExtCourse");
            dic.Add("收费标准", "dbo.SchoolExtCharge");
            dic.Add("师资力量及教学质量", "dbo.SchoolExtQuality");
            dic.Add("硬件设施及学生生活", "dbo.SchoolExtLife");
            return GetMenuItems(dic, extId);
        }

        /// <summary>
        /// 获取菜单项
        /// </summary>
        /// <param name="menuName"></param>
        /// <param name="tableName"></param>
        /// <param name="extId"></param>
        /// <returns></returns>
        private ExtMenuItem GetMenuItem(string menuName, string tableName, Guid extId)
        {
            var item = Connection
                .QueryFirstOrDefault<ExtMenuItem>(
                $"SELECT TOP 1 id AS Id, '{menuName}' AS Name, Round(Completion,4) AS Completion  FROM  {tableName} WHERE eid = @eid AND IsValid = 1",
                new { eid = extId });
            if (item == null) item = new ExtMenuItem { Id = null, Name = menuName, Completion = 0 };
            return item;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="extId"></param>
        /// <returns></returns>
        private List<ExtMenuItem> GetMenuItems(Dictionary<string, string> menuInfo, Guid extId)
        {
            if (menuInfo.Count() == 0)
                return null;
            var sqlList = new List<string>();
            foreach (var menu in menuInfo)
            {
                sqlList.Add($@"SELECT TOP 1 id AS Id, '{menu.Key}' AS Name, Round(Completion,4) AS Completion  FROM  {menu.Value} WHERE eid = @eid AND IsValid = 1");
            }
            var sql = string.Join(" UNION ", sqlList);
            var queryResult = Connection.Query<ExtMenuItem>(sql, new { eid = extId });
            var result = new List<ExtMenuItem>();
            foreach (var item in menuInfo)
            {
                var menu = queryResult.FirstOrDefault(p => p.Name.Equals(item.Key));
                if (menu == null)
                {
                    result.Add(new ExtMenuItem { Id = null, Name = item.Key, Completion = 0 });
                }
                else
                {
                    result.Add(menu);
                }
            }
            return result;
        }
        /// <summary>
        /// 获取简单的学部信息
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public List<ExtItem> GetSimpleExt(Guid sid)
        {
            var sql = "SELECT id AS ExtId,name AS ExtName,0 AS Completion FROM  dbo.SchoolExtension WHERE sid=@sid AND IsValid=1";
            List<ExtItem> list = Connection.Query<ExtItem>(sql, new { sid = sid }).ToList();
            foreach (var item in list)
            {
                var menus = GetMenuList(item.ExtId);
                item.Completion = menus.Sum(p => p.Completion) / menus.Count;
            }
            return list;
        }

        /// <summary>
        /// 删除学部
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="extId"></param>
        /// <returns></returns>
        public bool DelSchoolExt(Guid sid, Guid extId, Guid userId)
        {
            var sql = @" UPDATE dbo.SchoolExtension SET IsValid=0 ,ModifyDateTime=GETDATE(),Modifier=@userId  WHERE id=@extId AND sid=@sid";
            Connection.Execute(sql, new { extId = extId, sid = sid, userId = userId }, Transaction);
            return true;
        }
    }
}
