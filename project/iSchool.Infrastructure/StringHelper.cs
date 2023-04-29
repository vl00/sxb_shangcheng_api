using System;
using System.Text;
using System.Text.RegularExpressions;

namespace iSchool
{
    public static partial class StringHelper
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
        }

        public static string FormatWith(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        public static string FormatWith(this string str, params (string k, object v)[] args)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str)) return str;
            foreach (var (k, v) in args)
            {
                str = str.Replace(!k.StartsWith('{') ? ("{" + k + "}") : k, v?.ToString());
            }
            return str;
        }

        public static StringBuilder AppendLine(this StringBuilder builder, string str, params object[] args)
        {
            return builder.AppendLine(str, args);
        }

        //eg: left join MaintenanceTemplates it on it.Id = m.MaintenanceTemplateId
        //    where m.IsDeleted = 0
        //    {" and m.Code = @KeyWord ".If(!string.IsNullOrWhiteSpace(input.KeyWord))}
        //    {" and m.ProjectId = @ProjectId ".If(input.ProjectId.HasValue)}
        //    {" and a.ProductId = @ProductId ".If(input.ProductId.HasValue)}
        public static string If(this string str, bool condition)
        {
            return condition ? str : string.Empty;
        }

        /// <summary>
        /// 获取html里面的text并且删除空格
        /// </summary>
        public static string GetHtmlText(this string htmlText)
        {
            string noStyle = htmlText.Replace("&quot;", "\"").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&nbsp;", "");
            noStyle = Regex.Replace(noStyle, @"<[\w\W]*?>", "", RegexOptions.IgnoreCase);
            noStyle = Regex.Replace(noStyle, @"\s", "", RegexOptions.IgnoreCase);
            return noStyle;
        }

        /// <summary>
        /// 截取HTML页面的HEAD相关
        /// <para>过滤 空格，回车，双引号，尖括号</para>
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="length">截取长度</param>
        /// <returns></returns>
        public static string GetHtmlHeaderString(this string input, int length)
        {
            var result = string.Empty;
            if (string.IsNullOrWhiteSpace(input) || length < 1) return input;
            //result = Regex.Replace(input, @"<\/?[p|(span)|b|img].*?\/?>", "", RegexOptions.IgnoreCase);
            result = input.ReplaceHtmlTag();
            result = result.Replace("\r", "").Replace("\n", "").Replace("\"", "").Replace("<", "")
                .Replace(">", "").Replace("“", "").Replace("”", "").Replace("*", "").Replace(" ", "").Replace("【", "").Replace("】", "");
            return result.GetShortString(length);
        }
        /// <summary>
        /// 去除HTML标签
        /// </summary>
        /// <param name="html">带有html标签的文本</param>
        /// <returns></returns>
        public static string ReplaceHtmlTag(this string html)
        {
            //var a = System.Web.HttpUtility.UrlDecode(html);
            //var b = System.Web.HttpUtility.UrlEncode(html);
            string strText = Regex.Replace(html, "<[^>]+>", "");
            strText = Regex.Replace(strText, "&[^;]+;", "");
            return strText;
        }
        /// <summary>
        /// 将字符串根据长度从头截取 , 加上...返回
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <param name="length">截取的长度`</param>
        /// <returns></returns>
        public static string GetShortString(this string input, int length)
        {
            var result = string.Empty;
            if (string.IsNullOrWhiteSpace(input) || length < 1) return input;
            if (input.Length > length)
            {
                result = input.Substring(0, length) + "...";
            }
            else
            {
                result = input;
            }
            return result;
        }
    }
}