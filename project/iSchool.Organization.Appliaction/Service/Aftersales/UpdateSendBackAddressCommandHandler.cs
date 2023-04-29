using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class UpdateSendBackAddressCommandHandler : IRequestHandler<UpdateSendBackAddressCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public UpdateSendBackAddressCommandHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<bool> Handle(UpdateSendBackAddressCommand request, CancellationToken cancellationToken)
        {
            //退款/换货状态  
            //1. 提交申请  2.平台审核(发货) 3.平台审核(未发货)   4.平台退款  5.退款成功  6.审核失败
            //11.提交申请   12.平台审核   13.审核失败   14.寄回商品  15平台收货  16.验货失败   17.退款成功

            string sql = @"UPDATE OrderRefunds SET SendBackAddress = @SendBackAddress,SendBackUserName = @SendBackUserName,SendBackMobile = @SendBackMobile,ModifyDateTime =@ModifyDateTime,Modifier = @Modifier WHERE Id=@OrderRefundId AND [Status] =12";
            return (await _orgUnitOfWork.ExecuteAsync(sql, new { request.OrderRefundId,request.SendBackAddress,request.SendBackMobile,request.SendBackUserName, ModifyDateTime = DateTime.Now, Modifier = request.Auditor })) > 0;

            
        }
    }
}
