using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class MiniUpdateChildArchiveCommand:IRequest<ResponseResult>
    {
        /// <summary>
        /// 孩子档案id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 孩子头像
        /// </summary>
        public string HeadImg { get; set; }

        /// <summary>
        /// 性别 1.男  0.女
        /// </summary>
        public byte Sex { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string NikeName { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// 科目
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Subjs { get; set; }
    }
}
