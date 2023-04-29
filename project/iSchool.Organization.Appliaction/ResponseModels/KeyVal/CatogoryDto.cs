using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.KeyVal
{
    public class CatogoryDto
    {
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public long LastUpdateTime { get; set; }
        /// <summary>
        ///  一级
        /// </summary>
        public List<RootCatogoryVm> RootCatogoryList { get; set; }
    }
    public class RootCatogoryVm
    {
        /// <summary>
        /// 键
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 通用附加值。如好物类别的图片
        /// </summary>
        public string Attach { get; set; }
        /// <summary>
        /// 板块属于第几级
        /// </summary>
        public int Depth { get; set; }
        /// <summary>
        /// 下级板块
        /// </summary>

        public List<LevelSecondCatogoryVm>  Children { get; set; }
        /// <summary>
        /// 因为没有底级分类不显示
        /// </summary>
        public bool NotShow { get; set; } = false;

    }
    public class LevelSecondCatogoryVm
    {
        /// <summary>
        /// 键
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 通用附加值。如好物类别的图片
        /// </summary>
        public string Attach { get; set; }
        /// <summary>
        /// 板块属于第几级
        /// </summary>
        public int Depth { get; set; }
        /// <summary>
        /// 下级板块
        /// </summary>

        public List<LevelThreeCatogoryVm> Children { get; set; }

    }
    public class LevelThreeCatogoryVm
    {
        /// <summary>
        /// 键
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 通用附加值。如好物类别的图片
        /// </summary>
        public string Attach { get; set; }

        /// <summary>
        /// 板块属于第几级
        /// </summary>
        public int Depth { get; set; }
    }
}
