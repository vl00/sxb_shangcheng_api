using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
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
    /// 删除评测
    /// </summary>
    public class RemoveEvaluationCommandHandler : IRequestHandler<RemoveEvaluationCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient redis;
        IUserInfo me;
        IMediator mediator;

        public RemoveEvaluationCommandHandler(IOrgUnitOfWork unitOfWork,
           CSRedisClient redisClient,
           IMediator mediator,
           IUserInfo me)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.redis = redisClient;
            this.me = me;
            this.mediator = mediator;
        }


        public async Task<ResponseResult> Handle(RemoveEvaluationCommand request, CancellationToken cancellationToken)
        {
            if (!me.IsAuthenticated) throw new CustomResponseException("未登录");

            var dy = new DynamicParameters();
            dy.Add("@Id", request.Id);
            string sql = $@" select * from Evaluation where Id=@Id;";
            var removeModel = _orgUnitOfWork.Query<Evaluation>(sql, dy).FirstOrDefault();
            if (null == removeModel)
            {
                throw new CustomResponseException("参数错误");
            }
            if (!removeModel.IsValid)
            {
                throw new CustomResponseException("该评测已删除", 404);
            }
            if (me.UserId != removeModel.Userid)
            {
                throw new CustomResponseException("非法操作");
            }
            string specialSql = $@" select * from SpecialBind where evaluationid=@Id;";
            var specialModel = _orgUnitOfWork.Query<SpecialBind>(specialSql, dy).FirstOrDefault();

            // 不用判断有无EvaluationBind
            //string bindSql = $@"select * from EvaluationBind where evaluationid=@Id;";
            //var bindModel = _orgUnitOfWork.Query<EvaluationBind>(bindSql, dy).FirstOrDefault();
            //if (null == bindModel)
            //{
            //    throw new CustomResponseException("参数错误");
            //}

            // 删除前 检查活动评测能否删除
            for (var _01 = true; _01 && specialModel?.Specialid != null; _01 = false)
            {
                var editable = await mediator.Send(new CheckEvltEditableQuery { EvltId = request.Id });
                if (!editable.Enable)
                {
                    var day = Math.Ceiling(editable.DisableTtl?.TotalDays ?? 0);
                    throw new CustomResponseException(day > 0 ? $"活动评测审核成功{day}天内不能编辑." : $"活动评测审核成功后不能编辑.");
                }
            }

            try
            {
                _orgUnitOfWork.BeginTransaction();
                var delEvltSql = $@" update Evaluation set IsValid=0 where Id=@Id ;";
                await _orgUnitOfWork.ExecuteAsync(delEvltSql, dy, _orgUnitOfWork.DbTransaction);
                var delEvltBindSql = $@" update EvaluationBind set IsValid=0 where  evaluationid=@Id;";
                await _orgUnitOfWork.ExecuteAsync(delEvltBindSql, dy, _orgUnitOfWork.DbTransaction);
                var delEvltItemSql = $@" update EvaluationItem set IsValid=0 where  evaluationid=@Id;";
                await _orgUnitOfWork.ExecuteAsync(delEvltItemSql, dy, _orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.CommitChanges();               
            }
            catch (Exception ex)
            {
                try { _orgUnitOfWork.Rollback(); } catch { }
                return ResponseResult.Failed(ex.Message);
            }


            // 清理缓存
            await mediator.Send(new ClearFrontEvltCacheCommand { EvltId = removeModel.Id, SpclId = specialModel?.Id });                        

            return ResponseResult.Success("操作成功");
        }


        

    }
}
