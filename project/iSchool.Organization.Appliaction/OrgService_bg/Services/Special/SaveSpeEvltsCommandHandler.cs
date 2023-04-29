using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Domain.Enum;
using System.Linq;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 新增/编辑专题
    /// </summary>
    public class SaveSpeEvltsCommandHandler:IRequestHandler<SaveSpeEvltsCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public SaveSpeEvltsCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public Task<ResponseResult> Handle(SaveSpeEvltsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if(request.SpeEvltBings.Any()==false)
                    return Task.FromResult(ResponseResult.Success("暂无更新数据"));
                var bingIds = request.SpeEvltBings?.Where(_ => _.Value == true).Select(_ => _.Key).Distinct().ToList();//当前专题关联的评测Ids
                var delIds = string.Join("','", request.SpeEvltBings?.Select(_ => _.Key).ToList());//当前翻过的所有页   
                var otherSpecialDisassociate = string.Join("','", bingIds);
                var dp = new DynamicParameters().Set("specialid", request.Id);
                
                string delSql1 = $" delete [dbo].[SpecialBind] where specialid=@specialid and evaluationid in ('{delIds}') ;";//当前专题解绑前端传入的所有评测，不论关联或取消
                string delSql2 = $" delete [dbo].[SpecialBind] where specialid<>@specialid and evaluationid in ('{otherSpecialDisassociate}') ;";//其他专题解绑当前专题关联的评测
                StringBuilder strsql = new StringBuilder("insert into[dbo].[SpecialBind]([id], [specialid], [evaluationid], [IsValid]) values ");
                for (int i = 0; i < bingIds?.Count; i++)//重新绑定
                {
                    strsql.AppendFormat($"{",".If(i>0)}(NEWID(),@specialid,@evaluationid{i},1)");
                    dp.Set($"evaluationid{i}", bingIds[i]);
                }
                
                _orgUnitOfWork.BeginTransaction();

                string isql = strsql.ToString();
                _orgUnitOfWork.DbConnection.Execute(delSql1 + delSql2 + strsql.ToString(), dp, _orgUnitOfWork.DbTransaction);
                
                _orgUnitOfWork.CommitChanges();
                _redisClient.BatchDel(new List<string>() {
                    CacheKeys.Evlt.FormatWith("*")
                    ,$"org:spcl:id_*:pc:1p*"//一个pc专题页里评测ls首页缓存
                    ,$"org:spcl:id_*:total:orderby_*"//一个(大/小)专题页里评测ls分页总数
                    ,$"org:spcl:id_*:orderby_*"//一个(大/小)专题页里评测ls首页缓存
                    ,$"org:pc:relatedEvlts:*"//pc评测详情-相关评测s/pc课程详情-相关评测s/pc机构详情-相关评测s
                    //,$"org:course:courseid:*"//课程详情（含评测）
                });
                return Task.FromResult(ResponseResult.Success("操作成功"));
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return Task.FromResult(ResponseResult.Failed($"系统错误：【{ex.Message}】"));                
            }          
        
        }
    }
}
