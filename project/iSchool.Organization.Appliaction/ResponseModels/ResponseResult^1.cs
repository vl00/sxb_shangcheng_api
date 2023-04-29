using iSchool.Infrastructure.Common;
using iSchool.Organization.Domain.Enum;
using System;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 泛型的ResponseResult
    /// </summary>
    public class ResponseResult<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Succeed { get; set; }
       
        /// <summary>
        /// 返回时间
        /// </summary>
        public long MsgTimeStamp => TimeHelp.ToUnixTimestampByMilliseconds(DateTime.Now);

        /// <summary>
        /// 返回错误码
        /// </summary>
        public ResponseCode status { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string Msg { get; set; }


        /// <summary>
        /// 返回Model
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 返回一个成功的返回值
        /// </summary>
        /// <returns></returns>
        public static ResponseResult<T> Success()
        {
            return Success("操作成功");
        }

        /// <summary>
        /// 返回一个成功的返回值
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ResponseResult<T> Success(string message)
        {

            return Success(default, message);
        }

        /// <summary>
        /// 返回一个成功的返回值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResponseResult<T> Success(T data)
        {
            return Success(data, "操作成功");
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResponseResult<T> Failed()
        {
            return Failed(null);
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResponseResult<T> Failed(string msg)
        {
            return Failed(msg, default);
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResponseResult<T> Failed(T data)
        {
            return Failed("操作失败", data);
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResponseResult<T> Failed(string msg, T data)
        {
            return new ResponseResult<T>()
            {
                Succeed = false,
                status = ResponseCode.Failed,
                Msg = msg,
                Data = data
            };
        }


        /// <summary>
        /// 返回成功的返回值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ResponseResult<T> Success(T data, string msg)
        {
            return new ResponseResult<T>()
            {
                Succeed = true,
                status = ResponseCode.Success,
                Msg = msg,
                Data = data
            };
        }

        public ResponseResult<T> Set_status(int code)
        {
            this.status = (ResponseCode)code;
            return this;
        }

        public ResponseResult<T> SetData(T data)
        {
            this.Data = data;
            return this;
        }
    }
}
