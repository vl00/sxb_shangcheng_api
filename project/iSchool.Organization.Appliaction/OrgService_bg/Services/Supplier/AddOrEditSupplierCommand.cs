using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services.Supplier
{
    public class AddOrEditSupplierCommand : IRequest<ResponseResult>
    { /// <summary>
      /// 供应商Id
      /// </summary>
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        /// <summary>
        /// 是否新增
        /// </summary>
        public bool IsAdd { get; set; } = false;

        public string Name { get; set; }
        public string BankCardNo { get; set; }
        public string BankAddress { get; set; }
        public string CompanyName { get; set; }
        public bool IsPrivate { get; set; }

        /// <summary>
        /// 结算方式
        /// </summary>
        public int BillingType { get; set; }

        public List<ReturnAddressModel> ReturnAddress { get; set; }


        public List<Guid> OrgIds { get; set; }

        public class ReturnAddressModel {
            public Guid? Id { get; set; }
            public string Addr { get; set; }
            public string Receiver { get; set; }
            public string Phone { get; set; }
        }
    }
}
