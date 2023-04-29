using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class CourseGoodsPropsQueryHandler : IRequestHandler<CourseGoodsPropsQuery, CourseGoodsPropsDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;

        public CourseGoodsPropsQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, CSRedisClient redis,
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this.me = me;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<CourseGoodsPropsDto> Handle(CourseGoodsPropsQuery query, CancellationToken cancellation)
        {
            var result = new CourseGoodsPropsDto { BuyAmount = 1 };
            await default(ValueTask);

            var info = await _mediator.Send(new CourseBaseInfoQuery { CourseId = query.CourseId, No = query.CourseNo });
            result.Id = info.Id;
            result.Title = info.Title;
            result.Logo = info.Banner?.ToObject<string[]>()?.FirstOrDefault();
            // 机构信息
            //var org_info = await _mediator.Send(new OrgzBaseInfoQuery { OrgId = info.Orgid });

            // table
            var table = await _mediator.Send(new CourseGoodsPropsSmTableQuery { CourseId = info.Id });
            result.Table = table.Where(x => (x.PointExchange?.Points > 0 && query.IsFromPoints)  || (!query.IsFromPoints));
            //result.Table = result.Table.SelectMany(x => x.PropItems.Select(pi => new CourseGoodsPropsSmTableItem1Dto 
            //{
            //    GoodsId = x.GoodsId,
            //    CourseId = x.CourseId,
            //    Price = x.Price,
            //    PropGroupId = pi.PropGroupId,
            //    PropGroupName = pi.PropGroupName,
            //    Sort_pg = pi.Sort_pg,
            //    PropItemId = pi.PropItemId,
            //    PropItemName = pi.PropItemName,
            //    Sort_i = pi.Sort_i,
            //})).ToArray();

            // result.MinPrice + result.MaxPrice
            result.MinPrice = result.Table.Count() > 0 ? result.Table.Where(_ => (_.Stock ?? 0) > 0).Count() > 0 ? result.Table.Where(_ => (_.Stock ?? 0) > 0).Min(x => x.Price) : 0 : 0;
            result.MaxPrice = result.Table.Count() > 0 ? result.Table.Where(_ => (_.Stock ?? 0) > 0).Count() > 0 ? result.Table.Where(_ => (_.Stock ?? 0) > 0).Max(x => x.Price) : 0 : 0;
            if (query.IsFromPoints)
            {
                result.MinPoints = result.Table.Min(s => s.PointExchange?.Points) ?? 0;
                result.MaxPoints = result.Table.Max(s => s.PointExchange?.Points) ?? 0;

            }
            // 课程属性s列表            
            result.Props = result.Table.SelectMany(x => x.PropItems).GroupBy(x => (x.PropGroupId, x.PropGroupName, x.Sort_pg)).OrderBy(x => x.Key.Sort_pg)
                .Select(g =>
                {
                    var prop = new CoursePropsListItemDto
                    {
                        Id = g.Key.PropGroupId,
                        Name = g.Key.PropGroupName,
                        Sort_pg = g.Key.Sort_pg,
                        PropItems = g.OrderBy(x => x.Sort_i).DistinctBy(x => x.PropItemId).Select(x => new CoursePropItemsListItemDto
                        {
                            Id = x.PropItemId,
                            Name = x.PropItemName,
                            //Cover=x.PropItemCover,
                            Sort_i = x.Sort_i,
                        }).ToArray()
                    };
                    return prop;
                })
                .ToArray();

            return result;
        }

    }
}
