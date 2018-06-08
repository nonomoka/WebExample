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
                using (HttpClient client = new HttpClient())
                {
                    HttpContent contentPost = new StringContent(JsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(url, contentPost).Result;
                }


                //if (!(WebRequest.Create(url) is HttpWebRequest req))
                //{
                //    if (excuteTimes < 10)
                //    {
                //        Nami.Delay(1).Seconds().Do(() =>
                //        {
                //            DoOddsBatch(JsonData, excuteTimes++);
                //        });
                //    }
                //}
                //else
                //{
                //    request.Method = "POST"; // 方法
                //    request.KeepAlive = true; //是否保持連線
                //    request.ContentType = "application/json; charset=utf-8";
                //    var sb = new StringBuilder();
                //    sb.Append($"{JsonData}");
                //    var param = sb.ToString();
                //    var bs = Encoding.UTF8.GetBytes(param);

                //    Log.Debug($"SendOddsBatchToRiskMan => apiurl:{url}\n parame:{sb}\n");

                //    using (var reqStream = request.GetRequestStream())
                //    {
                //        reqStream.Write(bs, 0, bs.Length);
                //    }

                //    using (var response = (HttpWebResponse)request.GetResponse())
                //    {
                //        using (var sr = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
                //        {
                //            var result = sr.ReadToEnd();
                //            sr.Close();
                //            Log.Debug($"SendOddsBatchToRiskMan => apiurl:{url}\n parame:{sb}\n result:{(int)response.StatusCode}");
                //        }
                //    }
                //}
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

        /*
        public static void DoLiveMatchSend(string JsonData, int excuteTimes = 0)
        {
            try
            {
                var url = $"{ServerSite}/integration/live-match/";

                if (!(WebRequest.Create(url) is HttpWebRequest request))
                {
                    if (excuteTimes < 10)
                    {
                        Nami.Delay(1).Seconds().Do(() =>
                        {
                            DoLiveMatchSend(JsonData, excuteTimes++);
                        });
                    }
                }
                else
                {
                    request.Method = "POST"; // 方法
                    request.KeepAlive = true; //是否保持連線
                    request.ContentType = "application/json; charset=utf-8";
                    var sb = new StringBuilder();
                    sb.Append($"{JsonData}");
                    var param = sb.ToString();
                    var bs = Encoding.UTF8.GetBytes(param);

                    Log.Debug($"SendLiveMatchToRiskMan => apiurl:{url}\n parame:{sb}\n");

                    using (var reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(bs, 0, bs.Length);
                    }

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        using (var sr = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
                        {
                            var result = sr.ReadToEnd();
                            sr.Close();
                            Log.Debug($"SendLiveMatchToRiskMan => apiurl:{url}\n parame:{sb}\n result:{(int)response.StatusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"SendLiveMatchToRiskMan => {ex.StackTrace}\n{ex.Message}\n");
                Nami.Delay(1).Seconds().Do(() =>
                {
                    DoLiveMatchSend(JsonData, excuteTimes++);
                });

            }

            if (excuteTimes >= 10)
            {
                Log.Error($"SendLiveMatchToRiskManData=>{JsonData}\n");
            }
        }
        */
    }
}