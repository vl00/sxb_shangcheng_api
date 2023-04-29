using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 上传图片结果
    /// </summary>
    public class UploadImgResult
    {
        /// <summary>id</summary>
        public string Id { get; set; }
        /// <summary>原图</summary>
        public string Src { get; set; }
        /// <summary>缩略图</summary>
        public string Src_s { get; set; }
        ///// <summary>图片处理是否成功</summary>
        //public bool Succeed { get; set; } = true;
        /// <summary>前端位置</summary>
        public string Imgindex { get; set; }
    }

    /// <summary>
    /// 上传视频结果
    /// </summary>
    public class UploadVideoResult
    {
        /// <summary>id</summary>
        public string Id { get; set; }
        /// <summary>视频地址</summary>
        public string Src { get; set; }
        /// <summary>视频封面图(原图)</summary>
        public string CoverUrl { get; set; }
        /// <summary>视频封面图(压缩过)</summary>
        public string CoverUrl_s { get; set; }

        /// <summary>视频地址(同src)(wx-merge-url要求返回的字段)</summary>
        public string Url => Src;
    }

    public class WxUploadVideoResult
    {
        public string TempFilePath { get; set; }
    }
}
