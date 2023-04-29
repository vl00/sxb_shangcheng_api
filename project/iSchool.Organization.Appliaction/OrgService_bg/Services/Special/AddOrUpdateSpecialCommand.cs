using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 新增/编辑专题
    /// </summary>
    public class AddOrUpdateSpecialCommand: IRequest<ResponseResult>
    {
        /// <summary>
        /// 专题Id
        /// </summary>
        public Guid Id { get; set; }

        public string StrSmallSpecialIds { get; set; }

        public IEnumerable<Guid> SmallSpecialIds { get; set; }

        /// <summary>
        /// 专题类型（1:小专题；2：大专题）
        /// </summary>
        public int SpecialType { get; set; } = 1;

        /// <summary>
        /// 操作类型(true：新增；false：保存)
        /// </summary>
        public bool IsAdd { get; set; } = true;

        /// <summary>
        /// 专题名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 专题副标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 分享标题
        /// </summary>
        public string ShareTitle { get; set; }

        /// <summary>
        /// 分享副标题
        /// </summary>
        public string ShareSubTitle { get; set; }

        /// <summary>
        /// 专题海报
        /// </summary>
        public string Banner { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public Guid? UserId { get; set; }
    }
}
