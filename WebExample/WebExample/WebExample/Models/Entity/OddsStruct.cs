using System;

namespace WebExample.Models.Entity
{
    public class OddsStruct
    {
        /// <summary>
        /// 系統編號
        /// </summary>
        public long Tid { get; set; }
        public long OddsTid { get; set; }

        /// <summary>
        /// 賽事編號
        /// </summary>
        public long MatchId { get; set; }

        /// <summary>
        /// 何種玩法的代碼 ex 單、雙 大小 等等
        /// </summary>
        public long OddsId { get; set; }

        public long MsgNr { get; set; }

        /// <summary>
        /// 賠率
        /// </summary>
        public decimal Odds { get; set; }

        public string SpecialBetValue { get; set; }


        /// <summary>
        /// 1: 啓用 0: 封盤  2:已過時
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 賠率的種類
        /// </summary>
        public int OddsTypeId { get; set; }

        public long OddsIdOri { get; set; }
        public string Score { get; set; }

        public string ForTheRest { get; set; }
        public DateTime CreateTime { get; set; }
    }
}