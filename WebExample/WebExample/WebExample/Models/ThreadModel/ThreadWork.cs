using jIAnSoft.Framework.Nami.TaskScheduler;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using WebExample.Models.Replay;
using WebExample.Util;

namespace WebExample.Models.ThreadModel
{
    public class ThreadWork
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private object locker = new Object();

        public ThreadWork(JobParam jParam)
        {
            if (CacheTool.MatchList.Count <= 5)
            {
                Log.Info($"即將重播 {jParam.MatchID} 場的賽事走地與賠率資料");
                new Match(jParam.MatchID, jParam.Time).BetRadarStart();
                CacheTool.MatchAdd(jParam.MatchID);
            }
        }
    }
}