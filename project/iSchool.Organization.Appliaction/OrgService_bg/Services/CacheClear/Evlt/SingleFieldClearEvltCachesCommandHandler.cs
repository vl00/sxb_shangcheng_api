using CSRedis;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
using iSchool.Organization.Appliaction.OrgService_bg.Common;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 评测缓存清除--评测小更新(不涉及专题、机构、课程的变更)通用方法
    /// 1、可用于新增
    /// 2、不涉及到专题、机构、课程Id的切换（大编辑涉及）
    /// </summary>
    public class SingleFieldClearEvltCachesCommandHandler : IRequestHandler<SingleFieldClearEvltCachesCommand>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;        

        public SingleFieldClearEvltCachesCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
            _redis = redis;
        }

        public async Task<Unit> Handle(SingleFieldClearEvltCachesCommand request, CancellationToken cancellation)
        {
            string sql = @$" select sp.specialid,eb.orgid,eb.courseid from  [dbo].[EvaluationBind] eb
                            left join[dbo].[SpecialBind] sp on eb.evaluationid = sp.evaluationid and sp.IsValid = 1
                            where eb.IsValid = 1 and eb.evaluationid = @EvltId ";

            var oldData = _orgUnitOfWork.DbConnection.Query<AboutEvltIds>(sql, new DynamicParameters().Set("EvltId", request.Id)).FirstOrDefault();

            var delKeys = new List<string>
            {
                CacheKeys.Evlt.FormatWith(request.Id)//评测详情
                 ,"org:evltsMain:*"//评测首页缓存\分页列表\课程评测                  
                 ,"org:*:relatedEvlts:*"//pc评测详情-相关评测s、pc课程详情-相关评测s、pc机构详情-相关评测s
            };
            #region 不涉及专题、机构、课程变化可以精准删除以下缓存
            if (oldData != null)
            {
                if (oldData.SpecialId != null)//专题
                    delKeys.AddRange(new List<string>() {
                        $"org:spcl:id_{oldData.SpecialId}",
                        $"org:spcl:id_{oldData.SpecialId}:*"
                    });

                if (oldData.OrgId != null)//机构
                    delKeys.AddRange(new List<string>() {
                    $"org:organization:orgid:{oldData.OrgId}",
                    $"org:organization:evlts:orgid:{oldData.OrgId}",
                    $"org:organization:evlts:total:{oldData.OrgId}",
                    $"org:organization:orgid:{oldData.OrgId}:*"//del pc单个机构
                    });
                if (oldData.CourseId != null)//课程
                    delKeys.Add($"org:course:courseid:{oldData.CourseId}");

            }
            #endregion
            await _redis.BatchDelAsync(delKeys,10);               
            return default;
        }
    }

 

}
