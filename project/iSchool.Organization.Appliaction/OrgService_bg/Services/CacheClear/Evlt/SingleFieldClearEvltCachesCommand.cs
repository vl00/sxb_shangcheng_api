using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 评测缓存清除--评测小更新(不涉及专题、机构、课程的变更)通用方法
    /// 1、可用于新增
    /// 2、不涉及到专题、机构、课程Id的切换（大编辑涉及）
    /// </summary>
    public class SingleFieldClearEvltCachesCommand : IRequest
    {
        /// <summary>评测id</summary>
        public Guid Id { get; set; } = default!;
      
    }
}
