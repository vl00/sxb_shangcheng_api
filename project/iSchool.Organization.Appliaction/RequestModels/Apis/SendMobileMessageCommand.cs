using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Apis
{
    public class SendMobileMessageCommand : IRequest<bool>
    {
        //nationCode 国家码，如 86 为中国
        public string NationCode { get; set; } = "86";
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 模板ID
        /// </summary>
        public int TemplateId { get; set; }
        /// <summary>
        /// 模板对应填充的内容
        /// </summary>
        public List<string> TempalteParam { get; set; }


    }
}
