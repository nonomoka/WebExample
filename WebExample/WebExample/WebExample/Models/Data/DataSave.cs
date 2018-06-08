using jIAnSoft.Framework.Brook.Mapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using WebExample.Models.Entity;
using WebExample.Models.Replay;
using NLog;

namespace WebExample.Models.Data
{
    public static class DataSave
    {

        public static int DoSettle(long matchID, int oddsPlayType)
        {
            try
            {
                var list = Brook.Load(DbName.DevMainDb).Execute(
                    CommandType.StoredProcedure,
                    "[dbo].[USP_Backend_ReSettle]",
                    new DbParameter[]
                    {
                        new SqlParameter("@intMatchID", SqlDbType.BigInt)
                        {
                            Value = matchID
                        },
                        new SqlParameter("@intOddsPlayType", SqlDbType.Int)
                        {
                            Value = oddsPlayType
                        },
                        new SqlParameter("@strResult", SqlDbType.VarChar)
                        {
                            Size = 10,
                            Direction = ParameterDirection.Output
                        }

                    });
                return list;
            }
            catch (Exception e)
            {
                Log.Error(e, $"{e.StackTrace} {e.Message}");
            }

            return 0;
        }
        public static List<ReplayMatchList> MatchListGet(string matchDate)
        {
            var dt = Brook.Load(DbName.DevMainDb).Table(
                160,
                CommandType.Text,
                @"select MatchID,TournamentZH,Team1ZH,Team2ZH,MatchScore,Convert(Varchar(19), MatchDate, 120)MatchDate from dbo.TB_Matches (nolock)
                    where Convert(Varchar(10), MatchDate, 120) = @MatchDate
                    Order by MatchDate asc; ",
                new DbParameter[] {
                    new SqlParameter("@MatchDate", SqlDbType.VarChar)
                    {
                        Value = matchDate
                    },
                });

            var returnData = JsonConvert.DeserializeObject<List<ReplayMatchList>>(JsonConvert.SerializeObject(dt));
            return returnData;
        }
        public static List<ReplayMatchList> MatchListEnableGet(List<long> matches)
        {


            string matchesID = "";
            foreach (var id in matches)
            {
                matchesID += id + ",";
            }
            if (matchesID.Length > 0)
                matchesID = matchesID.Substring(0, matchesID.Length - 1);

            string sqlStr = "select aaa.MatchID,aaa.TournamentZH,aaa.Team1ZH,aaa.Team2ZH,aaa.MatchScore, ";
            sqlStr += "Convert(Varchar(19), aaa.MatchDate, 120)MatchDate,Case when bbb.CNT > 1  Then '全場/半場' else '全場' End PlayType ";
            sqlStr += "from dbo.TB_Matches (nolock) aaa ";
            sqlStr += "left join ";
            sqlStr += "( ";
            sqlStr += "	select MatchID , count(0) CNT ";
            sqlStr += "	from ( ";
            sqlStr += "			select MatchID , OddsPlayType ";
            sqlStr += "			from ( ";
            sqlStr += "				select distinct a.MatchID, a.OddsID,b.OddsPlayType ";
            sqlStr += "				from dbo.TB_Odds (nolock) a ";
            sqlStr += "				join dbo.TB_OddsTypes (nolock) b ";
            sqlStr += "				on a.OddsID = b.OddsID ";
            sqlStr += "				where a.MatchID in ( " + matchesID + " ) ";
            sqlStr += "				) aa ";
            sqlStr += "				group by MatchID , OddsPlayType ";
            sqlStr += "	) bb ";
            sqlStr += "	group by MatchID ";
            sqlStr += ") bbb ";
            sqlStr += "on aaa.MatchID = bbb.MatchID ";
            sqlStr += "where aaa.MatchID in ( " + matchesID + " ) ";
            sqlStr += "Order by aaa.MatchDate asc; ";


            var dt = Brook.Load(DbName.DevMainDb).Table(
                160,
                CommandType.Text,
                sqlStr,
                new DbParameter[] {

                });

            var returnData = JsonConvert.DeserializeObject<List<ReplayMatchList>>(JsonConvert.SerializeObject(dt));
            return returnData;
        }

