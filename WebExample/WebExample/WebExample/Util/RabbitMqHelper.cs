using System;
using System.Text;
using jIAnSoft.Framework.Nami.TaskScheduler;
using NLog;
using RabbitMQ.Client;

namespace WebExample.Util
{
    public class RabbitMqHelper
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly object _lockObj = new object();
        private ConnectionFactory _factory;

        private IConnection _mqConnection;
        private IModel _channel;


        private string HostName { get; }
        private string UserName { get; }
        private string Password { get; }
        private int Port { get; }
        private bool SelfClose { get; set; }

        private RabbitMqHelper()
        {
        }

        public RabbitMqHelper(string hostName, string userName, string password, int port)
        {
            HostName = hostName;
            Port = port;
            UserName = userName;
            Password = password;
        }
        public void Start()
        {
            Connect();
        }

        public void Connect()
        {
            //連線到 RabbitMQ 
            if (_mqConnection != null)
            {
                if (_mqConnection.IsOpen)
                {
                    Log.Info($"{HostName} _mqConnection.IsOpen=>{_mqConnection.IsOpen} return;");
                    return;
                }

                _mqConnection.ConnectionShutdown -= MqConnectionShutdown;
                _mqConnection.Close();
                _mqConnection.Dispose();
                _mqConnection = null;
            }

            Log.Info($"try to connect {HostName}");

            _factory = new ConnectionFactory
            {
                HostName = HostName,
                Port = Port,
                UserName = UserName,
                Password = Password
            };

            try
            {
                lock (_lockObj)
                {
                    _mqConnection = _factory.CreateConnection();
                    _mqConnection.ConnectionShutdown += MqConnectionShutdown;
                    _channel = _mqConnection.CreateModel();
                }

                Log.Info($"RabbitMQ Connection to {HostName} is open => {_mqConnection.IsOpen}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"{HostName} Connect Exception:{e.StackTrace} {e.Message}");
                Nami.Delay(5).Seconds().Do(Connect);
            }
        }

        public IModel CreateChannel()
        {
            IModel channel;
            lock (_lockObj)
            {
                channel = _mqConnection.CreateModel();
            }
            return channel;
        }

        private void MqConnectionShutdown(object sender, ShutdownEventArgs s)
        {
            if (SelfClose)
            {
                //自已關閉的操作不用重連
                return;
            }
            Log.Info($"{HostName} RabbitMQ onnection shutdown, So reconnect now.");
            Connect();
        }


        public void PublishMessageByFanout(string key, string routingKey, string msg)
        {
            try
            {
                if (_channel.IsClosed)
                {
                    Connect();
                }

                lock (_lockObj)
                {
                    var body = Encoding.UTF8.GetBytes(msg);
                    _channel.ExchangeDeclare(key, ExchangeType.Fanout);
                    _channel.BasicPublish(key, routingKey, null, body);
                }
                //Log.Info($"Sent message to MQ key:{key}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"{HostName} {e.StackTrace} {e.Message}");
            }
            //Cleaner.ExchangeDeclares.AddOrUpdate(key, DateTime.Now, (keya, oldValue) => DateTime.Now);
        }

        public void PublishMessageByFanout(string key, string routingKey, byte[] msg)
        {
            try
            {
                lock (_lockObj)
                {
                    _channel.ExchangeDeclare(key, ExchangeType.Fanout);
                    _channel.BasicPublish(key, routingKey, null, msg);
                }
                //Log.Info($"Sent message to MQ key:{key}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"{HostName} {e.StackTrace} {e.Message}");
            }
            //Cleaner.ExchangeDeclares.AddOrUpdate(key, DateTime.Now, (keya, oldValue) => DateTime.Now);
        }

        public void PublishMessageByDirect(string key, string routingKey, string msg)
        {
            try
            {
                lock (_lockObj)
                {
                    var body = Encoding.UTF8.GetBytes(msg);
                    _channel.ExchangeDeclare(key, "direct");
                    _channel.BasicPublish(key, routingKey, null, body);
                }
                //Log.Info($"Sent message to MQ key:{key}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"{HostName} {e.StackTrace} {e.Message}");
            }
        }

        public void PublishMessageByTopic(string key, string routingKey, string msg, bool durable = false)
        {
            PublishMessageByTopic(key, routingKey, Encoding.UTF8.GetBytes(msg), durable);
        }

        public void PublishMessageByTopic(string key, string routingKey, byte[] msg, bool durable = false)
        {
            try
            {
                if (_channel.IsClosed)
                {
                    Connect();
                }

                lock (_lockObj)
                {
                    _channel.ExchangeDeclare(key, ExchangeType.Topic, durable);
                    _channel.BasicPublish(key, routingKey, null, msg);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"{HostName} {e.StackTrace} {e.Message}");
            }
        }

        public void PublishMessage(IModel channel, string key, string msg)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(msg);
                lock (_lockObj)
                {
                    channel.ExchangeDeclare(key, "fanout");
                    channel.BasicPublish(key, "", null, body);
                }
                //Log.Info($"Sent message to MQ key:{key}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"{e.StackTrace} {e.Message}");
            }
        }


        public void DeleteExChange(string key)
        {
            try
            {
                lock (_lockObj)
                {
                    _channel.ExchangeDelete(key);
                }
                Log.Info($"DeleteExChange key:{key}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"{e.StackTrace} {e.Message}");
            }
        }

        public void DeleteExChange(IModel channel, string key)
        {
            try
            {
                lock (_lockObj)
                {
                    channel.ExchangeDelete(key);
                }
                Log.Info($"DeleteExChange key:{key}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"{e.StackTrace} {e.Message}");
            }
        }

        public void Close()
        {
            try
            {
                lock (_lockObj)
                {
                    SelfClose = true;
                    _channel.Close();
                    _mqConnection.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"{e.StackTrace} {e.Message}");
            }
        }
    }
}