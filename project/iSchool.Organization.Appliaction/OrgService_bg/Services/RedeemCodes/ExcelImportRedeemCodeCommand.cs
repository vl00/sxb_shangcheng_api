using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.RedeemCodes
{    
    public class ExcelImportRedeemCodeCommand : IRequest<bool>
    {
        public Guid CourseId { get; set; }
        public Stream Excel { get; set; }
        public Guid UserId { get; set; }
    }
}
