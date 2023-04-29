using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Supplier
{
    public class SupplierInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string BankCardNo { get; set; }
        public string BankAddress { get; set; }
        public string CompanyName { get; set; }
        public bool? IsPrivate { get; set; }

        public string ReturnAddress { get; set; }

        public int BillingType { get; set; }

        public List<Guid> OrgIds { get; set; }
    }
}
