using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    public static class ImgHelper
    {
        /// <summary>
        /// 生成评测封面图
        /// </summary>
        /// <param name="text"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static MemoryStream CreateEvltCover(string text, EvltCoverCreateOption option)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (text.Length > option.MaxStrLen) text = text[..(option.MaxStrLen + 1)];

            using var image = Image.FromFile(option.BgFile);
            using var bitmap = new Bitmap(image, image.Width, image.Height);
            using var g = Graphics.FromImage(bitmap);

            //下面定义一个矩形区域，以后在这个矩形里画上白底黑字
            float rectWidth = image.Width - option.RectX * 2; //200;  // text.Length * (fontSize + 40);
            float rectHeight = image.Height - option.RectY * 2; //fontSize + 40;
            RectangleF textArea = new RectangleF(option.RectX, option.RectY, rectWidth, rectHeight);
            //定义字体
            Font font = new Font(option.FontFamilyName, option.FontSize, FontStyle.Bold);               
            Brush fontBrush = new SolidBrush(option.FontColor);
            
            g.DrawString(text, font, fontBrush, textArea, StringFormat.GenericDefault);

            //设置 System.Drawing.Graphics对象的SmoothingMode属性为HighQuality 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //下面这个也设成高质量 
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //下面这个设成High 
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            return ms;
        }
    }
}
