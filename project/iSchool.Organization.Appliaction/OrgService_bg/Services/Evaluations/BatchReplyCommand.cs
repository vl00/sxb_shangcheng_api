using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;


namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 批量更新回复内容
    /// </summary>
    public class BatchReplyCommand:IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvltId { get; set; }

        /// <summary>
        /// 待更新的回复集合
        /// </summary>
        public IEnumerable<ReplyUpdateModel> ListReplys { get; set; }
    }

    public class ReplyUpdateModel
    {
        /// <summary>
        /// 回复Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 修改后的回复内容
        /// </summary>
        public string Comment { get; set; }
    }

}
