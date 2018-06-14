using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using WebExample.Models.Data;
using WebExample.Models.Entity;
using WebExample.Models.Entity.ActionCode;
using WebExample.Util;
using System.Threading;

namespace WebExample.Models.Replay
{
    public class MatchV1
    {
        private int avgTimeForOdds = 0; //平均值行時間
        private int modTimeForOdds = 0;
        private int avgIndex = 0;
        private int modIndex = 0;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public long MatchId { get; set; }

        public ScoutStruct[] Scout { get; set; }
        public OddsStruct[] Odds { get; set; }
        public MatchV1()
        {
        }

        public MatchV1(long matchId, int totaltime)
        {
            //先結算上一場賽事的單
            DataSave.DoSettle(matchId, 1);//全場
            DataSave.DoSettle(matchId, 2);//半場

            Scout = DataSave.FetchLiveScout(matchId);
            Odds = DataSave.FetchOdds(matchId);

            MatchId = matchId;

            var totalMiSec = totaltime * 60 * 1000;

            //基本上賠率資料會大於動畫資料
            modTimeForOdds = totalMiSec % (Odds.Length-1);
            avgTimeForOdds = totalMiSec / (Odds.Length-1);

            modIndex = (Odds.Length - 1) % (Scout.Length - 2);
            avgIndex = ((Odds.Length - 1) - modIndex) / (Scout.Length - 2);

        }

        public void BetRadarStart()
        {
            CacheTool.MatchAdd(MatchId);
            DataSave.UpdateMatchReplayStatus(MatchId, 6);
            DataSave.UpdateMatcScore(MatchId, "0:0");
            DataSave.UpdateOddsStatus(MatchId, 4);

            Log.Info($"賽事編號:{MatchId} 開始傳送走地資料");
            Log.Info($"賽事編號:{MatchId} 開始傳送賠率資料");
            int j = 0;
            for (int i = 0; i <= Odds.Length; i++)
            {
                PushProcess(i,j);
                if (j == 0 || i == modIndex || (i - modIndex) % avgIndex == 0)
                {
                    j = j + 1;
                }
                Thread.Sleep(avgTimeForOdds);
            }
        }

        #region 原有的Function

        private void PushProcess(int oIndex,int sIndex)
        {
            try
            {
                if (oIndex + 1 > Odds.Length)
                {
                    Log.Info($"賽事編號:{MatchId} 第{sIndex + 1}次走地資料送完");
                    Log.Info($"賽事編號:{MatchId},第{oIndex + 1}次賠率資料送完");
                    DataSave.SwitchOddsActive(MatchId);
                    //賠率送完即結算
                    DataSave.DoSettle(MatchId, 1);//全場
                    DataSave.DoSettle(MatchId, 2);//半場
                    CacheTool.MatchRemove(MatchId);
                    return;
                }

                if (sIndex == 0 || oIndex == modIndex || (oIndex - modIndex) % avgIndex == 0 )
                {
                    PushScoutToMq(sIndex);
                }

                PushOddstToSportServer(oIndex);
            }
            catch (Exception ex)
            {
                Log.Info($"賽事編號:{MatchId},第{oIndex + 1}次oIndex失敗，失敗原因:{ex.Message}，失敗原因:{ex.StackTrace}");
            }

        }


        private void PushScoutToMq(int index)
        {
            try
            {
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

                Log.Info($"賽事編號:{MatchId} 第{index + 1}次走地動畫&通知推送 {tSportServer}");
            }
            catch (Exception ex)
            {
                var tSportServer = JsonConvert.SerializeObject(Scout[index]);
                Log.Info($"賽事編號:{MatchId},第{index + 1}次失敗，失敗原因:{ex.Message}， 失敗原因:{ex.StackTrace}");
            }
        }

        private void PushOddstToSportServer(int index)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                var lodData = JsonConvert.SerializeObject(Odds[index]);
                Log.Info($"賽事編號:{MatchId},第{index + 1}次失敗，失敗原因:{ex.Message}，失敗原因:{ex.StackTrace}");
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
        #endregion
    }
}