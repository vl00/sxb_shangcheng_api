using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services
{
    public class BgMallFenleisLoadQueryHandler : IRequestHandler<BgMallFenleisLoadQuery, BgMallFenleisLoadQueryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        const int MaxDepth = 3;

        public BgMallFenleisLoadQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<BgMallFenleisLoadQueryResult> Handle(BgMallFenleisLoadQuery query, CancellationToken cancellation)
        {
            var result = new BgMallFenleisLoadQueryResult();
            await default(ValueTask);

            KeyValue kv0 = null;
            if (query.Code != null && query.Code > 0)
            {
                kv0 = await GetKV(query.Code.Value) 
                    ?? throw new CustomResponseException("分类被删除了");
            }
            else
            {
                kv0 = new KeyValue { Key = 0, Parent = 0, Depth = 0 };
            }
            if (kv0.Key > 0)
            {
                result.SetSeleted(MapTo(kv0, new BgMallFenleiItemDto()));
            }

            var depth = kv0.Depth ?? 0;
            switch (query.ExpandMode)
            {
                // 只返回直接下级
                case 1:
                    if (0 <= depth && depth < MaxDepth)
                    {
                        var ls = await GetChildrenByPcode(kv0.Key);
                        result.SetLs(depth + 1, ls);
                    }
                    break;

                // 返回该节点的所有级联节点s和每个上下级其同级的其他项s.其中,每个下级都取第1个加载之后的下级.
                case 2:
                    // 同级
                    if (depth >= 1)
                    {
                        var ls = await GetChildrenByPcode(kv0.Parent ?? 0);
                        result.SetLs(depth, ls);
                        result.SetSeleted(ls.FirstOrDefault(_ => _.Code == kv0.Key));
                    }
                    // 上级s
                    {
                        var code = kv0.Parent ?? 0;
                        for (var i = depth - 1; i > 0; i--)
                        {
                            var ls = await GetSameDepthLsByCode(code);
                            result.SetLs(i, ls);
                            result.SetSeleted(ls.FirstOrDefault(_ => _.Code == code));
                            code = ls.FirstOrDefault()?.Pcode ?? 0; // here is same pcode
                        }
                    }
                    // 下级s
                    {
                        var pcode = kv0.Key;
                        for (var i = depth + 1; i <= MaxDepth; i++)
                        {
                            var ls = await GetChildrenByPcode(pcode);
                            result.SetLs(i, ls);
                            var t1 = ls.FirstOrDefault();  // 每个下级都取第1个加载之后的下级
                            if (t1 == null) break;
                            result.SetSeleted(t1);
                            pcode = t1.Code;
                        }
                    }
                    break;
            }

            return result;
        }

        async Task<KeyValue> GetKV(int code)
        {
            var sql = $"select top 1 * from [keyvalue] where IsValid=1 and [type]=@ty16 and [key]=@code";
            var kv = await _orgUnitOfWork.QueryFirstOrDefaultAsync<KeyValue>(sql, new { code, ty16 = Consts.Kvty_MallFenlei });
            return kv;
        }

        async Task<IEnumerable<BgMallFenleiItemDto>> GetChildrenByPcode(int pcode)
        {
            var sql = "select * from KeyValue kv where kv.isvalid=1 and kv.type=@ty16 and kv.parent=@pcode order by sort";
            var ls = await _orgUnitOfWork.QueryAsync<KeyValue>(sql, new { pcode, ty16 = Consts.Kvty_MallFenlei });
            return ls.Select(_ => MapTo(_, new BgMallFenleiItemDto())).AsList();
        }

        async Task<IEnumerable<BgMallFenleiItemDto>> GetSameDepthLsByCode(int code)
        {
            var sql = @"
select * from KeyValue kv where kv.isvalid=1 and kv.type=@ty16 
and kv.parent=(select top 1 parent from KeyValue where isvalid=1 and type=@ty16 and [key]=@code) 
order by sort
";
            var ls = await _orgUnitOfWork.QueryAsync<KeyValue>(sql, new { code, ty16 = Consts.Kvty_MallFenlei });
            return ls.Select(_ => MapTo(_, new BgMallFenleiItemDto())).AsList();
        }

        static BgMallFenleiItemDto MapTo(KeyValue s, BgMallFenleiItemDto t)
        {
            t ??= new BgMallFenleiItemDto();
            t.Code = s.Key;
            t.Name = s.Name;
            t.Sort = s.Sort ?? 0;
            t.Pcode = s.Parent ?? 0;
            t.Depth = s.Depth ?? 0;
            t.Img = s.Attach;
            return t;
        }
    }
}
