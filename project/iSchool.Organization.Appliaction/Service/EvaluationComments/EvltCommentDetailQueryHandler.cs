using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class EvltCommentDetailQueryHandler : IRequestHandler<EvltCommentDetailQuery, EvltCommentDetailDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IConfiguration config;
        IMapper mapper;
        public EvltCommentDetailQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, CSRedisClient redis,
            IConfiguration config, IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.config = config;
            this.mapper = mapper;
        }

        public async Task<EvltCommentDetailDto> Handle(EvltCommentDetailQuery req, CancellationToken cancellation)
        {
            var dto = new EvltCommentDetailDto();
            var sql = $@"select id,userid,CreateTime,comment,username,likes,CommentCount,evaluationid
from  EvaluationComment
where  IsValid=1 and Id=@EvltCmtId ";
            dto = unitOfWork.Query<EvltCommentDetailDto>(sql, new DynamicParameters(req)).FirstOrDefault();
            if (null == dto)
            {
                throw new CustomResponseException("参数有误");
            }

            // mapper.Map(editModel, dto);
            var r = mediator.Send(new UserSimpleInfoQuery { UserIds = new List<Guid>() { dto.UserId } }).Result.FirstOrDefault();
            dto.Username = r.Nickname;
            dto.UserImg = r.HeadImgUrl ?? config["AppSettings:UserDefaultHeadImg"];
            dto.IsMy = dto.UserId == me.UserId ? true : false;
            var evaltModel = await unitOfWork.QueryFirstOrDefaultAsync<Evaluation>(@"select * from Evaluation where id = @id ", new { id = dto.EvaluationId });
            dto.IsAuthor = dto.UserId == evaltModel.Userid ? true : false;

            // like count + IsLikeByMe
            do
            {
                var lks = await mediator.Send(new EvltCommentLikesQuery { Ids = new[] { (dto.EvaluationId, dto.Id) } });
                if (!lks.Items.TryGetOne(out var v)) break;
                dto.Likes = v.Value.Likecount;
                dto.IsLikeByMe = v.Value.IsLikeByMe;

                #region old code
                //// like count
                //var rdk = CacheKeys.EvaluationCommentLikesCount.FormatWith(dto.EvaluationId);
                //var cc = await redis.HMGetAsync(rdk, dto.Id.ToString());

                //if (cc == null || cc.Length <= 0 || cc[0] == null)
                //{
                //    sql = @"select Id as Item1,likes as Item2 from EvaluationComment where IsValid=1 and id=@id";
                //    var likes = await unitOfWork.DbConnection.QueryFirstOrDefaultAsync<(Guid, int)>(sql, new { id = dto.Id });
                //    dto.Likes = likes.Item2;
                //    var pipe = redis.StartPipe();
                //    pipe.HSet(rdk, dto.Id.ToString(), dto.Likes);
                //    pipe.Expire(rdk, 60 * 60 * 6);
                //    await pipe.EndPipeAsync();
                //}
                //else
                //{
                //    var redisLikeCount = Convert.ToInt32(cc[0]);
                //    dto.Likes = redisLikeCount < 0 ? 0 : redisLikeCount;
                //}

                //// IsLikeByMe
                //if (me.IsAuthenticated)
                //{
                //    var rdk_likeme = CacheKeys.MyCommentLikes.FormatWith(me.UserId, dto.EvaluationId);
                //    var cc_likeme = await redis.HMGetAsync(rdk_likeme, dto.Id.ToString());

                //    if (cc_likeme == null || cc_likeme.Length <= 0 || cc_likeme[0] == null)
                //    {

                //        //                    sql = $@"select cmmt.id from EvaluationComment cmmt 
                //        //left join (select commentid from [Like] 
                //        //    where type={(2)} and  useid=@UserId and commentid=@id
                //        //    group by commentid
                //        //) lk on lk.commentid=cmmt.id
                //        //where cmmt.IsValid=1 and cmmt.id=@id and lk.commentid is not null
                //        //";
                //        //                    var likes_likeme = await unitOfWork.DbConnection.QueryFirstOrDefaultAsync<Guid>(sql, new { id = dto.Id, UserId = me.UserId, });
                //        //                    if (null != likes_likeme && Guid.Empty != likes_likeme)
                //        //                    {
                //        //                        dto.IsLikeByMe = true;
                //        //                    }


                //        //                    var pipe = redis.StartPipe();

                //        //                    pipe.HSet(rdk, dto.Id.ToString(), 1);
                //        //                    pipe.Expire(rdk, 60 * 60 * 6);
                //        //                    await pipe.EndPipeAsync();
                //    }
                //    else
                //    {
                //        dto.IsLikeByMe = cc_likeme[0].In("0", "null", "") ? false : true;

                //    }

                //}
                #endregion
            }
            while (false);


            await GetComments(dto);
            return dto;
        }
        // 评论前n条
        async Task GetComments(EvltCommentDetailDto dto)
        {
            var pg = await mediator.Send(new ChildrenCommentsQuery
            {
                EvltId = dto.EvaluationId,
                EvltCommentId = dto.Id,
                PageIndex = 1,
                PageSize = 10,
                Naf = dto.Now
            });
            dto.Comments = pg.CurrentPageItems.AsArray();
            foreach (var m in dto.Comments)
            {
                m.Now = dto.Now;
            }
        }

    }
}
