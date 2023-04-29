using iSchool;
using iSchool.BgServices;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.HostedServices
{
    public class RbmqHostedService : IHostedService
    {
        private readonly RabbitMQConnection _rabbit;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _config;
        readonly ILoggerFactory _loggerFactory;

        private List<RbmqAckableConsumer> _consumers;

        public RbmqHostedService(RabbitMQ.Client.ConnectionFactory connFactory, IConfiguration config, ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory)
        {
            this._rabbit = new RabbitMQConnection(connFactory, loggerFactory);
            this._serviceScopeFactory = serviceScopeFactory;
            this._config = config;
            this._loggerFactory = loggerFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _consumers = new List<RbmqAckableConsumer>();
            if (_config["rabbit:consumers:default:enable"].In("1", "true", "True"))
            {
                var queueName = _config["rabbit:consumers:default:queue"];
                var consumer = new RbmqAckableConsumer(_rabbit, _loggerFactory.CreateLogger("rbmq_qMsgs"), queue: queueName, 
                    qos: Convert.ToUInt16(_config["rabbit:consumers:default:qos"]), 
                    exclusive: (_config["rabbit:consumers:default:exclusive"]?.ToLower().In("1", "true") == true)
                );
                consumer.OnReceived += Consumer_OnReceived;
                consumer.OnPreReconnect += Consumer_OnPreReconnect;                
                _consumers.Add(consumer);
                Start_consumer(consumer, "rabbit:consumers:default");               
            }
            return Task.CompletedTask;
        }

        private async void Start_consumer(RbmqAckableConsumer consumer, string configName)
        {
            using var sp = _serviceScopeFactory.CreateScope();
            var nlog = sp.ServiceProvider.GetService<NLog.ILogger>();
            var log = _loggerFactory.CreateLogger("rbmq_qMsgs");
            for (var i = 0; ;)
            {
                try
                {
                    if (i == 0) log.LogDebug($"rbmq消费启动ing.config='{configName}'");
                    consumer.Start();
                    log.LogDebug($"rbmq消费启动ed.config='{configName}'");
                    return;
                }
                catch (Exception ex)
                {
                    var msg = GetLogMsg();
                    msg.Properties["Error"] = $"rbmq消费启动有错.config='{configName}'.err={ex.Message}";
                    msg.Properties["StackTrace"] = ex.StackTrace;
                    msg.Properties["ErrorCode"] = 33330;
                    nlog.Error(msg);

                    log.LogError(ex, $"rbmq消费启动有错.config='{configName}'.conn isopened={_rabbit.IsOpened}");

                    if (_rabbit.IsOpened && ++i > 1000) return;
                    await Task.Delay(1000 * 60);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var consumers = _consumers;
            if (consumers != null)
            {
                _consumers = null;
                foreach (var consumer in consumers)
                {
                    consumer.OnPreReconnect -= Consumer_OnPreReconnect;
                    consumer.OnReceived -= Consumer_OnReceived;
                    consumer.Stop();
                }
            }
            _rabbit.Close();
            return Task.CompletedTask;
        }

        private async Task Consumer_OnPreReconnect(RbmqAckableConsumer consumer, RbmqAckableConsumerOnReconnectEventArgs e)
        {            
            using var sp = _serviceScopeFactory.CreateScope();
            var log = sp.ServiceProvider.GetService<NLog.ILogger>();

            var msg = GetLogMsg();
            msg.Properties["Error"] = $"rbmq连接断了在重连中.is1st={e.Is1st},errcode={e.Errcode},errmsg={e.Errmsg}";
            msg.Properties["Params"] = e.ToJsonString(camelCase: true);
            msg.Properties["ErrorCode"] = 33331;
            log.Error(msg);

            await Task.Delay(e.Heartbeat);
        }

        private Task Consumer_OnReceived(RbmqAckableConsumer consumer, RbmqAckableConsumerOnReceivedEventArgs arg)
        {
            var (channel, e) = arg;
            var task = Task.CompletedTask;
            var sp = default(IServiceScope);
            object msg = null;

            var log = _loggerFactory.CreateLogger("rbmq_qMsgs");
            var isbodylarge = e.Body.Length > 1024 * 4;
            var str_logbody = (string)null;

            // find msg
            if (e.Exchange == _config["rabbit:Consumer_OnReceived:FinanceCenter:exchange"] 
                && e.RoutingKey == _config["rabbit:Consumer_OnReceived:FinanceCenter:routingkey"])
            {
                try
                {
                    var jsonstr = str_logbody = Encoding.UTF8.GetString(e.Body.Span);
                    var j = JObject.Parse(jsonstr);
                    msg = new WxPayRequest { WxPayCallback = j.ToObject<iSchool.Organization.Appliaction.ViewModels.WxPayCallbackNotifyMessage>() };
                }
                catch (Exception ex)
                {
                    task = Task.FromException(ex);                    
                }
                goto LB_upTask;
            }

            // up task
            LB_upTask:
            {
                log.LogInformation($"rbmq接收到消息: exchange={e.Exchange}, routingkey={e.RoutingKey}" 
                    + "\n" + (str_logbody != null ? str_logbody 
                        : !isbodylarge ? Encoding.UTF8.GetString(e.Body.Span)
                        : $"((body length = {e.Body.Length}))")
                    );
            }
            if (msg != null)
            {
                sp = _serviceScopeFactory.CreateScope();
                var medr = sp.ServiceProvider.GetService<IMediator>();
                if (msg is INotification) task = medr.Publish(msg);
                else if (msg is IBaseRequest) task = medr.Send(msg);
                task.ContinueWith((t, o) => OnTaskCompleted(t, (IServiceScope)o), sp);
            }
            
            _ = task.ContinueWith((t, o) => OnHandled((RbmqAckableConsumerOnReceivedEventArgs)o), arg);
            return null;
        }

        static void OnTaskCompleted(Task t, IServiceScope sp)
        {
            try
            {
                if (t.Exception != null)
                {
                    var ex = ExceptionDispatchInfo.Capture(t.Exception).SourceException;

                    var log0 = sp.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("rbmq_qMsgs");
                    log0.LogError(ex, $"rbmq消费有错.{33333}.");

                    var log = sp.ServiceProvider.GetService<NLog.ILogger>();
                    var msg = GetLogMsg();
                    msg.Properties["Error"] = $"rbmq消费有错.err={ex.Message}";
                    msg.Properties["StackTrace"] = ex.StackTrace;
                    msg.Properties["ErrorCode"] = 33333;
                    log.Error(msg);
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                sp.Dispose();
            }
        }

        static void OnHandled(RbmqAckableConsumerOnReceivedEventArgs arg)
        {
            var (channel, e) = arg;

            if (channel.IsClosed || !channel.IsOpen) return;
            try
            {
                channel.BasicAck(e.DeliveryTag, false);
            }
            catch
            {
                // ignore
            }
        }

        static NLog.LogEventInfo GetLogMsg()
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
            msg.Properties["Caption"] = "rbmq consumer";
            //msg.Properties["UserId"] = null;
            msg.Properties["Level"] = "错误"; 
            //msg.Properties["Error"] = $"检测敏感词意外失败.网络异常.err={ex.Message}";
            //msg.Properties["StackTrace"] = ex.StackTrace;
            //msg.Properties["ErrorCode"] = 3;
            return msg;
        }
    }
}
