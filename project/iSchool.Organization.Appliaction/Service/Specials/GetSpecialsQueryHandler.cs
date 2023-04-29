using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 某个大专题的小专题集合
    /// </summary>
    public class GetSpecialsQueryHandler : IRequestHandler<GetSpecialsQuery, IEnumerable<SmallSpecialItem>>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;        

        public GetSpecialsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;            
        }

        public async Task<IEnumerable<SmallSpecialItem>> Handle(GetSpecialsQuery req, CancellationToken cancellation)
        {
            var id = req.SpecialId;//大专题Id
            string rdkNo = CacheKeys.SpclNo.FormatWith(req.No), rdk = null;
            IEnumerable<SmallSpecialItem> dy = null;            

            if (id == default)
            {                
                var str_id = await redis.GetAsync<string>(rdkNo);
                if (str_id != null)
                {
                    id = Guid.Parse(str_id);
                }
            }
            if (id != default)
            {
                rdk = CacheKeys.Rdk_Big_spcl.FormatWith(id);
                dy = await redis.GetAsync<IEnumerable<SmallSpecialItem>>(rdk);
            }
            if (dy == null)
            {
                var sql = $@"
SELECT spe.id,spe.No as Id_s,spe.title,spe.subtitle  FROM [dbo].[SpecialSeries] ss 
left join  [dbo].[Special] spe on ss.smallspecial=spe.id and spe.IsValid=1
where ss.IsValid=1  and spe.status={SpecialStatusEnum.Ok.ToInt()}  and spe.type={SpecialTypeEnum.SmallSpecial.ToInt()}  {"and ss.bigspecial=@Id".If(id != default)} {"and ss.No=@No".If(id == default)} order by ss.sort
           
            ";
                var list = (await unitOfWork.QueryAsync<SmallSpecialItem>(sql, new { req.No, Id = id })).ToList();                
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(list[i].Id_s));
                }
                dy = list;
                rdk ??= CacheKeys.Rdk_spcl.FormatWith(id);
                await redis.StartPipe()
                    .Set(rdkNo, id, 60 * 60 * 1)
                    .Set(rdk, dy, 60 * 100)
                    .EndPipeAsync();
            }
           
            return dy;
        }
    }
}
