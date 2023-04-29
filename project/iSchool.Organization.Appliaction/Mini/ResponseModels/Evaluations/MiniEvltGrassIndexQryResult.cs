using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class MiniEvltGrassIndexQryResult //: IPageMeInfo
    {
        /// <summary>分页信息</summary>
        public PagedList<MiniEvaluationItemDto> PageInfo { get; set; } = default!;

        /// <summary>用户(我)信息</summary>
        public IUserInfo? Me { get; set; }

        /// <summary>排序</summary>
        public IEnumerable<KeyValuePair<string, string>> Orderbys { get; set; } = default!;
        /// <summary>学科</summary>
        public IEnumerable<KeyValuePair<string, string>> Subjs { get; set; } = default!;
        /// <summary>内容形式</summary>
        public IEnumerable<KeyValuePair<string, string>> Ctts { get; set; } = default!;
        /// <summary>品牌</summary>
        public IEnumerable<KeyValuePair<string, string>> Brands { get; set; } = default!;
    }
}
#nullable disable
