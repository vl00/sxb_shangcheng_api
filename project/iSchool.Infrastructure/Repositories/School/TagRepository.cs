using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using System;
using System.Collections.Generic;
using Dapper;
using System.Linq;
namespace iSchool.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private UnitOfWork UnitOfWork { get; set; }

        public TagRepository(IUnitOfWork unitOfWork)
        {
            UnitOfWork = (UnitOfWork)unitOfWork;
        }

        public List<TagItem> GetSchoolTagItems(Guid id, int type)
        {
            var sql = @"SELECT DISTINCT tag.id AS Id,tag.name FROM [dbo].[GeneralTagBind] AS bind 
                LEFT JOIN [dbo].[GeneralTag] AS tag ON  tag.id=bind.tagID 
                WHERE bind.dataID=@id and bind.dataType=@type";
            return UnitOfWork.DbConnection.Query<TagItem>(sql, new { id = id, type = type }).ToList();
        }

        /// <summary>
        ///删除学校tags
        /// </summary>
        /// <param name="binds"></param>
        /// <returns></returns>
        public int DeleteSchoolTags(GeneralTagBind bind)
        {
            var sql = @"DELETE dbo.GeneralTagBind WHERE tagID=@tagId AND dataID=@dataId";
            return UnitOfWork.DbConnection.Execute(sql, new { tagId = bind.TagId, dataId = bind.DataId }, UnitOfWork.DbTransaction);

        }

        /// <summary>
        /// 搜索通用标签
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<string> SearchGeneralTag(string text, int top)
        {
            var sql = $" SELECT top {top} name FROM dbo.GeneralTag WHERE name LIKE @Name";
            var result = UnitOfWork.DbConnection.Query<string>(sql, new { Name = $"%{text}%" }, UnitOfWork.DbTransaction);
            return result.ToList();
        }

        public int DeleteTagByDataId(Guid dataId, int type)
        {
            var sql = $"DELETE dbo.GeneralTagBind WHERE dataID=@dataId AND dataType=@type";
            var result = UnitOfWork.DbConnection.Execute(sql, new { dataId = dataId, type = type }, UnitOfWork.DbTransaction);
            return result;
        }
    }
}
