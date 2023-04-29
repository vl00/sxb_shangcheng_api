using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 新增/编辑机构
    /// </summary>
    public class AddOrEditOrgCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 是否新增
        /// </summary>
        public bool IsAdd { get; set; } = false;

        /// <summary>
        /// 机构名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 副标题1
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 副标题2
        /// </summary>
        public string SubDesc { get; set; }

        /// <summary>
        /// LOGO
        /// </summary>
        public string LOGO { get; set; }

        /// <summary>
        /// 机构分类(Json格式)
        /// </summary>
        public string Types { get; set; }

        /// <summary>
        /// 好物分类(Json格式)
        /// </summary>
        public string GoodthingTypes { get; set; }


        public int[] BrandTypes { get; set; }

        /// <summary>
        /// 最小年龄
        /// </summary>
        public int? MinAge { get; set; }

        /// <summary>
        /// 最大年龄
        /// </summary>
        public int? MaxAge { get; set; }

        /// <summary>
        /// 教学模式(Json格式)
        /// </summary>
        public string Modes { get; set; }

        /// <summary>
        /// 机构简介
        /// </summary>
        public string Intro { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        public Guid? UserId { get; set; }

    }
}
