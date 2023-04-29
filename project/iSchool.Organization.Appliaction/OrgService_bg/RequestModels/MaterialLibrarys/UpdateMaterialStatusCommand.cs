using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class UpdateMaterialStatusCommand : IRequest<bool>
    {
        /// <summary>
        /// 素材id
        /// </summary>
        public Guid Id { get; set; }

        public byte Status { get; set; }


        public Guid Userid { get; set; }
    }
}
