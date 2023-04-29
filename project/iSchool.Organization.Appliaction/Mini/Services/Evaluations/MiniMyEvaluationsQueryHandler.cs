using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Mini.Services.Evaluations
{
    public class MiniMyEvaluationsQueryHandler : IRequestHandler<MiniMyEvaluationsQuery, MiniMyEvaluationsDto>
    {
        private readonly IUserInfo me;
        private readonly IMediator _mediator;
        private readonly OrgUnitOfWork _unitOfWork;

        public MiniMyEvaluationsQueryHandler(IOrgUnitOfWork unitOfWork, IUserInfo me, IMediator mediator)
        {
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            _mediator = mediator;
        }

        public async Task<MiniMyEvaluationsDto> Handle(MiniMyEvaluationsQuery request, CancellationToken cancellationToken)
        {
            var orderby = new StringBuilder();
            if (request.OrderBy == 1)
            {
                //按照点赞数
                orderby.Append("(evlts.likes+isnull(evlts.Shamlikes,0)) desc ");
            }
            else if (request.OrderBy == 2)
            {
                //按照分享数
                orderby.Append("evlts.SharedTime desc ");
            }
            else
            {
                //默认 按照时间
                orderby.Append("evlts.CreateTime desc ");
            }

            var evaltSql = $@"SELECT evlts.*,item.* FROM (
                SELECT * FROM dbo.Evaluation evlts WHERE IsValid=1
                AND status=1 AND userid=@userid order by {orderby} OFFSET @offset ROWS FETCH NEXT @next ROWs ONLY) AS evlts
                LEFT JOIN  dbo.EvaluationItem AS item ON evlts.id=item.evaluationid AND item.IsValid=1
                 ORDER BY {orderby},item.type
                ";

            var evaltCountSql = @"SELECT COUNT(1) FROM dbo.Evaluation AS evalt  
                WHERE evalt.IsValid=1 AND  evalt.userid=@userid and status=1";

            var evalList = new List<MiniEvaluationItemDto>();

            // 总数
            var count = _unitOfWork.QueryFirstOrDefault<int>(evaltCountSql, new { userid = me.UserId });
            if (count == 0)
                return new MiniMyEvaluationsDto
                {
                    PageInfo = evalList.ToPagedList(request.PageSize, request.PageIndex, 0)
                };

            _unitOfWork.Query<Evaluation, EvaluationItem, bool>(evaltSql, (evalt, item) =>
            {
                //如果是存在的情况下补充旧数据
                var evalDto = evalList.FirstOrDefault(p => p.Id == evalt.Id);
                if (evalDto == null)
                {
                    evalList.Add(new MiniEvaluationItemDto
                    {
                        Id = evalt.Id,
                        Id_s = UrlShortIdUtil.Long2Base32(evalt.No),
                        AuthorHeadImg = me.HeadImg,
                        AuthorId = me.UserId,
                        AuthorName = me.UserName,
                        Content = item?.Content,
                        CreateTime = evalt.CreateTime,
                        Imgs = string.IsNullOrEmpty(item?.Pictures) ? new List<string> { }
                        : JsonSerializationHelper.JSONToObject<IEnumerable<string>>(item.Pictures),
                        Imgs_s = string.IsNullOrEmpty(item?.Thumbnails) ? new List<string> { }
                         : JsonSerializationHelper.JSONToObject<IEnumerable<string>>(item.Thumbnails),
                        LikeCount = evalt.Likes,
                        SharedCount = evalt.SharedTime ?? 0,
                        Stick = evalt.Stick,
                        Title = evalt.Title,
                        VideoCoverUrl = item?.VideoCover,
                        VideoUrl = item?.Video
                    });
                }
                else
                {
                    //如果是存在的情况下补充旧数据
                    if (!string.IsNullOrEmpty(item?.Content))
                    {
                        evalDto.Content += $"\n{item?.Content}";
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
                userid = me.UserId,
                offset = (request.PageIndex - 1) * request.PageSize,
                next = request.PageSize
            }, splitOn: "id,id");

            //获取点赞数
            var likes = await _mediator.Send(new EvltLikesQuery { EvltIds = evalList.Select(p => p.Id).ToArray() });
            evalList.ForEach(p =>
            {
                if (likes.Items.TryGetValue(p.Id, out var lk))
                {
                    p.LikeCount = lk.Likecount;
                    p.IsLikeByMe = lk.IsLikeByMe;
                }
            });


            var result = new MiniMyEvaluationsDto
            {
                PageInfo = evalList.ToPagedList(request.PageSize, request.PageIndex, count)
            };


            // 查用户信息
            if (result.PageInfo.CurrentPageItems.Any())
            {
                var uInfos = await _mediator.Send(new UserSimpleInfoQuery
                {
                    UserIds = result.PageInfo.CurrentPageItems.Select(_ => _.AuthorId)
                });
                foreach (var item in result.PageInfo.CurrentPageItems)
                {
                    if (!uInfos.TryGetOne(out var u, _ => _.Id == item.AuthorId)) continue;
                    item.AuthorName = u.Nickname;
                    item.AuthorHeadImg = u.HeadImgUrl;
                }
            }

            // 关联主体
            if (evalList.Count > 0)
            {
                var rr = await _mediator.Send(new GetEvltRelatedsQueryArgs { EvltIds = evalList.Select(_ => _.Id).ToArray() });
                foreach (var item in result.PageInfo.CurrentPageItems)
                {
                    item.RelatedMode = (int)EvltRelatedModeEnum.Other;
                    if (!rr.TryGetOne(out var x, _ => _.EvltId == item.Id)) continue;
                    item.RelatedMode = x.RelatedMode;
                    item.RelatedCourses = x.RelatedCourses;
                    item.RelatedOrgs = x.RelatedOrgs;
                }
            }

            return result;
        }
    }
}
