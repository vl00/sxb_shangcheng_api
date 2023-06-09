﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;

namespace iSchool.Infrastructure.Download
{
    /// <summary>
    /// 下载文件
    /// </summary>
    public class DownloadHelper
    {
        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="picUrl">图片Http地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="timeOut">Request最大请求时间，如果为-1则无限制</param>
        /// <returns></returns>
        public static bool DownloadPicture(string picUrl, string savePath, int timeOut)
        {
            //picUrl = "http://203.156.245.58/sipgl/login/img";
            savePath = "C:/img/" + DateTime.Now.ToString("HHmmssffff") + ".jpg";
            bool value = false;
            WebResponse response = null;
            Stream stream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(picUrl);
                if (timeOut != -1) request.Timeout = timeOut;
                response = request.GetResponse();
                stream = response.GetResponseStream();
                if (!response.ContentType.ToLower().StartsWith("text/"))
                    value = SaveBinaryFile(response, savePath);
            }
            finally
            {
                if (stream != null) stream.Close();
                if (response != null) response.Close();
            }
            return value;
        }

        private static bool SaveBinaryFile(WebResponse response, string savePath)
        {
            bool value = false;
            byte[] buffer = new byte[1024];
            Stream outStream = null;
            Stream inStream = null;
            try
            {                
                using (outStream = System.IO.File.Create(savePath))
                {
                    inStream = response.GetResponseStream();
                    int l;
                    do
                    {
                        l = inStream.Read(buffer, 0, buffer.Length);
                        if (l > 0) outStream.Write(buffer, 0, l);
                    } while (l > 0);
                    value = true;
                }

            }
            catch(Exception ex)
            {

            }
            finally
            {
                if (outStream != null) outStream.Close();
                if (inStream != null) inStream.Close();
            }
            return value;
        }

        //方法三 根据路径下载图片
        public Image GetImage(string url, out string imageStrCookie)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            imageStrCookie = "";
            if (response.Headers.HasKeys() && null != response.Headers["Set-Cookie"])
            {
                imageStrCookie = response.Headers.Get("Set-Cookie");
            }
            return Image.FromStream(response.GetResponseStream());

        }
    }
}
