using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Mini.Services.Evaluations
{
    class MiniEvaluationSearchQueryHandler : IRequestHandler<MiniEvaluationSearchQuery, List<MiniEvaluationItemDto>>
    {
        private readonly OrgUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public MiniEvaluationSearchQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public async Task<List<MiniEvaluationItemDto>> Handle(MiniEvaluationSearchQuery request, CancellationToken cancellationToken)
        {

            var evalList = new List<MiniEvaluationItemDto>();
            var result = new List<MiniEvaluationItemDto>();

            var evaltSql = @"
                SELECT evalt.*,item.* FROM dbo.Evaluation AS evalt 
                LEFT JOIN dbo.EvaluationItem  item  ON evalt.id=item.evaluationid AND item.IsValid=1
                WHERE evalt.IsValid=1 AND  evalt.status=1 AND evalt.id IN @ids
                order by CHARINDEX(','+ltrim(evalt.id)+',',@idStr)";

            _unitOfWork.Query<Evaluation, EvaluationItem, bool>(evaltSql, (evalt, item) =>
            {
                var evalDto = evalList.FirstOrDefault(p => p.Id == evalt.Id);
                if (evalDto == null)
                {
                    //不存在情况下
                    var dto = new MiniEvaluationItemDto
                    {
                        Id = evalt.Id,
                        Id_s = UrlShortIdUtil.Long2Base32(evalt.No),
                        Content = item?.Content,
                        CreateTime = evalt.CreateTime,
                        Imgs = string.IsNullOrEmpty(item?.Pictures) ? new List<string> { }
                          : JsonSerializationHelper.JSONToObject<IEnumerable<string>>(item.Pictures),
                        Imgs_s = string.IsNullOrEmpty(item?.Thumbnails) ? new List<string> { }
                           : JsonSerializationHelper.JSONToObject<IEnumerable<string>>(item.Thumbnails),
                        //LikeCount = evalt.Likes,
                        SharedCount = evalt.SharedTime ?? 0,
                        Stick = evalt.Stick,
                        Title = evalt.Title,
                        VideoCoverUrl = item?.VideoCover,
                        VideoUrl = item?.Video,
                        AuthorId = evalt.Userid
                    };
                    evalList.Add(dto);
                }
                else
                {
                    //如果是存在的情况下补充旧数据
                    if (!string.IsNullOrEmpty(item?.Content))
                    {
                        evalDto.Content += $"\n{item.Content}";
                    }
                    if (!string.IsNullOrEmpty(item?.Pictures))
                    {
                        var pic = JsonSerializationHelper.JSONToObject<IEnumerable<string>>(item.Pictures);
                        evalDto.Imgs = evalDto.Imgs.Union(pic);
                    }
                    if (!string.IsNullOrEmpty(item?.Thumbnails))
                    {
                        var thum = JsonSerializationHelper.JSONToObject<IEnumerable<string>>(item.Thumbnails);
                        evalDto.Imgs_s = evalDto.Imgs_s.Union(thum);
                    }
                }
                return true;
            }, new
            {
                ids = request.Ids,
                idStr = $",{string.Join(',', request.Ids)},"
            }, splitOn: "id,id").ToList();


            //查询用户信息 进行赋值
            var useIds = evalList.Select(p => p.AuthorId).Distinct();
            var uInfos = await _mediator.Send(new UserSimpleInfoQuery
            {
                UserIds = useIds
            });

            //获取点赞数
            var likes = await _mediator.Send(new EvltLikesQuery { EvltIds = request.Ids.ToArray() });

            evalList.ForEach(p =>
            {
                var userInfo = uInfos.FirstOrDefault(u => u.Id == p.AuthorId);
                p.AuthorName = userInfo?.Nickname;
                p.AuthorHeadImg = userInfo?.HeadImgUrl;
                if (likes.Items.TryGetValue(p.Id, out var lk))
                {
                    p.LikeCount = lk.Likecount;
                    p.IsLikeByMe = lk.IsLikeByMe;
                }

            });

            return evalList;
        }
    }
}
