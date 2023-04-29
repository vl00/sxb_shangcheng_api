using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 后台管理--展示模板、兑换统计
    /// </summary>
    public class ShowMsgDHCodeQueryHandler : IRequestHandler<ShowMsgDHCodeQuery, MsgAndDHCodeDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        public ShowMsgDHCodeQueryHandler(IMediator mediator, IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public Task<MsgAndDHCodeDto> Handle(ShowMsgDHCodeQuery request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters();
            dy.Set("Courseid", request.CourseId);
            

            string sql = $@" 
select msg.*
,(select count(1)  from [dbo].[RedeemCode] as rcode where rcode.IsVaild=1 and Courseid=@Courseid)as TotalNumber
,(select count(1)  from [dbo].[RedeemCode] as rcode where rcode.IsVaild=1 and Courseid=@Courseid and rcode.Used=1 )as SendNumber
,(select count(1)  from [dbo].[RedeemCode] as rcode where rcode.IsVaild=1 and Courseid=@Courseid and rcode.Used=0 )as StockNumber
,(select TOP 1 convert(varchar,  CreatTime,120)  from [dbo].[RedeemCode] as rcode where rcode.IsVaild=1 and Courseid=@Courseid ORDER BY CreatTime DESC) as CreatTime
from dbo.MsgTemplate as  msg where msg.Courseid=@Courseid
                            ;";

            var data = _orgUnitOfWork.Query<MsgAndDHCodeDto>(sql, dy).FirstOrDefault();

            return Task.FromResult(data);
        }

    }

    /// <summary>
    /// 短信模板实体
    /// </summary>
    public class MsgDto
    {
        /// <summary>
        /// 短信模板Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
		/// 发送时间
		/// </summary> 
		public DateTime? CreateTime { get; set; }

        /// <summary>
		/// 兑换码
		/// </summary> 
		public string Code { get; set; }


        /// <summary>
        /// 发送人
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

    }


    /// <summary>
    /// 短信模板、兑换信息实体
    /// </summary>
    public class MsgAndDHCodeDto: MsgTemplate
    {

        /// <summary>
        /// 兑换码总数
        /// </summary>
        public int TotalNumber { get; set; }

        /// <summary>
        /// 已发送数量
        /// </summary>
        public int SendNumber { get; set; }

        /// <summary>
        /// 剩余可用数量
        /// </summary>
        public int StockNumber { get; set; }

        /// <summary>
        /// 上次导入时间(入库时间)
        /// </summary>
        public string CreatTime { get; set; }
    }

}
