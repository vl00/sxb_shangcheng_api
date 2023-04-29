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
    /// 保存--专题关联评测
    /// </summary>
    public class SaveSpeEvltsCommand : IRequest<ResponseResult>
    {
        /// <summary> 专题Id </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 评测Id-(true:关联；false:取关)
        /// </summary>
        public Dictionary<Guid,bool> SpeEvltBings { get; set; }
    }
}
