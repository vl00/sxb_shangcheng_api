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
    /// 后台管理--课程兑换列表
    /// </summary>
    public class SearchExchangesQueryHandler : IRequestHandler<SearchExchangesQuery, PagedList<ExchangeDto>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        public SearchExchangesQueryHandler(IMediator mediator, IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public Task<PagedList<ExchangeDto>> Handle(SearchExchangesQuery request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters();
            dy.Set("CourseId", request.CourseId);
            dy.Set("skipCount", (request.PageIndex-1)*request.PageSize);
            dy.Set("pageSize", request.PageSize);
            

            string listSql = $@" 
                                SELECT ex.userid,convert(varchar, ex.CreateTime,120) as CreateTime,ex.code,ord.code as OrderCode
,ex.Creator,ex.status
FROM [dbo].[Exchange] as ex 
 left join [dbo].[Order] as ord  on ex.orderid=ord.id and ord.IsValid=1where ex.IsValid=1 and ord.courseid='{request.CourseId}'
order by ex.CreateTime DESC OFFSET {(request.PageIndex-1)*request.PageSize} ROWS FETCH NEXT {request.PageSize} ROWS ONLY
                            ;";
            string countSql = $@" 
                               SELECT count(1) FROM [dbo].[Exchange] as ex 
 left join [dbo].[Order] as ord  on ex.orderid=ord.id and ord.IsValid=1where ex.IsValid=1 and ord.courseid='{request.CourseId}';
                             ;";

            var totalItemCount =  _orgUnitOfWork.Query<int>(countSql, dy).FirstOrDefault();

            var data = _orgUnitOfWork.Query<ExchangeDto>(listSql,null).ToList();

            #region 操作发送短信用户信息           
            var usersId = data.Where(_=>_.Creator!=null).Distinct().ToList();
            if (usersId.Any() == true)
            {
                List<Guid> creatorIds = new List<Guid>();
                usersId.ForEach(_c => { creatorIds.Add((Guid)_c.Creator); });
                 var userInfo = _mediator.Send(new UserInfosByUserIdsOrMobileQuery() { UserIds = creatorIds }).Result;
                data.ForEach(_d =>
                {
                    var u = userInfo.FirstOrDefault(_u => _u.UserId == _d.UserId);
                    _d.UserName = u?.NickName?? "系统自动发送";
                    _d.StatusDesc = ((ExchangeStatus)_d.Status).GetDesc();
                });
            }
            else
            {
                data.ForEach(_d =>
                {                    
                    _d.UserName ="系统自动发送";
                    _d.StatusDesc = ((ExchangeStatus)_d.Status).GetDesc();
                });
            }

            #endregion


            var result = data.ToPagedList(request.PageSize, request.PageIndex, totalItemCount);
            return Task.FromResult(result);
        }

    }

    /// <summary>
    /// 兑换列表实体
    /// </summary>
    public class ExchangeDto
    {
        public Guid UserId { get; set; }

        /// <summary>
		/// 发送时间
		/// </summary> 
		public string CreateTime { get; set; }

        /// <summary>
		/// 兑换码
		/// </summary> 
		public string Code { get; set; }

        /// <summary>
        /// 发送人Id
        /// </summary>
        public Guid? Creator { get; set; }

        /// <summary>
        /// 发送人名字
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

        public int Status { get; set; }

        /// <summary>
        /// 发送状态
        /// </summary>
        public string StatusDesc { get; set; }

    }


}
