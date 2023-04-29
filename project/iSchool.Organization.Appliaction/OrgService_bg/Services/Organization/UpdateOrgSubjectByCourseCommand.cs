using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 根据机构下的课程变更，实时更新机构的科目
    /// </summary>
    public class UpdateOrgSubjectByCourseCommand:IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid? OrgId { get; set; }

        /// <summary>
        /// 操作类型（1：新增/上架；2：编辑；3：下架）
        /// </summary>
        public int OperationType { get; set; }

        /// <summary>
        /// 所有操作传入需变更的科目
        /// </summary>
        public int? NewSubject { get; set; }

        /// <summary>
        /// 编辑时，需传入编辑前的科目
        /// </summary>
        public int? OldSubject { get; set; } = null;

        /// <summary>
        /// 编辑时，需传入编辑前的机构
        /// </summary>
        public Guid? OldOrgId { get; set; } = null;

    }
}
