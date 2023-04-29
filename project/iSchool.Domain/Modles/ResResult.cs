using System;

namespace iSchool.Domain.Modles
{
    /// <summary>
    /// 
    /// </summary>
    public class ResResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Succeed { get; set; }
       
        /// <summary>
        /// 返回时间
        /// </summary>
        public long MsgTimeStamp => new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();

        /// <summary>
        /// 返回错误码
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 返回Model
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 返回一个成功的返回值
        /// </summary>
        /// <returns></returns>
        public static ResResult Success()
        {
            return Success("操作成功");
        }

        /// <summary>
        /// 返回一个成功的返回值
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ResResult Success(string message)
        {
            return Success(null, message);
        }

        /// <summary>
        /// 返回一个成功的返回值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResResult Success<TData>(TData data)
        {
            return Success(data, "操作成功");
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResResult Failed()
        {
            return Failed(null);
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResResult Failed(string msg)
        {
            return Failed(msg, null);
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResResult Failed<TData>(TData data)
        {
            return Failed("操作失败", data);
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResResult Failed(string msg, object data)
        {
            return new ResResult()
            {
                Succeed = false,
                status = 201,
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
        public static ResResult Success(object data, string msg)
        {
            return new ResResult()
            {
                Succeed = true,
                status = 200,
                Msg = msg,
                Data = data
            };
        }
       
    }
}
