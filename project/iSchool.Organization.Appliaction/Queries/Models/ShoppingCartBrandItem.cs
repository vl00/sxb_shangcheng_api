using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Queries.Models
{
    public class ShoppingCartBrandItem
    {
        public Guid BrandId { get; set; }

        public IEnumerable<ShoppingCartSKUItem>  ShoppingCartSKUItems { get; set; }
    }
}
