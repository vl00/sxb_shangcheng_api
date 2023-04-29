using AutoMapper;
using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Infras;
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
    public class CourseGoodsSimpleInfoByPropItemsQueryHandler : IRequestHandler<CourseGoodsSimpleInfoByPropItemsQuery, ApiCourseGoodsSimpleInfoDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        IUserInfo _me;
        CSRedisClient _redis;        
        IMapper _mapper;
        IConfiguration _config;

        public CourseGoodsSimpleInfoByPropItemsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._me = me;
            this._redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<ApiCourseGoodsSimpleInfoDto> Handle(CourseGoodsSimpleInfoByPropItemsQuery query, CancellationToken cancellation)
        {
            CourseGoodsSimpleInfoDto goodsInfo = null;
            await default(ValueTask);

            if (query.PropItemIds?.Length < 1)
            {
                throw new CustomResponseException("参数错误", Consts.Err.Selsku_EmptyPropItemIds);
            }

            //
            // find goodsId
            var sha1 = HashAlgmUtil.Encrypt(string.Join('|', query.PropItemIds.OrderBy(_ => _).Select(_ => _.ToString())), "sha1");
            Guid goodsId = default, courseId = default;
            var rdk = CacheKeys.CourseGoodsPropItemsSha1.FormatWith(sha1);
            var needUprdk = false;
            if (goodsId == default)
            {
                var jtoken = await _redis.GetAsync<JToken>(rdk);
                if (jtoken != null)
                {
                    goodsId = (Guid?)jtoken["goodsid"] ?? default;
                    courseId = (Guid?)jtoken["courseid"] ?? default;                    
                }
            }
            if (goodsId == default)
            {
                var sql = $@"
select top 2 g.id,g.Courseid,count(i.id)as itemcount from CourseGoods g
left join CourseGoodsPropItem gi on g.id=gi.goodsid
left join CoursePropertyItem i on i.id=gi.PropItemId
where g.IsValid=1 and g.show=1 and i.IsValid=1 and g.courseid=@CourseId
{"and i.id=@PropItemIds_0".If(query.PropItemIds.Length == 1)}
{"and i.id in @PropItemIds".If(query.PropItemIds.Length > 1)}
group by g.id,g.Courseid having(string_agg(convert(nvarchar(max),i.[id]),'|') within group(order by convert(nvarchar(max),i.[id]))=@hsh)
";
                var ls = await _orgUnitOfWork.DbConnection.QueryAsync<(Guid, Guid, int)>(sql, new 
                {
                    PropItemIds_0 = query.PropItemIds[0],
                    query.PropItemIds,
                    query.CourseId,
                    hsh = string.Join('|', query.PropItemIds.Select(_ => _.ToString()).OrderBy(_ => _)),
                });
                var c = ls?.Count() ?? 0;
                if (c > 1)
                {
                    throw new CustomResponseException("参数错误", Consts.Err.Selsku_MultGoods);
                }
                else if (c == 0)
                {
                    return new ApiCourseGoodsSimpleInfoDto
                    {
                        CourseId = query.CourseId,
                        PropItems = Array.Empty<CoursePropItemsListItemDto>(),
                    };
                }

                var itemcount = 0;
                (goodsId, courseId, itemcount) = ls.First();
                if (itemcount != query.PropItemIds.Length)
                {
                    throw new CustomResponseException("参数错误", Consts.Err.Selsku_PropItemCountNotSame);
                }

                needUprdk = true;                
            }

            // 
            // find goods by goodsId
            goodsInfo = await _mediator.Send(new CourseGoodsSimpleInfoByIdQuery { GoodsId = goodsId });
            do
            {
                if (goodsInfo?.CourseId != query.CourseId)
                    throw new CustomResponseException("参数错误", Consts.Err.Selsku_CourseNotSame);

                foreach (var i in query.PropItemIds)
                {
                    if (!goodsInfo.PropItems.Any(_ => _.Id == i))
                        throw new CustomResponseException("参数错误", Consts.Err.Selsku_PropItemNotSame);
                }
            }
            while (false);
            if (goodsInfo == null) throw new CustomResponseException("没结果", Consts.Err.Selsku_NoResult);

            if (needUprdk)
            {
                await _redis.SetAsync(rdk, new { goodsid = goodsId, courseid = courseId }, 60 * 15);
            }

            var result = _mapper.Map<ApiCourseGoodsSimpleInfoDto>(goodsInfo);

            // find course
            var course = await _mediator.Send(new CourseBaseInfoQuery { CourseId = goodsInfo.CourseId });
            result._Course = course;

            if (course.NewUserExclusive && _me.IsAuthenticated)
            {
                result.MeIsNewUser = (await _mediator.Send(new UserIsCourseTypeNewBuyerQuery 
                {
                    UserId = _me.UserId,
                    CourseType = (CourseTypeEnum)course.Type,
                })).IsNewBuyer;
            }

            return result;
        }

    }
}
