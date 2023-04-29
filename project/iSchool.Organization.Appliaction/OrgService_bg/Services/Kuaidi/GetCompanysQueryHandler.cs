using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Kuaidi
{
    /// <summary>
    /// 后台管理--获取快递公司信息
    /// </summary>
    public class GetCompanysQueryHandler : IRequestHandler<GetCompanysQuery, KeyValuePair<string, string>[]>
    {
        OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        public GetCompanysQueryHandler(IMediator mediator, IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public Task<KeyValuePair<string, string>[]> Handle(GetCompanysQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Handle_GetCompanyCodesQuery()) ;
        }

        #region 快递公司基础数据
        /// <summary>
        /// [com_name, code, code100, alias...]
        /// </summary>
        /// <returns></returns>
        static IEnumerable<string[]> KdcodeDatas_for_transformat()
        {
            #region g Kuaidi-company-code



            #endregion g Kuaidi-company-code

            // and more...
        }

        private KeyValuePair<string, string>[] Handle_GetCompanyCodesQuery()
        {
            return KdcodeDatas_for_transformat().Select(arr => KeyValuePair.Create(arr[1], arr[0])).OrderBy(_ => _.Key).ToArray();
        }
        #endregion


    }
}
