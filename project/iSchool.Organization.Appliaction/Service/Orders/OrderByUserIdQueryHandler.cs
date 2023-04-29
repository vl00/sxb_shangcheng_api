using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Order
{
    public class OrdersByUserIdQueryHandler : IRequestHandler<OrdersByUserIdQuery, List<OrdersByUserIdQueryResponse>>
    {
        OrgUnitOfWork _unitOfWork;

        public OrdersByUserIdQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public async Task<List<OrdersByUserIdQueryResponse>> Handle(OrdersByUserIdQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string sql = $@"
                        SELECT top {request.PageSize} * FROM 
                        (
                            select ROW_NUMBER() over(order by o.id desc) as rownum, o.userid,o.id,o.code,o.payment,o.CreateTime,o.status,c.banner,c.name from [dbo].[Order] o 
                            left join [dbo].[Course] c 
                            on o.courseid=c.id and o.IsValid=1 and c.IsValid=1
                            where o.userid='{request.Userid}' and o.type>=2
                        )TT WHERE rownum>((@PageIndex-1)*@PageSize)
                        ";
            var response = _unitOfWork.DbConnection.Query<OrdersByUserIdQueryResponse>(sql,request).ToList();
            return response;
        }
    }
}
