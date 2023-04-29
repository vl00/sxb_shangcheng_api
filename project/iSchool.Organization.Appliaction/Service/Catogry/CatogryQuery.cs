using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.KeyValues
{
    /// <summary>
    /// 类别
    /// </summary>
    public class CatogryQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 类别
        /// </summary>
        public int Type { get; set; } = Consts.Kvty_MallFenlei;
        /// <summary>
        /// 查多级分类的根级是哪一级
        /// </summary>
        public int Root { get; set; } = 1;
    }
}
