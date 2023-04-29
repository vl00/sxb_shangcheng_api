using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 编辑机构通用方法
    /// </summary>
    public class AddOrEditOrgCommandHandler : IRequestHandler<AddOrEditOrgCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        SmLogUserOperation _smLogUserOperation;
        IMediator _mediator;

        public AddOrEditOrgCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, SmLogUserOperation smLogUserOperation,
            IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            this._smLogUserOperation = smLogUserOperation;
            _mediator = mediator;
        }

        public Task<ResponseResult> Handle(AddOrEditOrgCommand request, CancellationToken cancellationToken)
        {
            // 商品分类
            if (request.BrandTypes?.Length > 0)
            {
                var ctys = request.BrandTypes.Distinct();
                foreach (var cty in ctys)
                {
                    try
                    {
                        var d = _mediator.Send(new OrgService_bg.RequestModels.BgMallFenleisLoadQuery { Code = cty, ExpandMode = 1 }).Result;
                        if (d?.Selected_d3?.Code != cty) throw new Exception("不是3级分类");
                    }
                    catch (Exception ex)
                    {
                        throw new CustomResponseException("保存失败." + ex.Message);
                    }
                }
                request.BrandTypes = ctys.ToArray();
            }

            string sql = "";

            Domain.Organization org0 = null;
            if (!request.IsAdd)
            {
                sql = "select * from Organization where Id=@Id";
                org0 = _orgUnitOfWork.DbConnection.QueryFirstOrDefault<Domain.Organization>(sql, new { request.Id });
            }

            if (request.IsAdd)
                sql = $@" insert into Organization ([id], [name], [logo], [intro], [status], [types],[GoodthingTypes], [modes], [CreateTime], [Creator], [minage], [maxage], [desc], [subdesc],[BrandTypes])
                             values(@id, @name, @logo, @intro, @status, @types,@GoodthingTypes, @modes, @Time, @UserId, @minage, @maxage, @desc, @subdesc,@brandTypes) ;";
            else
                sql = $@"update Organization set [name]=@name, logo=@logo, intro=@intro, [status]=@status, [types]=@types,[GoodthingTypes]=@GoodthingTypes, modes=@modes,ModifyDateTime=@Time,Modifier=@UserId,  minage=@minage, maxage=@maxage, [desc]=@desc, subdesc=@subdesc,brandTypes=@brandTypes where id=@id;";

            var count = _orgUnitOfWork.DbConnection.Execute(sql, new DynamicParameters()
                .Set("id", request.Id)
                .Set("name", request.Name)
                .Set("logo", request.LOGO)
                .Set("intro", request.Intro)
                .Set("status", 1)//机构状态(1:上架;0:下架)
                .Set("types", request.Types)
                .Set("GoodthingTypes", request.GoodthingTypes)
                .Set("modes", request.Modes)
                .Set("Time", DateTime.Now)
                .Set("UserId", request.UserId)
                .Set("minage", request.MinAge)
                .Set("maxage", request.MaxAge)
                .Set("desc", request.Desc)
                .Set("subdesc", request.SubDesc)
                .Set("brandTypes", JsonSerializationHelper.Serialize(request.BrandTypes))
                );
            if (count == 1)
            {
                #region 清除API那边相关的缓存
                _ = _redisClient.BatchDelAsync(new List<string>()
                    {
                         CacheKeys.Del_Organizations.FormatWith("*")
                        ,CacheKeys.OrgDetails.FormatWith(request.Id)
                        ,"org:courses:*"//课程列表有用到机构名称
                        ,"org:course:courseid:*"//课程详情有用到机构名称

                        //机构pc需清除的缓存
                        ,$"org:organization:orgid:{request.Id}:pc:*"
                        ,$"org:organization:orgz:{request.Id}:info"

                         ,"org:evlt:info:*"//评测详情有机构信息、课程信息、专题信息

                        ,"org:pc:relatedEvlts:*"//pc评测详情-相关评测s、pc课程详情-相关评测s、pc机构详情-相关评测s
                    }, 100);
                #endregion

                // add user log
                _smLogUserOperation.SetUserId(request.UserId ?? default)
                    .SetClass(nameof(AddOrEditOrgCommand))
                    .SetParams("_", request)
                    .SetParams("orgid", request.Id)
                    .SetMethod(request.IsAdd ? "add" : "update")
                    .SetOldata("organization", org0)
                    .SetTime(DateTime.Now);

                return Task.FromResult(ResponseResult.Success("操作成功"));
            }
            else
            {
                return Task.FromResult(ResponseResult.Failed("操作失败"));
            }

        }
    }
}
