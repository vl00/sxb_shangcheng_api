using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{
    public class ExcelImportOrderOutdepotStatusCommand : IRequest<ResponseResult>
    {
        public Stream Excel { get; set; }
        public Guid UserId { get; set; }
    }
}
