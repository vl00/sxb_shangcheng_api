using Dapper.Contrib.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    [Dapper.Contrib.Extensions.Table("CouponInfo")]
    public class CouponInfo : Entity, IAggregateRoot
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public CouponInfoVaildDateType VaildDateType { get; set; }
        public DateTime? VaildStartDate { get; set; }
        public DateTime? VaildEndDate { get; set; }

        /// <summary>
        /// Unit = Hours
        /// </summary>
        public double VaildTime { get; private set; }

        public int MaxTake { get; set; }
        public int Stock { get; private set; }

        public int Total { get; set; }
        public CouponType CouponType { get; set; }
        public decimal? Fee { get; set; }
        public decimal? FeeOver { get; set; }
        public decimal? Discount { get; set; }
        /// <summary>
        /// 体验价格
        /// </summary>
        public decimal? PriceOfTest { get; set; }
        public DateTime? GetStartTime { get; set; }
        public DateTime? GetEndTime { get; set; }
        public decimal? MaxFee { get; private set; }

        public string Remark { get; set; }

        /// <summary>
        /// 1 上线 0 下线
        /// </summary>
        public CouponInfoState Status { get; private set; } = CouponInfoState.Online;
        public bool? CanMultiple { get; set; }
        public string Link { get; set; }
        public DateTime? CreateTime { get; set; }
        public Guid Creator { get; set; }

        public Guid Updator { get; set; }

        public DateTime? UpdateTime { get; set; }


        public bool? CanBack { get; set; }

        public string ICon { get; set; }

        //public bool IsBindUse { get; set; }
        public bool IsHide { get; set; }

        [Write(false)]
        public int Number { get; set; }

        public string KeyWord { get; private set; }

        public string EnableRange_JSN { get; private set; }



        public static CouponInfo CreateFrom(Guid id, CouponType couponType, decimal? feeover, decimal? fee, decimal? discount, decimal? priceOftest, string enableRange_JSN)
        {
            return new CouponInfo()
            {
                Id = id,
                CouponType = couponType,
                FeeOver = feeover,
                Fee = fee,
                Discount = discount,
                PriceOfTest = priceOftest,
                EnableRange_JSN = enableRange_JSN
            };
        }

        public static CouponInfo CreateFrom(Guid id, CouponType couponType, decimal? feeover, decimal? fee, decimal? discount, decimal? priceOftest)
        {
            return new CouponInfo()
            {
                Id = id,
                CouponType = couponType,
                FeeOver = feeover,
                Fee = fee,
                Discount = discount,
                PriceOfTest = priceOftest,
            };
        }


        public double GetValidDays()
        {
            return TimeSpan.FromHours(VaildTime).TotalDays;
        }

        public void SetVaildTime(double days)
        {
            VaildTime = TimeSpan.FromDays(days).TotalHours;
        }

        public void SetMaxFee(CouponType couponType, decimal? fee, decimal? feeover, decimal? testPrice)
        {
            if (couponType == CouponType.TiYan) MaxFee = null;
            if (couponType == CouponType.LiJian) MaxFee = fee;
            if (couponType == CouponType.ManJian) MaxFee = fee;
            if (couponType == CouponType.ZheKou) MaxFee = null;
            MaxFee = null;

        }


        public void SetStatus(CouponInfoState state) {
            if (state == CouponInfoState.Online)
            {
                if (this.Status == CouponInfoState.Offline || this.Status == CouponInfoState.LoseEfficacy)
                {
                    throw new Exception("上线状态不能从过期状态流转过来。");
                }
            }
            this.Status = state;
        }




        public IEnumerable<EnableRange> GetEnableRanges()
        {
            return EnableRange.GetEnableRangesFromJson(this.EnableRange_JSN);
        }
        public void SetEnableRanges(string enableRangesJson)
        {
            IEnumerable<EnableRange> enableRanges = EnableRange.Creates(enableRangesJson);
            this.SetEnableRanges(enableRanges);

        }
        /// <summary>
        /// 对可用范围进行校验，并且将其set进EnableRange_JSN字段，同时更新其keyword 字段。
        /// </summary>
        /// <param name="enableRanges"></param>
        public void SetEnableRanges(IEnumerable<EnableRange> enableRanges)
        {
            List<string> appendKeyWords = new List<string>();
            foreach (var enableRange in enableRanges)
            {
                if (enableRange is SKUEnableRange)
                {
                    SKUEnableRange skuEnableRange = enableRange as SKUEnableRange;
                    foreach (var item in skuEnableRange.SKUItems)
                    {
                        if (!string.IsNullOrEmpty(item.CourseName))
                        {
                            appendKeyWords.Add(item.CourseName);

                        }
                    }

                }
                else if (enableRange is CourseBrandEnableRange)
                {
                    CourseBrandEnableRange courseBrandEnableRange = enableRange as CourseBrandEnableRange;
                    if (!string.IsNullOrEmpty(courseBrandEnableRange.Name))
                    {
                        appendKeyWords.Add(courseBrandEnableRange.Name);

                    }
                }
                else if (enableRange is GoodsTypeEnableRange)
                {

                }
            }
            this.EnableRange_JSN = Newtonsoft.Json.JsonConvert.SerializeObject(enableRanges);
            if (!string.IsNullOrEmpty(KeyWord))
            {
                var existsKeyWords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(KeyWord);
                if (existsKeyWords.Any())
                {
                    appendKeyWords.AddRange(existsKeyWords);
                }
            }
            this.KeyWord = Newtonsoft.Json.JsonConvert.SerializeObject(appendKeyWords.Distinct());
        }

        /// <summary>
        /// 券本身特性的使用检查
        /// </summary>
        /// <returns></returns>
        public (bool res, string msg) UseCheckCouponType(decimal totalprice)
        {
            //需要作券本身的特性限制。
            //目前仅有满减券
            if (this.CouponType == CouponType.ManJian)
            {
                if (totalprice < this.FeeOver)
                {
                    return (false, "未达到满减条件");
                }
            }
            return (true, null);
        }



        public void InitialStock(int count)
        {
            this.Stock = count;
            this.Total = count;
        }


        /// <summary>
        /// 计算优惠金额(注意，这里返回的是优惠金额，并非支付价格)
        /// </summary>
        /// <param name="price"></param>
        public (decimal totalPrice, decimal couponAmount,IEnumerable<BuySKU> useInWhatSkus) ComputeAmount(IEnumerable<BuySKU> buySKUs)
        {
            if (buySKUs == null || !buySKUs.Any()) return (0, 0, buySKUs);
            decimal totalPrice = 0;
            decimal expect = 0; //优惠金额
            List<BuySKU> useInWhatSkus = new List<BuySKU>();    //用在了那些SKU上面
            switch (CouponType)
            {
                case CouponType.TiYan:
                    //规则：多SKU符合体验券使用范围时，优先则单价最低的抵扣。
                    var cheepSku = buySKUs.Where(b=>b.Number == 1).OrderBy(b => b.UnitPrice).FirstOrDefault();
                    if (cheepSku != null)
                    {
                        totalPrice = cheepSku.UnitPrice * cheepSku.Number;
                        //如果总价小于体验价，那么优惠个寂寞
                        if (totalPrice < this.PriceOfTest.GetValueOrDefault())
                        {
                            expect = 0;
                        }
                        else {
                            expect = (totalPrice) - this.PriceOfTest.GetValueOrDefault();
                        }                        useInWhatSkus.Add(cheepSku);
                    }
                    break;
                case CouponType.ZheKou:
                    totalPrice = buySKUs.Sum(b => b.UnitPrice * b.Number);
                    expect = totalPrice - (totalPrice * this.Discount.GetValueOrDefault());
                    useInWhatSkus.AddRange(buySKUs);
                    break;
                case CouponType.ManJian:
                    totalPrice = buySKUs.Sum(b => b.UnitPrice * b.Number);
                    if (totalPrice >= this.FeeOver)
                    {
                        expect = this.Fee.GetValueOrDefault();
                        useInWhatSkus.AddRange(buySKUs);
                    }
                    break;
                case CouponType.LiJian:
                    totalPrice = buySKUs.Sum(b => b.UnitPrice * b.Number);
                    expect = this.Fee.GetValueOrDefault();
                    useInWhatSkus.AddRange(buySKUs);
                    break;
                default:
                    return (0,0, useInWhatSkus);
            }
            if (expect < 0 || totalPrice <= expect)
                expect = totalPrice - 0.00M;     
            return (totalPrice, (decimal)((int)(expect * 100)) / 100, useInWhatSkus); //去掉厘单位

        }



        public void ReduceStock(int count)
        {
            if (this.Stock <= 0)
            {
                throw new Exception($"库存已为0。CouponId = {this.Id}");
            }
            int remain = this.Stock - count;
            if (remain < 0)
            {
                throw new Exception($"库存不足。CouponId = {this.Id}");
            }
            this.Stock = remain;

        }

        /// <summary>
        /// 是否为品牌券
        /// </summary>
        /// <returns></returns>
        public bool IsBrandCoupon() {
            return this.GetEnableRanges().Any(s => s.Type == CouponEnableRangeType.SpcialBrand);
        }


        /// <summary>
        /// 对SKU进行可用判定（不仅仅对指定范围、还有体验券特性以及满减券的特性）
        /// 这里有三种情况
        /// 第一种，购买的SKU全部能用
        /// 第二种，购买的SKU只有部分可用
        /// 第三中，购买的SKU全部不可用
        /// </summary>
        /// <param name="buySKUs">输出能用券的SKU</param>
        /// <returns></returns>
        public IEnumerable<BuySKU> EnableRangeJudge(IEnumerable<BuySKU> buySKUs)
        {

            var enableRanges = this.GetEnableRanges();
            List<BuySKU> passEnableRangeVertifys = new List<BuySKU>();
            if (enableRanges.Any())
            {
                foreach (var enableRange in enableRanges)
                {
                    if (enableRange is SKUEnableRange)
                    {
                        var skuEnableRange = enableRange as SKUEnableRange;
                        if (skuEnableRange.IsBindUse)
                        {
                            var enableSKUIdGroups = skuEnableRange.SKUItems.Select(s => s.Id);
                            bool canPass = true;
                            foreach (var item in enableSKUIdGroups)
                            {
                                if (!buySKUs.Any(b => b.SKUId == item))
                                {
                                    canPass = false;
                                    break;
                                }
                            }
                            if (canPass)
                            {
                                passEnableRangeVertifys.AddRange(buySKUs.Where(b => enableSKUIdGroups.Any(i => i == b.SKUId)));
                            }
                        }
                        else
                        {
                            var enableSKUIdGroups = skuEnableRange.SKUItems.Select(s => s.Id);
                            passEnableRangeVertifys.AddRange(buySKUs.Where(b => enableSKUIdGroups.Any(i => i == b.SKUId)));
                        }
                        continue;
                    }
                    if (enableRange is CourseBrandEnableRange)
                    {
                        var courseBrandEnableRange = enableRange as CourseBrandEnableRange;
                        passEnableRangeVertifys.AddRange(buySKUs.Where(b => courseBrandEnableRange.Id == b.BrandId));

                        continue;
                    }
                    if (enableRange is GoodsTypeEnableRange)
                    {
                        var goodsTypeEnableRange = enableRange as GoodsTypeEnableRange;
                        passEnableRangeVertifys.AddRange(buySKUs.Where(b =>
                        {
                            if (b.GoodTypes == null || !b.GoodTypes.Any(g => g == goodsTypeEnableRange.Id))
                                return false;
                            else
                                return true;
                        }));
                        continue;
                    }
                }
                passEnableRangeVertifys = passEnableRangeVertifys.Distinct().ToList(); ;
            }
            else {
                passEnableRangeVertifys.AddRange(buySKUs);
            }
            //券特性判断
            if (passEnableRangeVertifys.Any())
            {
                if (this.CouponType == CouponType.ManJian)
                {
                    decimal totalPrice = passEnableRangeVertifys.Sum(p => p.UnitPrice * p.Number);
                    if (this.FeeOver > totalPrice)
                    {
                        //不满足满减特性，券不可用
                        passEnableRangeVertifys.Clear();
                    }
                }
                else if (this.CouponType == CouponType.TiYan)
                {
                    if (!passEnableRangeVertifys.Any(b => b.Number == 1))
                    {
                        //没有数量为1的购买商品，不可用
                        passEnableRangeVertifys.Clear();
                    }
                }
            }


            return passEnableRangeVertifys;
        }


        public IEnumerable<BuySKU> CanUseJudge(IEnumerable<BuySKU> buySKUs)
        {

            List<BuySKU> buys = buySKUs.ToList();

            //券特性判断
            if (buySKUs.Any())
            {
                if (this.CouponType == CouponType.ManJian)
                {
                    decimal totalPrice = buySKUs.Sum(p => p.UnitPrice * p.Number);
                    if (this.FeeOver > totalPrice)
                    {
                        //不满足满减特性，券不可用
                        buys.Clear();
                    }
                }
                else if (this.CouponType == CouponType.TiYan)
                {
                    if (!buys.Any(b => b.Number == 1))
                    {
                        //没有数量为1的购买商品，不可用
                        buys.Clear();
                    }
                }
            }
            return buys;
        }






        /// <summary>
        /// 到底有那些购买的SKU能使用我这张券，你尽管传一堆SKU进来，看看符不符合。
        /// </summary>
        /// <param name="buySKUs"></param>
        /// <returns></returns>
        public (IEnumerable<BuySKU>,decimal couponAmount,decimal totalPrice) WhatCanIUseInBuySKUs(IEnumerable<BuySKU> buySKUs)
        {
            var passEnableRangeVertifys = EnableRangeJudge(buySKUs);
            if (passEnableRangeVertifys.Any())
            {
                //计算出这批SKU的总价
                var (totalPrice, couponAmount,useInWhatSkus) = ComputeAmount(passEnableRangeVertifys);
                return (useInWhatSkus, couponAmount, totalPrice);

            }
            return (passEnableRangeVertifys, 0, 0);

          
        }

        public (IEnumerable<BuySKU>, decimal couponAmount, decimal totalPrice) WhatCanIUseInBuySKUsNoEnableRangeJudge(IEnumerable<BuySKU> buySKUs)
        {
            var passEnableRangeVertifys = CanUseJudge(buySKUs);
            if (passEnableRangeVertifys.Any())
            {
                //计算出这批SKU的总价
                var (totalPrice, couponAmount, useInWhatSkus) = ComputeAmount(passEnableRangeVertifys);
                return (useInWhatSkus, couponAmount, totalPrice);

            }
            return (passEnableRangeVertifys, 0, 0);


        }

    }





}
