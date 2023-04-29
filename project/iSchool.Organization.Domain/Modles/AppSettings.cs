using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Modles
{
    public class AppSettings
    {
        public static bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        public string CreateQRCodeUrl { get; set; }

        /// <summary>
        /// 获取二维码
        /// </summary>
        public string GetQRCodeUrl { get; set; }

        /// <summary>
        /// 微信服务号Token(微信服务号Token( 正式："weixin_access_token_fwh"  测试"weixin_access_token_gamut",)
        /// </summary>
        public string WXServiceNumberToken { get; set; }

        /// <summary>
        /// 获取AccessToken的api地址
        /// </summary>
        public string AccessTokenApi { get; set; }

        /// <summary>
        /// 课程详情url（订阅后推送的查看详情url）
        /// </summary>
        public string WXCourseDetialUrl { get; set; }

        /// <summary>
        /// 订阅模板消息Id
        /// </summary>
        public string TemplateId { get; set; }
        /// <summary>
        /// 课程购买订阅模板消息Id
        /// </summary>
        public string CourseBookkWechatTemplateId { get; set; }

        /// <summary>
        /// 机构详情底图Url
        /// </summary>
        public string OrgBaseMap { get; set; }

        /// <summary>
        /// 活动1测评详情页
        /// </summary>
        public string ActEvltDetails { get; set; }

        /// <summary>
        /// 微信客服消息api
        /// </summary>
        public string CustomerServiceAPI { get; set; }

        /// <summary>
        /// 微信客服消息--素材Id
        /// </summary>
        public string Media_Id { get; set; }


        /// <summary>
        /// UV Api(用于获取评测的UV)
        /// </summary>
        public string UVApi { get; set; }
        /// <summary>
        /// 分销中心请求地址
        /// </summary>

        public string DrpfxBaseUrl { get; set; }
    }
}
