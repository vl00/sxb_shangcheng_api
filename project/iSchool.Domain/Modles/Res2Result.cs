using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace iSchool.Domain.Modles
{        
    [DebuggerTypeProxy(typeof(Res2Result_DebuggerView))]
    public abstract partial class Res2Result
    {
        /// <summary>返回时间</summary>
        public long MsgTimeStamp => new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();

        public bool Succeed { get; set; }

        /// <summary>错误码|状态码</summary>
        public int Status { get; set; }

        /// <summary>(错误)消息</summary>
        public string Msg { get; set; }

        /// <summary>数据</summary>        
        public object Data
        {
            get => GetData();
            set => SetData(value);
        }

        protected abstract object GetData();
        protected abstract void SetData(object data);

        class Res2Result_DebuggerView
        {
            readonly Res2Result _this;

            public Res2Result_DebuggerView(Res2Result _this) => this._this = _this;

            public bool Succeed => _this.Succeed;
            public int Status => _this.Status;
            public string Msg => _this.Msg;
            public object Data => _this.GetData();
        }
    }

    public partial class Res2Result
    {
        public static Res2Result<object> Success(int code = 200) 
            => new Res2Result<object> { Succeed = true, Status = code };

        public static Res2Result<T> Success<T>(T data = default, int code = 200) 
            => new Res2Result<T> { Succeed = true, Data = data, Status = code };

        public static Res2Result<object> Fail(int errcode)
            => new Res2Result<object> { Succeed = false, Status = errcode };

        public static Res2Result<object> Fail(string errmsg, int errcode = 201) // =400
            => new Res2Result<object> { Succeed = false, Msg = errmsg, Status = errcode };

        public static Res2Result<T> Fail<T>(int errcode = 201) // =400
            => new Res2Result<T> { Succeed = false, Status = errcode };

        public static Res2Result<T> Fail<T>(string errmsg, int errcode = 201) // =400
            => new Res2Result<T> { Succeed = false, Msg = errmsg, Status = errcode };
    }

    public class Res2Result<T> : Res2Result
    {
        /// <summary>返回时间</summary>
        public new long MsgTimeStamp => base.MsgTimeStamp;

        public new bool Succeed { get => base.Succeed; set => base.Succeed = value; }

        /// <summary>错误码|状态码</summary>
        public new int Status { get => base.Status; set => base.Status = value; }

        /// <summary>(错误)消息</summary>
        public new string Msg { get => base.Msg; set => base.Msg = value; }

        /// <summary>数据</summary>
        public new T Data { get; set; }

        protected override object GetData() => Data;
        protected override void SetData(object data) => Data = (T)data;

        public Res2Result<T> SetData(T data)
        {
            Data = data;
            return this;
        }

        public Res2Result<T> SetMsg(string msg)
        {
            Msg = msg;
            return this;
        }
    }
}
