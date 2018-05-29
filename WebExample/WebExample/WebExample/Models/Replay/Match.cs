using jIAnSoft.Framework.Nami.TaskScheduler;
using Newtonsoft.Json;
using NLog;

using System;
using System.Collections.Generic;
using WebExample.Models.Data;
using WebExample.Models.Entity;
using WebExample.Models.Entity.ActionCode;
using WebExample.Util;

namespace WebExample.Models.Replay
{
    public class Match
    {
        //private static Dictionary<long,int> maxTimeDiff = new Dictionary<long, int>();
        private int maxTimeDiff = 0;

        private int avgTimeForOdds = 0; //平均值行時間
        private int modTimeForOdds = 0;
        private int avgTimeForScout = 0;
        private int modTimeForScout = 0;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public long MatchId { get; set; }

        public ScoutStruct[] Scout { get; set; }
        public OddsStruct[] Odds { get; set; }

        public Match()
        {
        }

        public Match(long matchId, int totaltime)
        {
            Scout = DataSave.FetchLiveScout(matchId);
            Odds = DataSave.FetchOdds(matchId);
            MatchId = matchId;

            var totalMiSec = totaltime * 60 * 1000;

            modTimeForOdds = totalMiSec % Odds.Length;
            avgTimeForOdds = totalMiSec / Odds.Length;

            modTimeForScout = totalMiSec % Scout.Length;
            avgTimeForScout = totalMiSec / Scout.Length;
        }

        public void Start()
        {
            DataSave.UpdateMatchReplayStatus(MatchId, 6);
            DataSave.UpdateMatcScore(MatchId, "0:0");
            DataSave.UpdateOddsStatus(MatchId, 4);

            if (Scout.Length > 0)
            {
                PushScoutToMq(0);
                Log.Info($"賽事編號:{MatchId} 開始傳送走地資料");
            }
            else
            {
                Log.Info($"賽事編號:{MatchId} 沒有走地資料");
            }

            if (Odds.Length > 0)
            {
                PushOddstToSportServer(0);
                Log.Info($"賽事編號:{MatchId} 開始傳送賠率資料");
            }
            else
            {
                Log.Info($"賽事編號:{MatchId} 沒有賠率資料");
            }
        }

        private void PushScoutToMq(int index)
        {
            if (index + 1 >= Scout.Length)
            {
                Log.Info($"賽事編號:{MatchId} 走地資料送完");
                DataSave.UpdateMatchStatus(MatchId, 100);
                return;
            }

            Scout[index].Type = Scout[index].TypeId;

            if (Scout[index].Type == 1013 && (Scout[index].ExtraInfo == 6 ||
                                              Scout[index].ExtraInfo == 7 ||
                                              Scout[index].ExtraInfo == 31 ||
                                              Scout[index].ExtraInfo == 100))
            {
                DataSave.UpdateMatchCurrentPeriodStart(MatchId);
                DataSave.UpdateMatchStatus(MatchId, Scout[index].ExtraInfo);
            }

            if (index > 1 && Scout[index].MatchScore != Scout[index - 1].MatchScore)
            {
                DataSave.UpdateMatcScore(MatchId, Scout[index].MatchScore);
            }

            var tj = new TransJson
            {
                Success = true,
                Code = "1.0",
                Message = null,
                Key = "RMQ_LiveCompetition",
                Result = Scout[index]
            };
            //走地動畫
            ToMq("livescout", Scout[index].MatchId.ToString(), JsonConvert.SerializeObject(tj));
            //通知 Clinet 狀態
            var tSportServer = JsonConvert.SerializeObject(Scout[index]);
            ToMq("livescout", "sport.server", tSportServer);

            Log.Info($"賽事編號:{MatchId} 走地動畫&通知推送 {tSportServer}");

            int timer = 0;
            if (index <= Odds.Length - 1)
                timer = avgTimeForScout;
            else
                timer = modTimeForScout;

            Nami.Delay(timer).Do(() => { PushScoutToMq(index + 1); });
        }

        private void PushOddstToSportServer(int index)
        {
            try
            {
                if (index + 1 >= Odds.Length)
                {
                    Log.Info($"賽事編號:{MatchId} 賠率資料送完");
                    DataSave.SwitchOddsActive(MatchId);
                    DataSave.UpdateMatchStatus(MatchId, 100);
                    CacheTool.RemoveThread(MatchId);
                    return;
                }

                Odds[index].Status = 1;
                var ot = new OperationRequest
                {
                    OperationCode = (int)OperationCode.MainService,
                    ActionCode = (int)MainServiceActionCode.BroadcastOdds,
                    Parameters = new Dictionary<byte, object> { { 1, Odds[index] } }
                };
                var lodData = JsonConvert.SerializeObject(Odds[index]);
                Log.Info($"賽事編號:{MatchId},第{index + 1}次發送賠率資料:{lodData}");
    
                var pevMsgNr = (index == 0) ? 0 : Odds[index - 1].MsgNr;
                if (Odds[index].MsgNr > pevMsgNr || pevMsgNr == 0)
                {
                    DataSave.SwitchOddsActive(Odds[index].MatchId, /*Odds[index].OddsId,*/ Odds[index].MsgNr);
                    DataSave.UpdateMatchCurrentPeriodStart(Odds[index].MatchId);
                }
               
                int timer = 0;
                if (index <= Odds.Length - 1)
                    timer = avgTimeForOdds;
                else
                    timer = modTimeForOdds;
 
                Nami.Delay(timer).Do(() => { PushOddstToSportServer(index + 1); });
            }
            catch (Exception ex)
            {
                var lodData = JsonConvert.SerializeObject(Odds[index]);
                Log.Info($"賽事編號:{MatchId},第{index + 1}次失敗，賠率資料:{lodData},失敗原因:{ex.Message}, 失敗原因:{ex.StackTrace}");
            }

        }

        private static void ToMq(string key, string routingKey, string msg)
        {
            try
            {
                MqWapper.Instance().Topic(key, routingKey, msg);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ex.StackTrace} {ex.Message}");
            }
        }
    }
}