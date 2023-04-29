using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.Queries.Models;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Queries
{
    public class OrderQueries : IOrderQueries
    {
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;
        public OrderQueries(IOrgUnitOfWork unitOfWork, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public async Task<AdvanceOrderDetailResponse> GetAdvanceOrderDetailAsync(Guid advanceOrderId)
        {
            string sql = $@"
SELECT
    Top 1
	O.AdvanceOrderNo,
    O.PaymentTime
FROM
	[Order] O
WHERE
 	O.IsValid = 1
 	AND O.Status >= 103
	AND O.Type >= 2
	AND O.AdvanceOrderId = @advanceOrderId
";

            var advanceOrder = await _orgUnitOfWork.QueryFirstOrDefaultAsync<AdvanceOrderDetailResponse>(sql, new { advanceOrderId });
            if (advanceOrder != null)
            {
                advanceOrder.OrderDetails = await GetOrderDetailsAsync(advanceOrderId);
            }
            return advanceOrder;
        }

        public async Task<IEnumerable<AdvanceOrderDetail>> GetOrderDetailsAsync(Guid advanceOrderId)
        {
            string sql = $@"
SELECT
	OD.Id,
	OD.Name,
	-- 原价
	OD.Origprice * OD.Number AS OriTotalAmount,
	-- 应付
	OD.price * OD.Number AS TotalAmount,
	-- 折后实付
	ISNULL(OD.Payment, OD.price * OD.Number) AS Payment,
	OD.Ctn
FROM
	[Order] O
	INNER JOIN OrderDetial OD ON OD.orderid = O.id
WHERE
 	O.IsValid = 1
 	AND O.Status >= 103
	AND O.Type >= 2
	AND O.AdvanceOrderId = @advanceOrderId
	
";
            return await _orgUnitOfWork.QueryAsync<AdvanceOrderDetail>(sql, new { advanceOrderId });
        }


    }
}
