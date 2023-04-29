using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Repository.Interfaces
{
    public interface IRankReposiory : IDependency
    {
        /// <summary>
        /// 重排排行榜
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="rankId"></param>
        /// <param name="Placing"></param>
        /// <param name="IsJux">是否并列</param>
        /// <returns></returns>
        int SortRank(Guid sid, Guid rankId, double placing, bool isJux);

        /// <summary>
        ///删除排行榜的数据
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="rankId"></param>
        /// <returns></returns>
        int DelRank(Guid sid, Guid rankId);
    }
}
