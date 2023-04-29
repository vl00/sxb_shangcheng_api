using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{    
    /// <summary>
    /// 批量导入更新订单信息
    /// </summary>
    public class ExcelImportOrderInfoCommand : IRequest<bool>
    {
     
        public Stream Excel { get; set; }
        public Guid UserId { get; set; }
    }
}
