using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Supplier
{
    public class SupplierReturnAddress
    {
        public Guid? Id { get; set; }
        public Guid SupplierId { get; set; }
        public string Addr { get; set; }
        public string Receiver { get; set; }
        public string Phone { get; set; }
        public int Sort { get; set; }
		
	}
}
