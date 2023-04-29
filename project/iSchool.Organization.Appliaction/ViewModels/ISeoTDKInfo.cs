using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction
{
    /// <summary>
    /// seo TDK
    /// </summary>
    public interface ISeoTDKInfo
    {
        /// <summary>TDK d '&#60;meta name="Description" &#62;'</summary>
        string Tdk_d { get; set; }

        /**
        /// <summary>TDK t '&#60;title&#62;'</summary>
        string Tdk_t { get; set; }
        /// <summary>TDK k '&#60;meta name="Keywords" &#62;'</summary>
        string Tdk_k { get; set; }
        */
    }

    public static class SeoTDKInfoUtil
    {
        public static string GetTDK(ISeoTDKInfo model) => TryGetTDK(model, out var tdk_d) ? tdk_d : null;

        public static bool TryGetTDK(ISeoTDKInfo model, out string tdk_d)
        {
            tdk_d = null;
            switch (model)
            {
                case PcCourseDetailDto course1:
                    {
                        var description = HtmlHelper.NoHTML(course1.Detail).GetHtmlHeaderString(160);
                        tdk_d = description.Length > 160 ? description[0..160] : description;
                    }
                    return true;
                case PcEvltDetailDto evlt1:
                    {
                        var ctts = string.Join('\n', evlt1.Contents.Select(_ => _?.Content ?? "")).Replace("\n", "");
                        ctts = HtmlHelper.NoHTML(ctts).GetHtmlHeaderString(160);
                        tdk_d = ctts.Length > 160 ? ctts[0..160] : ctts;
                    }
                    return true;
            }
            return false;
        }
    }
}
