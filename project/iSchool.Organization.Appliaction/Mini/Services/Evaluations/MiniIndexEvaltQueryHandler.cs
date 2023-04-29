using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
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

namespace iSchool.Organization.Appliaction.Services
{

    public class MiniIndexEvaltQueryHandler : IRequestHandler<MiniIndexEvaltQuery, MiniIndexEvalts>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;

        public MiniIndexEvaltQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _mediator = mediator;
        }

        public async Task<MiniIndexEvalts> Handle(MiniIndexEvaltQuery request, CancellationToken cancellationToken)
        {

            var result = new MiniIndexEvalts();

            var sql = @" SELECT eval.*,item.* FROM
                 (SELECT * FROM dbo.Evaluation WHERE IsValid=1 AND status=1 ORDER BY stick DESC,viewcount DESC 
                 OFFSET @offset ROWS FETCH NEXT @next ROWs ONLY) eval LEFT JOIN dbo.EvaluationItem AS item
                 ON item.evaluationid=eval.id AND item.IsValid=1  ORDER BY eval.stick DESC,eval.viewcount DESC,item.type";

            var countSql = "SELECT COUNT(1) FROM dbo.Evaluation WHERE IsValid = 1 AND status = 1 ";


            var evalList = new List<MiniEvaluationItemDto>();

            _orgUnitOfWork.Query<Evaluation, EvaluationItem, bool>(sql, (evalt, item) =>
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
                          LikeCount = evalt.Likes,
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
              { offset = (request.PageIndex - 1) * request.PageSize, next = request.PageSize }, splitOn: "id,id").ToList();
            //查询用户信息 进行赋值
            var useIds = evalList.Select(p => p.AuthorId).Distinct();
            var uInfos = await _mediator.Send(new UserSimpleInfoQuery
            {
                UserIds = useIds
            });

            //获取点赞数
            var likes = await _mediator.Send(new EvltLikesQuery
            {
                EvltIds = evalList.Select(p => p.Id).ToArray()
            });

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


            var count = _orgUnitOfWork.QueryFirstOrDefault<int>(countSql);

            result.PageInfo = evalList.ToPagedList(request.PageSize, request.PageIndex, count);


            return result;
        }
    }
}
