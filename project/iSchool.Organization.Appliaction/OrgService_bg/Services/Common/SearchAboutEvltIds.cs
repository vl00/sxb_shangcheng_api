using iSchool.Organization.Appliaction.OrgService_bg.Common;
using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    /// <summary>
    /// 关于评测的其他Id(专题、机构、课程)
    /// </summary>
    public class SearchAboutEvltIds: IRequest<AboutEvltIds>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvltId { get; set; }
    }
}
