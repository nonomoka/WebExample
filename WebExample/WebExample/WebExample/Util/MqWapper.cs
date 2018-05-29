using System.Text;
using jIAnSoft.Framework.Nami.Fibers;
using NLog;
using WebExample.Controllers;

namespace WebExample.Util
{
    public class MqWapper
    {
        private RabbitMqHelper Master { get; set; }
        private RabbitMqHelper Slave { get; set; }
        private RabbitMqHelper Third { get; set; }

        private static MqWapper _instance;
        private static IFiber _fiber;

        public static MqWapper Instance()
        {
            return _instance ?? (_instance = new MqWapper());
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private MqWapper()
        {
            _fiber = new PoolFiber();
            _fiber.Start();
            if (!string.IsNullOrEmpty(HomeController.AppConfig.RabbitMqMasterHostName))
            {
                Master = new RabbitMqHelper(
                    HomeController.AppConfig.RabbitMqMasterHostName,
                    HomeController.AppConfig.RabbitMqMasterName,
                    HomeController.AppConfig.RabbitMqMasterPassword,
                    HomeController.AppConfig.RabbitMqMasterPort
                );
            }

            if (!string.IsNullOrEmpty(HomeController.AppConfig.RabbitMqSlaveHostName))
            {

                Slave = new RabbitMqHelper(
                    HomeController.AppConfig.RabbitMqSlaveHostName,
                    HomeController.AppConfig.RabbitMqSlaveName,
                    HomeController.AppConfig.RabbitMqSlavePassword,
                    HomeController.AppConfig.RabbitMqSlavePort
                );
            }

            if (!string.IsNullOrEmpty(HomeController.AppConfig.RabbitMqThirdHostName))
            {
                Third = new RabbitMqHelper(
                    HomeController.AppConfig.RabbitMqThirdHostName,
                    HomeController.AppConfig.RabbitMqThirdName,
                    HomeController.AppConfig.RabbitMqThirdPassword,
                    HomeController.AppConfig.RabbitMqThirdPort
                );
            }
        }

        public void Start()
        {
            Master?.Connect();
            Slave?.Connect();
            Third?.Connect();
        }

        public void Topic(string key, string routingKey, string msg, bool durable = false)
        {
            Topic(key, routingKey, Encoding.UTF8.GetBytes(msg), durable);
        }

        public void Topic(string key, string routingKey, byte[] msg, bool durable = false)
        {
            _fiber.Enqueue(() => { Master?.PublishMessageByTopic(key, routingKey, msg, durable); });
            _fiber.Enqueue(() => { Slave?.PublishMessageByTopic(key, routingKey, msg, durable); });
            _fiber.Enqueue(() => { Third?.PublishMessageByTopic(key, routingKey, msg, durable); });
        }
    }
}