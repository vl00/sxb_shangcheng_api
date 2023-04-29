using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    public class QRCodeHelper 
    {

        #region 普通二维码
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">存储内容</param>
        /// <param name="pixel">像素大小</param>
        /// <returns></returns>
        public static Bitmap GetPTQRCode(string url, int pixel)
        {
            using QRCodeGenerator generator = new QRCodeGenerator();
            using QRCodeData codeData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M, true);
            using QRCoder.QRCode qrcode = new QRCoder.QRCode(codeData);
            Bitmap qrImage = qrcode.GetGraphic(pixel, Color.Black, Color.White, true);
            return qrImage;
        }

        /// <summary>
        /// 普通二维码(base64)
        /// </summary>
        /// <param name="url">存储内容</param>
        /// <param name="pixel">像素大小</param>
        /// <returns></returns>
        public static string GetNormalBase64Qrcode(string url, int pixel)
        {
            using var bitmap = GetPTQRCode(url, pixel);
            return ImgToBase64String(bitmap);
        }
        #endregion

        #region 带logo的二维码
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">存储内容</param>
        /// <param name="pixel">像素大小</param>
        /// <param name="logoPath">logod地址</param>
        /// <returns></returns>
        public static string GetLogoQRCode(string url, string logoPath, int pixel)
        {
            using QRCodeGenerator generator = new QRCodeGenerator();
            using QRCodeData codeData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M, true);
            using QRCoder.QRCode qrcode = new QRCoder.QRCode(codeData);
            using Bitmap icon = new Bitmap(logoPath);
            using Bitmap qrImage = qrcode.GetGraphic(pixel, Color.Black, Color.White, icon, 15, 6, true);
            #region 参数介绍
            //GetGraphic方法参数介绍
            //pixelsPerModule //生成二维码图片的像素大小 ，我这里设置的是5
            //darkColor       //暗色   一般设置为Color.Black 黑色
            //lightColor      //亮色   一般设置为Color.White  白色
            //icon             //二维码 水印图标 例如：Bitmap icon = new Bitmap(context.Server.MapPath("~/images/zs.png")); 默认为NULL ，加上这个二维码中间会显示一个图标
            //iconSizePercent  //水印图标的大小比例 ，可根据自己的喜好设置
            //iconBorderWidth  // 水印图标的边框
            //drawQuietZones   //静止区，位于二维码某一边的空白边界,用来阻止读者获取与正在浏览的二维码无关的信息 即是否绘画二维码的空白边框区域 默认为true
            #endregion
            return ImgToBase64String(qrImage);
        }
       
        private static string ImgToBase64String(Bitmap bmp)
        {
            using MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            return "data:image/jpeg;base64," + Convert.ToBase64String(arr);
        }

        #endregion
    }
}
