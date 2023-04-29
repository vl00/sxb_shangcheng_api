using iSchool.Organization.Appliaction.Options;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class CancelFxInfoCommandHandler : IRequestHandler<CancelFxInfoCommand>
    {
        HttpClient _client;
        ILogger<CancelFxInfoCommandHandler> _logger;
        public CancelFxInfoCommandHandler(
            IHttpClientFactory httpClientFactory
            , IOptions<AftersalesOption> options
            , ILogger<CancelFxInfoCommandHandler> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri(options.Value.MarketingUrl);
            _logger = logger;
        }

        public async Task<Unit> Handle(CancelFxInfoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string url = "/api/FxOrder/CancelFxInfo";
                var response = await _client.PostAsync(url, new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var result =  JObject.Parse(await response.Content.ReadAsStringAsync());
                if (!result["succeed"].Value<bool>())
                {
                    throw new Exception(result["msg"].Value<string>());
                }
                _logger.LogInformation($"调用撤销收益服务result= {result}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用撤销收益服务异常。");
            }
            return Unit.Value;

        }
    }
}
