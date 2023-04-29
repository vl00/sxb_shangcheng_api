using CSRedis;
using iSchool.Api.ModelBinders;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace iSchool.Organization.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        IMediator _mediator;

        public HomeController(IMediator _mediator)
        {
            this._mediator = _mediator;
        }

        /// <summary>
        /// 检测已登录用户是否绑定手机 <br/>
        /// 未登录 照旧返回 status=401 <br/>
        /// 未绑定手机 返回 status=40004
        /// </summary>
        /// <returns>返回结果时整个ResponseResult</returns>
        [Authorize]
        [HttpGet("um")]
        public async Task<ResponseResult> Action_um()
        {
            var b = await _mediator.Send(new CheckUserBindMobileCommand());
            if (b) return ResponseResult.Success();
            var r = ResponseResult.Failed("未绑定手机号");
            r.status = Domain.Enum.ResponseCode.NotBindMobile;
            return r;
        }


        #region upload

        /// <summary>
        /// 上传图片<br/>
        /// 具体详情可参考 /upload.html
        /// </summary>
        /// <param name="fid">文件id.当`index=1时为null`,后续`值=之前的返回id`</param>
        /// <param name="p">服务文件夹</param>
        /// <param name="index">当前上传的块下标,不能小于1</param>
        /// <param name="total">总块数</param>
        /// <param name="blockSize">块大小</param>
        /// <param name="file">块数据,就是前端的file的分割</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext"></param>
        /// <param name="imgindex">前端位置</param>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpPost("img")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadImgResult), 200)]
        [DisableRequestSizeLimit]
        public async Task<ResponseResult> UploadImg([FromForm(Name = "id")] string fid, [FromForm] string p, [FromForm(Name = "imgindex")] string imgindex,
            [FromForm] long index, [FromForm] long total, [FromForm(Name = "size")] long blockSize,
            [BindFormFile(0)] IFormFile file, [FromForm] string fileName, [FromForm] string ext,
            [FromServices] IConfiguration config)
        {
            if (file == null || file.Length == 0L)
            {
                return ResponseResult.Failed("上传文件不能为空", new UploadImgResult { Imgindex = imgindex });
            }

            var x = await Save_bys_to_tmp(config, file, fid, p, index, blockSize, fileName, ext);
            fid = x.fid;

            if (index < total)
            {
                return ResponseResult.Success(new UploadImgResult
                {
                    Id = fid,
                    Imgindex = imgindex,
                });
            }

            var result = new UploadImgResult() { Id = fid, Imgindex = imgindex };
            using var fs = System.IO.File.Open(x.Path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);
            var err = await Upload_bys_to_hushushu(result, x.UploadUrl, fs);
            if (err == null) return ResponseResult.Success(result);
            else return ResponseResult.Failed(err, result);
        }

        /// <summary>
        /// 上传图片1
        /// </summary>
        /// <param name="file">块数据,就是前端的file的分割</param>
        /// <param name="p">服务文件夹</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名</param>
        /// <param name="imgindex">前端位置</param>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpPost("img1")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadImgResult), 200)]
        [DisableRequestSizeLimit]
        public async Task<ResponseResult> UploadImg1([BindFormFile(0)] IFormFile file, [FromForm] string p, [FromForm] string imgindex,
            [FromForm] string fileName, [FromForm] string ext,
            [FromServices] IConfiguration config)
        {
            return await UploadImg(null, p, imgindex, 1, 1, file.Length, file, fileName, ext, config);
        }

        /// <summary>
        /// 上传视频<br/>
        /// 具体详情可参考 /upload.html
        /// </summary>
        /// <param name="fid">文件id.当`index=1时为null`,后续`值=之前的返回id`</param>
        /// <param name="p">服务文件夹</param>
        /// <param name="index">当前上传的块下标,不能小于1</param>
        /// <param name="total">总块数</param>
        /// <param name="blockSize">块大小</param>
        /// <param name="file">块数据,就是前端的file的分割</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名</param>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpPost("video")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadVideoResult), 200)]
        [DisableRequestSizeLimit]
        public async Task<ResponseResult> UploadVideo([FromForm(Name = "id")] string fid, [FromForm] string p,
            [FromForm] long index, [FromForm] long total, [FromForm(Name = "size")] long blockSize,
            [BindFormFile(0)] IFormFile file, [FromForm] string fileName, [FromForm] string ext,
            [FromServices] IConfiguration config)
        {
            if (file == null || file.Length == 0L)
            {
                return ResponseResult.Failed("上传文件不能为空", new UploadVideoResult { });
            }

            var x = await Save_bys_to_tmp(config, file, fid, p, index, blockSize, fileName, ext);
            fid = x.fid;

            if (index < total)
            {
                return ResponseResult.Success(new UploadVideoResult
                {
                    Id = fid,
                });
            }

            var result = new UploadVideoResult { Id = fid };
            try
            {
                using var fs = System.IO.File.Open(x.Path, FileMode.Open, FileAccess.Read);
                fs.Seek(0, SeekOrigin.Begin);
                var err = await Upload_bys_to_hushushu(result, x.UploadUrl, fs);
                if (err == null) return ResponseResult.Success(result);
                else return ResponseResult.Failed(err, result);
            }
            finally
            {
#if !DEBUG
                try { System.IO.File.Delete(x.Path); } catch { }
#endif
            }
        }

        /// <summary>
        /// 上传视频1
        /// </summary>
        /// <param name="file">块数据,就是前端的file的分割</param>
        /// <param name="p">服务文件夹</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ext">扩展名</param>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpPost("video1")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadVideoResult), 200)]
        [DisableRequestSizeLimit]
        public async Task<ResponseResult> UploadVideo1([BindFormFile(0)] IFormFile file, [FromForm] string p,
            [FromForm] string fileName, [FromForm] string ext,
            [FromServices] IConfiguration config)
        {
            return await UploadVideo(null, p, 1, 1, file.Length, file, fileName, ext, config);
        }

        [NonAction]
        static async Task<(string fid, string Path, string UploadUrl)> Save_bys_to_tmp(IConfiguration config, IFormFile file,
            string fid, string p, long index, long blockSize, string fileName, string ext)
        {
            ext ??= "png";
            p ??= "eval";
#if DEBUG
            p = "test/" + p;
#endif
            index = index >= 1 ? index : throw new CustomResponseException("index不能小于1");
            if (index == 1 && string.IsNullOrEmpty(fid)) fid = Guid.NewGuid().ToString("n");
            ext = (ext[0] == '.' ? ext[1..] : ext).ToLower();

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"images/temp")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"images/temp"));

            var path = Path.Combine(Directory.GetCurrentDirectory(), $"images/temp/{fid}.{ext}");
            var steam = file.OpenReadStream();
            using (var fs = System.IO.File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.Seek((index - 1) * blockSize, SeekOrigin.Begin);
                await steam.CopyToAsync(fs);
                await fs.FlushAsync();
            }

            return (fid, path, config[Consts.BaseUrl_UploadUrl].FormatWith($"{p}/{fid}", $"{fid}.{ext}"));
        }

        [NonAction]
        static async Task<string> Upload_bys_to_hushushu(object result0, string url, Stream fs, int timeout = 60000)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.Timeout = timeout;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
            req.ContentLength = fs.Length;
            using var sr_s = await req.GetRequestStreamAsync();
            await fs.CopyToAsync(sr_s);
            var res = (HttpWebResponse)(await req.GetResponseAsync());
            if (req.HaveResponse)
            {
                if (!((int)res.StatusCode).In(200, 201, 202, 204, 206))
                {
                    return "上传失败,网络异常.";
                }
                else
                {
                    using var st = res.GetResponseStream();
                    using var re = new StreamReader(st);
                    var restr = await re.ReadToEndAsync();
                    var rez = JToken.Parse(restr);
                    re.Close();
                    if ((int?)rez["status"] == 0)
                    {
                        var url0 = default(string);
                        switch (result0)
                        {
                            case UploadImgResult result:
                                {
                                    url0 = result.Src = rez["cdnUrl"].ToString();
                                    result.Src_s = rez["compress"]?["cdnUrl"]?.ToString();
                                }
                                break;
                            case UploadVideoResult result:
                                {
                                    url0 = result.Src = rez["cdnUrl"].ToString();
                                    result.CoverUrl = rez["cover"]?["cdnUrl"]?.ToString();
                                    result.CoverUrl_s = rez["cover"]?["cdnUrl"]?.ToString();
                                }
                                break;
                            default:
                                throw new NotSupportedException("暂无支持.");
                        }
                        if (url0 == null) return "上传失败.可能是格式原因导致没结果";
                        else return null;
                    }
                    else
                    {
                        return $"上传失败: restr=`{restr}`";
                    }
                }
            }
            else return "上传失败,没响应.";
        }

        #endregion upload

        #region wx upload

        /// <summary>
        /// miniprogram-file-uploader要求的uploadUrl
        /// </summary>
        /// <param name="__apidesc">说明：失败时返回的body就是错误消息</param>
        /// <param name="fid">文件id</param>
        /// <param name="fileName">含扩展名的文件名</param>
        /// <param name="index">从0开始</param>
        /// <param name="chunkSize">本块大小,最后一块会变小</param>
        /// <param name="totalChunks">总块数</param>
        /// <param name="totalSize">总大小</param>
        /// <param name="loggerFactory"></param>
        /// <param name="redis"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpPost("wx/video")]
        [Consumes("application/octet-stream")]
        [ProducesResponseType(typeof(WxUploadVideoResult), 200)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Wx_UploadVideo([FromQuery] string __apidesc,
            [FromQuery(Name = "identifier")] string fid, [FromQuery] string fileName,
            [FromQuery] long index, [FromQuery] long chunkSize, [FromQuery] long totalChunks, [FromQuery] long totalSize,
            [FromServices] ILoggerFactory loggerFactory, [FromServices] CSRedisClient redis,
            [FromServices] IConfiguration config)
        {
            var body = HttpContext.Request.Body;
            var log = loggerFactory.CreateLogger("upload");
            await default(ValueTask);

            log.LogDebug("upload args: " + new { identifier = fid, fileName, index, chunkSize, totalChunks, totalSize }.ToJsonString(camelCase: true));

            if (HttpContext.Request.ContentLength == 0L)
            {
                return StatusCode(501, "no body");
            }

            var blockSize = await redis.GetAsync<long?>(CacheKeys.WxUploadBlockSize) ?? 5242880;
            if ((index == 0 && totalChunks == 1 && blockSize < chunkSize)
                || (index == 0 && totalChunks > 1 && blockSize != chunkSize))
            {
                return StatusCode(501, "block-size error");
            }

            try
            {
                var x = await Save_bys_to_tmp_bywx(config, body, fid, index, blockSize, fileName);
                return StatusCode(200, new WxUploadVideoResult
                {
                    TempFilePath = $"{x.fid}.{x.ext}"
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "上传失败");
                return StatusCode(500, ex.Message);
            }
        }

        [NonAction]
        static async Task<(string fid, string ext)> Save_bys_to_tmp_bywx(IConfiguration config, Stream body,
            string fid, long index, long blockSize, string fileName)
        {
            var ext = fileName?.LastIndexOf('.') is int li && li > -1 ? fileName[(li + 1)..] : "";
            ext = !string.IsNullOrEmpty(ext) ? ext.ToLower() : "mp4";

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"images/temp")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"images/temp"));

            var path = Path.Combine(Directory.GetCurrentDirectory(), $"images/temp/{fid}.{ext}");

            using (var fs = System.IO.File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.Seek(index * blockSize, SeekOrigin.Begin);
                await body.CopyToAsync(fs);
                await fs.FlushAsync();
            }

            return (fid, ext);
        }

        /// <summary>
        /// miniprogram-file-uploader要求的merge接口
        /// </summary>
        /// <param name="__apidesc">
        /// 说明：接口正常返回看看有无其他数据<br/>
        /// 失败时返回的body就是错误消息
        /// </param>
        /// <param name="fid">文件id</param>
        /// <param name="fileName">含扩展名的文件名</param>
        /// <param name="loggerFactory"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpGet("wx/video/mg")]
        [ProducesResponseType(typeof(UploadVideoResult), 200)]
        public async Task<IActionResult> Wx_UploadVideo_merge([FromQuery] string __apidesc,
            [FromQuery(Name = "identifier")] string fid, [FromQuery] string fileName,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] IConfiguration config)
        {
            var log = loggerFactory.CreateLogger("upload");
#if DEBUG
            var p = "test/org";
#else
            var p = "org";
#endif
            await default(ValueTask);

            log.LogDebug($"merge args: curr='{Path.Combine(Directory.GetCurrentDirectory(), $"images/temp")}'\n" + new { identifier = fid, fileName }.ToJsonString(camelCase: true));

            var ext = fileName?.LastIndexOf('.') is int li && li > -1 ? fileName[(li + 1)..] : "";
            ext = !string.IsNullOrEmpty(ext) ? ext.ToLower() : "mp4";

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"images/temp/{fid}.{ext}");

            var result = new UploadVideoResult { Id = fid };
            try
            {
                using var fs = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read);
                fs.Seek(0, SeekOrigin.Begin);
                var err = await Upload_bys_to_hushushu(result, config[Consts.BaseUrl_UploadUrl].FormatWith($"{p}/{fid}", $"{fid}.{Path.GetExtension(filePath).TrimStart('.')}"), fs);
                if (err == null) return StatusCode(200, result);
                //else return ResponseResult.Failed(err, result);
                else return StatusCode(500, err);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "内部上传失败");
                return StatusCode(500, ex.Message);
            }
            finally
            {
#if !DEBUG
                try { System.IO.File.Delete(filePath); } catch { }
#endif
            }
        }

        #endregion wx upload

        /// <summary>
        /// 用于轮询
        /// </summary>
        /// <param name="id">之前给出的轮询id</param>
        /// <returns></returns>
        [HttpGet("poll/{id}")]
        [ProducesResponseType(typeof(PollResult), 200)]
        public async Task<ResponseResult> Poll(string id)
        {
            var r = await _mediator.Send(new PollCallRequest
            {
                Query = new PollQuery { Id = id }
            });
            return ResponseResult.Success(r.PollQryResult);
        }

        /// <summary>
        /// 根据url生成base64的二维码
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pixel"></param>
        /// <returns></returns>
        [HttpGet("urlqrcode")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<ResponseResult> GetUrlQrcode([FromQuery] string url, [FromQuery] int pixel = 5)
        {
            url = string.IsNullOrEmpty(url) ? null : HttpUtility.UrlDecode(url);
            if (string.IsNullOrEmpty(url)) return ResponseResult.Failed("url为空");
            await Task.CompletedTask;
            return ResponseResult.Success(QRCodeHelper.GetNormalBase64Qrcode(url, pixel), null);
        }

        [AllowAnonymous]
        [HttpGet("checkorder")]
        public async Task<ResponseResult> CheckOrderStatus(Guid id)
        {
            var res = await _mediator.Send(new FinanceCheckOrderPayStatusQuery { OrderId = id });

            return ResponseResult.Success(res);
        }
    }
}
