using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class GetKuaidiDetailQuery : IRequest<KuaidiDetailDto>
    {
        /// <summary>收货人名字</summary> 
        public string RecvUsername { get; set; } = default!;
        /// <summary>收货人手机</summary> 
        public string RecvMobile { get; set; } = default!;
        /// <summary>收货地址</summary> 
        public string Address { get; set; } = default!;
        /// <summary>邮编</summary> 
        public string Postalcode { get; set; }
        /// <summary>省</summary> 
        public string Province { get; set; }
        /// <summary>市</summary> 
        public string City { get; set; }
        /// <summary>区</summary> 
        public string Area { get; set; }


        /// <summary>快递单号</summary> 
        public string ExpressCode { get; set; }
        /// <summary>快递公司编码</summary> 
        public string ExpressType { get; set; }

        public DateTime? SendExpressTime { get; set; }

    }
}
