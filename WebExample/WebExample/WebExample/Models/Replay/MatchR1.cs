using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using WebExample.Models.Data;
using WebExample.Models.Entity;
using WebExample.Models.Entity.ActionCode;
using WebExample.Util;
using System.Threading;
using System.Linq;

namespace WebExample.Models.Replay
{
    public class MatchR1
    {
        private int avgTimeForOdds = 0; //平均值行時間
        private int modTimeForOdds = 0;
        private int avgIndex = 0;
        private int modIndex = 0;
        private int flag = 0;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public long MatchId { get; set; }

        public ScoutStruct[] Scout { get; set; }
        public OddsStruct[] Odds { get; set; }
        public List<RiskData> RiskOddsDataList { get; set; }


        public MatchR1()
        {
        }

        public MatchR1(long matchId, int totaltime)
        {
            //先結算上一場賽事的單
            DataSave.DoSettle(matchId, 1);//全場
            DataSave.DoSettle(matchId, 2);//半場
            RiskOddsDataList = new List<RiskData>();
            Scout = DataSave.FetchLiveScout(matchId);
            Odds = DataSave.FetchOdds(matchId);

            long tempMsgnr = -1L;

            #region 組批次檔
            RiskData rData = new RiskData();
            LiveMatchData lData = new LiveMatchData();
            for (int i = 0; i < Odds.Length; i++)
            {
                var tmpOddsTypeId = long.Parse(string.IsNullOrEmpty(Odds[i].OddsTypeID) ? "0" : Odds[i].OddsTypeID);
                var tmpSubtype = long.Parse(string.IsNullOrEmpty(Odds[i].Subtype) ? "0" : Odds[i].Subtype);
                if (Odds[i].MsgNr == tempMsgnr)
                {
                    //RiskMan 如果有不同的 TypeId & SubType 要新增
                    bool isNeedToInsert = true;
                    foreach (var typeIdList in rData.OddsTypeIdList)
                    {
                        if (typeIdList.OddsTypeId == tmpOddsTypeId
                         && typeIdList.SubType == tmpSubtype)
                        {
                            isNeedToInsert = false;
                            break;
                        }
                    }

                    if (isNeedToInsert)
                    {
                        rData.OddsTypeIdList.Add(
                             new OddsTypeIdList()
                             {
                                 OddsTypeId = tmpOddsTypeId,
                                 SubType = tmpSubtype
                             }
                        );
                    }

                    rData.OddsTypeIdList.Where(x => x.OddsTypeId == tmpOddsTypeId && x.SubType == tmpSubtype).First().OddsChangeList.Add(
                    new OddsChangeList()
                    {
                        OddsId = Odds[i].OddsId,
                        OddsIdOri = Odds[i].OddsIdOri.ToString(),
                        Odds = Odds[i].Odds.ToString(),
                        SpecialBetValue = Odds[i].SpecialBetValue,
                        ForTheRest = Odds[i].ForTheRest,
                        Score = Odds[i].Score,
                        Status = 1,
                        Timestamp = Odds[i].CreateTime
                    });

                    if (i == Odds.Length - 1)
                    {
                        RiskOddsDataList.Add(rData);
                    }
                }
                else
                {
                    if (i != 0)
                    {
                        RiskOddsDataList.Add(rData);
                        rData = new RiskData();
                    }
                    tempMsgnr = Odds[i].MsgNr;
                    rData.MatchId = Odds[i].MatchId;
                    rData.Msgnr = Odds[i].MsgNr;
                    rData.Source = 1;
                    rData.OddsKind = 2;
                    //RiskMan 如果有不同的 TypeId & SubType 要新增
                    bool isNeedToInsert = true;
                    foreach (var typeIdList in rData.OddsTypeIdList)
                    {
                        if (typeIdList.OddsTypeId == tmpOddsTypeId
                         && typeIdList.SubType == tmpSubtype)
                        {
                            isNeedToInsert = false;
                            break;
                        }
                    }

                    if (isNeedToInsert)
                    {
                        rData.OddsTypeIdList.Add(
                             new OddsTypeIdList()
                             {
                                 OddsTypeId = tmpOddsTypeId,
                                 SubType = tmpSubtype
                             }
                        );
                    }

                    rData.OddsTypeIdList.Where(x => x.OddsTypeId == tmpOddsTypeId && x.SubType == tmpSubtype).First().OddsChangeList.Add(
                    new OddsChangeList()
                    {
                        OddsId = Odds[i].OddsId,
                        OddsIdOri = Odds[i].OddsIdOri.ToString(),
                        Odds = Odds[i].Odds.ToString(),
                        SpecialBetValue = Odds[i].SpecialBetValue,
                        ForTheRest = Odds[i].ForTheRest,
                        Score = Odds[i].Score,
                        Status = 1,
                        Timestamp = Odds[i].CreateTime
                    });
                }
            }

            #endregion 

            MatchId = matchId;

            var totalMiSec = totaltime * 60 * 1000;

            //比較看看是賠率批次大還是動畫大
            if (RiskOddsDataList.Count >= Scout.Length)
            {
                flag = 1;
                modTimeForOdds = totalMiSec % (RiskOddsDataList.Count - 1);
                avgTimeForOdds = totalMiSec / (RiskOddsDataList.Count - 1);
                modIndex = (RiskOddsDataList.Count - 1) % (Scout.Length - 2);
                avgIndex = ((RiskOddsDataList.Count - 1) - modIndex) / (Scout.Length - 2);
            }
            else
            {
                flag = 0;
                modTimeForOdds = totalMiSec % (Scout.Length - 1);
                avgTimeForOdds = totalMiSec / (Scout.Length - 1);
                modIndex = (Scout.Length - 1) % (RiskOddsDataList.Count - 2);
                avgIndex = ((Scout.Length - 1) - modIndex) / (RiskOddsDataList.Count - 2);
            }
        }

