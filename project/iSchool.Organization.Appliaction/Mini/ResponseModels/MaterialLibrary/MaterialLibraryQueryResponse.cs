using iSchool.Organization.Appliaction.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Mini.ResponseModels.MaterialLibrary
{
    /// <summary>
    /// 素材列表的返回实体类
    /// </summary>
    public class MaterialLibraryQueryResponse
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfoResult PageInfo { get; set; }

        /// <summary>
        /// 素材列表
        /// </summary>
        public List<MiniMaterialLibraryItemDto> MaterialLibraryDatas { get; set; }
    }
}
