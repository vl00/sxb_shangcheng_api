using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Material
{
    public class AddorEditMaterialCommandHandler : IRequestHandler<AddorEditMaterialCommand, Res2Result<Guid>>
    {
        OrgUnitOfWork _orgUnitOfWork;

        public AddorEditMaterialCommandHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<Res2Result<Guid>> Handle(AddorEditMaterialCommand cmd, CancellationToken cancellation)
        {
            var isUpdate = cmd.Id != null && cmd.Id != default(Guid);

            if (cmd.CourseId == default)
            {
                throw new CustomResponseException("课程不能为空", 400);
            }

            var material = new MaterialLibrary();
            if (isUpdate)
            {
                material = await _orgUnitOfWork.DbConnection.GetAsync<MaterialLibrary>(cmd.Id);
                if (material == null || !material.IsValid)
                    throw new CustomResponseException("查询不到该素材圈");
            }

            material.Modifier = cmd.Userid;
            material.ModifyDateTime = DateTime.Now;
            material.pictures = JsonSerializationHelper.Serialize(cmd.Pictures ?? new List<string>());
            material.thumbnails = JsonSerializationHelper.Serialize(cmd.Thumbnails ?? new List<string>());
            material.Title = cmd.Title;
            material.video = cmd.Video;
            material.videoCover = cmd.VideoCover;
            material.Content = cmd.Content;
            material.CourseId = cmd.CourseId;
            material.IsValid = true;

            if (isUpdate)
            {
                await _orgUnitOfWork.DbConnection.UpdateAsync(material);
            }
            else
            {
                material.Id = Guid.NewGuid();
                material.Status = 1;
                material.CreateTime = DateTime.Now;
                material.Creator = Guid.Parse("D7CEBA85-8D0A-684B-855A-DAC873DAF9BF");  // 固定由他发
                material.DownloadTime = 0;
                await _orgUnitOfWork.DbConnection.InsertAsync(material);
            }

            return Res2Result.Success(material.Id);
        }
    }
}
