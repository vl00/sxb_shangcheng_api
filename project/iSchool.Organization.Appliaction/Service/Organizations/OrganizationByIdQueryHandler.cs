using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Evaluations;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    public class OrganizationByIdQueryHandler : IRequestHandler<OrganizationByIdQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;
        const int time = 60 * 60;
        AppSettings appSettings;

        public OrganizationByIdQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IMediator mediator,IOptions<AppSettings> options)
        {
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
            appSettings = options.Value;
        }

        public async Task<ResponseResult> Handle(OrganizationByIdQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;            

            OrganizationByIdQueryResponse data;
            var dy = new DynamicParameters();
            dy.Add("@No", request.No);
            string sql = $@" select  Id, Name, Logo, Authentication,Intro,No,'{appSettings.OrgBaseMap}' as OrgBaseMap   from [dbo].[Organization] where IsValid=1  and status=1 and no=@No  ";

            #region get guid id
            string id_key = CacheKeys.orgidbyno.FormatWith(request.No);
            request.OrganizationId = _redisClient.Get<Guid>(id_key);

            if (request.OrganizationId == default)
            {
                data = _unitOfWork.Query<OrganizationByIdQueryResponse>(sql, dy).FirstOrDefault();
                if (data == null) throw new CustomResponseException("机构详情不存在",404);
                else
                {
                    request.OrganizationId = data.Id;
                    _redisClient.Set(id_key, request.OrganizationId, time);                    
                }
            }
            #endregion

            string key = string.Format(CacheKeys.OrgDetails, request.OrganizationId);
            data = _redisClient.Get<OrganizationByIdQueryResponse>(key);
            if (data == null)
            {            
                data = _unitOfWork.Query<OrganizationByIdQueryResponse>(sql, dy).FirstOrDefault();
                if (data == null) throw new CustomResponseException("机构详情不存在",404);
                if (data != null)
                {
                    data.No = UrlShortIdUtil.Long2Base32(Convert.ToInt64(data.No));
                    #region 相关课程
                    var courses = _mediator.Send(new OrganizationRelatedCoursesQuery { OrganizationId = request.OrganizationId, PageInfo = new PageInfo() { PageIndex = 1, PageSize = 1000 } }).Result;
                    List<RelatedCoursesDto> coursesDto = new List<RelatedCoursesDto>();
                    if (courses != null && courses.Count > 0)
                    {
                        foreach (var item in courses)
                        {
                            coursesDto.Add(new RelatedCoursesDto()
                            {
                                Banner = item.Banner == null ? null : JsonSerializationHelper.JSONToObject<List<string>>(item.Banner),
                                CNO = item.CNO,//在相关课程api已转
                                Id = item.Id,
                                Name = item.Name,
                                OrigPrice = item.OrigPrice,
                                Price = item.Price,
                                Stock = item.Stock,
                                Title = item.Title
                            });
                        }
                    }
                    data.RelatedCourses = new List<RelatedCoursesDto>();
                    data.RelatedCourses = coursesDto;
                    #region 不需要再缓存，直接调用相关课程
                    //string courseKey = string.Format(CacheKeys.CourseDetails, request.OrganizationId, 1, 10);
                    //var coursesData = _redisClient.Get<List<RelatedCoursesDto>>(courseKey);
                    //data.RelatedCourses = new List<RelatedCoursesDto>();
                    //if (coursesData != null)
                    //{
                    //    data.RelatedCourses = coursesData;
                    //}
                    //else
                    //{

                    //   var courses  = _mediator.Send(new OrganizationRelatedCoursesQuery { OrganizationId = request.OrganizationId, PageInfo = new PageInfo() { PageIndex = 1, PageSize = 10 } }).Result;
                    //    List<RelatedCoursesDto> coursesDto = new List<RelatedCoursesDto>();
                    //    if (courses!=null && courses.Count > 0)
                    //    {
                    //        foreach (var item in courses)
                    //        {
                    //            coursesDto.Add(new RelatedCoursesDto()
                    //            {
                    //                Banner = item.Banner == null ? null : JsonSerializationHelper.JSONToObject<List<string>>(item.Banner),                                    
                    //                CNO = item.CNO,//在相关课程api已转
                    //                Id = item.Id,
                    //                Name = item.Name,
                    //                OrigPrice = item.OrigPrice,
                    //                Price = item.Price,
                    //                Stock = item.Stock,
                    //                Title = item.Title
                    //            });
                    //        }
                    //    }
                    //    data.RelatedCourses = coursesDto;
                    //    _redisClient.Set(courseKey, data.RelatedCourses, time);
                    //} 
                    #endregion
                    #endregion

                    #region 相关评测
                    data.RelatedEvaluations = new List<EvaluationItemDto>();
                    data.RelatedEvaluations = (await _mediator.Send(new OrgRelatedEvaluationQuery
                    {
                        OrgId = request.OrganizationId,
                        PageIndex = 1,
                    })).CurrItems.ToList();
                    #endregion

                    #region 评测推荐
                    data.RecommendEvaluations = new List<EvaluationItemDto>();
                    data.RecommendEvaluations = (await _mediator.Send(new EvaluationLoadMoreQuery
                    {
                        Age = "0",
                        Subj = "0",
                        Stick = 1,
                        PageIndex = 1,
                    })).CurrItems.Take(6).ToList();
                    #endregion
                }

                _redisClient.Set(key, data, time);                
            }

            // 商品数 
            {
                var dict = await _mediator.Send(new PcGetOrgsCountsQuery { OrgIds = new[] { request.OrganizationId } });
                data.GoodsCount = dict.GetValueEx(request.OrganizationId).GoodsCount;
            }

            return ResponseResult.Success(data);
        }
    }
}
