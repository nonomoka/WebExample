using System;

namespace WebExample.Models.Entity
{
    public class ScoutStruct
    {
        public long MatchId { get; set; }
        public long EventId { get; set; }
        public int TypeId { get; set; }
        public int ScoutFeedType { get; set; }
        public int BetStatus { get; set; }
        public string Info { get; set; }
        public int Side { get; set; }
        public string MatchTime { get; set; }
        public string MatchScore { get; set; }
        public DateTime ServerTime { get; set; }
        public long Player1 { get; set; }
        public long Player2 { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public long ExtraInfo { get; set; }
        public DateTime CreateTime { get; set; }

        public int Type { get; set; }
    }
}