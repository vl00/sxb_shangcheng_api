using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction
{
    /// <summary>
    /// 生成评测封面图
    /// </summary>
    public class EvltCoverCreateOption
    {
        /// <summary>
        /// 背景图
        /// </summary>
        public string BgFile { get; set; }
        /// <summary>
        /// 最大字数
        /// </summary>
        public int MaxStrLen { get; set; }

        /// <summary>
        /// 字体大小
        /// </summary>
        public float FontSize { get; set; }
        
        public float RectX { get; set; }
        public float RectY { get; set; }

        /// <summary>
        /// 字体名
        /// </summary>
        public string FontFamilyName { get; set; }
        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color FontColor { get; set; }
    }
}
