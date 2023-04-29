using CSRedis;
using iSchool.Domain.Enum;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.KeyVal;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 下拉框数据源管理
    /// </summary>
    [Route("api")]
    [ApiController]
    public class SelectItemsController : Controller
    {
        private IMediator _mediatR;
        private CSRedisClient _redisClient;

        public SelectItemsController(IMediator mediatR, CSRedisClient redisClient)
        {
            _mediatR = mediatR;
            _redisClient = redisClient;
        }

        /// <summary>
        /// 科目列表
        /// </summary>
        /// <returns>返回科目列表</returns>
        [HttpGet("Sbjects")]
        [ProducesResponseType(typeof(List<SelectItemsKeyValues>), 200)]
        public ResponseResult GetAllSbject()
        {
            return _mediatR.Send(new KeyValueSelectItemsQuery() {  Type=1}).Result;
        }
        /// <summary>
        /// 首页课程分类导航
        /// </summary>
        /// <returns>返回首页课程分类导航</returns>
        [HttpGet("IdxCourseSbjects")]
        [ProducesResponseType(typeof(List<SelectItemsKeyValues>), 200)]
        public ResponseResult GetAlIdxCourseSbjects()
        {
            return _mediatR.Send(new KeyValueSelectItemsQuery() { Type = 15 }).Result;
        }
        /// <summary>
        /// 机构分类列表
        /// </summary>
        /// <returns>返回科目列表</returns>
        [HttpGet("OrgType")]
        [ProducesResponseType(typeof(List<SelectItemsKeyValues>), 200)]
        public ResponseResult GetAllOrgType()
        {
            return _mediatR.Send(new KeyValueSelectItemsQuery() { Type = 0 }).Result;
        }
        /// <summary>
        /// 好物分类
        /// </summary>
        /// <returns></returns>
       
        [HttpGet("GoodThing")]
        [ProducesResponseType(typeof(List<SelectItemsKeyValues>), 200)]
        public ResponseResult GetAllGoodThingType()
        {
            return _mediatR.Send(new KeyValueSelectItemsQuery() { Type = 15}).Result;
        }
        /// <summary>
        /// 订单状态列表
        /// </summary>
        /// <returns>返回订单状态列表</returns>
        [HttpGet("OrderStatus")]
        [ProducesResponseType(typeof(List<SelectItemsKeyValues>), 200)]
        public ResponseResult GetAllOrderStatus()
        {
            string key = string.Format(CacheKeys.selectItems, "orderstatus");
            var data = _redisClient.Get<List<SelectItemsKeyValues>>(key);
            if(data!=null && data.Count > 0)
            {
                return ResponseResult.Success(data);
            }
            else
            {
                data = EnumUtil.GetSelectItems2<OrderStatus>();
                _redisClient.Set(key, data);
                return ResponseResult.Success(data);
            }
            
        }

        /// <summary>
        /// 年龄段列表
        /// </summary>
        /// <returns>返回年龄段列表</returns>
        [HttpGet("AgeGroups")]
        [ProducesResponseType(typeof(List<SelectItemsKeyValues>), 200)]
        public ResponseResult GetAgeGroups()
        {

            string key = string.Format(CacheKeys.selectItems, "agegroups");
            var data = _redisClient.Get<List<SelectItemsKeyValues>>(key);
            if (data != null && data.Count > 0)
            {
                return ResponseResult.Success(data);
            }
            else
            {
                data = EnumUtil.GetSelectItems2<AgeGroup>().OrderBy(item=>item.Sort).ToList();
                _redisClient.Set(key, data);
                return ResponseResult.Success(data);
            }            
        }

        /// <summary>
        /// 精品课程筛选排序条件
        /// </summary>
        /// <returns></returns>
        [HttpGet("CourseSortType")]
        [ProducesResponseType(typeof(List<SelectItemsKeyValues>), 200)]
        public ResponseResult GetCourseSortType()
        {

            string key = string.Format(CacheKeys.selectItems, "coursesorttype");
            var data = _redisClient.Get<List<SelectItemsKeyValues>>(key);
            if (data != null && data.Count > 0)
            {
                return ResponseResult.Success(data);
            }
            else
            {
                data = EnumUtil.GetSelectItems2<CourseFilterSortType>().OrderBy(item => item.Sort).ToList();
                _redisClient.Set(key, data);
                return ResponseResult.Success(data);
            }
        }


        /// <summary>
        /// 精品课程筛选过滤类型
        /// </summary>
        /// <returns>返回年龄段列表</returns>
        [HttpGet("CourseFilterType")]
        [ProducesResponseType(typeof(List<SelectItemsKeyValues>), 200)]
        public ResponseResult GetCourseFilterType()
        {

            string key = string.Format(CacheKeys.selectItems, "coursefiltertype");
            var data = _redisClient.Get<List<SelectItemsKeyValues>>(key);
            if (data != null && data.Count > 0)
            {
                return ResponseResult.Success(data);
            }
            else
            {
                data = EnumUtil.GetSelectItems2<CourseFilterCutomizeType>().OrderBy(item => item.Sort).ToList();
                _redisClient.Set(key, data);
                return ResponseResult.Success(data);
            }
        }
        /// <summary>
        /// 程筛选过滤类型
        /// </summary>
        /// <returns>返回年龄段列表</returns>
        [HttpGet("CourseFilterTypeV1")]
        [ProducesResponseType(typeof(List<SelectItemsKeyValues>), 200)]
        public ResponseResult GetCourseFilterTypeV1()
        {

            string key = string.Format(CacheKeys.selectItems, "coursefiltertypev1");
            var data = _redisClient.Get<List<SelectItemsKeyValues>>(key);
            if (data != null && data.Count > 0)
            {
                return ResponseResult.Success(data);
            }
            else
            {
                data = EnumUtil.GetSelectItems2<CourseFilterCutomizeTypeV1>().OrderBy(item => item.Sort).ToList();
                _redisClient.Set(key, data);
                return ResponseResult.Success(data);
            }
        }
        /// <summary>
        /// 好物分类
        /// </summary>
        /// <returns></returns>

        [HttpGet("CatogryQuery")]
        [ProducesResponseType(typeof(CatogoryDto), 200)]
        public ResponseResult GetAllGoodThingTypeMuchLevel()
        {
            return _mediatR.Send(new CatogryQuery() {Root=1}).Result;
        }
        /// <summary>
        /// 种草圈的好物分类
        /// </summary>
        /// <returns></returns>
        [HttpGet("GrassCatogryQuery")]
        [ProducesResponseType(typeof(CatogoryDto), 200)]
        public ResponseResult GetGrassAllGoodThingType()
        {
            return _mediatR.Send(new CatogryQuery() { Root=2}).Result;
        }
    }
}
