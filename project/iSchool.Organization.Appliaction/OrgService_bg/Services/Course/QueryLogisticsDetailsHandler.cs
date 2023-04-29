using CSRedis;
using Dapper;
using Enyim.Caching;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Courses;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 物流详情
    /// </summary>
    public class QueryLogisticsDetailsHandler : IRequestHandler<QueryLogisticsDetails, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IHttpClientFactory httpClientFactory;
        AppSettings appSettings;

        public QueryLogisticsDetailsHandler(IOrgUnitOfWork unitOfWork,IWXUnitOfWork wXUnitOfWork           
            , IHttpClientFactory httpClientFactory
            , IOptions<AppSettings> options           
            )
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.httpClientFactory = httpClientFactory;
            this.appSettings = options.Value;
        }


        public async Task<ResponseResult> Handle(QueryLogisticsDetails request, CancellationToken cancellationToken)
        {
           
            try
            {
                #region 通过雄哥api获取物流信息 
                using var httpClient = httpClientFactory.CreateClient(string.Empty);
                var getUrl = request.LogisticeApi.FormatWith(request.LogisticeCode, "");//{1=}&com={1}&customer={2}
                var res_loginstics = await httpClient.GetAsync(getUrl);
                res_loginstics.EnsureSuccessStatusCode();             
                var r_kuaidi = (await res_loginstics.Content.ReadAsStringAsync()).ToObject<KuaidiApiResult>(true);                
                var kuaidi = new LogisticeInfo();
                if (r_kuaidi.status==200 && r_kuaidi.data.Errcode == 0)
                {
                    var data = r_kuaidi.data;
                    kuaidi.DHCode = "兑换码TODO";
                    kuaidi.LogName = "快递公司名称TODO";
                    kuaidi.LogNumber = data.Nu;
                    kuaidi.IsCompleted = data.IsCompleted;
                    if (data.Items.Any() == true)
                    {
                        var items =data.Items.Select(_=>new LogisticeInfoItem() { Desc=_.Desc, Time=Convert.ToDateTime(_.Time) });
                        kuaidi.Items = items.OrderByDescending(_ => _.Time).ToList();
                    }
                }
                else
                    return ResponseResult.Failed("第三方接口返回错误：" + r_kuaidi.data.Errmsg);
                #endregion
                return ResponseResult.Success(kuaidi);
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return ResponseResult.Failed(ex.Message);
            }
        }
    }


    public class KuaidiApiResult
    {
        public KuaidiNuDataDto data { get; set; }
        public bool success { get; set; }
        public int status { get; set; }
        public string msg { get; set; }
    }

}
