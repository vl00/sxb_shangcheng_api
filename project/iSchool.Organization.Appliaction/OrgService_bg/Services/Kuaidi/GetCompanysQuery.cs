using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Kuaidi
{
    /// <summary>
    /// 后台管理--获取快递公司信息
    /// </summary>
    public class GetCompanysQuery : IRequest<KeyValuePair<string, string>[]>
    {
      
    }
}
