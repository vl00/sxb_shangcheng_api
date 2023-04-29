using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.BgServices
{
    public static partial class RabbitmqUtils
    {
        public static IBasicProperties GetPersistentProps(this IModel channel, IDictionary<string, object> headers = null)
        {
            var props = channel.CreateBasicProperties();
            props.DeliveryMode = 2;
            if (headers?.Count > 0) props.Headers = headers;
            return props;
        }

        public static bool ConfirmPublish(this IModel channel, string exchange, string routingKey, IBasicProperties basicProperties, ReadOnlyMemory<byte> body, TimeSpan? timeout = null)
        {
            return ConfirmPublish(channel, exchange, routingKey, false, basicProperties, body, timeout);
        }

        public static bool ConfirmPublish(this IModel channel, string exchange, string routingKey, bool mandatory = false, IBasicProperties basicProperties = null, ReadOnlyMemory<byte> body = default, TimeSpan? timeout = null)
        {
            channel.ConfirmSelect();
            channel.BasicPublish(exchange, routingKey, mandatory, basicProperties, body);
            if (timeout == null || timeout == Timeout.InfiniteTimeSpan) return channel.WaitForConfirms();
            else
            {
                var b = channel.WaitForConfirms(timeout.Value, out var isTimeout);
                if (isTimeout) throw new TimeoutException("rabbitmq ConfirmselectPublish timeout!!");
                return b;
            }
        }

        public static bool ConfirmPublish(this IModel channel, RabbitMessage msg, TimeSpan? timeout = null)
        {
            var (exchange, routingKey, mandatory, fprops, body) = msg;
            var props = fprops(channel);
            return ConfirmPublish(channel, exchange, routingKey, mandatory, props, body, timeout);
        }

    }
}
