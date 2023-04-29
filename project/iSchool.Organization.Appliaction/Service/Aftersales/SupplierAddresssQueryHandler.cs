using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Appliaction.ViewModels.Aftersales;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class SupplierAddresssQueryHandler : IRequestHandler<SupplierAddresssQuery, Address>
    {
        OrgUnitOfWork _orgUnitOfWork;

        public SupplierAddresssQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<Address> Handle(SupplierAddresssQuery request, CancellationToken cancellationToken)
        {
            //{ "Addr":"广州","Receiver":"深海","Phone":"18268121359"}
            Address address = (await _orgUnitOfWork.QueryFirstOrDefaultAsync<Address>(@"
SELECT  
  JSON_VALUE([ReturnAddress],'$.Addr') SendBackAddress, JSON_VALUE([ReturnAddress],'$.Receiver') SendBackUserName, JSON_VALUE([ReturnAddress],'$.Phone') SendBackMobile
  FROM [Organization].[dbo].[SupplierAddress]
  JOIN CourseGoods ON CourseGoods.SupplieAddressId = SupplierAddress.Id OR (  CourseGoods.SupplieAddressId IS NULL AND CourseGoods.SupplierId = SupplierAddress.SupplierId )
  JOIN OrderRefunds ON OrderRefunds.ProductId = CourseGoods.Id
Where  OrderRefunds.Id = @orderRefundId AND ISJSON(ReturnAddress) = 1 and IsVaild =1 
Order By Sort 
", new { orderRefundId = request.OrderRefundId })) ;
            return address;
        }
    }
}
