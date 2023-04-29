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
    public class CourseMultiGoodsSettleInfosQueryHandler : IRequestHandler<CourseMultiGoodsSettleInfosQuery, CourseMultiGoodsSettleInfosQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;
        IConfiguration config;

        public CourseMultiGoodsSettleInfosQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<CourseMultiGoodsSettleInfosQryResult> Handle(CourseMultiGoodsSettleInfosQuery query, CancellationToken cancellation)
        {
            var result = new CourseMultiGoodsSettleInfosQryResult();
            await default(ValueTask);

            var courseDtos = new List<CourseOrderProdItemDto>(query.Goods.Length);
            for (var i = 0; i < query.Goods.Length; i++)
            {
                var goods = query.Goods[i];
                if (courseDtos.Any(c => c.GoodsId == goods.Id)) continue;

                try
                {
                    var goodsInfo = await _mediator.Send(new CourseGoodsSettleInfoQuery
                    {
                        Id = goods.Id,
                        BuyCount = goods.BuyCount,
                        UseQrcode = false,
                        AllowNotValid = query.AllowNotValid,

                    });
                    courseDtos.Add(goodsInfo.CourseDto);
                }
                catch
                {
                    if (!query.AllowNotValid)
                        throw;
                }
            }
            result.CourseDtos = courseDtos.ToArray();

            // 小助手qrcode
            if (query.UseQrcode)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), config[$"AppSettings:org_assistant"]);
                var bys = await File.ReadAllBytesAsync(path);
                result.Qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }

            if (query.UserId != null)
            {
                result.UserPoints = await GetUserPoints(query.UserId.Value);
            }

            return result;
        }

        async Task<int> GetUserPoints(Guid userId)
        {
            string sql = @"SELECT [Points]  FROM [iSchoolPointsMall].[dbo].[AccountPoints]  WHERE UserId = @userId";
            return await _orgUnitOfWork.ExecuteScalarAsync<int>(sql, new { userId });

        }

    }
}
