
using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Apis;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 课程收藏或取消
    /// </summary>
    public class CourseBookCommandHandler : IRequestHandler<CourseBookCommand, ResponseResult>
    {
        IConfiguration _config;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IUserInfo _userInfo;
        IRepository<iSchool.Organization.Domain.Course> _courseRepo;
        IRepository<iSchool.Organization.Domain.Organization> _orgRepo;
        UserUnitOfWork _userUnitOfWork;
        public CourseBookCommandHandler(IConfiguration config, IOrgUnitOfWork unitOfWork, CSRedisClient redisClient, IUserInfo userInfo
            , IRepository<iSchool.Organization.Domain.Course> courseRepo
            , IRepository<iSchool.Organization.Domain.Organization> orgRepo, IUserUnitOfWork userUnitOfWork, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _userUnitOfWork = (UserUnitOfWork)userUnitOfWork;
            _redisClient = redisClient;
            _userInfo = userInfo;
            _courseRepo = courseRepo;
            _orgRepo = orgRepo;
             _mediator = mediator;
            _config = config;
        }
        public async Task<ResponseResult> Handle(CourseBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (null == _userInfo || !_userInfo.IsAuthenticated)
                {
                    return ResponseResult.Failed("未登录！");
                }

                
                var courseM = _courseRepo.Get(request.CourseId);
                if (null == courseM) return ResponseResult.Failed("参数错误_courseid！");
                var orgM = _orgRepo.Get(courseM.Orgid);
                if (null == orgM || !orgM.Authentication) return ResponseResult.Failed("参数错误_courseid！");
                var re = @"^1\d{10}$";//正则表达式
                if (!Regex.IsMatch(request.Mobile, re))
                {
                   ResponseResult.Failed("手机号码格式不正确！");
                }
                if (!string.IsNullOrEmpty(request.Mobile))//验证手机
                {
                    if (string.IsNullOrEmpty(request.VerifyCode))//系统读取的手机号。不用验证
                    {

                        var userSql = "select top 1 *  from [userinfo] where id=@UserId and mobile is not null";
                        var user = _userUnitOfWork.QueryFirstOrDefault<userinfo>(userSql, new { _userInfo.UserId });
                        if (user?.Mobile != request.Mobile)
                        {
                            return ResponseResult.Failed("请填写手机验证码！");

                        }
                    }
                    else {

                        RndCodeModel codeM = _redisClient.Get<RndCodeModel>(CacheKeys.VerifyCodeKey.FormatWith(request.Mobile, "orgnization"));
                        if (string.IsNullOrEmpty(request.VerifyCode))
                        {
                            return ResponseResult.Failed("请填写手机验证码！");

                        }
                        else if (null == codeM)
                        {
                            return ResponseResult.Failed("验证码已过期或不存在！");

                        }
                        else if (codeM.Code.ToLower() != request.VerifyCode.ToLower())
                        {
                            return ResponseResult.Failed("验证码无效！");

                        }
                        _redisClient.Del(CacheKeys.VerifyCodeKey.FormatWith(request.Mobile, "orgnization"));
                    }

                   
                }
                var rdk = CacheKeys.UeserCourseBuyExist.FormatWith(request.CourseId, request.Mobile);
                var r = _redisClient.Get<int>(rdk);
                if (0 == r)
                {
                    var s_dy = new DynamicParameters()
                     .Set("courseid", request.CourseId)
                     .Set("mobile", request.Mobile);
                    string s_sql = $@" select count(1) from [dbo].[Order] where courseid=@courseid and mobile=@mobile and isvalid=1;";
                    r = _orgUnitOfWork.QueryFirstOrDefault<int>(s_sql, s_dy);
                    r = r > 0 ? r : -1;
                    _redisClient.Set(rdk, r, TimeSpan.FromDays(3));
                }
                if (r > 0)
                {
                    return ResponseResult.Success("该课程已经购买成功！");
                }
                _orgUnitOfWork.BeginTransaction();

                var dy = new DynamicParameters()
               .Set("id", Guid.NewGuid())
               .Set("courseid", request.CourseId)
               .Set("userid", _userInfo.UserId)
               .Set("mobile", request.Mobile)
               .Set("CreateTime", DateTime.Now)
               .Set("status", (int)OrderStatus.UnDelivered)
                  .Set("Remark", request.Remark)
               .Set("type", (int)OrderType.CourseBuy)
               .Set("IsValid", true);

                string addSql = $@" INSERT INTO [dbo].[Order]([id], [courseid], [userid], [mobile],  [status], [type],
 [CreateTime], [IsValid],[Remark]) VALUES (@id, @courseid, @userid, @mobile,@status,@type,@CreateTime,@IsValid,@Remark);";

                var retCount = _orgUnitOfWork.DbConnection.Execute(addSql, dy, _orgUnitOfWork.DbTransaction);
               
                _orgUnitOfWork.CommitChanges();
                _redisClient.Del(rdk);
                if (retCount >= 0)
                {
                    try
                    {
                        //courseM.Title，限制长度

                        var cmd = new SendMobileMessageCommand() { };
                        //发送短信
                        cmd.TemplateId = Convert.ToInt32(_config.GetSection("AppSettings:QcloudCourseBookMessageTemplateId").Value); 
                        var listParam = new List<string>() { courseM.Title, "48", "" };
                        cmd.TempalteParam = listParam;
                        cmd.Mobile = request.Mobile;
                        await _mediator.Send(cmd);
                    }
                    catch (Exception ex)
                    {

                       
                    }
                    return ResponseResult.Success("购买成功!");
                }

                return ResponseResult.Failed("操作失败");
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return ResponseResult.Failed("操作失败");
            }

        }


    }
}
