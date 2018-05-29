using System.Configuration;
using System.Linq;


namespace WebExample.Models.Entity
{
    public class AppConfig
    {
        public string SportServerMaster { get; set; }
        public string SportServerSlave { get; set; }

        public string RabbitMqMasterHostName { get; set; }
        public int RabbitMqMasterPort { get; set; }
        public string RabbitMqMasterName { get; set; }
        public string RabbitMqMasterPassword { get; set; }

        public string RabbitMqSlaveHostName { get; set; }
        public int RabbitMqSlavePort { get; set; }
        public string RabbitMqSlaveName { get; set; }
        public string RabbitMqSlavePassword { get; set; }

        public string RabbitMqThirdHostName { get; set; }
        public int RabbitMqThirdPort { get; set; }
        public string RabbitMqThirdName { get; set; }
        public string RabbitMqThirdPassword { get; set; }

        // public string SportServer { get; set; }

        public AppConfig()
        {
            Refresh();
        }

        public void Refresh()
        {
            SportServerMaster = ConfigurationManager.AppSettings.AllKeys.ToList().Contains("SportServerMaster")
                ? ConfigurationManager.AppSettings["SportServerMaster"]
                : "";
            SportServerSlave = ConfigurationManager.AppSettings.AllKeys.ToList().Contains("SportServerSlave")
                ? ConfigurationManager.AppSettings["SportServerSlave"]
                : "";

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RabbitMqMaster"]))
            {
                var rabbitMqMasterConfig = ConfigurationManager.AppSettings["RabbitMqMaster"].Split('|');
                RabbitMqMasterHostName = rabbitMqMasterConfig[0];
                RabbitMqMasterPort = int.Parse(rabbitMqMasterConfig[1]);
                RabbitMqMasterName = rabbitMqMasterConfig[2];
                RabbitMqMasterPassword = rabbitMqMasterConfig[3];
            }
            else
            {
                RabbitMqMasterHostName = string.Empty;
                RabbitMqMasterPort = 0;
                RabbitMqMasterName = string.Empty;
                RabbitMqMasterPassword = string.Empty;
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RabbitMqSlave"]))
            {
                var rabbitMqSlaveConfig = ConfigurationManager.AppSettings["RabbitMqSlave"].Split('|');
                RabbitMqSlaveHostName = rabbitMqSlaveConfig[0];
                RabbitMqSlavePort = int.Parse(rabbitMqSlaveConfig[1]);
                RabbitMqSlaveName = rabbitMqSlaveConfig[2];
                RabbitMqSlavePassword = rabbitMqSlaveConfig[3];
            }
            else
            {
                RabbitMqSlaveHostName = string.Empty;
                RabbitMqSlavePort = 0;
                RabbitMqSlaveName = string.Empty;
                RabbitMqSlavePassword = string.Empty;
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RabbitMqThird"]))
            {
                var rabbitMqThirdConfig = ConfigurationManager.AppSettings["RabbitMqThird"].Split('|');
                RabbitMqThirdHostName = rabbitMqThirdConfig[0];
                RabbitMqThirdPort = int.Parse(rabbitMqThirdConfig[1]);
                RabbitMqThirdName = rabbitMqThirdConfig[2];
                RabbitMqThirdPassword = rabbitMqThirdConfig[3];
            }
            else
            {
                RabbitMqThirdHostName = string.Empty;
                RabbitMqThirdPort = 0;
                RabbitMqThirdName = string.Empty;
                RabbitMqThirdPassword = string.Empty;
            }
        }
    }
}