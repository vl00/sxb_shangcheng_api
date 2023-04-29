using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Supplier
{
    public class SupplierItem
    {
        public int RowNum { get; set; }
        public Guid Id { get; set; }
        public string Name{ get; set; }
        public string BankCardNo { get; set; }
        public string BankAddress { get; set; }
        public string CompanyName { get; set; }
        public string IsPrivate { get; set; }
        public string BillingType { get; set; }
        public string ReturnAddress { get; set; }
        public int ReturnAddressCount { get; set; }
        public string OrgName { get; set; }
    }
}
