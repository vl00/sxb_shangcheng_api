using iSchool.Organization.Appliaction.Queries.Models;
using System;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Queries
{
    public interface IOrderQueries
    {
        Task<AdvanceOrderDetailResponse> GetAdvanceOrderDetailAsync(Guid advanceOrderId);
    }
}