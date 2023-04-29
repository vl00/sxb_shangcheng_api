using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{

    /// <summary>
    /// 释放订单占用的兑换码
    /// </summary>
    public class ReleaseOccupiedCodeCommandHandler : IRequestHandler<ReleaseOccupiedCodeCommand, ResponseResult>
    {


        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public ReleaseOccupiedCodeCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }

        public async Task<ResponseResult> Handle(ReleaseOccupiedCodeCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            try
            {
                string key = CacheKeys.notUsedSingleCode.FormatWith(request.CourseId, "*");

                string sql = $@"  update dbo.RedeemCode set [OccupiedTime]=null where Courseid=@Courseid and IsVaild=@IsVaild and Used=@Used ;";
                var dy = new DynamicParameters();               
                dy.Set("Courseid", request.CourseId);
                dy.Set("IsVaild", true);
                dy.Set("Used", false);//未使用被占用的

                //1、清未订单绑定兑换码的缓存
                _= _redisClient.BatchDelAsync(key, 10);

                //2、更新课程下的兑换表占用时间为null                
                var response = _orgUnitOfWork.ExecuteScalar<int>(sql, dy);
                return ResponseResult.Success(response);

            }
            catch (Exception ex)
            {
                return ResponseResult.Failed($"系统错误：{ex.Message}");
            }
        }
    }
}
