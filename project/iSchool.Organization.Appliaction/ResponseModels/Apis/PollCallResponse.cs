using iSchool.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public partial class PollCallResponse
    {
        /// <summary>轮询结果</summary>
        public PollResult? PollQryResult { get; set; }
        /// <summary>设置轮询结果是否成功</summary>
        public bool? IsSetResultOk { get; set; }

        public bool? IsPreSetOk { get; set; }
    }

    public partial class PollResult
    {
        /** redis 'org:poll:{id}' hash
         *  f={              
              result-type
              result              
              rrc : 1 //read_and_del, read_more_times
            }
         */

        public string Id { get; set; } = default!;
        /// <summary>表示是否轮询有结果</summary>
        public bool HasResult { get; set; }
        [JsonIgnore]
        public Type ResultType { get; set; } = default!;
        [JsonIgnore]
        public string ResultStr { get; set; } = default!;

        private bool _isResultCalled;
        private object? _result;
        /// <summary>
        /// 轮询结果.<br/>
        /// 具体结果字段结构请查看`__Result_{xxx}`
        /// </summary>
        public object? Result
        {
            get
            {
                if (!_isResultCalled)
                {
                    _isResultCalled = true;
                    _result = ResultType != null ? JsonExtensions.ToObject(ResultStr, ResultType) :
                        string.IsNullOrEmpty(ResultStr) ? null : JToken.Parse(ResultStr);
                }
                return _result;
            }
            set
            {
                HasResult = true;
                _isResultCalled = true;
                _result = value;
            }
        }

        public int Rrc { get; set; } = -2;

#if DEBUG

        /*
        /// <summary>
        /// 请使用`result`字段.此字段仅仅用于文档说明.
        /// </summary>
        [IgnoreDataMember]
        public int[] _fff01 => Result is int[] _r ? _r : default!; 
        //*/


        /// <inheritdoc cref="PollResult__Result_wx_buy_course"/>
        public PollResult__Result_wx_buy_course __Result_wx_buy_course => default!;
        /// <summary>
        /// 轮询结果 - 微信购买课程回掉<br/>
        /// 请使用`result`字段.此字段仅仅用于文档说明.
        /// </summary>
        public class PollResult__Result_wx_buy_course : WxPayOkOrderDto { }
#endif
    }

#nullable disable
}
