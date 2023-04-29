using CSRedis;
using Dapper;
using iSchool.BgServices;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services
{
    public class SmLogUserOperationEventHandler : INotificationHandler<SmLogUserOperation>
    {
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        IServiceProvider services;
        IHostEnvironment _hostEnvironment;

        public SmLogUserOperationEventHandler(IMediator mediator, CSRedisClient redis, IHostEnvironment _hostEnvironment,
            IConfiguration config, IServiceProvider services)
        {
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._hostEnvironment = _hostEnvironment;
            this.services = services;
        }

        public async Task Handle(SmLogUserOperation log, CancellationToken cancellationToken)
        {
            var env = _hostEnvironment.EnvironmentName?.ToLower();
            var conn = services.GetService<RabbitMQConnectionForPublish>();
            await default(ValueTask);

            using var channel = conn.OpenChannel();
            channel.ConfirmPublish(new RabbitMessage("amq.topic", $"log.org.{env.ToLower()}.{nameof(SmLogUserOperation).ToLower()}")
                .SetMessageId(log.Id)
                .SetBody(log.ToJsonString(camelCase: true)));
        }
    }
}
