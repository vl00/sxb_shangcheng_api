using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    [Obsolete]
    public class CourseStockRequest : IRequest<CourseStockResponse>
    {        
        /// <summary>扣减库存(会从db加载到cache)</summary>
        public CourseStockCommand? StockCmd { get; set; }       
        public AddCourseStockCommand? AddStock { get; set; }
        public GetCourseStockQuery? GetStock { get; set; }

        /// <summary>库存从cache同步到db</summary>
        public SyncSetCourseStockCommand? SyncSetStock { get; set; }
        /// <summary>后台改变库存</summary>
        public BgSetCourseStockCommand? BgSetStock { get; set; }
    }

    public class CourseStockCommand
    {
        public Guid CourseId { get; set; }
        public int Num { get; set; }
    }

    public class AddCourseStockCommand
    {
        public Guid CourseId { get; set; }
        public int Num { get; set; }
        public bool FromDBIfNotExists { get; set; }
    }

    public class GetCourseStockQuery
    {
        public Guid CourseId { get; set; }
        public bool FromDBIfNotExists { get; set; }
    }

    public class SyncSetCourseStockCommand
    {
        public Guid CourseId { get; set; }
        public int AddNum { get; set; }
    }

    public class BgSetCourseStockCommand
    {
        public Guid CourseId { get; set; }
        public int StockCount { get; set; }
    }

#nullable disable
}
