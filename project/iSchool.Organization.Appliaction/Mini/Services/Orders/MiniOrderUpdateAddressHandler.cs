using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    public class MiniOrderUpdateAddressHandler : IRequestHandler<MiniOrderUpdateAddressCmd, bool>
    {
        private readonly IUserInfo me;
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;

        public MiniOrderUpdateAddressHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, IUserInfo me)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this.me = me;

        }

        public async Task<bool> Handle(MiniOrderUpdateAddressCmd cmd, CancellationToken cancellation)
        {



            var sql = $@"
select top 1 o.* from [Order] o 
left join [OrderDetial] d on d.orderid=o.id --and d.producttype={ProductType.Course.ToInt()}
where 1=1 and o.IsValid=1 and o.AdvanceOrderNo=@AdvanceOrderNo
order by o.CreateTime desc
";
            var order = _orgUnitOfWork.QueryFirstOrDefault<Order>(sql, new { AdvanceOrderNo = cmd.AdvanceOrderNo });

            if (order == null)
            {
                throw new CustomResponseException("订单不存在.");
            }
            //发货后不给改了
            if (order.Status != (int)OrderStatusV2.Shipping && order.Status != (int)OrderStatusV2.Shipped && order.Status != (int)OrderStatusV2.Unpaid && order.Status != (int)OrderStatusV2.PaidFailed)
            {
                throw new CustomResponseException("当前订单状态不允许修改地址.");
            }
            if (order.Userid != me.UserId)
            {
                throw new CustomResponseException("非法操作.");
            }
            if (string.IsNullOrEmpty(cmd.AddressDto.Address))
            {
                throw new CustomResponseException("请填写收货地址");
            }
            if (!(cmd.AddressDto.Province == order.RecvProvince && cmd.AddressDto.City == order.RecvCity))
            {
                throw new CustomResponseException("地址超出可修改范围");
            }
            var addr = cmd.AddressDto;
            var updateCommentCountSql = "update [order] set Address=@Address,Mobile=@Mobile,RecvProvince=@RecvProvince,RecvCity=@RecvCity,RecvArea=@RecvArea,RecvPostalcode=@RecvPostalcode,RecvUsername=@RecvUsername where AdvanceOrderNo=@AdvanceOrderNo";
            var affectCount = await _orgUnitOfWork.DbConnection.ExecuteAsync(updateCommentCountSql, new
            {
                AdvanceOrderNo = cmd.AdvanceOrderNo,
                Address = addr.Address,
                Mobile = addr.RecvMobile,
                RecvProvince = addr.Province,
                RecvCity = addr.City,
                RecvArea = addr.Area,
                RecvPostalcode = addr.Postalcode,
                RecvUsername = addr.RecvUsername,
            }, _orgUnitOfWork.DbTransaction);
            return affectCount > 0;
        }

    }
}
