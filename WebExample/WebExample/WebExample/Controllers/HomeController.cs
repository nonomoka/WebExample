using jIAnSoft.Framework.Nami.TaskScheduler;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using WebExample.Models.Data;
using WebExample.Models.Entity;
using WebExample.Models.Replay;
using WebExample.Models.Resp;
using WebExample.Util;

namespace WebExample.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static readonly AppConfig AppConfig = new AppConfig();
        

        // GET: Home
        public ActionResult Index()
        {
            MqWapper.Instance().Start();
            var matches = InitMatchList();
            ViewBag.MatchList = DataSave.MatchListEnableGet(matches);
            return View();
        }

        /// <summary>
        /// 查詢賽式列表
        /// </summary>
        /// <param name="matchDate"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MatchListGet(string matchDate)
        {
            ResponesJson jsonResp = new ResponesJson();
            try
            {
                if (matchDate.Length != 10)
                {
                    jsonResp.Success = false;
                    jsonResp.ResultData = $"日期格式不對(yyyy-MM-dd) => {matchDate}";
                }
                var matchList = DataSave.MatchListGet(matchDate);
                jsonResp.Success = true;
                jsonResp.ResultData = matchList;
            }
            catch (Exception ex)
            {
                jsonResp.ResultData = ex.Message;
                jsonResp.Success = false;
            }
            return Content(JsonConvert.SerializeObject(jsonResp));
            
        }

        public ActionResult CustomRun(int customTime,string matchIdList)
        {
            ResponesJson jsonResp = new ResponesJson();
            try
            {
                var matches = InitMatchList();

                //指定賽事資料
                string[] matchArray = matchIdList.Split(',');
                List<long> checklist = new List<long>();
                foreach (var id in matchArray)
                {
                    checklist.Add(long.Parse(id));
                }

                //checklist.Add(13854605);
                //checklist.Add(13649001);

                List<long> mlist = new List<long>();

                foreach (var matchid in checklist)
                {
                    if (matches.Contains(matchid))
                        mlist.Add(matchid);
                }

                if (mlist.Count == 0)
                {
                    Log.Info($"無賽事走地與賠率資料");
                    jsonResp.Success = false;
                    jsonResp.ResultData = "無賽事走地與賠率資料";
                    return Content(JsonConvert.SerializeObject(jsonResp));
                }

                foreach (var matchid in mlist)
                {
                    if (!CacheTool.ThreadExist(matchid))
                    {
                        Thread worker = new Thread(RunTask);
                        JobParam jParam = new JobParam();
                        jParam.MatchID = matchid;
                        jParam.Time = customTime;
                        worker.Start(jParam);
                        CacheTool.AddThread(matchid, worker);
                    }
                }

                System.Threading.Thread.Sleep(1000);
                jsonResp.Success = true;
                jsonResp.ResultData = RefreshList();
            }
            catch (Exception ex)
            {
                jsonResp.ResultData = ex.Message;
                jsonResp.Success = false;
            }
            return Content(JsonConvert.SerializeObject(jsonResp));

        }

        public ActionResult RefreshExecuteList()
        {
            ResponesJson jsonResp = new ResponesJson();
            try
            {
                jsonResp.Success = true;
                jsonResp.ResultData = RefreshList();
            }
            catch (Exception ex)
            {
                jsonResp.ResultData = ex.Message;
                jsonResp.Success = false;
            }
            return Content(JsonConvert.SerializeObject(jsonResp));
        }

        private static void RunTask(object param)
        {
            var jobParam = (JobParam)param;
            Nami.Delay(1).Seconds().Do(() =>
            {
                Log.Info($"即將重播 {jobParam.MatchID} 場的賽事走地與賠率資料");
                new Match(jobParam.MatchID, jobParam.Time).Start();
            });
        }


        public ActionResult RadomRun(int randomTime,int randomCnt)
        {
            ResponesJson jsonResp = new ResponesJson();
            try
            {
                List<long> mlist = new List<long>();
                var matches = InitMatchList();
                Random rand = new Random(Guid.NewGuid().GetHashCode());
                List<int> listLinq = new List<int>(Enumerable.Range(0, matches.Count()-1));
                listLinq = listLinq.OrderBy(num => rand.Next()).ToList<int>();

                for (int i = 0; i < randomCnt; i++)
                {
                    mlist.Add(matches[listLinq[i]]);
                }

                foreach (var matchid in mlist)
                {
                    if (!CacheTool.ThreadExist(matchid))
                    {
                        Thread worker = new Thread(RunTask);
                        CacheTool.AddThread(matchid, worker);
                        JobParam jParam = new JobParam();
                        jParam.MatchID = matchid;
                        jParam.Time = randomTime;
                        worker.Start(jParam);
                    }
                }

                System.Threading.Thread.Sleep(1000);
                jsonResp.Success = true;
                jsonResp.ResultData = RefreshList();
            }
            catch (Exception ex)
            {
                jsonResp.ResultData = ex.Message;
                jsonResp.Success = false;
            }
            return Content(JsonConvert.SerializeObject(jsonResp));

        }

        private List<ExecuteMatchListGet> RefreshList()
        {
            var ThreadList = CacheTool.ThreadList;
            List<ExecuteMatchListGet> eList = new List<ExecuteMatchListGet>();
            foreach (var tid in ThreadList)
            {
                var data = DataSave.ExecuteMatchListGet(tid.Key);
                if (data.Count > 0)
                {
                    eList.Add(data[0]);
                }
            }
            return eList;
        }

        private List<long> InitMatchList()
        {
            //取有賠率與走地的賽事編號
            var matches = new List<long>();
            var matchStr = CacheService.GetInitMatchList();
            if (!string.IsNullOrEmpty(matchStr))
            {
                matches = JsonConvert.DeserializeObject<List<long>>(matchStr);
            }
            else
            {
                var scoutMatchIds = DataSave.FetchLiveScout();
                var oddsMatchIds = DataSave.FetchOdds();
                foreach (var matchId in oddsMatchIds)
                {
                    foreach (var scoutMatchId in scoutMatchIds)
                    {
                        if (matchId > 0 && matchId == scoutMatchId && !matches.Contains(matchId))
                        {
                            matches.Add(matchId);
                        }
                    }
                }
                CacheService.SetInitMatchList(matches, false);
            }
            return matches;
        }

        private static void RunTaskTest(object param)
        {
            var jobParam = (int)param;
            Nami.Delay(jobParam).Seconds().Do(() =>
            {
                Log.Info($"Test Thread");
            });
        }
    }
}