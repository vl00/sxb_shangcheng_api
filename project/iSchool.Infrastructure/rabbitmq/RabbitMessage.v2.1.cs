using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace iSchool.BgServices
{
    [StructLayout(LayoutKind.Auto)]
    public struct RabbitMessage
    {
        string _exchange, _routingKey;
        bool _mandatory;
        Action<IBasicProperties> _basicProperties;
        ReadOnlyMemory<byte> _body;

        public RabbitMessage(string exchange, string routingKey)
        {
            _exchange = exchange;
            _routingKey = routingKey;
            _mandatory = false;
            _basicProperties = null;
            _body = null;
        }

        public RabbitMessage Reset()
        {
            _exchange = _routingKey = null;
            _mandatory = false;
            _basicProperties = null;
            _body = null;
            return this;
        }

        public RabbitMessage SetExchange(string exchange)
        {
            _exchange = exchange ?? "";
            return this;
        }

        public RabbitMessage SetRoutingKey(string routingKey)
        {
            _routingKey = routingKey;
            return this;
        }

        public RabbitMessage SetPubAddress(string exchange, string routingKey)
        {
            _exchange = exchange ?? "";
            _routingKey = routingKey;
            return this;
        }

        public RabbitMessage SetBody(string content) => SetBody(Encoding.UTF8.GetBytes(content));

        public RabbitMessage SetBody(byte[] body) => SetBody(new ReadOnlyMemory<byte>(body));

        public RabbitMessage SetBody(in ReadOnlyMemory<byte> body)
        {
            _body = body;
            return this;
        }

        public RabbitMessage SetMandatory(bool mandatory)
        {
            _mandatory = mandatory;
            return this;
        }

        public RabbitMessage SetType(string type)
        {
            _basicProperties += set_prop;
            return this;

            void set_prop(IBasicProperties prop)
            {
                if (type != null) prop.Type = type;
                else prop.ClearType();
            }
        }

        public RabbitMessage SetMessageId(string messageId)
        {
            _basicProperties += set_prop;
            return this;

            void set_prop(IBasicProperties prop)
            {
                if (messageId != null) prop.MessageId = messageId;
                else prop.ClearMessageId();
            }
        }

        public RabbitMessage SetCorrelationId(string correlationId)
        {
            _basicProperties += set_prop;
            return this;

            void set_prop(IBasicProperties prop)
            {
                if (correlationId != null) prop.CorrelationId = correlationId;
                else prop.ClearCorrelationId();
            }
        }

        public RabbitMessage SetHeader(string key, object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _basicProperties += set_prop;
            return this;

            void set_prop(IBasicProperties prop)
            {
                prop.Headers ??= new Dictionary<string, object>();
                prop.Headers[key] = value;
            }
        }

        public RabbitMessage SetHeader(IEnumerable<(string, object)> kvs)
        {
            if (kvs?.Any() == true) _basicProperties += set_prop;
            return this;

            void set_prop(IBasicProperties prop)
            {
                prop.Headers ??= new Dictionary<string, object>();
                foreach (var kv in kvs)
                    prop.Headers[kv.Item1] = kv.Item2;
            }
        }

        public RabbitMessage SetBasicProperties(Action<IBasicProperties> props)
        {
            _basicProperties += props;
            return this;
        }

        public IBasicProperties GetBasicProperties(IModel channel)
        {
            var props = channel.CreateBasicProperties();
            props.DeliveryMode = 2;
            _basicProperties?.Invoke(props);
            return props;
        }

        /// <summary>
        /// var (exchange, routingKey, mandatory, fprops, body) = msg;
        /// </summary>
        public void Deconstruct(out string exchange, out string routingKey, out bool mandatory, out Func<IModel, IBasicProperties> props, out ReadOnlyMemory<byte> body)
        {
            exchange = _exchange ?? "";
            routingKey = _routingKey;
            mandatory = _mandatory;
            props = GetBasicProperties;
            body = _body;
        }

        public static implicit operator (string, string, bool, Func<IModel, IBasicProperties>, ReadOnlyMemory<byte>)(RabbitMessage msg)
        {
            return msg.ToValueTuple();
        }

        public (string Exchange, string RoutingKey, bool Mandatory, Func<IModel, IBasicProperties> Props, ReadOnlyMemory<byte> Body) ToValueTuple()
        {
            return (_exchange ?? "", _routingKey, _mandatory, GetBasicProperties, _body);
        }
    }
}
