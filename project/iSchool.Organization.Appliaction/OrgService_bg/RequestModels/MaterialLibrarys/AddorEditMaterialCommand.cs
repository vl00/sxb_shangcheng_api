using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class AddorEditMaterialCommand : IRequest<Res2Result<Guid>>
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }

        public string Content { get; set; }

        public Guid CourseId { get; set; }

        public List<string> Thumbnails { get; set; }

        public List<string> Pictures { get; set; }

        public string Video { get; set; }


        public string VideoCover { get; set; }

        public Guid? Userid { get; set; } = null;
    }
}
