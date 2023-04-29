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
    public class MiniMaterialLibraryListQueryHandler : IRequestHandler<MiniMaterialLibraryListQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        const int time = 60 * 60;
        private readonly IMediator _mediator;
        public MiniMaterialLibraryListQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IMediator mediator)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(MiniMaterialLibraryListQuery request, CancellationToken cancellationToken)
        {

            await Task.CompletedTask;
            var data = new MaterialLibraryQueryResponse();
            var dy = new DynamicParameters();
            #region Where
            string sqlWhere = $@" where Status=1 and IsValid=1 ";
            var sortFilter = "order by CreateTime desc";
            dy.Add("@PageIndex", request.PageIndex);
            dy.Add("@PageSize", request.PageSize);
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                sqlWhere += $" and  (Title like '%{request.SearchText}%' or Content  like '%{request.SearchText}%'  )";
            }
            #endregion

            string sql = $@"
                        select top {request.PageSize} * 
                        from(
                        	select ROW_NUMBER() over({sortFilter}) rownum ,* from MaterialLibrary
                            {sqlWhere} 
                        )TT where rownum> (@PageIndex-1)*@PageSize ;";
            string sqlPage = $@"
                            select COUNT(1) as TotalCount 
                            from [dbo].[MaterialLibrary] 

                            {sqlWhere} 
                            ;";

            data.MaterialLibraryDatas = new List<MiniMaterialLibraryItemDto>();
            var dBDatas = _unitOfWork.Query<MaterialLibraryDataDB>(sql, dy).ToList();
            if (dBDatas != null)
            {
                var uInfos = await _mediator.Send(new UserSimpleInfoQuery
                {
                    UserIds = dBDatas.Select(_ => _.Creator)
                });

                foreach (var dbData in dBDatas)
                {

                    var item = new MiniMaterialLibraryItemDto();
                    if (!uInfos.TryGetOne(out var u, _ => _.Id == dbData.Creator)) continue;
                    item.Id = dbData.Id;
                    item.AuthorName = u.Nickname;
                    item.AuthorHeadImg = u.HeadImgUrl;
                    item.AuthorId = dbData.Creator;
                    item.Content = dbData.Content;
                    item.CourseId = dbData.CourseId;
                    item.CreateTime = dbData.CreateTime.UnixTicks();
                    item.DownloadTime = dbData.DownloadTime;
                    if (!dbData.pictures.IsNullOrEmpty())
                        item.Imgs = JsonConvert.DeserializeObject<List<string>>(dbData.pictures);
                    if (!dbData.thumbnails.IsNullOrEmpty())
                        item.Imgs_s = JsonConvert.DeserializeObject<List<string>>(dbData.thumbnails);

                    item.VideoCoverUrl = dbData.videoCover;
                    item.VideoUrl = dbData.video;
                    item.Title = dbData.Title;
                    data.MaterialLibraryDatas.Add(item);


                }


                data.PageInfo = new PageInfoResult();
                data.PageInfo = _unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();
                data.PageInfo.PageIndex = request.PageIndex;
                data.PageInfo.PageSize = request.PageSize;
                data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);


            }
            return ResponseResult.Success(data);
        }
    }
}
