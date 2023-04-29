using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace iSchool.Organization.Appliaction.Services
{
    public class UserOrderRelProdsQueryHandler : IRequestHandler<RelOrderProdsQuery, RelOrderProdsQueryResult>
    {
        private readonly IUserInfo me;
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;

        public UserOrderRelProdsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper, IUserInfo me)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
            this.me = me;
        }

        public async Task<RelOrderProdsQueryResult> Handle(RelOrderProdsQuery request, CancellationToken cancellation)
        {
            var data = new RelOrderProdsQueryResult();

            //
            // 2021-11-05 需求来源：嘉辉哥 已购买商品关联列表：只出现确认收货后，已完成的商品
            //

            var sql = $@"--{OrderStatusV2.Ship.ToInt()},{OrderStatusV2.Shipping.ToInt()},
SELECT no as No,banner,a.id,title,subtitle from 
( SELECT c.id,MAX(o.CreateTime) as createtime from  OrderDetial od join [order] o on od.orderid=o.id join Course c 
 on od.courseid=c.id  where userid=@userid
 and  o.status in ({OrderStatusV2.Shipped.ToInt()},{OrderStatusV2.Completed.ToInt()}) GROUP BY c.id
ORDER BY createtime DESC  OFFSET @offset ROWS FETCH NEXT @next ROWS ONLY   ) a join Course cc on a.id=cc.id";
            var countSql = $@"--{OrderStatusV2.Ship.ToInt()},{OrderStatusV2.Shipping.ToInt()},
SELECT count(1) from (
SELECT DISTINCT(od.courseid) from  OrderDetial od join [order] o on od.orderid=o.id   where  userid=@userid
 and   o.status in ({OrderStatusV2.Shipped.ToInt()},{OrderStatusV2.Completed.ToInt()})) a";

            var list = new List<OrderRelProdItemDto>();

            // 总数
            var count = _orgUnitOfWork.QueryFirstOrDefault<int>(countSql, new { userid = me.UserId });
            if (count == 0)
            {
                return new RelOrderProdsQueryResult
                {
                    PageInfo = list.ToPagedList(request.PageSize, request.PageIndex, 0)
                };
            }
            var dbList = _orgUnitOfWork.Query<OrderRelProdItemDB>(sql, new
            {
                userid = me.UserId,
                offset = (request.PageIndex - 1) * request.PageSize,
                next = request.PageSize
            }).ToList();
            foreach (var item in dbList)
            {
                var addM = new OrderRelProdItemDto();
                addM.Id_s = UrlShortIdUtil.Long2Base32(item.No);
                addM.Banner = JsonConvert.DeserializeObject<List<string>>(item.Banner);
                addM.Title = item.Title;
                addM.Id = item.Id;
                addM.Subtitle = item.Subtitle;
                list.Add(addM);
            }


            data.PageInfo = list.ToPagedList(request.PageSize, request.PageIndex, count);

            return data;
        }
    }


}
