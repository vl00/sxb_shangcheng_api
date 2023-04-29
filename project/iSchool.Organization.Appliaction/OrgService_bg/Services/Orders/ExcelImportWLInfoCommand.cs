using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{    
    /// <summary>
    /// 批量导入更新订单的物流信息
    /// </summary>
    public class ExcelImportWLInfoCommand : IRequest<ResponseResult>
    {
     
        public Stream Excel { get; set; }
        public Guid UserId { get; set; }
    }
}
