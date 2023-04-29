using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{

    /// <summary>
    /// 获取单个兑换码
    /// </summary>
    public class QuerySingleRedeemCodeHandler : IRequestHandler<QuerySingleRedeemCode, RedeemCode>
    {
        
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;
        private static object obj = new object();
        const int time = 60 * 60;

        public QuerySingleRedeemCodeHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IMediator mediator)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            this._mediator = mediator;
        }

        public async Task<RedeemCode> Handle(QuerySingleRedeemCode request, CancellationToken cancellationToken)
        {
            RedeemCode response ;
            //find   Failure record and reusing
            var failRecord = _orgUnitOfWork.Query<Exchange>(@$" select * from dbo.Exchange where IsValid=1 and orderid='{request.OrderId}' 
and status={ExchangeStatus.Fail_In_Send.ToInt()} ;").FirstOrDefault();
            if (failRecord != null)
            {
                //--IsVaild=1 and 
                response = _orgUnitOfWork.Query<RedeemCode>(@$" select * from dbo.RedeemCode where Courseid='{request.CourseId}' and Used=1 and Code='{failRecord.Code}' ; ;").FirstOrDefault();
                return response;
            }

            // find used
            var rinfo = (await _mediator.Send(new GetOrderRedeemInfoQueryArgs { OrderIds = new[] { request.OrderId } })).FirstOrDefault();
            if (rinfo?.Redeem0?.IsVaild == true)
            {
                Debug.Assert(rinfo.Redeem0.Used == true);
                return rinfo.Redeem0;
            }    
           
             response = GetSingleDHCodeByNotUsed(request.OrderId, request.CourseId, _orgUnitOfWork, _redisClient);
            //if (response == null)
            //    throw new CustomResponseException("当前已无可用兑换码！");
            return response;
        }

        /// <summary>
        /// 获取一个兑换码(未使用的兑换码)
        /// </summary>
        /// <param name="orderid">订单Id</param>
        /// <param name="courseid">课程Id</param>
        /// <param name="_orgUnitOfWork"></param>
        /// <param name="_redisClient"></param>
        /// <returns></returns>
        private static RedeemCode GetSingleDHCodeByNotUsed(Guid orderid, Guid courseid, OrgUnitOfWork _orgUnitOfWork, CSRedisClient _redisClient)
        {
            RedeemCode newCode = null;
            lock (obj)
            {
                string key = CacheKeys.notUsedSingleCode.FormatWith(courseid, orderid);
                var rdata = _redisClient.Get<RedeemCode>(key);
                if (rdata != null)
                {
                    newCode = rdata;
                }                    
                else
                {
                    string sql = @$" select top 5 * from dbo.RedeemCode where IsVaild=1 and Used=0 and Courseid='{courseid}' order by NEWID()";
                    var listNewCode = _orgUnitOfWork.Query<RedeemCode>(sql).ToList();
                    foreach (var item in listNewCode)
                    {
                        if (_redisClient.Set(CacheKeys.CodeIsLock.FormatWith(courseid, item.Code), item.Code, time + 10, RedisExistence.Nx))
                        {
                            if (_redisClient.Set(key, item, time, RedisExistence.Nx))
                                return item;

                            else _ = _redisClient.DelAsync(CacheKeys.CodeIsLock.FormatWith(courseid, item.Code));
                        }                        
                    }                    
                }
            }
            return newCode;
        }

    }
}