        public void RiskmanStart()
        {
            CacheTool.MatchAdd(MatchId);
            DataSave.UpdateMatchReplayStatus(MatchId, 6);
            DataSave.UpdateMatcScore(MatchId, "0:0");
            DataSave.DeleteAndBackupOdds(MatchId);

            Log.Info($"賽事編號:{MatchId} 開始傳送走地資料");
            Log.Info($"賽事編號:{MatchId} 開始傳送賠率資料");

            if (flag == 1)
            {
                for (int i = 0, j = 0; i <= RiskOddsDataList.Count; i++)
                {
                    PushProcessByOdds(i, j);
                    if (j == 0 || i == modIndex || (i - modIndex) % avgIndex == 0)
                    {
                        j = j + 1;
                    }
                    Thread.Sleep(avgTimeForOdds);
                }
            }
            else
            {
                for (int i = 0, j = 0; i <= Scout.Length; i++)
                {
                    PushProcessByScout(i, j);
                    if (j == 0 || i == modIndex || (i - modIndex) % avgIndex == 0)
                    {
                        j = j + 1;
                    }
                    Thread.Sleep(avgTimeForOdds);
                }
            }
           
        }

        #region 原有的Function

        private void PushProcessByOdds(int oIndex, int sIndex)
        {
            try
            {
                if (oIndex + 1 > RiskOddsDataList.Count)
                {
                    Log.Info($"賽事編號:{MatchId} 第{sIndex + 1}次走地資料送完");
                    Log.Info($"賽事編號:{MatchId},第{oIndex + 1}次賠率資料送完");
                    //賠率送完即結算
                    DataSave.DoSettle(MatchId, 1);//全場
                    DataSave.DoSettle(MatchId, 2);//半場
                    CacheTool.MatchRemove(MatchId);
                    return;
                }

                if (sIndex == 0 || oIndex == modIndex || (oIndex - modIndex) % avgIndex == 0)
                {
                    PushScoutToMq(sIndex);
                }

                PushOddsToRiskMan(oIndex);
            }
            catch (Exception ex)
            {
                Log.Info($"賽事編號:{MatchId},第{oIndex + 1}次oIndex失敗，失敗原因:{ex.Message}，失敗原因:{ex.StackTrace}");
            }
        }

        private void PushProcessByScout(int ssIndex, int ooIndex)
        {
            try
            {
                if (ssIndex + 1 > Scout.Length)
                {
                    Log.Info($"賽事編號:{MatchId} 第{ssIndex + 1}次走地資料送完");
                    Log.Info($"賽事編號:{MatchId},第{ooIndex + 1}次賠率資料送完");
                    //賠率送完即結算
                    DataSave.DoSettle(MatchId, 1);//全場
                    DataSave.DoSettle(MatchId, 2);//半場
                    CacheTool.MatchRemove(MatchId);
                    return;
                }
                if (ooIndex == 0 || ssIndex == modIndex || (ssIndex - modIndex) % avgIndex == 0)
                {
                    PushOddsToRiskMan(ooIndex);
                }
                PushScoutToMq(ssIndex);
            }
            catch (Exception ex)
            { 
                Log.Info($"賽事編號:{MatchId},第{ssIndex + 1}次ssIndex失敗，失敗原因:{ex.Message}，失敗原因:{ex.StackTrace}");
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

        private void PushOddsToRiskMan(int index)
        {
            try
            {
                var message = $"賽事編號:{MatchId},第{index + 1}次賠率資料傳送";
                SendRisk.DoOddsBatch(message, JsonConvert.SerializeObject(RiskOddsDataList[index]));
                Log.Info(message + $", Data => {JsonConvert.SerializeObject(RiskOddsDataList[index])}");
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