using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels.Orders;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Orders
{

    public class OrgRelStatementDetailHandler : IRequestHandler<OrgRelStatementDetailCommand, StatementDetailResponseDto>
    {
      
        OrgUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public OrgRelStatementDetailHandler(IOrgUnitOfWork unitOfWork, IMediator mediator)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
        }

        public async Task<StatementDetailResponseDto> Handle(OrgRelStatementDetailCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var orderDetail = await _unitOfWork.QueryFirstOrDefaultAsync<OrderDetailNeed>(@"SELECT banner,productid,orderid,detail,propname,id from OrderDetial CROSS APPLY OPENJSON(ctn) WITH (detail NVARCHAR(100)  '$.""title""',propname NVARCHAR(100) '$.propItemNames[0]',banner NVARCHAR(1000)  '$.""banner""' ) where  Id=@Id ", new { Id = request.OrderDetailId });
            if (null == orderDetail) throw new CustomResponseException("参数有误，找不到数据orderDetail");
            var orderM = await _unitOfWork.QueryFirstOrDefaultAsync<iSchool.Organization.Domain.Order>(@"SELECT * from [Order]  where  Id=@Id ", new { Id = orderDetail.orderid });
            if (null == orderM) throw new CustomResponseException("参数有误，找不到数据orderM");
            // 查用户信息
            var reward= await _unitOfWork.QueryFirstOrDefaultAsync<iSchool.Organization.Domain.EvaluationReward>(@"SELECT * from [EvaluationReward]  where  OrderId=@OrderId  and GoodsId=@GoodsId", 
                new { OrderId = orderDetail.orderid, GoodsId= orderDetail.productid });
            if (null == reward) throw new CustomResponseException("参数有误，找不到数据reward");
            var uInfos =( await _mediator.Send(new UserSimpleInfoQuery
            {
                UserIds = new List<Guid>() { orderM.Userid }
            })).FirstOrDefault() ;
            

            var r = new StatementDetailResponseDto()
            {
                Bonus = reward.Reward,
                UserHeadImg = uInfos?.HeadImgUrl,
                UserNick = uInfos?.Nickname,
                OrderStatusDec =EnumUtil.GetDesc((OrderStatusV2)orderM.Status),
                CourseCover = orderDetail.banner,
                CourseProp = orderDetail.propname,
                CourseTitle = orderDetail.detail,
                PayAmount = orderM.Totalpayment,
                PayTime = DateTimeExchange.ToUnixTimestampByMilliseconds(orderM.Paymenttime.Value)

            };
            return r;
        }
        protected class OrderDetailNeed
        {
            public Guid id { get; set; }
            public string detail { get; set; }
            public string propname { get; set; }
            public Guid orderid { get; set; }
            public string banner { get; set; }
            public Guid productid { get; set; }
        }
    }
}
