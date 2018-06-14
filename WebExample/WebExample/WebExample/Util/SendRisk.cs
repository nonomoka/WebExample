using jIAnSoft.Framework.Nami.TaskScheduler;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace WebExample.Util
{
    internal static class SendRisk
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static string ServerSite = "http://192.168.4.52:8000";
        
        /// <summary>
        /// 批次-發送賠率資料到 Riskman ,失敗十次記在Log
        /// </summary>
        public static void DoOddsBatch(string JsonData, int excuteTimes = 0)
        {
            try
            {
                var url = $"{ServerSite}/integration/batch-odd-change/";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST"; // 方法
                request.KeepAlive = true; //是否保持連線
                request.ContentType = "application/json; charset=utf-8";
                var sb = new StringBuilder();
                sb.Append($"{JsonData}");
                var param = sb.ToString();
                var bs = Encoding.UTF8.GetBytes(param);

                Log.Debug($"SendOddsBatchToRiskMan => apiurl:{url}\n parame:{sb}\n");

                using (var reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        var result = sr.ReadToEnd();
                        sr.Close();
                        Log.Debug($"SendOddsBatchToRiskMan => apiurl:{url}\n parame:{sb}\n result:{(int)response.StatusCode}");
                    }
                }
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"SendOddsBatchToRiskMan => {ex.StackTrace}\n{ex.Message}\n");
                Nami.Delay(1).Seconds().Do(() =>
                {
                    DoOddsBatch(JsonData, excuteTimes++);
                });

            }

            if (excuteTimes >= 10)
            {
                Log.Error($"SendOddsBatchToRiskManData=>{JsonData}\n");
            }
        }
        public static void DoOddsBatch(string message, string JsonData, int excuteTimes = 0)
        {
            try
            {
                var url = $"{ServerSite}/integration/batch-odd-change/";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST"; // 方法
                request.KeepAlive = true; //是否保持連線
                request.ContentType = "application/json; charset=utf-8";
                var sb = new StringBuilder();
                sb.Append($"{JsonData}");
                var param = sb.ToString();
                var bs = Encoding.UTF8.GetBytes(param);

                Log.Debug($"SendOddsBatchToRiskMan => message:{message}\n apiurl:{url}\n parame:{sb}\n");

                using (var reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        var result = sr.ReadToEnd();
                        sr.Close();
                        Log.Debug($"SendOddsBatchToRiskMan => message:{message}\n apiurl:{url}\n parame:{sb}\n result:{(int)response.StatusCode}");
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"SendOddsBatchToRiskMan => message:{message}\n {ex.StackTrace}\n{ex.Message}\n");
                Nami.Delay(1).Seconds().Do(() =>
                {
                    DoOddsBatch(JsonData, excuteTimes++);
                });

            }

            if (excuteTimes >= 10)
            {
                Log.Error($"SendOddsBatchToRiskManData=> message:{message}\n {JsonData}\n");
            }
        }
    }
}