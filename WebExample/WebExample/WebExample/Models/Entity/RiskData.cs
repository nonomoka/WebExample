using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebExample.Models.Entity
{
    public class RiskData
    {
        /// <summary>
        /// 賽事編號
        /// </summary>
        public long MatchId { get; set; }
        /// <summary>
        /// 批次號
        /// </summary>
        public long Msgnr { get; set; }
        /// <summary>
        /// 資料來源 1:Betradar  2:BackOffice  3:RiskMan
        /// </summary>
        public int Source { get; set; }
        /// <summary>
        /// 1:早盤 2:走地
        /// </summary>
        public int OddsKind { get; set; }
        /// <summary>
        /// Betradar 的玩法
        /// </summary>
        public List<OddsTypeIdList> OddsTypeIdList { get; set; }

        public RiskData()
        {
            OddsTypeIdList = new List<OddsTypeIdList>();
        }
    }
    public class OddsTypeIdList
    {
        public long? OddsTypeId { get; set; }
        public long? SubType { get; set; }
        public List<OddsChangeList> OddsChangeList { get; set; }
        public OddsTypeIdList()
        {
            OddsChangeList = new List<OddsChangeList>();
        }
    }
    public class OddsChangeList
    {
        /// <summary>
        /// 玩法編號
        /// </summary>
        public long OddsId { get; set; }
        /// <summary>
        /// Betradar 走地賠率的ID
        /// </summary>
        public string OddsIdOri { get; set; }
        /// <summary>
        /// 賠率
        /// </summary>
        public string Odds { get; set; }
        /// <summary>
        /// 盤口值 ex:-0.5 or 0.5 須自己處理"+"號
        /// </summary>
        public string SpecialBetValue { get; set; }
        /// <summary>
        /// 以 0:0 來看這場比賽的盤口值 主隊觀點(SpecialBetValue + 主隊減客隊分差)
        /// </summary>
        public string ForTheRest { get; set; }
        /// <summary>
        /// 當下比分
        /// </summary>
        public string Score { get; set; }
        /// <summary>
        /// Betradar 賠率是否啟用
        /// 1:啟用 , 0:停用
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 特別投注選項名稱中文
        /// </summary>
        public string Optionzh { get; set; }
        /// <summary>
        /// Betradar 時間戳記
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// (RiskMan)該筆賠率最大的賠付金額
        /// </summary>
        public decimal? MaxPayOut { get; set; }
        /// <summary>
        /// (RiskMan) 前端顯示排賠率順序
        /// </summary>
        public int? OddsSort { get; set; }
        /// <summary>
        /// (RiskMan)收單狀態 0 拒收 1收單
        /// </summary>
        public int BetStatus { get; set; }


        public OddsChangeList()
        {
            this.OddsIdOri = null;
            this.ForTheRest = "";
            this.Score = "";
            this.Optionzh = "";
            this.MaxPayOut = null;
            this.OddsSort = null;
            this.BetStatus = 1;
            this.Optionzh = "";

        }
    }
    public class EarlyMatchData
    {
        public long MatchId { get; set; }
        public long Msgnr { get; set; }
        public string MatchDate { get; set; }
        public int TournamentId { get; set; }
        public string TournamentEn { get; set; }
        public string TournamentZh { get; set; }
        public long? HomeCompetitorId { get; set; }
        public string HomeCompetitorEn { get; set; }
        public string HomeCompetitorZh { get; set; }
        public long? AwayCompetitorId { get; set; }
        public string AwayCompetitorEn { get; set; }
        public string AwayCompetitorZh { get; set; }
    }
    public class LiveMatchData
    {
        public long Msgnr { get; set; }
        public long MatchId { get; set; }
        public string Score { get; set; }
        public int EventStatus { get; set; }
        public string TimeExtend { get; set; }
    }
}