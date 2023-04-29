using iSchool.Domain.Modles;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Repository.Interfaces
{

    /// <summary>
    /// 分部仓储接口
    /// </summary>
    public interface ISchoolExtReposiory : IRepository<SchoolExtension>
    {
        /// <summary>
        /// 获取简单的分部信息
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        List<ExtItem> GetSimpleExt(Guid sid);
        /// <summary>
        /// 获取分部下的菜单信息
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>

        List<ExtMenuItem> GetMenuList(Guid ExtId);


        /// <summary>
        /// 删除学部
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="extId"></param>
        /// <returns></returns>
        bool DelSchoolExt(Guid sid, Guid extId, Guid userId);
    }

}
