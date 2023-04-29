using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.BgServices
{
#nullable enable

    public class RbmqAckableConsumer
    {
        readonly ILogger? log;
        readonly IRabbitMQConnection _rabbit;        
        IModel? _channel;
        AsyncEventingBasicConsumer? _consumer;        
        readonly Dictionary<string, object> _args;

        public ushort Qos { get; }
        public string Queue { get; }
        public bool IsExclusive { get; }

        public event Func<RbmqAckableConsumer, RbmqAckableConsumerOnReceivedEventArgs, Task?>? OnReceived;
        public event Func<RbmqAckableConsumer, RbmqAckableConsumerOnReconnectEventArgs, Task?>? OnPreReconnect;

        public RbmqAckableConsumer(IRabbitMQConnection rabbit, ILogger? log, string queue, ushort qos = 0, bool exclusive = false, IDictionary<string, object>? args = null)
        {
            this.log = log;
            _rabbit = rabbit ?? throw new ArgumentNullException(nameof(rabbit));
            this.Queue = queue;
            this.Qos = qos;
            this.IsExclusive = exclusive;
            
            _args = args != null ? new Dictionary<string, object>(args) : new Dictionary<string, object>();

            if (_rabbit.IsAutoReconnect)
                throw new InvalidOperationException("RbmqAckConsumer can't run by auto-re-connect.");
            if (!_rabbit.ConnFactory.DispatchConsumersAsync)
                throw new InvalidOperationException("RabbitMQ Client ConnectionFactory need to config 'DispatchConsumersAsync' to true.");
        }

        public void Start()
        {
            var ctag = (string?)null;
            Init_channel_consumer(ref ctag);
        }

        public void Stop()
        {
            var csr = _consumer;
            _consumer = null;
            if (csr != null) 
            {
                csr.Received -= On_consumer_Received;             
                csr.Registered -= On_consumer_Registered;                
            }

            using var cnl = _channel;
            _channel = null;
            if (cnl != null)
            {                
                cnl.ModelShutdown -= On_channel_ModelShutdown;
                cnl.CallbackException -= On_channel_CallbackException;
                cnl.Abort();
            }
        }

        private void Init_channel_consumer(ref string? consumerTag)
        {
            if (consumerTag != null)
            {                
                consumerTag = null;
            }

            _channel = _rabbit.OpenChannel();      
            if (Qos > 0) _channel.BasicQos(0, Qos, false);

            // must need to config 'ConnectionFactory.DispatchConsumersAsync=true' to use AsyncEventingBasicConsumer
            _consumer = new AsyncEventingBasicConsumer(_channel);
            _consumer.Registered += On_consumer_Registered;
            _consumer.Received += On_consumer_Received;

            _channel.BasicConsume
            (
                consumer: _consumer, 
                queue: Queue,
                autoAck: false,
                consumerTag: string.Empty, // null会报错
                arguments: _args,
                exclusive: this.IsExclusive
            );

            _channel.ModelShutdown += On_channel_ModelShutdown;
            _channel.CallbackException += On_channel_CallbackException;
        }

        void Try_unbind_onshutdown()
        {
            if (_consumer != null)
            {
                _consumer.Received -= On_consumer_Received;
                _consumer.Registered -= On_consumer_Registered;
            }
            if (_channel != null)
            {
                _channel.ModelShutdown -= On_channel_ModelShutdown;
                _channel.CallbackException -= On_channel_CallbackException;
            }
        }

        void On_channel_ModelShutdown(object? sender, ShutdownEventArgs e)
        {
            log?.LogDebug("channel{0} shutdown, Initiator='{1}', ReplyCode='{2}', ReplyText='{3}'.", _consumer != null ? $" {_consumer.ConsumerTags[0]}" : string.Empty, e.Initiator, e.ReplyCode, e.ReplyText);
            Try_unbind_onshutdown();
            On_rabbit_ConnClosed(e.ReplyCode, e.ReplyText);            
        }

        void On_channel_CallbackException(object? sender, CallbackExceptionEventArgs e)
        {
            log?.LogError(e.Exception, "channel{0} callback error", _consumer != null ? $" {_consumer.ConsumerTags[0]}" : string.Empty);
            Try_unbind_onshutdown();
            On_rabbit_ConnClosed(-1, e.Exception.Message);            
        }

        private async void On_rabbit_ConnClosed(int errcode, string errmsg)
        {
            var consumerTag = _consumer?.ConsumerTags[0];
            RbmqAckableConsumerOnReconnectEventArgs? retryInfo = null;
            for (var i1 = true; true; i1 = false)
            {
                try
                {
                    if (i1) await Task.Delay(100).ConfigureAwait(false); 
                    Stop();

                    var eh = OnPreReconnect;
                    if (eh == null)
                    {
                        await Task.Delay(_rabbit.ConnFactory.RequestedHeartbeat).ConfigureAwait(false);
                    }
                    else
                    {
                        retryInfo ??= new RbmqAckableConsumerOnReconnectEventArgs(consumerTag, _rabbit.ConnFactory.RequestedHeartbeat, errcode, errmsg);
                        retryInfo.Is1st = i1;
                        var t = eh(this, retryInfo);
                        if (t != null) await t!.ConfigureAwait(false);
                    }

                    Init_channel_consumer(ref consumerTag);
                    break;
                }
                catch (Exception ex)
                {
                    log?.LogDebug(ex, $"{(!_rabbit.IsOpened ? "reopen error" : "reopen ok but relink error")} and will retry again...");
                }
            }
        }

        Task On_consumer_Registered(object sender, ConsumerEventArgs e)
        {                       
            log?.LogDebug($"channel '{e.ConsumerTags[0]}' on queue='{this.Queue}' open ok");
            return Task.CompletedTask;
        }

        private async Task On_consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {                
                var t = OnReceived?.Invoke(this, new RbmqAckableConsumerOnReceivedEventArgs(_channel!, e));
                if (t != null) await t!.ConfigureAwait(false); 
            }
            catch (Exception ex)
            {
                log?.LogError(ex, $"rabbitmq consumer {e.ConsumerTag} received error");
            }            
        }         
    }

    public class RbmqAckableConsumerOnReconnectEventArgs
    {
        internal RbmqAckableConsumerOnReconnectEventArgs(string? consumerTag, TimeSpan heartbeat, int errcode, string errmsg)
        {
            this.ConsumerTag = consumerTag;
            this.Heartbeat = heartbeat;
            this.Errcode = errcode;
            this.Errmsg = errmsg;
        }

        private bool? _Is1st;
        /// <summary>conn断线后是否第一次重连</summary>
        public bool Is1st
        {
            get => _Is1st ?? false;
            internal set => _Is1st = _Is1st == null && value;
        }

        public readonly string? ConsumerTag;
        public readonly TimeSpan Heartbeat;
        public readonly int Errcode;
        public readonly string Errmsg;
    }

    public readonly struct RbmqAckableConsumerOnReceivedEventArgs
    {
        public RbmqAckableConsumerOnReceivedEventArgs(IModel channel, BasicDeliverEventArgs e)
        {
            this.Channel = channel;
            this.Args = e;
        }

        public readonly IModel Channel;
        public readonly BasicDeliverEventArgs Args;
        public readonly string ConsumerTag => Args?.ConsumerTag!;

        /// <summary>
        /// var (channel, e) = args;
        /// </summary>
        public readonly void Deconstruct(out IModel channel, out BasicDeliverEventArgs e)
        {
            channel = this.Channel;
            e = this.Args;
        }
    }

#nullable disable
}