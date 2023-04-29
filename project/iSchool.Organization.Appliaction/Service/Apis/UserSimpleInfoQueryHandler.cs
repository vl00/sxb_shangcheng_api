using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class UserSimpleInfoQueryHandler : IRequestHandler<UserSimpleInfoQuery, IEnumerable<UserSimpleInfoQueryResult>>
    {
        IHttpClientFactory httpClientFactory;
        IConfiguration config;
        CSRedisClient redis;

        public UserSimpleInfoQueryHandler(IHttpClientFactory httpClientFactory, IConfiguration config, CSRedisClient redis)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;
            this.redis = redis;
        }

        public async Task<IEnumerable<UserSimpleInfoQueryResult>> Handle(UserSimpleInfoQuery q, CancellationToken cancellation)
        {
            if (q.UserIds?.Any() != true) return Enumerable.Empty<UserSimpleInfoQueryResult>();

            var pipe = redis.StartPipe();
            foreach (var uid in q.UserIds)
                pipe.Get(CacheKeys.UserSimpleInfo.FormatWith(uid));
            var rr = (await pipe.EndPipeAsync()).Select(_s => _s?.ToString()?.ToObject<UserSimpleInfoQueryResult>()).ToList();

            var ur = get_Empty(q.UserIds, rr);
            if (!ur.Any())
            {
                rr.ForEach(x => x.HeadImgUrl = !x.HeadImgUrl.IsNullOrEmpty() ? x.HeadImgUrl : config["AppSettings:UserDefaultHeadImg"]);
                return rr;
            }

            using var http = httpClientFactory.CreateClient(string.Empty);
            var url = $"{config[Consts.BaseUrl_usercenter]}/ApiUser/GetUsers";
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.SetContent(new StringContent((new { UserIds = ur.Select(_ => _.uid) }).ToJsonString(true), Encoding.UTF8, "application/json"));
            var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var r = (await res.Content.ReadAsStringAsync()).ToObject<ResponseResult<UserSimpleInfoQueryResult[]>>();
            if (!r.Succeed) throw new CustomResponseException("获取用户信息失败");

            foreach (var (i, uid) in ur)
            {
                var info = r.Data.FirstOrDefault(_ => _.Id == uid);
                if (info == null) info = new UserSimpleInfoQueryResult { Id = uid };
                rr[i] = info;
                rr[i].HeadImgUrl = !rr[i].HeadImgUrl.IsNullOrEmpty() ? rr[i].HeadImgUrl : config["AppSettings:UserDefaultHeadImg"];
            }

            var rx = rr.Where(_ => _ != null);

            pipe = redis.StartPipe();
            foreach (var user in rx)
                pipe.Set(CacheKeys.UserSimpleInfo.FormatWith(user.Id), user, 60 * 5);
            await pipe.EndPipeAsync();

            return rx;
        }

        static IEnumerable<(int i, Guid uid)> get_Empty(IEnumerable<Guid> uids, List<UserSimpleInfoQueryResult> ls)
        {            
            var i = -1;
            foreach (var uid in uids)
            {
                i++;
                if (ls[i] == null || ls[i].Id == default || uid != ls[i].Id)
                    yield return (i, uid);
            }
        }
    }
}
