using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;

namespace iSchool.Organization.Appliaction.Mini.Services.Courses
{
    public class MiniMaterialLibraryDetailQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 素材ID
        /// </summary>
        public Guid Id { get; set; }
    }
}
