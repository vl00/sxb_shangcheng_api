using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    public class UpdateSendBackAddressCommand:IRequest<bool>
    {
        public Guid OrderRefundId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string SendBackAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string SendBackUserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string SendBackMobile{ get; set; }

        public Guid Auditor { get; set; }

    }
}
