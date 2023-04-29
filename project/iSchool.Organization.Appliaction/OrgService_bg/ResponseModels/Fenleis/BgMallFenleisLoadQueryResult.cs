using iSchool.Organization.Appliaction.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ResponseModels
{
    public class BgMallFenleisLoadQueryResult
    {
        /// <summary>1级分类s</summary>
        public IEnumerable<BgMallFenleiItemDto> D1s { get; set; } = default;
        /// <summary>2级s</summary>
        public IEnumerable<BgMallFenleiItemDto> D2s { get; set; } = null;
        /// <summary>3级s</summary>
        public IEnumerable<BgMallFenleiItemDto> D3s { get; set; } = null;

        /// <summary>选中的1级</summary>
        public BgMallFenleiItemDto Selected_d1 { get; set; } = null;
        /// <summary>选中的2级</summary>
        public BgMallFenleiItemDto Selected_d2 { get; set; } = null;
        /// <summary>选中的3级</summary>
        public BgMallFenleiItemDto Selected_d3 { get; set; } = null;



        public void SetLs(int depth, IEnumerable<BgMallFenleiItemDto> ls)
        {
            switch (depth)
            {
                case 1:
                    this.D1s = ls;
                    break;
                case 2:
                    this.D2s = ls;
                    break;
                case 3:
                    this.D3s = ls;
                    break;
            }
        }

        public void SetSeleted(BgMallFenleiItemDto dto)
        {
            switch (dto?.Depth)
            {
                case 1:
                    this.Selected_d1 = dto;
                    break;
                case 2:
                    this.Selected_d2 = dto;
                    break;
                case 3:
                    this.Selected_d3 = dto;
                    break;
            }
        }
    }
}
