using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 根据机构名称(品牌)，查询机构列表
    /// </summary>
    public class OrganizationByNameQueryHandler : IRequestHandler<OrganizationByNameQuery, ResponseResult>
    {
        OrgUnitOfWork unitOfWork;
        CSRedisClient cSRedis;
        const int time = 60 * 60;//cache timeout

        public OrganizationByNameQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient cSRedis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.cSRedis = cSRedis;
        }



        public async Task<ResponseResult> Handle(OrganizationByNameQuery request, CancellationToken cancellationToken)
        {
            string key = string.Format(CacheKeys.OrgByNameList, request.OrgName, request.PageInfo.PageIndex + "&" + request.PageInfo.PageSize);
            OrganizationByNameResponse data = cSRedis.Get<OrganizationByNameResponse>(key);
            if (data != null)
            {
                return ResponseResult.Success(data);
            }
            else
            {

                #region Where
                var dy = new DynamicParameters();
                string sqlWhere = $" where 1=1 and  o.IsValid=1  and o.status=1 ";


                //机构名称模糊查询
                if (!string.IsNullOrEmpty(request.OrgName))
                {
                    dy.Add("@OrgName", request.OrgName);
                    sqlWhere += $"  and ( o.name  like '%{request.OrgName}%') ";
                }

                #endregion

                dy.Add("@PageIndex", request.PageInfo.PageIndex);
                dy.Add("@PageSize", request.PageInfo.PageSize);

                string sql = $@"select top {request.PageInfo.PageSize} *  from 
                            (                               
                                select ROW_NUMBER() over(order by id desc) rownumber,  o.id, o.name, o.logo, o.no
                                from [dbo].[Organization] o 
                                {sqlWhere}
                            )TT where rownumber>(@PageSize*(@PageIndex-1))
                             ";
                string sqlPage = $@" 
                                select 
                                COUNT(1) AS TotalCount from
                                (
                                    select distinct  o.id, o.name, o.logo, o.authentication
                                    from [dbo].[Organization] o 
                                    {sqlWhere}
                                 )T1 
                                ;";
                data = new OrganizationByNameResponse();
                data.OrganizationDatas = new List<OrganizationDataOfName>();
                data.OrganizationDatas = unitOfWork.Query<OrganizationDataOfName>(sql, dy).ToList();
                for (int i = 0; i < data.OrganizationDatas.Count; i++)
                {
                    data.OrganizationDatas[i].No= UrlShortIdUtil.Long2Base32(Convert.ToInt64(data.OrganizationDatas[i].No));
                }
                data.PageInfo = new PageInfoResult();
                data.PageInfo = unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();
                data.PageInfo.PageIndex = request.PageInfo.PageIndex;
                data.PageInfo.PageSize = request.PageInfo.PageSize;
                data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);
                cSRedis.Set(key, data, time);
                return ResponseResult.Success(data);
            }
        }

    }
}
