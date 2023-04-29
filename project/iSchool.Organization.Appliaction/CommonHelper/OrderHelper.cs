using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class OrderHelper
    {
        /// <summary>
        /// 用于前端显示
        /// </summary>
        /// <param name="orderStatus"></param>
        /// <returns></returns>
        public static string GetStatusDesc4Front(OrderStatusV2 orderStatus)
        {
            return orderStatus switch
            {
                OrderStatusV2.Paid => OrderStatusV2.Ship.GetDesc(),
                OrderStatusV2.Shipped => OrderStatusV2.Completed.GetDesc(),
                _ => orderStatus.GetDesc(),
            };
        }

        /// <summary>
        /// 支付方式 用于前端显示
        /// </summary>
        /// <param name="paymentType"></param>
        /// <returns></returns>
        public static string GetPaytypeDesc0(PaymentType paymentType)
        {
            var name = paymentType.GetName();
            var prex = name.IndexOf('_') switch
            {
                -1 => name,
                int i => name[..i],
            };
            return prex.ToEnum<PaymentType>().GetDesc();
        }

        /// <summary>
        /// 使用automapper转换会报错??
        /// </summary>
        public static CourseOrderProdItemDto ConvertTo_CourseOrderProdItemDto(OrderDetial orderDetail, CourseOrderProdItemDto item = null)
        {
            item ??= new CourseOrderProdItemDto();
            var ctn = orderDetail.Ctn.IsNullOrEmpty() ? null : JObject.Parse(orderDetail.Ctn);
            item._ctn = ctn;
            item.Id = orderDetail.Courseid;
            item.Id_s = ctn["no"]?.ToString() is string _no && long.TryParse(_no, out var _lno) ? UrlShortIdUtil.Long2Base32(_lno) : null;
            item.Title = ctn["title"]?.ToString();
            item.Subtitle = ctn["subtitle"]?.ToString();
            item.ProdType = orderDetail.Producttype;
            item.Price = orderDetail.Price;
            item.Origprice = orderDetail.Origprice;
            item.BuyCount = orderDetail.Number; //
            item.OrderDetailId = orderDetail.Id;
            item.GoodsId = orderDetail.Productid;
            item._Ver = ctn["_Ver"]?.ToString();
            item.Banner = ctn["banner"]?.ToString() is string _banner && !_banner.IsNullOrEmpty() ? new[] { _banner } : new string[0];
            item.PropItemIds = ctn["propItemIds"]?.ToObject<Guid[]>();
            item.PropItemNames = ctn["propItemNames"]?.ToObject<string[]>();
            item.Status = orderDetail.Status;
            item.StatusDesc = EnumUtil.GetDesc((OrderStatusV2)item.Status);
            item.OrgInfo = new CourseOrderProdItem_OrgItemDto();
            item.OrgInfo.Id = Guid.TryParse(ctn["orgId"]?.ToString(), out var _oid) ? _oid : Guid.Empty;
            item.OrgInfo.Id_s = long.TryParse(ctn["orgNo"]?.ToString(), out var _ono) ? UrlShortIdUtil.Long2Base32(_ono) : null;
            item.OrgInfo.Name = ctn["orgName"]?.ToString();
            item.OrgInfo.Logo = ctn["orgLogo"]?.ToString();
            item.OrgInfo.Desc = ctn["orgDesc"]?.ToString();
            item.OrgInfo.Subdesc = ctn["orgSubdesc"]?.ToString();
            item.OrgInfo.Authentication = bool.TryParse(ctn["authentication"]?.ToString(), out var _auth) && _auth;
            item.SupplierInfo = new CourseOrderProdItem_SupplierInfo();
            item.SupplierInfo.Id = Guid.TryParse(ctn["supplierId"]?.ToString(), out var _sid) ? _sid : Guid.Empty;
            item.NewUserExclusive = bool.TryParse(ctn["isNewUserExclusive"]?.ToString(), out var _NewUserExclusive) && _NewUserExclusive;
            return item;
        }

        /// <summary>
        /// 获取course的tags
        /// </summary>
        public static List<string> GetTagsFromCourse(Course course)
        {
            if (course == null) return null;

            var tags = new List<string>();

            //年龄标签
            if (course.Minage != null && course.Maxage != null)
            {
                tags.Add($"{course.Minage}-{course.Maxage}岁");
            }
            else if (course.Minage != null && course.Maxage == null)
            {
                tags.Add($"大于{course.Minage}岁");
            }
            else if (course.Maxage != null && course.Minage == null)
            {
                tags.Add($"小于{course.Maxage}岁");
            }

            //科目标签
            if (course.Subject != null)
                tags.Add(EnumUtil.GetDesc((SubjectEnum)course.Subject.Value));

            //低价体验
            if (course.Price != null && course.Price <= 10)
                tags.Add("低价体验");
            if (course.NewUserExclusive)
                tags.Add("新人专享");
            if (course.CanNewUserReward)
                tags.Add("新人立返");
            if (course.LimitedTimeOffer)
                tags.Add("限时补贴");

            return tags;
        }

        /// <summary>
        /// 提交wx预支付前组装参数
        /// </summary>
        /// <param name="product"></param>
        /// <param name="advanceOrderId"></param>
        /// <returns></returns>
        public static List<ApiOrderByProduct> OrderDetailSpread2ApiOrderByProducts(OrderDetial product, Guid advanceOrderId)
        {
            return product.PaymentSpreadPrice().Select(s => new ApiOrderByProduct
            {
                ProductId = product.Productid,
                ProductType = product.Producttype,
                Status = product.Status,
                Amount = s.unitPrice * s.number,
                Remark = product.Remark,
                BuyNum = s.number,
                Price = s.unitPrice,
                AdvanceOrderId = advanceOrderId,
                OrderDetailId = product.Id,
                OrderId = product.Orderid,
            }).ToList();
        }
    }
}
