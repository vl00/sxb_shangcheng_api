using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IRabbitMQChannel = RabbitMQ.Client.IModel;

namespace iSchool.BgServices
{
    public interface IRabbitMQConnection
    {
        /// <summary>usually be used for config</summary>
        ConnectionFactory ConnFactory { get; }
        /// <summary>heartbeat seconds</summary>
        ushort Heartbeat { get; }
        /// <summary>see https://www.rabbitmq.com/dotnet-api-guide.html#recovery </summary>
        bool IsAutoReconnect { get; }
        bool IsOpened { get; }
        void Open();
        void Close();
        /// <summary>open connection and create a new channel</summary>
        IRabbitMQChannel OpenChannel();
    }

    public static class RabbitMQ_Extension
    {
        public static T TryOpen<T>(this T connection) where T : IRabbitMQConnection
        {
            if (!connection.IsOpened) connection.Open();
            return connection;
        }
    }

    public class RabbitMQConnection : IRabbitMQConnection
    {
        readonly ILogger _log;
        IConnection _connection;
        readonly object sync_root = new object();

        public RabbitMQConnection(ConnectionFactory connectionFactory, ILoggerFactory loggerFactory = null)
        {
            _log = loggerFactory?.CreateLogger(GetType());
            ConnFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public bool IsAutoReconnect => ConnFactory.AutomaticRecoveryEnabled && ConnFactory.TopologyRecoveryEnabled;
        public bool IsOpened => _connection != null && _connection.IsOpen;
        public ushort Heartbeat => Convert.ToUInt16(ConnFactory.RequestedHeartbeat.TotalSeconds);
        public ConnectionFactory ConnFactory { get; }

        public IRabbitMQChannel OpenChannel()
        {
            if (!IsOpened) Open();
            return _connection.CreateModel();
        }

        public void Open()
        {
            if (!IsOpened)
            {
                lock (sync_root)
                {
                    if (_connection != null)
                    {
                        if (_connection.IsOpen) return;
                        CloseCore();
                    }
                    for (int i = 0, c = 2; i < c; i++)
                    {
                        try
                        {
                            _connection = ConnFactory.CreateConnection();
                            break;
                        }
                        catch
                        {
                            if (i + 1 == c) throw;
                            else Task.Delay(500).Wait();
                        }
                    }
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;
                }
            }
        }

        public void Close()
        {
            if (_connection == null) return;
            lock (sync_root)
            {
                if (_connection == null) return;
                CloseCore();
            }
        }

        void CloseCore()
        {
            try
            {
                _connection.ConnectionShutdown -= OnConnectionShutdown;
                _connection.CallbackException -= OnCallbackException;
                _connection.ConnectionBlocked -= OnConnectionBlocked;

                _connection.Close();
            }
            catch (AlreadyClosedException)
            {
                // ignore
            }
            catch (IOException)
            {
                // ignore
            }
            finally
            {
                DisposeConnection(_connection, 1000 * 2);
                _connection = null;
            }
        }

        void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            _log?.LogDebug("rabbitmq conn blocked: {0}", e.Reason);
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _log?.LogDebug(e.Exception, "rabbitmq conn callback error");
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _log?.LogDebug("rabbitmq conn shutdown, code={0}, reply={1}", e.ReplyCode, e.ReplyText);
        }

        static bool DisposeConnection(IConnection connection, int timeoutMillisec = -1)
        {
            if (connection == default) throw new ArgumentNullException(nameof(connection));
            var tsk = new Task((o) => ((IConnection)o).Dispose(), connection);
            tsk.Start();
            return tsk.Wait(timeoutMillisec);
        }
    }

    public class RabbitMQConnectionForPublish : RabbitMQConnection, IDisposable
    {
        public RabbitMQConnectionForPublish(ConnectionFactory connectionFactory, ILoggerFactory loggerFactory = null)
            : base(connectionFactory, loggerFactory)
        { }

        public void Dispose()
        {
            this.Close();
        }
    }
}
