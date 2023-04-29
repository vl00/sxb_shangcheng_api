using iSchool.Domain.Repository.Interfaces;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using iSchool.Infrastructure.Extensions;
using iSchool.Infrastructure.Dapper;

namespace iSchool.Organization.Appliaction.Mini.Services.GrowUp
{
    public class MiniGrowUpDataQueryHandler : IRequestHandler<MiniGrowUpDataQuery, MiniGrowUpDataDto>
    {

        private readonly IUserInfo me;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Course> _courseRepository;

        private readonly IMediator _mediator;
        private readonly OrgUnitOfWork _unitOfWork;

        public MiniGrowUpDataQueryHandler(IUserInfo me,
            IRepository<Order> orderRepository, IRepository<Course> courseRepository,
            IMediator mediator, IOrgUnitOfWork unitOfWork)
        {
            this.me = me;
            _orderRepository = orderRepository;
            _courseRepository = courseRepository;
            _mediator = mediator;
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;

        }

        public async Task<MiniGrowUpDataDto> Handle(MiniGrowUpDataQuery request, CancellationToken cancellationToken)
        {

            MiniGrowUpDataDto dto = new MiniGrowUpDataDto();
            //孩子档案
            var childArchives = await _mediator.Send(new MiniChildArchivesQuery());
            dto.ChildArchives = childArchives;
            //我的精选课程

            var courseSql = @"SELECT course.*,org.*,[order].* FROM dbo.[Order] AS [order]
                                 LEFT JOIN dbo.Course AS course ON course.id = [order].courseid
                                 LEFT JOIN dbo.Organization AS org ON course.orgid=org.id
                                 WHERE [order].IsValid=1  
                                 AND [order].userid=@userid 
                                AND[order].status IN @status AND course.type=1
                                AND[order].appointmentStatus < @appointmentStatus 
                                ORDER BY [order].CreateTime DESC";


            var courseList = _unitOfWork.Query<Course, Domain.Organization, Order, (MiniCourseItemDto course, Guid OrderId)>
                     (courseSql, (course, org, order) =>
                     {
                         var banners = course.Banner_s ?? course.Banner;
                         var courseDto = new MiniCourseItemDto()
                         {
                             Id = course.Id,
                             Id_s = UrlShortIdUtil.Long2Base32(course.No),
                             Authentication = org.Authentication,
                             Banner = string.IsNullOrEmpty(banners)
                             ? new List<string>() :
                             JsonSerializationHelper.JSONToObject<List<string>>(banners),
                             Name = course.Name,
                             OrgName = org.Name,
                             OrigPrice = course.Origprice,
                             Price = course.Price,
                             Title = course.Title,
                             Logo = org.Logo,
                             Tags = new List<string>()
                         };

                         return (courseDto, order.Id);
                     }, new
                     {
                         userid = me.UserId,
                         status = new int[] {
                           (int)OrderStatusV2.Shipped,
                           (int)OrderStatusV2.Completed
                         },
                         appointmentStatus = (int)BookingCourseStatusEnum.Finished
                     }, splitOn: "id,id,id").ToList();
            dto.MyCourses = courseList;

            //我的种草圈(默认第一页)
            var evalList = new List<MiniEvaluationItemDto>();
            var evaltCountSql = @"SELECT COUNT(1) FROM dbo.Evaluation AS evalt  
                WHERE evalt.IsValid=1 AND  evalt.userid=@userid and status=1";
            //总数
            var count = _unitOfWork.QueryFirstOrDefault<int>(evaltCountSql, new { userid = me.UserId });

            if (count != 0)
            {
                //查询我的种草圈第一页
                var evaltSql = $@"SELECT evlts.*,item.* FROM (
                SELECT  TOP {request.PageSize} * FROM dbo.Evaluation WHERE IsValid=1
                AND status=1 AND userid=@userid ORDER BY CreateTime DESC) AS evlts
                LEFT JOIN  dbo.EvaluationItem AS item ON evlts.id=item.evaluationid AND item.IsValid=1
                 ORDER BY evlts.CreateTime DESC,item.type
                ";

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
                            //LikeCount = evalt.Likes,
                            SharedCount = evalt.SharedTime ?? 0,
                            Stick = evalt.Stick,
                            Title = evalt.Title,
                            VideoCoverUrl = item?.VideoCover,
                            VideoUrl = item?.Video,
                            Cover = evalt.Cover
                        });
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
                    userid = me.UserId
                }, splitOn: "id,id");
            }

            dto.Evaluations = evalList.ToPagedList(request.PageSize, 1, count);

            // 查用户信息
            if (dto.Evaluations.CurrentPageItems.Any())
            {
                var uInfos = await _mediator.Send(new UserSimpleInfoQuery
                {
                    UserIds = dto.Evaluations.CurrentPageItems.Select(_ => _.AuthorId)
                });
                var likes = await _mediator.Send(new EvltLikesQuery { EvltIds = evalList.Select(p => p.Id).ToArray() });

                foreach (var item in dto.Evaluations.CurrentPageItems)
                {
                    var userInfo = uInfos.FirstOrDefault(u => u.Id == item.AuthorId);
                    item.AuthorName = userInfo?.Nickname;
                    item.AuthorHeadImg = userInfo?.HeadImgUrl;
                    if (likes.Items.TryGetValue(item.Id, out var lk))
                    {
                        item.LikeCount = lk.Likecount;
                        item.IsLikeByMe = lk.IsLikeByMe;
                    }
                }
            }

            // 关联主体
            if (evalList.Any())
            {
                var rr = await _mediator.Send(new GetEvltRelatedsQueryArgs { EvltIds = evalList.Select(_ => _.Id).ToArray() });
                foreach (var item in evalList)
                {
                    item.RelatedMode = (int)EvltRelatedModeEnum.Other;
                    if (!rr.TryGetOne(out var x, _ => _.EvltId == item.Id)) continue;
                    item.RelatedMode = x.RelatedMode;
                    item.RelatedCourses = x.RelatedCourses;
                    item.RelatedOrgs = x.RelatedOrgs;
                }
            }

            return dto;
        }
    }
}
