using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Courses
{
    public class CoursesItem
    {

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 课程短Id
        /// </summary>
        public string Id_s { get; set; }

        public long No { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrgId { get; set; }

        /// <summary>
        /// 机构
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public string SupplierNames { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程副标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 科目Id
        /// </summary>
        public int? SubjectId { get; set; }

        /// <summary>
        /// 科目（json/name）
        /// </summary>
        public string Subjects { get; set; }

        /// <summary>
        /// 好物分类（json/name）
        /// </summary>
        public string GoodthingTypes { get; set; }

        public string SubjectsOrGoodthingTypes { get; set; }

        /// <summary>
        /// 类型1=课程2=好物
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 销量
        /// </summary>
        public int SellCount { get; set; }

        /// <summary>
        /// 总量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 退单数
        /// </summary>
        public int ChargebackCount { get; set; }

        /// <summary>
        /// 状态(1:上架；0：下架)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 上架时间(格式：yyy/MM/dd)
        /// </summary>
        public string LastOnShelfTime { get; set; }

        /// <summary>
        /// 下架时间(格式：yyy/MM/dd)
        /// </summary>
        public string LastOffShelfTime { get; set; }

        /// <summary>
        /// 是否隐形上架
        /// </summary>
        public bool IsInvisibleOnline { get; set; }
    }
}
