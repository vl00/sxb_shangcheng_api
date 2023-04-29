using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    public class PollCallRequest : IRequest<PollCallResponse>
    { 
        /// <summary>轮询查询</summary>
        public PollQuery? Query { get; set; }
        /// <summary>设置轮询返回结果</summary>
        public PollSetResultCommand? SetResultCmd { get; set; }
        /// <summary>预设结果</summary>
        public PollPreSetCommand? PreSetCmd { get; set; }
    }
    
    public class PollQuery : IRequest<PollResult>
    {
        public string Id { get; set; } = default!;
        public bool IgnoreRrc { get; set; }
        public int DelayMs { get; set; } = 5000;
    }

    public class PollSetResultCommand : IRequest<bool>
    {
        public string Id { get; set; } = default!;
        public object Result { get; set; } = default!;
        public bool CheckIfExists { get; set; }
        public int ExpSec { get; set; } = 120;
        public int Rrc { get; set; } = -1;
    }

    public class PollPreSetCommand : IRequest<bool>
    {
        public string Id { get; set; } = default!;
        public string? ResultType { get; set; }
        public string? ResultStr { get; set; }
        public int ExpSec { get; set; } = 120;
    }

#nullable disable
}
