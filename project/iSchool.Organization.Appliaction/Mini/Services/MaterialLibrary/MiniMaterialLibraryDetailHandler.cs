using CSRedis;
using Dapper;
using iSchool.Domain.Enum;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.Mini.ResponseModels.MaterialLibrary;
using iSchool.Organization.Appliaction.Mini.Services.Courses;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    ///  素材圈
    /// </summary>
    public class MiniMaterialLibraryDetailHandler : IRequestHandler<MiniMaterialLibraryDetailQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        private readonly IMediator _mediator;
        public MiniMaterialLibraryDetailHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IMediator mediator)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(MiniMaterialLibraryDetailQuery request, CancellationToken cancellationToken)
        {

            await Task.CompletedTask;
            var item = new MiniMaterialLibraryItemDto();
            string sql = $"SELECT * from MaterialLibrary where id=@Id and IsValid=1 and Status=1; ";

            var dBData = (await _unitOfWork.QueryAsync<MaterialLibraryDataDB>(sql, new { Id= request.Id})).FirstOrDefault();
            if (dBData != null)
            {
                var uInfos = await _mediator.Send(new UserSimpleInfoQuery
                {
                    UserIds = new List<Guid>() { dBData.Creator }
                }); ;
                if (uInfos.TryGetOne(out var u, _ => _.Id == dBData.Creator))
                {
                    item.Id = dBData.Id;
                    item.AuthorName = u.Nickname;
                    item.AuthorHeadImg = u.HeadImgUrl;
                    item.AuthorId = dBData.Creator;
                    item.Content = dBData.Content;
                    item.CourseId = dBData.CourseId;
                    item.CreateTime = dBData.CreateTime.UnixTicks();
                    item.DownloadTime = dBData.DownloadTime;
                    if (!dBData.pictures.IsNullOrEmpty())
                        item.Imgs = JsonConvert.DeserializeObject<List<string>>(dBData.pictures);
                    if (!dBData.thumbnails.IsNullOrEmpty())
                        item.Imgs_s = JsonConvert.DeserializeObject<List<string>>(dBData.thumbnails);

                    item.VideoCoverUrl = dBData.videoCover;
                    item.VideoUrl = dBData.video;
                    item.Title = dBData.Title;



                }

            }
            return ResponseResult.Success(item);
        }


    }
}
