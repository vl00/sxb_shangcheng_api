using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.EvaluationComments;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class MyEvaluationCommentQueryHandler : IRequestHandler<MyEvaluationCommentQuery, PagedList<MyEvltCommentDto>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;

        public MyEvaluationCommentQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redis, IUserInfo me,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
        }

        public async Task<PagedList<MyEvltCommentDto>> Handle(MyEvaluationCommentQuery request, CancellationToken cancellation)
        {
            string sql = $@" 
                            select  top  {request.PageSize} * from (
                            SELECT  ROW_NUMBER() over(order by CreateTime desc) as rownum,* FROM [Organization].[dbo].[EvaluationComment]  where IsValid=1 and  userid=@userid
                           and fromid is null  )TT  Where rownum>@SkipCount order by rownum 
                        ;";
            string pageSql = $@" 
                              select COUNT(1) AS TotalItemCount,{request.PageIndex} AS CurrentPageIndex,{ request.PageSize} AS PageSize
                              FROM [Organization].[dbo].[EvaluationComment] where IsValid=1 and  userid=@userid   and fromid is null 
                             ;";
            var dy = new DynamicParameters()
                .Set("userid", me.UserId)
                .Set("SkipCount", (request.PageIndex - 1) * request.PageSize);
            var data = _orgUnitOfWork.Query<PagedList<MyEvltCommentDto>>(pageSql, dy).FirstOrDefault();
            data.CurrentPageItems = new List<MyEvltCommentDto>();
            data.CurrentPageItems = _orgUnitOfWork.Query<MyEvltCommentDto>(sql, dy).ToList();
            foreach (var item in data.CurrentPageItems)
            {
                item.UserImg = me.HeadImg;
                item.Username = me.UserName;
                item.Type = item.FromId == null || Guid.Empty == item.FromId ? 0 : 1;
                await GetContents(item);
             
           

               

            }
            return data;

        }
        async Task GetContents(MyEvltCommentDto dto)
        {
            switch (dto.Type)
            {
                //跟产品确认了,我的评论不查回复的
                case 1://回复评论
//                    var rdkCmt = CacheKeys.EvltCommnet.FormatWith(dto.FromId);
//                    var tempCmtContent = await redis.HGetAsync<MyEvltCommentDto>(rdkCmt, "contents");

//                    if (tempCmtContent == null)
//                    {
//                        var sql = $@"
//select * from EvaluationComment
//where IsValid=1 and  Id=@Id
//";
//                        var commentModel = await _orgUnitOfWork.QueryFirstOrDefaultAsync<EvaluationComment>(sql, new { Id = dto.FromId });
//                        if (null == commentModel) return;
//                        tempCmtContent = mapper.Map(commentModel,dto);
//                        _ = redis.HSetAsync(rdkCmt, "contents", tempCmtContent);
//                    }
//                    dto.TargetValid = tempCmtContent.IsValid;
                 
//                    dto.TargetContent = tempCmtContent.Comment.Length > 50 ? tempCmtContent.Comment[0..50] : tempCmtContent.Comment;

                    break;
                case 0://回复评测
                    var rdk = CacheKeys.Evlt.FormatWith(dto.EvaluationId);
                    var tempContent = await redis.HGetAsync<EvaluationContentDto[]>(rdk, "contents");

                    if (tempContent == null)
                    {
                        var sql = $@"
select item.* from Evaluation evlt 
join EvaluationItem item on item.evaluationid=evlt.id
where evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} and item.IsValid=1 and evlt.id=@Id
order by item.type
";
                        var items = await _orgUnitOfWork.QueryAsync<EvaluationItem>(sql, new {Id= dto.EvaluationId });
                        tempContent = items.Select(x => mapper.Map<EvaluationContentDto>(x)).ToArray();

                        _ = redis.HSetAsync(rdk, "contents", tempContent);
                    }

                    var ctts = string.Join('\n', tempContent.Select(_ => _?.Content ?? ""));
                    ctts = HtmlHelper.NoHTML(ctts);
                    dto.TargetContent = ctts.Length > 50 ? ctts[0..50] : ctts;
                    var evaltModel = await _orgUnitOfWork.QueryFirstOrDefaultAsync<Evaluation>(@"select * from Evaluation where id = @id", new { id= dto.EvaluationId }) ;
                    if (null != evaltModel)
                    {
                        dto.TargetValid = evaltModel.IsValid;
                        dto.TargetImgLink = evaltModel.Cover;
                        dto.TargetTitle = evaltModel.Title;
                        dto.Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(evaltModel.No));
                        dto.TargetStatus = evaltModel.Status;
                    }
                    break;
                  
                default:
                    break;
            }

        }


    }
}
