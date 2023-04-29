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
    public class CourseGoodsSettleInfoQueryHandler : IRequestHandler<CourseGoodsSettleInfoQuery, CourseGoodsSettleInfoQryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public CourseGoodsSettleInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<CourseGoodsSettleInfoQryResult> Handle(CourseGoodsSettleInfoQuery query, CancellationToken cancellation)
        {
            var result = new CourseGoodsSettleInfoQryResult { CourseDto = new CourseOrderProdItemDto() };
            await default(ValueTask);

            // find goods info
            var goods = await _mediator.Send(new CourseGoodsSimpleInfoByIdQuery { GoodsId = query.Id, AllowNotValid = query.AllowNotValid });
            if (goods?.IsValid != true && !query.AllowNotValid) 
                throw new CustomResponseException("商品已下架", Consts.Err.CourseGoodsIsOffline);

            // course info
            var course_info = await _mediator.Send(new CourseBaseInfoQuery { CourseId = goods.CourseId, AllowNotValid = query.AllowNotValid });
            mapper.Map(course_info, result.CourseDto);
            if ((course_info?.IsValid != true || course_info?.Status != CourseStatusEnum.Ok.ToInt()) && !query.AllowNotValid)
                throw new CustomResponseException("商品已下架", Consts.Err.CourseOffline);

            // org info
            var org_info = await _mediator.Send(new OrgzBaseInfoQuery { OrgId = course_info.Orgid, AllowNotValid = query.AllowNotValid });
            result.CourseDto.OrgInfo = mapper.Map<CourseOrderProdItem_OrgItemDto>(org_info);
            if ((org_info?.IsValid != true || org_info?.Status != OrganizationStatusEnum.Ok.ToInt()) && !query.AllowNotValid)
                throw new CustomResponseException("商品已下架", Consts.Err.CourseOffline);

            // goods price
            result.CourseDto.PropItemIds = goods.PropItems.Select(_ => _.Id).ToArray();
            result.CourseDto.PropItemNames = goods.PropItems.Select(_ => _.Name).ToArray();
            result.CourseDto.ProdType = goods.Type;
            result.CourseDto.GoodsId = query.Id;
            result.CourseDto.PointsInfo = goods.PointExchange;
            result.CourseDto.BuyCount = query.BuyCount;
            result.CourseDto.Price = goods.Price;
            result.CourseDto.Origprice = goods.Origprice;
            result.CourseDto.LimitedTimeOffer = course_info?.LimitedTimeOffer;
            result.CourseDto.NewUserExclusive = course_info?.NewUserExclusive;
            result.CourseDto.SupplierInfo = new CourseOrderProdItem_SupplierInfo { Id = goods.SupplierId ?? default };

            // 小助手qrcode
            if (query.UseQrcode)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), config[$"AppSettings:org_assistant"]);
                var bys = await File.ReadAllBytesAsync(path);
                result.Qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }

            result.CourseDto.IsValid = goods.IsValid && course_info.IsValid && org_info.IsValid 
                && course_info.Status == CourseStatusEnum.Ok.ToInt() && org_info.Status == OrganizationStatusEnum.Ok.ToInt();
            result.CourseDto.Banner = goods.Cover != null ? new[] { goods.Cover } : result.CourseDto.Banner;

            // 加载库存
            if (result.CourseDto.IsValid)
            {
                result.CourseDto.Stock = (await _mediator.Send(new CourseGoodsStockRequest
                {
                    GetStock = new GetGoodsStockQuery { Id = query.Id, FromDBIfNotExists = true }
                })).GetStockResult ?? 0;
            }

            return result;
        }

    }
}
