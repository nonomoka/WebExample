using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebExample.Models.Replay
{
    public class JobParam
    {
        public long MatchID { get; set; }
        public int Time  { get; set; }
    }
    public class ReplayMatch
    {

    }
    public class ReplayMatchList
    {
        public long MatchID { get; set; }
        public string TournamentZH { get; set; }
        public string Team1ZH { get; set; }
        public string Team2ZH { get; set; }
        public string MatchScore { get; set; }
        public string PlayType { get; set; }
        public string MatchDate { get; set; }
    }
    public class ExecuteMatchListGet
    {
        public long MatchID { get; set; }
        public string TournamentZH { get; set; }
        public string Team1ZH { get; set; }
        public string Team2ZH { get; set; }
    }
    
}