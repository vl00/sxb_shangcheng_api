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
    /// 增加种草下载素材数
    /// </summary>
    public class MiniAddDownloadMaterialCountCmd : IRequest<bool>
    {
        public Guid EvltId { get; set; }
    }

#nullable disable
}
