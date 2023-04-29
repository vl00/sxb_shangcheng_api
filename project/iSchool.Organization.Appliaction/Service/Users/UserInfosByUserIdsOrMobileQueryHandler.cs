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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    /// <summary>
    /// 用户列表，【微信昵称】
    /// </summary>
    public class UserInfosByUserIdsOrMobileQueryHandler : IRequestHandler<UserInfosByUserIdsOrMobileQuery, List<UserInfoByUserIdsOrMobileResponse>>
    {
        OrgUnitOfWork _unitOfWork;
        

        public UserInfosByUserIdsOrMobileQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
           
        }

        public async Task<List<UserInfoByUserIdsOrMobileResponse>> Handle(UserInfosByUserIdsOrMobileQuery query, CancellationToken cancellation)
        {             
            var dp = new DynamicParameters();
            string where = "";
            if (!string.IsNullOrEmpty(query.OrdMobile))
            {
                where += $" and mobile = @mobile  ";
                dp.Set("mobile", query.OrdMobile);
            }
            if (query.UserIds?.Any() == true)
            {
                where += $" and u.id in ('{string.Join("','", query.UserIds)}')   ";
               
            }

            string sql = @$" select distinct u.id as  userid,u.nickname,u.mobile,'' as  wxnickname from [iSchoolUser].[dbo].[userInfo] u
 where u.channel is null 
                           {where}
                          ";
            var result= _unitOfWork.DbConnection.Query<UserInfoByUserIdsOrMobileResponse>(sql, dp).ToList();
            if (result?.Any() == true)
            {
                string wx = @$" select distinct wx.userID,wx.nickname as wxnickname from  [iSchoolUser].[dbo].[unionid_weixin] as wx 
 where  wx.valid=1 and wx.userID in ('{string.Join("','",result.Select(_=>_.UserId))}')  ;";

                var wnicks = _unitOfWork.DbConnection.Query<UserInfoByUserIdsOrMobileResponse>(wx).ToList();
                if (wnicks?.Any() == true)
                {
                    foreach (var item in wnicks)
                    {
                        item.WXNickName = wnicks.FirstOrDefault(_ => _.UserId == item.UserId)?.WXNickName??item.NickName;
                    }
                }
            }
            
                          
                         

            return result;
        }

    }
}
