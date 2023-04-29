using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Organization
{

    public class CatogoryOrganizationQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfo PageInfo { get; set; }        

        /// <summary>
        /// 商品分类ID
        /// </summary>
        public int? CatogoryId { get; set; }

        /// <summary>
        /// 认证
        /// </summary>
        public bool? Authentication { get; set; }
        /// <summary>
        /// 类目级数，默认第二级
        /// </summary>

        public int? CatogoryLevel { get; set; } 
        /// <summary>
        /// 搜索内容
        /// </summary>

        public string SearchText { get; set; }

    }


    

}
