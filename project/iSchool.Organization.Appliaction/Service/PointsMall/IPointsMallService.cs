using iSchool.Organization.Appliaction.Service.PointsMall.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.PointsMall
{
    public interface IPointsMallService
    {

        /// <summary>
        /// 冻结积分，从用户积分账户中冻结某部分积分。
        /// </summary>
        /// <param name="request"></param>
        /// <returns>返回 freezeIds</returns>
        Task<Guid> FreezePoints(FreezePointsRequest request);

        /// <summary>
        /// 解冻积分，将冻结积分从冻结状态回归到用户积分账户中。
        /// </summary>
        /// <param name="freezeId">冻结ID条目列表，可从FreezePoints获得</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> DeFreezePoints(Guid freezeId,Guid userId);



        /// <summary>
        /// 加冻结积分（仅对冻结部分进行增加操作，并非从现有积分中冻结）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Guid> AddFreezePoints(FreezePointsRequest request);

        /// <summary>
        /// 扣除冻结积分
        /// </summary>
        /// <param name="freezeId"></param>
        /// <param name="userId"></param>
        /// <param name="originType">来源类型 5->下订单，6->订单失效</param>
        /// <returns></returns>
        Task<bool> DeductFreezePoints(Guid freezeId, Guid userId, int originType);


        /// <summary>
        /// 加积分
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="points">为负时减去积分</param>
        /// <param name="originId"></param>
        /// <param name="remark"></param>
        /// <param name="originType">来源类型 5->下订单，6->订单失效</param>
        /// <returns></returns>
        Task<bool> AddAccountPoints(Guid userId, long points, string originId, string remark, int originType);


    }
}
