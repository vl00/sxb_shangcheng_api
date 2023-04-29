using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 清除评测相关缓存
    /// </summary>
    public class ClearEvltCachesCommandHandler : IRequestHandler<ClearEvltCachesCommand>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;        

        public ClearEvltCachesCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
            _redis = redis;
        }

        public async Task<Unit> Handle(ClearEvltCachesCommand cmd, CancellationToken cancellation)
        {
            switch (cmd.Type)
            {
                case 1:// 新增
                    {
                        await _mediator.Send(new SingleFieldClearEvltCachesCommand() { Id = cmd.Id });
                    }
                    break;
                case 2: // 大编辑编辑BatchDel(涉及专题、机构、课程变更)
                    {
                        await _redis.BatchDelAsync(new List<string>() {
                                CacheKeys.Evlt.FormatWith(cmd.Id)//评测详情
                                ,"org:evltsMain:*"//评测首页缓存\分页列表\课程评测                                
                                ,"org:spcl:id_*"//专题
                                ,"org:organization:orgid:*"//机构详情
                                ,"org:organization:evlts:orgid:*"//机构详情-相关评测
                                ,"org:organization:evlts:total:*"////机构详情-相关评测总条数
                                //,$"org:evlt:comment:top20:evlt_{cmd.Id}"
                                ,"org:evlt:comment:top*"
                                ,"org:*:relatedEvlts:*"//pc评测详情-相关评测s、pc课程详情-相关评测s、pc机构详情-相关评测s
                            }, 10);
                    }
                    break;
                case 3: // 点赞修改，直接调雄哥的方法
                    {
                        await _mediator.Send(new ClearLikesCachesCommand() { Ids = new List<Guid>() { cmd.Id } });
                    }
                    break;
                case 4://不影响评论的修改
                    {
                        await _mediator.Send(new SingleFieldClearEvltCachesCommand() { Id = cmd.Id });
                    }
                    break;
            }
            return default;
        }
    }

 

}
