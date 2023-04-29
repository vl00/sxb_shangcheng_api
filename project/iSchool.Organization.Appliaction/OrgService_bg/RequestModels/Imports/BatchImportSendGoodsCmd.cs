using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Imports
{
    public class BatchImportSendGoodsCmd : IRequest<BatchImportSendGoodsCmdResult>
    {
        public Stream ExcelStream { get; set; }
        public Guid UserId { get; set; }
    }
}
