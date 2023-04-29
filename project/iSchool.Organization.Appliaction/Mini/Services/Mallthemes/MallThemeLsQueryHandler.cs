using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class MallThemeLsQueryHandler : IRequestHandler<MallThemeLsQuery, MallThemeLsQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public MallThemeLsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<MallThemeLsQryResult> Handle(MallThemeLsQuery query, CancellationToken cancellation)
        {
            var result = new MallThemeLsQryResult();
            var now = DateTime.Now;
            await default(ValueTask);

            long pageIndex = query.PageIndex;
            switch (query.Id ?? "")
            {
                case string _ when query.PageIndex > 0 && !query.Id.IsNullOrEmpty():
                    throw new CustomResponseException("PageIndex与Id只能传其中一个");

                // 本期
                case string _ when query.PageIndex == 0 && query.Id.IsNullOrEmpty():
                    {
                        var sql = "select top 1 t.id from MallThemes t where t.IsValid=1 and t.StartTime<=@now and t.endtime>=@now order by t.StartTime ";
                        var id = await _orgUnitOfWork.QueryFirstOrDefaultAsync<Guid>(sql, new { now });
                        if (id == default) throw new CustomResponseException("找不到本期主题");

                        sql = @"select _no from (select row_number()over(order by t.StartTime)as _no,t.id,t.no from MallThemes t where t.IsValid=1) t where t.id=@id ";
                        var _no = await _orgUnitOfWork.QueryFirstOrDefaultAsync<long>(sql, new { id });
                        if (_no <= 0) throw new CustomResponseException("找不到本期主题.");

                        pageIndex = _no / query.PageSize + 1;
                    }
                    break;

                // 具体某期
                case string _ when query.PageIndex == 0 && !query.Id.IsNullOrEmpty():
                    {
                        var id = Guid.TryParse(query.Id, out var _id) ? _id : default;
                        var no = id == default ? UrlShortIdUtil.Base322Long(query.Id) : default;

                        var sql = $@"select _no from(select row_number()over(order by t.StartTime)as _no,t.id,t.no from MallThemes t where t.IsValid=1)t where 1=1 {"and t.id=@id".If(id != default)} {"and t.no=@no".If(no != default)}";
                        var _no = await _orgUnitOfWork.QueryFirstOrDefaultAsync<long>(sql, new { id, no });
                        if (_no <= 0) throw new CustomResponseException("找不到该期主题.");

                        pageIndex = _no / query.PageSize + 1;
                    }
                    break;
            }

            // on 正常分页
            if (pageIndex > 0)
            {
                var sql = $@"
select count(1) from MallThemes t where t.IsValid=1

select * from(select row_number()over(order by t.StartTime) as no,t.Id,t.no as Id_s,t.Name,t.Logo,t.MListPicture,t.PcListPicture,t.StartTime,t.EndTime
    from MallThemes t where t.IsValid=1
)T where no between (@pageIndex-1)*@PageSize+1 and @pageIndex*@PageSize
";
                var gr = await _orgUnitOfWork.QueryMultipleAsync(sql, new { pageIndex, query.PageSize });
                var cc = await gr.ReadFirstAsync<int>();
                var items = (await gr.ReadAsync<MallThemeDto>()).AsArray();
                foreach (var item in items)
                {
                    item.Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(item.Id_s));
                    item.IsCurrent = (item.StartTime ?? DateTime.MinValue) <= now && (item.EndTime ?? DateTime.MaxValue) >= now;
                }
                result.Pages = items.ToPagedList(query.PageSize, (int)pageIndex, cc);
            }

            return result;
        }

    }
}
