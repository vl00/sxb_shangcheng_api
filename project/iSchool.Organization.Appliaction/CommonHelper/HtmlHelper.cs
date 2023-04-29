using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    public class HtmlHelper
    {
        /// <summary>
        /// 替换HTML标记
        /// </summary>
        /// <param name="Htmlstring"></param>
        /// <returns></returns>
        public static string NoHTML(string Htmlstring)
        {

            if (string.IsNullOrEmpty(Htmlstring))
                return "";
            //删除脚本
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);

            //删除HTML
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<img[^>]*>;", "", RegexOptions.IgnoreCase);
            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");
            return Htmlstring;
        }

        /// <summary>
        /// 提取html中的文本
        /// </summary>        
        /// <returns></returns>
        public static string GetHtmlText(string htmlText, bool img = true)
        {
            if (string.IsNullOrWhiteSpace(htmlText)) return htmlText;

            string noStyle = htmlText.Replace("&quot;", "\"").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&nbsp;", "");
            if (img)
            {
                noStyle = Regex.Replace(noStyle, @"<\/?[p|(span)].*?\/?>", "", RegexOptions.IgnoreCase);
            }
            else
            {
                noStyle = Regex.Replace(noStyle, @"<\/?[p|b|(span)|img].*?\/?>", "", RegexOptions.IgnoreCase);
            }
            //noStyle = Regex.Replace(noStyle, @"\s", "", RegexOptions.IgnoreCase);
            return noStyle;
        }

        /// <summary>
        /// 获取百度快递单查询接口url from html
        /// </summary>
        /// <param name="htmlStr"></param>
        /// <param name="nu">运单号</param>
        /// <param name="error">错误</param>
        /// <returns>api url</returns>
        public static string GetBaiduKuaidiApiUrlFromHtml(string htmlStr, string nu, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(htmlStr))
            {
                error = "html不能为空";
                return null;
            }
            if (string.IsNullOrEmpty(nu))
            {
                error = "运单号不能为空";
                return null;
            }

            var html = htmlStr!.AsSpan();
            if (html.IndexOf($"<title>{HttpUtility.HtmlEncode(nu)}_百度搜索</title>") == -1)
            {
                error = "获取百度快递单查询接口失败";
                return null;
            }

            for (var tokenV2 = "tokenV2".AsSpan(); true;)
            {
                var i = html.IndexOf(tokenV2);
                if (i == -1) break;

                // try find start "
                var i0 = html[..i].LastIndexOfAny('\'', '"');
                if (i0 == -1) break;

                // try find end "
                var i1 = html[i..].IndexOfAny('\'', '"');
                if (i1 == -1) break;

                // check url
                var url = html[(i0 + 1)..(i + i1)];
                if (url.StartsWith("https://express.baidu.com/express/api/express?", StringComparison.OrdinalIgnoreCase))
                    return new string(url);

                // try next
                if (i + tokenV2.Length >= html.Length) break;
                html = html[(i + tokenV2.Length)..];
            }
            return string.Empty;
        }

    }
}
