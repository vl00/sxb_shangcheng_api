using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSRedis;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels.Statistics;
using MediatR;
using System.Linq;
using iSchool.Organization.Domain;

namespace iSchool.Organization.Appliaction.Service
{
    public class SalesStatisticsQueryHandler : IRequestHandler<SalesStatisticsQuery, SalesStatisticsResponse>
    {
        private readonly CSRedisClient _redis;
        private readonly OrgUnitOfWork _unitOfWork;

        public SalesStatisticsQueryHandler(CSRedisClient redisClient, IOrgUnitOfWork unitOfWork)
        {
            _redis = redisClient;
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public async Task<SalesStatisticsResponse> Handle(SalesStatisticsQuery request, CancellationToken cancellationToken)
        {

            var today = DateTime.Now;
            SalesStatisticsResponse res = new SalesStatisticsResponse();

            if (_redis.Exists(string.Format(CacheKeys.ToDaySaleData, today.Day)))
            {
                res = _redis.Get<SalesStatisticsResponse>(string.Format(CacheKeys.ToDaySaleData, today.Day));
            }
            else
            {
                //查询顶部的数据（缓存5分钟）
                var sql1 = @"--今日订单数
                    SELECT ISNULL( COUNT(1),0) ordercount
                    FROM
                    (
                        SELECT AdvanceOrderNo todayordercount
                        FROM dbo.[Order]
                        WHERE IsValid = 1
                              AND CreateTime >= CONVERT(VARCHAR(100), GETDATE(), 23)
                        GROUP BY AdvanceOrderNo
                    ) a;
                    --今日销售额
					SELECT  ISNULL(SUM(e.sales),0)  sales FROM(
                    SELECT   orders.payment- ISNULL((SELECT SUM(Price) FROM dbo.OrderRefunds WHERE OrderId=orders.id AND (Status=5 OR Status=17) and  IsValid=1 ),0) sales
                    FROM dbo.[Order] AS orders
                        LEFT JOIN dbo.OrderDetial AS detail
                            ON orders.id = detail.orderid
                    WHERE orders.IsValid = 1
                          AND
                          (
                              orders.status = 103
                              OR orders.status >= 300
                          )
                          AND orders.CreateTime >= CONVERT(VARCHAR(100), GETDATE(), 23))e
                    --支付人数
                    SELECT  ISNULL(COUNT(1),0) paycount
                    FROM
                    (
                        SELECT COUNT(1) ordercount
                        FROM dbo.[Order] orders
                        WHERE orders.IsValid = 1
                              AND
                              (
                                  orders.status = 103
                                  OR orders.status >= 300
                              )
                              AND orders.CreateTime >= CONVERT(VARCHAR(100), GETDATE(), 23)
                        GROUP BY orders.userid
                    ) b;
                    --今日支付订单数
                    SELECT  ISNULL(COUNT(1),0) payusercount
                    FROM
                    (
                        SELECT AdvanceOrderNo orderpaycount
                        FROM dbo.[Order] orders
                        WHERE IsValid = 1
                              AND
                              (
                                  orders.status = 103
                                  OR orders.status >= 300
                              )
                              AND orders.CreateTime >= CONVERT(VARCHAR(100), GETDATE(), 23)
                        GROUP BY orders.AdvanceOrderNo
                    ) c;
                    
                    --复购数
                    SELECT ISNULL(COUNT(1),0) repurchase
                    FROM
                    (
                        SELECT ISNULL(COUNT(1), 0) AS buycount,
                               today.userid
                        FROM
                        (
                            SELECT *
                            FROM dbo.[Order] AS today
                            WHERE today.IsValid = 1
                                  AND
                                  (
                                      today.status = 103
                                      OR today.status >= 300
                                  )
                        ) today
                            LEFT JOIN
                            (
                                SELECT *
                                FROM dbo.[Order] AS pastday
                                WHERE pastday.IsValid = 1
                                      AND
                                      (
                                          pastday.status = 103
                                          OR pastday.status >= 300
                                      )
                            ) pastday
                                ON pastday.userid = today.userid
                    			AND pastday.CreateTime<today.CreateTime AND  pastday.AdvanceOrderNo!=today.AdvanceOrderNo
                        WHERE pastday.userid IS NOT NULL  AND today.CreateTime> CONVERT(VARCHAR(100), GETDATE(), 23)
                        GROUP BY today.userid
                        HAVING ISNULL(COUNT(1), 0) >= 1
                    ) d;
                    
                    --累计销售额
				   SELECT SUM(f.allsale)allsale FROM(	
                    SELECT orders.payment- ISNULL((SELECT SUM(Price) FROM dbo.OrderRefunds WHERE OrderId=orders.id AND (Status=5 OR Status=17) and  IsValid=1),0)  allsale
                    FROM dbo.[Order] AS orders
                        LEFT JOIN dbo.OrderDetial AS detail
                            ON orders.id = detail.orderid
                    WHERE orders.IsValid = 1
                          AND
                          (
                              orders.status = 103
                              OR orders.status >= 300
                          ))f;
                    ";


                var data = await _unitOfWork.QueryMultipleAsync(sql1);

                res.TodayOrderCount = data.Read<int>().First();
                res.TodaySales = data.Read<decimal>().First();
                res.TodayPayCount = data.Read<int>().First();
                res.TodayOrderPayCount = data.Read<int>().First();
                res.TodayRepurchase = data.Read<int>().First();
                res.AllSale = data.Read<decimal>().First();
                //保存缓存
                _redis.Set(string.Format(CacheKeys.ToDaySaleData, today.Day), res, 5 * 60);
            }
            //查询折线图的数据（缓存当天）
            if (_redis.Exists(string.Format(CacheKeys.SalesViewData, today.Day, request.Day)))
            {
                res.SalesView = _redis.Get<List<View>>(string.Format(CacheKeys.SalesViewData, today.Day, request.Day));
            }
            else
            {
                var sql2 = @"
                SELECT a.GroupDay day,b.ordercount,c.Sales,d.payusercount,e.paycount,f.repurchase
                FROM
                (
                    SELECT CONVERT(NVARCHAR(10), DATEADD(DAY, -1 * number, GETDATE()), 120) AS GroupDay, number
                           type
                    FROM master..spt_values
                    WHERE type = 'p'
                          AND number <= @day
                ) a
                    --订单数
                    LEFT JOIN
                    (
                        SELECT CONVERT(CHAR(32), CreateTime, 23) AS day,
                               COUNT(*) AS ordercount
                        FROM dbo.[Order]
                        WHERE IsValid = 1
                        GROUP BY CONVERT(CHAR(32), CreateTime, 23)
                    ) b
                        ON b.day = a.GroupDay
                    --销售额
					LEFT JOIN
                    (
                       SELECT SUM(payment-refundprice)Sales ,saledata.day FROM (
					    SELECT CONVERT(CHAR(32), orders.CreateTime, 23) AS day,
                               orders.payment,(SELECT ISNULL( SUM(Price),0) FROM dbo.OrderRefunds WHERE orders.id=OrderId AND IsValid=1 AND (Status=5 OR Status=17)) refundprice
                        FROM dbo.[Order] AS orders
                            LEFT JOIN dbo.OrderDetial AS detial
                                ON orders.id = detial.orderid
                        WHERE orders.IsValid = 1
                              AND
                              (
                                  orders.status = 103
                                  OR orders.status >= 300
                              ))  saledata
                        GROUP BY saledata.day
                    ) c
                        ON c.day = a.GroupDay
                    --支付人数
                    LEFT JOIN
                    (
                        SELECT COUNT(1) payusercount,
                               payuserdata.day
                        FROM
                        (
                            SELECT CONVERT(CHAR(32), CreateTime, 23) day,
                                   COUNT(1) count1
                            FROM dbo.[Order] AS orders
                            WHERE orders.IsValid = 1
                                  AND
                                  (
                                      orders.status = 103
                                      OR orders.status >= 300
                                  )
                            GROUP BY CONVERT(CHAR(32), CreateTime, 23),
                                     orders.userid
                        ) payuserdata
                        GROUP BY payuserdata.day
                    ) d
                        ON d.day = a.GroupDay
                    --支付笔数
                    LEFT JOIN
                    (
                        SELECT COUNT(1) paycount,
                               paydate.day
                        FROM
                        (
                            SELECT CONVERT(CHAR(32), CreateTime, 23) day,
                                   orders.AdvanceOrderNo
                            FROM dbo.[Order] AS orders
                            WHERE orders.IsValid = 1
                                  AND
                                  (
                                      orders.status = 103
                                      OR orders.status >= 300
                                  )
                            GROUP BY CONVERT(CHAR(32), CreateTime, 23),
                                     orders.AdvanceOrderNo
                        ) paydate
                        GROUP BY paydate.day
                    ) e
                        ON e.day = a.GroupDay
                    --复购人数
                    LEFT JOIN
                    (
                        SELECT a.day,
                               COUNT(1) repurchase
                        FROM
                        (
                            SELECT CONVERT(CHAR(32), today.CreateTime, 23) day,
                                   today.userid,
                                   COUNT(1) time
                            FROM dbo.[Order] AS today
                                LEFT JOIN dbo.[Order] AS pastday
                                    ON today.CreateTime > pastday.CreateTime
                                       AND today.userid = pastday.userid
                                       AND pastday.IsValid = 1
                                       AND pastday.AdvanceOrderNo!=today.AdvanceOrderNo
                                       AND
                                       (
                                           pastday.status = 103
                                           OR pastday.status >= 300
                                       )
                            WHERE today.IsValid = 1
                                  AND
                                  (
                                      today.status = 103
                                      OR today.status >= 300
                                  )
                                  AND pastday.userid IS NOT NULL
                            GROUP BY CONVERT(CHAR(32), today.CreateTime, 23),
                                     today.userid
                            HAVING COUNT(1) >= 1
                        ) a GROUP BY a.day
                    ) f
                        ON f.day = a.GroupDay
                		ORDER BY a.GroupDay  ";

                var list = (await _unitOfWork.QueryAsync<View>(sql2, new { day = request.Day })).ToList();
                list.RemoveAt(list.Count() - 1);
                //保存缓存
                _redis.Set(string.Format(CacheKeys.SalesViewData, today.Day, request.Day),
                    list, new TimeSpan(23, 59, 59));
                res.SalesView = list;
            }
            return res;
        }
    }
}
