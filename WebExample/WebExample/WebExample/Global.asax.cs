using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebExample.Util;

namespace WebExample
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            Log.Info("服務器重啟");
            MqWapper.Instance().Start();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            string Message = "";
            Exception ex = Server.GetLastError();
            Message = "發生錯誤的網頁:{0}錯誤訊息:{1}堆疊內容:{2}";
            Message = String.Format(Message, Request.Path + Environment.NewLine, ex.GetBaseException().Message + Environment.NewLine, Environment.NewLine + ex.StackTrace);
            Log.Debug(Message);
        }
    }
}
