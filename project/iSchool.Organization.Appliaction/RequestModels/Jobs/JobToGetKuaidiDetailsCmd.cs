using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 定时获取未完成的快递单详情
    /// </summary>
    public class JobToGetKuaidiDetailsCmd : IRequest
    {
        public int Count { get; set; } = 10;

        public int Min { get; set; } = 120;

        public bool AllowUpSF { get; set; }
    }
}