        public static List<ExecuteMatchListGet> ExecuteMatchListGet(long matchID)
        {
            var dt = Brook.Load(DbName.DevMainDb).Table(
                160,
                CommandType.Text,
                @"select MatchID,TournamentZH,Team1ZH,Team2ZH,MatchScore,Convert(Varchar(19), MatchDate, 120)MatchDate from dbo.TB_Matches (nolock)
                    where MatchID = @MatchID
                    Order by MatchDate asc; ",
                new DbParameter[] {
                    new SqlParameter("@MatchID", SqlDbType.BigInt)
                    {
                        Value = matchID
                    },
                });

            var returnData = JsonConvert.DeserializeObject<List<ExecuteMatchListGet>>(JsonConvert.SerializeObject(dt));
            return returnData;
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void UpdateOddsStatus(long matchId, int status)
        {
            Brook.Load(DbName.DevMainDb).Execute(
                "UPDATE [dbo].[TB_Odds] SET [Status] = @Status WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchId
                    },
                    new SqlParameter("@Status", SqlDbType.Int)
                    {
                        Value = status
                    }
                });
        }

        public static void UpdateMatchStatus(long matchid, long statusCode)
        {
            Brook.Load(DbName.DevBetradar).Execute(
                "UPDATE [dbo].[LiveScore] SET [StatusCode]= @StatusCode WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    },
                    new SqlParameter("@StatusCode", SqlDbType.Int)
                    {
                        Value = statusCode
                    }
                });
            Brook.Load(DbName.DevMainDb).Execute(
                "UPDATE [dbo].[TB_Matches] SET [StatusCode]= @StatusCode WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    },
                    new SqlParameter("@StatusCode", SqlDbType.Int)
                    {
                        Value = statusCode
                    }
                });
        }

        public static void UpdateMatchReplayStatus(long matchid, long statusCode)
        {
            Brook.Load(DbName.DevBetradar).Execute(
                "UPDATE [dbo].[LiveScore] SET [MatchDate] = @MatchDate ,[StatusCode]= @StatusCode WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    },
                    new SqlParameter("@MatchDate", SqlDbType.DateTime)
                    {
                        Value = DateTime.Now
                    },
                    new SqlParameter("@StatusCode", SqlDbType.Int)
                    {
                        Value = statusCode
                    }
                });
            Brook.Load(DbName.DevMainDb).Execute(
                "UPDATE [dbo].[TB_Matches] SET [MatchDate] = @MatchDate ,[PreMatchDate]= @MatchDate,[CurrentPeriodStart] = @MatchDate,[StatusCode]= @StatusCode,[LiveBet]=1 WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    },
                    new SqlParameter("@MatchDate", SqlDbType.DateTime)
                    {
                        Value = DateTime.Now
                    },
                    new SqlParameter("@StatusCode", SqlDbType.Int)
                    {
                        Value = statusCode
                    }
                });
        }

        public static void UpdateMatcScore(long matchId, string score)
        {
            Brook.Load(DbName.DevMainDb).Execute(
                "UPDATE [dbo].[TB_Matches] SET [MatchScore] = @MatchScore WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchId
                    },
                    new SqlParameter("@MatchScore", SqlDbType.VarChar, 32)
                    {
                        Value = score
                    }
                });
        }

        public static void UpdateMatchCurrentPeriodStart(long matchId)
        {
            Brook.Load(DbName.DevMainDb).Execute(
                "UPDATE [dbo].[TB_Matches] SET [CurrentPeriodStart] = GETDATE() WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchId
                    }
                });
        }

        public static long[] FetchLiveScout()
        {
            var t = Brook.Load(DbName.DevBetradar).Table(
                160,
                CommandType.Text,
                "SELECT [MatchId] FROM [Betradar].[dbo].[LiveScoutSoccerEvent] WITH(NOLOCK) GROUP BY [MatchID];",
                new DbParameter[] { });
            var list = new List<long>();
            foreach (DataRow row in t.Rows)
            {
                var id = long.Parse(row["MatchId"].ToString());
                list.Add(id);
            }

            return list.ToArray();
        }

        public static IEnumerable<long> FetchOdds()
        {
            var t = Brook.Load(DbName.DevMainDb).Table(
                160,
                CommandType.Text,
                "SELECT [MatchID] FROM [MainDB].[dbo].[TB_Odds] WITH(NOLOCK) WHERE MsgNr < 10000 GROUP BY [MatchID] order by [MatchID];",
                new DbParameter[] { });
            var list = new List<long>();
            foreach (DataRow row in t.Rows)
            {
                var id = long.Parse(row["MatchId"].ToString());
                list.Add(id);
            }

            return list.ToArray();
        }


        public static ScoutStruct[] FetchLiveScout(long matchid)
        {
            var startTime = Brook.Load(DbName.DevBetradar).One<DateTime>(
                "SELECT MIN([ServerTime]) FROM [Betradar].[dbo].[LiveScoutSoccerEvent] WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    }
                });
            var serverTime = startTime.AddMinutes(15);
            var list = Brook.Load(DbName.DevBetradar).Query<ScoutStruct>(
                "SELECT [MatchId],[EventId],[TypeId]AS[TypeId],[ScoutFeedType],[BetStatus],[Info],[Side],[MatchTime],[MatchScore],[ServerTime],[Player1],[Player2],[PosX],[PosY],[ExtraInfo],[CreateTime] FROM [Betradar].[dbo].[LiveScoutSoccerEvent] WHERE [MatchId] = @MatchId AND [ServerTime] >= @ServerTime order by [EventId];",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    },
                    new SqlParameter("@ServerTime", SqlDbType.DateTime)
                    {
                        Value = serverTime
                    }
                });
            return list.ToArray();
        }

        public static OddsStruct[] FetchOdds(long matchid)
        {
            var list = Brook.Load(DbName.DevMainDb).Query<OddsStruct>(
                @"SELECT [Tid],[MatchId],[MsgNr],a.[OddsID] AS [OddsId],cast(b.OddsTypeID as varchar(20)) as OddsTypeID,
                    cast(b.Subtype as varchar(20)) as Subtype,[Odds],[SpecialBetValue],[OddsID_ori] AS [OddsIdOri],[Score],[ForTheRest],[CreateTime] 
                    FROM [MainDB].[dbo].[TB_Odds] (nolock) a
                    left join [MainDB].[dbo].[TB_OddsTypes] (nolock) b 
                    on a.OddsID = b.OddsID " +
                    "WHERE [MatchId] = @MatchId AND MsgNr < 10000 order by [MsgNr],CreateTime asc;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    }
                });
            return list.ToArray();
        }
        public static DataTable FetchOddsByBetradar(long matchid)
        {
            var dt = Brook.Load(DbName.BetaBetradar).Table(160,
                CommandType.Text,
                @"select JsonData,MsgNr from Betradar.dbo.LiveOddsData (nolock) where MatchID = @MatchId and CodeEvent = 'OddsChange' and Msgnr > 0 order by MsgNr asc",
                new DbParameter[] {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    },
                });

            return dt;
        }

        public static int InsertAccount(string account)
        {
            try
            {
                var list = Brook.Load(DbName.DevMainDb).Execute(
                    CommandType.StoredProcedure,
                    "[dbo].[USP_Account_Create]",
                    new DbParameter[]
                    {
                        new SqlParameter("@strAccount", SqlDbType.VarChar)
                        {
                            Value = account
                        },
                        new SqlParameter("@intAccountType", SqlDbType.TinyInt)
                        {
                            Value = 1
                        },
                        new SqlParameter("@strPassword", SqlDbType.VarChar)
                        {
                            Value = "3c3839b6a1c387864ca752e654101a52"
                        },
                        new SqlParameter("@strResult", SqlDbType.VarChar)
                        {
                            Size = 10,
                            Direction = ParameterDirection.Output
                        }

                    });
                return list;
            }
            catch (Exception e)
            {
                Log.Error(e, $"{e.StackTrace} {e.Message}");
            }

            return 0;
        }

        public static string FetchMatchTournamentZh(long matchid)
        {
            return Brook.Load(DbName.DevMainDb).One<string>(
                "SELECT [TournamentZH] AS [TournamentZh] FROM [MainDB].[dbo].[TB_Matches] WHERE [MatchID] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    }
                });
        }

        public static void UpdateMatchReplayTitel(long matchid, string title)
        {

            Brook.Load(DbName.DevMainDb).Execute(
                "UPDATE [dbo].[TB_Matches] SET [TournamentZH] = @Title WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    },
                    new SqlParameter("@Title", SqlDbType.NVarChar)
                    {
                        Size = 64,
                        Value =title
                    }
                });
        }

        public static void SwitchOddsActive(long matchid, /*long oddsId,*/ long msgNr)
        {
            Brook.Load(DbName.DevMainDb).Execute(
                "UPDATE [dbo].[TB_Odds] SET [Status] = 1 WHERE [MatchId] = @MatchId /*AND [OddsID] = @OddsID*/ AND [MsgNr] = @MsgNr;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    },
//                    new SqlParameter("@OddsID", SqlDbType.BigInt)
//                    {
//                        Value = oddsId
//                    },
                    new SqlParameter("@MsgNr", SqlDbType.Int)
                    {
                        Value = msgNr
                    }
                });
            Brook.Load(DbName.DevMainDb).Execute(
                "UPDATE [dbo].[TB_Odds] SET [Status] = 2 WHERE [MatchId] = @MatchId /*AND [OddsID] = @OddsID*/ AND [MsgNr] < @MsgNr;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    },
//                    new SqlParameter("@OddsID", SqlDbType.Int)
//                    {
//                        Value = oddsId
//                    },
                    new SqlParameter("@MsgNr", SqlDbType.Int)
                    {
                        Value = msgNr
                    }
                });
        }

        public static void SwitchOddsActive(long matchid)
        {
            Brook.Load(DbName.DevMainDb).Execute(
                "UPDATE [dbo].[TB_Odds] SET [Status] = 2 WHERE [MatchId] = @MatchId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = matchid
                    }
                });
        }

        /// <summary>
        /// 刪除走地事件 
        /// </summary>
        public static void DeleteScoutEvent(ScoutStruct scout)
        {
            Brook.Load(DbName.DevBetradar).Execute(
                "DELETE FROM [Betradar].[dbo].[LiveScoutSoccerEvent] WHERE [MatchId] = @MatchId AND [EventId] >= @EventId;",
                new DbParameter[]
                {
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = scout.MatchId
                    },
                    new SqlParameter("@EventId", SqlDbType.BigInt)
                    {
                        Value = scout.EventId
                    }
                });
        }
        /// <summary>
        /// 寫入走地事件 
        /// </summary>
        /// <param name="scout"></param>
        public static void InsertScoutEvent(ScoutStruct scout)
        {
            Brook.Load(DbName.DevBetradar).Execute(
                "INSERT INTO [dbo].[LiveScoutSoccerEvent]([ScoutFeedType],[MatchId],[BetStatus],[EventId],[Info],[Side],[TypeId],[MatchTime],[MatchScore],[ServerTime],[Player1],[Player2],[PosX],[PosY],[ExtraInfo],[CreateTime])VALUES(@ScoutFeedType,@MatchId,@BetStatus,@EventId,@Info,@Side,@TypeId,@MatchTime,@MatchScore,@ServerTime,@Player1,@Player2,@PosX,@PosY,@ExtraInfo,@CreateTime);",
                new DbParameter[]
                {
                    new SqlParameter("@ScoutFeedType", SqlDbType.Int)
                    {
                        Value = scout.ScoutFeedType
                    },
                    new SqlParameter("@MatchId", SqlDbType.BigInt)
                    {
                        Value = scout.MatchId
                    },
                    new SqlParameter("@BetStatus", SqlDbType.Int)
                    {
                        Value = scout.BetStatus
                    },
                    new SqlParameter("@EventId", SqlDbType.BigInt)
                    {
                        Value = scout.EventId
                    },
                    new SqlParameter("@Info", SqlDbType.NVarChar, -1)
                    {
                        Value = scout.Info
                    },
                    new SqlParameter("@Side", SqlDbType.Int)
                    {
                        Value = scout.Side
                    },
                    new SqlParameter("@TypeId", SqlDbType.Int)
                    {
                        Value = scout.TypeId
                    },
                    new SqlParameter("@MatchTime", SqlDbType.VarChar, 24)
                    {
                        Value = scout.MatchTime
                    },
                    new SqlParameter("@MatchScore", SqlDbType.VarChar, 12)
                    {
                        Value = scout.MatchScore
                    },
                    new SqlParameter("@ServerTime", SqlDbType.DateTime)
                    {
                        Value = scout.ServerTime
                    },
                    new SqlParameter("@Player1", SqlDbType.BigInt)
                    {
                        Value = scout.Player1
                    },
                    new SqlParameter("@Player2", SqlDbType.BigInt)
                    {
                        Value = scout.Player2
                    },
                    new SqlParameter("@PosX", SqlDbType.Int)
                    {
                        Value = scout.PosX
                    },
                    new SqlParameter("@PosY", SqlDbType.Int)
                    {
                        Value = scout.PosY
                    },
                    new SqlParameter("@ExtraInfo", SqlDbType.BigInt)
                    {
                        Value = scout.ExtraInfo
                    },
                    new SqlParameter("@CreateTime", SqlDbType.DateTime)
                    {
                        Value = scout.CreateTime
                    }
                });
        }

        #region 賠率備份與刪除
        public static void DeleteAndBackupOdds(long matchId)
        {
            try
            {
                Brook.Load(DbName.DevMainDb).Execute(
                CommandType.StoredProcedure,
                "[dbo].[USP_Odds_Replay]",
                new DbParameter[]
                {
                        new SqlParameter("@intMatchID", SqlDbType.BigInt)
                        {
                            Value = matchId
                        },
                        new SqlParameter("@strResult", SqlDbType.VarChar)
                        {
                            Size = 10,
                            Direction = ParameterDirection.Output
                        }

                });
            }
            catch (Exception e)
            {
                Log.Error(e, $"{e.StackTrace} {e.Message}");
            }
        }
        #endregion 

    }
}