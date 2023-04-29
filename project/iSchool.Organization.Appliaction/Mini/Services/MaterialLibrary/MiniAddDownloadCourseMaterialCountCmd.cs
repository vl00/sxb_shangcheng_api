using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 增加下载素材数
    /// </summary>
    public class MiniAddDownloadCourseMaterialCountCmd : IRequest<bool>
    {
        /// <summary>
        /// 素材ID
        /// </summary>
        public Guid Id { get; set; }
    }

#nullable disable
}
