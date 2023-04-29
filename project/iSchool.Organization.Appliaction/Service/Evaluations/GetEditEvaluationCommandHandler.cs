using CSRedis;
using Dapper;
using iSchool.Infrastructure;

using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 获取评测编辑选项
    /// </summary>
    public class GetEditEvaluationCommandHandler : IRequestHandler<GetEditEvaluationCommand, ResponseResult>
    {
        OrgUnitOfWork orgUnitOfWork;
        CSRedisClient _redisClient;
        IUserInfo _me;

        public GetEditEvaluationCommandHandler(IOrgUnitOfWork unitOfWork
            , CSRedisClient redisClient,
           IUserInfo me
            )
        {
            this.orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._redisClient = redisClient;
            this._me = me;

        }


        public  Task<ResponseResult> Handle(GetEditEvaluationCommand request, CancellationToken cancellationToken)
        {
            try
            {
             
                if( !_me.IsAuthenticated) return Task.FromResult(ResponseResult.Failed("未登录"));

                var dy = new DynamicParameters();
                dy.Add("@Id", request.Id);
                string sql = $@" select * from Evaluation where Id=@Id;";
                var editModel = orgUnitOfWork.Query<Evaluation>(sql, dy).FirstOrDefault();
                if (null== editModel)
                {
                    return Task.FromResult(ResponseResult.Failed("参数错误"));
                }
                if (_me.UserId != editModel.Userid)
                {
                    return Task.FromResult(ResponseResult.Failed("非法操作"));
                }
                if (!editModel.IsValid)
                {
                    return Task.FromResult(ResponseResult.Failed("评测不存在"));
                }
               
                var dto = new AddEvaluationCommand();
                if (editModel.Mode == (int)EvltContentModeEnum.Normal)
                {
                    string sqlItem = $@" select * from EvaluationItem where evaluationid=@Id;";
                    var editModelItem = orgUnitOfWork.Query<EvaluationItem>(sqlItem, dy).FirstOrDefault();
                    if (null == editModelItem)
                    {
                        return Task.FromResult(ResponseResult.Failed("参数错误"));
                    }
                    var ctt1 = new EvltContent1();
                    ctt1.Id = editModelItem.Id;
                    ctt1.Title = editModel.Title;
                    ctt1.Content = editModelItem.Content;
                    ctt1.Pictures = JsonSerializationHelper.JSONToObject<string[]>(editModelItem.Pictures);
                    ctt1.Thumbnails = JsonSerializationHelper.JSONToObject<string[]>(editModelItem.Thumbnails);
                    dto.Ctt1 = ctt1;
                    dto.Mode = EvltContentModeEnum.Normal.ToInt();
                }
                else if (editModel.Mode == (int)EvltContentModeEnum.Pro)
                {
                    string sqlItem = $@" select * from EvaluationItem where evaluationid=@Id order by [type];";
                    var editModelItem = orgUnitOfWork.Query<EvaluationItem>(sqlItem, dy);
                    if (null == editModelItem || 0 == editModelItem.Count())
                    {
                        return Task.FromResult(ResponseResult.Failed("参数错误"));
                    }
                    var ctt2 = new EvltContent2();
                    ctt2.Title = editModel.Title;
                    var listStep = new List<EvltContent2Step>();
                    foreach (var item in editModelItem)
                    {
                        var step = new EvltContent2Step();
                        step.Id = item.Id;
                        step.Content = item.Content;
                        step.Pictures = JsonSerializationHelper.JSONToObject<string[]>(item.Pictures);
                        step.Thumbnails = JsonSerializationHelper.JSONToObject<string[]>(item.Thumbnails);
                        listStep.Add(step);


                    }
                    ctt2.Steps = listStep.ToArray();
                    dto.Ctt2 = ctt2;
                    dto.Mode = EvltContentModeEnum.Pro.ToInt();
                }
                dto.EvaluationId = request.Id;
                return Task.FromResult(ResponseResult.Success(dto));
            }
            catch (Exception ex)
            {
                return  Task.FromResult(ResponseResult.Failed(ex.Message));
            }
        }




    }
}
