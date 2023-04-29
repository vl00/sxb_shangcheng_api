using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using Microsoft.Extensions.Configuration;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    public class CatogoryOrganizationQueryHandler : IRequestHandler<CatogoryOrganizationQuery, ResponseResult>
    {
        OrgUnitOfWork unitOfWork;
        CSRedisClient cSRedis;
        IConfiguration _config;
        const int time = 60 * 60;//cache timeout

        public CatogoryOrganizationQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient cSRedis, IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.cSRedis = cSRedis;
            _config = config;
        }



        public async Task<ResponseResult> Handle(CatogoryOrganizationQuery request, CancellationToken cancellationToken)
        {
            OrganizationAllResponse data = null;
            string key = string.Format(CacheKeys.CatogoryOrgList, request.CatogoryId, request.Authentication, request.PageInfo.PageIndex + "&" + request.PageInfo.PageSize);
            if (string.IsNullOrEmpty(request.SearchText))
            {
                 data = cSRedis.Get<OrganizationAllResponse>(key);
            }

       
            // 机构大全没内容需要显示的qrcode
            string qrcode = null;
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), _config[$"AppSettings:hd2:hd_tile_qrcode"]);
                var bys = await File.ReadAllBytesAsync(path);
                qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }

            if (data != null)
            {
                data.Qrcode = qrcode;
                return ResponseResult.Success(data);
            }
            else
            {

                data = CatogryTypeList(request);
                if(string.IsNullOrEmpty(request.SearchText))
                cSRedis.Set(key, data, time);
                data.Qrcode = qrcode;
                return ResponseResult.Success(data);
            }
        }

        public OrganizationAllResponse CatogryTypeList(CatogoryOrganizationQuery request)
        {
            OrganizationAllResponse data;
            #region Where
            var dy = new DynamicParameters();
            string sqlWhere = "";
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                dy.Add("@SearchText", $"%{request.SearchText}%");
                sqlWhere += $"  and o.name like @SearchText ";
            }
            else {

                if (request.CatogoryLevel == 2)
                {
                    var children = unitOfWork.DbConnection.Query<string>($"SELECT[key] from KeyValue where type = 16 and parent =@parent", new { parent = request.CatogoryId });
                    dy.Add("@CatogoryIds", children);
                    sqlWhere += $"  and AA.[catogryid] in @CatogoryIds  ";

                }
                else if (request.CatogoryLevel == 3)
                {
                    dy.Add("@CatogoryId", request.CatogoryId);
                    sqlWhere += $"  and AA.[catogryid]=@CatogoryId  ";
                }
            }


            //认证
            if (request.Authentication != null)
            {
                dy.Add("@authentication", request.Authentication);
                sqlWhere += $"  and o.authentication=@authentication     ";
            }
            #endregion

            dy.Add("@PageIndex", request.PageInfo.PageIndex);
            dy.Add("@PageSize", request.PageInfo.PageSize);

            string sql = $@"select top {request.PageInfo.PageSize} *  from 
                            (                               
                                select ROW_NUMBER() over(order by t.id desc) rownumber,* from
                                (
                                 select distinct o.id, o.name, o.logo, o.authentication,o.no
                                from [dbo].[Organization] o 
                                left join (SELECT id, value AS [catogryid] FROM [Organization]CROSS APPLY OPENJSON([BrandTypes])) AA on o.id=AA.id
                                where o.IsValid=1  and o.status=1
                                {sqlWhere}
                                )t
                            )TT where rownumber>(@PageSize*(@PageIndex-1))
                             ";
            string sqlPage = $@" 
                                select
                                COUNT(1) AS TotalCount from
                                (
                                     select distinct o.id, o.name, o.logo, o.authentication,o.no
                                from [dbo].[Organization] o 
                                left join (SELECT id, value AS [catogryid] FROM [Organization]CROSS APPLY OPENJSON([BrandTypes])) AA on o.id=AA.id
                                where o.IsValid=1  and o.status=1
                                {sqlWhere}
                                 )T1 
                                ;";
            data = new OrganizationAllResponse();
            data.OrganizationDatas = new List<OrganizationData>();
            data.OrganizationDatas = unitOfWork.Query<OrganizationData>(sql, dy).ToList();
            for (int i = 0; i < data.OrganizationDatas.Count; i++)
            {
                data.OrganizationDatas[i].No = UrlShortIdUtil.Long2Base32(Convert.ToInt64(data.OrganizationDatas[i].No));
            }
            data.PageInfo = new PageInfoResult();
            data.PageInfo = unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();
            data.PageInfo.PageIndex = request.PageInfo.PageIndex;
            data.PageInfo.PageSize = request.PageInfo.PageSize;
            data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);


            return data;
        }
    }
}
