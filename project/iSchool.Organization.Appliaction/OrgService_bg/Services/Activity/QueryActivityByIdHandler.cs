using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Appliaction.ViewModels;

namespace iSchool.Organization.Appliaction.OrgService_bg
{

    /// <summary>
    /// 待编辑活动信息
    /// </summary>
    public class QueryActivityByIdHandler : IRequestHandler<QueryActivityById, AddUpdateActivityShowDto>
    {
        
        OrgUnitOfWork _orgUnitOfWork;        

        public QueryActivityByIdHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;            
        }

        public async Task<AddUpdateActivityShowDto> Handle(QueryActivityById request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var dy = new DynamicParameters()
            .Set("ActivityId", request.ActivityId);

            #region 活动基本信息
            string sql = $@" Select id, title, logo, starttime, endtime, limit, budget from Activity act where IsValid=1 and id=@ActivityId;";
            var dBData = _orgUnitOfWork.DbConnection.Query<AddUpdateActivityShowDto>(sql, dy).FirstOrDefault();
            if (dBData == null) throw new CustomResponseException("活动基本信息不存在！");
            #endregion

            #region 活动旧专题
            string speSql = $" select  contentid from  [dbo].[ActivityExtend] where type={ActivityExtendType.Special.ToInt()} and activityid=@ActivityId ;";
            dBData.ListOldSpecials= _orgUnitOfWork.DbConnection.Query<Guid>(speSql, dy).ToList();
            #endregion

            #region 活动旧规则
            string ruleSql = $" select * from [dbo].[ActivityRule] where IsValid=1 and activityid=@ActivityId ;";
            var listRues= _orgUnitOfWork.DbConnection.Query<ActivityRule>(ruleSql, dy).ToList();
            if (listRues.Any() == true)
            {
                //1、停止/继续活动
                var rule1 = listRues.FirstOrDefault(_ => _.Type == ActivityRuleType.StopOrKeepActivity.ToInt());
                dBData.StopOrKeepActivity = rule1?.Number == null ? 2 : (int)rule1?.Number;

                //2、审核通过N天内不能修改
                var rule2 = listRues.FirstOrDefault(_ => _.Type == ActivityRuleType.OperationNotAllowed.ToInt());
                dBData.NDaysNotAllowChange = rule2?.Number;

                //3、第N篇额外奖金
                var rule3 = listRues.Where(_ => _.Type == ActivityRuleType.ExtraBonus.ToInt()).OrderBy(_=>_.Price);
                if (rule3.Any() == true)
                {
                    var dic = new Dictionary<int, decimal>();
                    foreach (var item in rule3)
                    {
                        dic.Add((int)item.Number, (decimal)item.Price);
                    }
                    dBData.NExtraBonus = dic;
                }

                //4、单篇奖金
                var rule4 = listRues.FirstOrDefault(_ => _.Type == ActivityRuleType.SingleBonus.ToInt());
                dBData.Price = rule4?.Price==null?0:(int)rule4?.Price;
            }

            #endregion
            return dBData;
        }

    }
}
