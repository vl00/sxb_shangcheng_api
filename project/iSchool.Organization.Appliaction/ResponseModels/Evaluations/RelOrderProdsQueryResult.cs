using iSchool.Infrastructure.Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{

    /// <summary>
    ///  种草选择关联订单商品
    /// </summary>

    public class RelOrderProdsQueryResult
    {

        public PagedList<OrderRelProdItemDto> PageInfo { get; set; } = default!;
    }
    public class OrderRelProdItemDto
    {


        public Guid Id { get; set; }
        /// <summary>课程短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>课程名称</summary>
        public string Title { get; set; } = default!;
        /// <summary>课程副标题</summary>
        public string Subtitle { get; set; }



        /// <summary>课程banner图片地址</summary>
        public List<string> Banner { get; set; } = default!;

    }

    public class OrderRelProdItemDB
    {

        public int No { get; set; }
        public Guid Id { get; set; }
        /// <summary>课程短id</summary>

        /// <summary>课程名称</summary>
        public string Title { get; set; } = default!;
        /// <summary>课程副标题</summary>
        public string Subtitle { get; set; }



        /// <summary>课程banner图片地址</summary>
        public string Banner { get; set; } = default!;

    }
}
