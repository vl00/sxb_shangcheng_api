using iSchool.Domain.Modles;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Repository.Interfaces
{
    public interface ITagRepository : IDependency
    {
        /// <summary>
        ///获取学校的tags
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<TagItem> GetSchoolTagItems(Guid id, int type);

        /// <summary>
        /// 删除学校的tag
        /// </summary>
        /// <param name="binds"></param>
        /// <returns></returns>
        int DeleteSchoolTags(GeneralTagBind bind);
        /// <summary>
        /// 根据学校id删除学校的所有标签
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        int DeleteTagByDataId(Guid dataId, int type);
        /// <summary>
        /// 搜索通用标签
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        List<string> SearchGeneralTag(string text, int top);
    }
}
