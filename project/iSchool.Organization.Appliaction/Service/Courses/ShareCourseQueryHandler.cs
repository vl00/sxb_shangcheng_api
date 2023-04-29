using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Infrastructure.Extensions;
using System.IO;

namespace iSchool.Organization.Appliaction.Service.Course
{
    public class ShareCourseQueryHandler : IRequestHandler<ShareCourseQuery, ResponseResult>
    {
        AppSettings appSettings;
        OrgUnitOfWork _orgUnitOfWork;
        public ShareCourseQueryHandler(IOrgUnitOfWork unitOfWork, IOptions<AppSettings> options)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.appSettings = options.Value;
        }

        public  Task<ResponseResult> Handle(ShareCourseQuery request, CancellationToken cancellationToken)
        {
            string sql = $"select * from [dbo].[Course] where id=@id and IsValid=1  and status=1 and type={CourseTypeEnum.Course.ToInt()};";
            var data= _orgUnitOfWork.Query<Domain.Course>(sql, new DynamicParameters().Set("id", request.Id)).FirstOrDefault();
            if (data == null) throw new CustomResponseException("课程详情不存在");
            var DetailsUrl = appSettings.WXCourseDetialUrl.FormatWith(UrlShortIdUtil.Long2Base32(Convert.ToInt64(data.No)));
            var qRCode =  QRCodeHelper.GetLogoQRCode(UrlAddparms(DetailsUrl, "userid", request.FxHeaducode), Path.Combine("App_Data/images/iSchoollogo.png"), 5);

            ShareLinkDto dto = new ShareLinkDto()
            {
                Banner = data.Banner == null ? "" : JsonSerializationHelper.JSONToObject<List<string>>(data.Banner).FirstOrDefault(),
                Base64QRCode = qRCode,
                MainTitle = data.Title,
                SubTitle = data.Subtitle,
                UserHeadImg = request.UserInfo.HeadImg,
                Username = request.UserInfo.UserName
            };
            return Task.FromResult(ResponseResult.Success(dto));
        }

        static string UrlAddparms(string url, string k, object v)
        {
            if (v == null || Equals(v, "")) return url;

            if (k.IsNullOrEmpty()) k = "";
            else k += "=";

            if (url.IndexOf('?') > -1) return url + '&' + k + v;
            else return url + '?' + k + v;
        }
    }
}
