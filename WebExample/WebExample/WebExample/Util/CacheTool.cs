
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Caching;
using System;
using Newtonsoft.Json;
using NLog;

namespace WebExample.Util
{
    public class CacheTool
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static Dictionary<long, Thread> ThreadList = new Dictionary<long, Thread>();
        public static void AddThread(long matchID, Thread thd)
        {
            if (!ThreadList.ContainsKey(matchID))
            {
                ThreadList.Add(matchID, thd);
            }
        }
        public static Thread GetThread(long matchID)
        {
            if (ThreadList.ContainsKey(matchID))
            {
                return ThreadList[matchID];
            }
            else
            {
                return null;
            }
        }
        public static bool ThreadExist(long matchID)
        {
            if (ThreadList.ContainsKey(matchID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void RemoveThread(long matchID)
        {
            Log.Info($"賽事Process:{matchID} ,是否存在{ThreadList.ContainsKey(matchID)}");
            if (ThreadList.ContainsKey(matchID))
            {
                ThreadList.Remove(matchID);
                Log.Info($"賽事Process:{matchID} 移除");
            }

        }

        private static ObjectCache _cache = MemoryCache.Default;
        public static T GetOrAdd<T>(string key, Func<T> valudFunc, double expiredSec, bool forceUpdate, CacheEntryUpdateCallback action = null)
        {
            if (!_cache.Contains(key))
            {
                AddOrGetExist(key, valudFunc, expiredSec, action);
            }
            else
            {
                if (forceUpdate)
                {
                    _cache.Remove(key);
                    AddOrGetExist(key, valudFunc, expiredSec, action);
                }
            }
            return (T)_cache.Get(key);
        }
        public static T Get<T>(string key)
        {
            return (T)_cache[key];
        }
        public static T AddOrUpdate<T>(string key, T value, double expiredSec, CacheEntryUpdateCallback action = null)
        {
            if (_cache.Contains(key))
            {
                _cache[key] = value;
            }
            else
            {
                AddOrGetExist(key, value, expiredSec, action);
            }
            return (T)_cache[key];
        }
        private static T AddOrGetExist<T>(string key, T value, double expiredSec, CacheEntryUpdateCallback action = null)
        {
            return (T)_cache.AddOrGetExisting(key, value, new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expiredSec),
                UpdateCallback = action
            });
        }
        private static T AddOrGetExist<T>(string key, Func<T> valudFunc, double expiredSec, CacheEntryUpdateCallback action = null)
        {
            return (T)_cache.AddOrGetExisting(key, valudFunc(), new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expiredSec),
                UpdateCallback = action
            });
        }

        public static List<long> MatchList = new List<long>();
        public static void MatchAdd(long matchID)
        {
            if (!MatchList.Contains(matchID))
            {
                MatchList.Add(matchID);
            }
        }
       
        public static bool MatchExist(long matchID)
        {
            if (MatchList.Contains(matchID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void MatchRemove(long matchID)
        {
            Log.Info($"賽事Process:{matchID} ,是否存在{MatchList.Contains(matchID)}");
            if (MatchList.Contains(matchID))
            {
                MatchList.Remove(matchID);
                Log.Info($"賽事Process:{matchID} 移除");
            }

        }
    }
    public class CacheService
    {
        public static string SetInitMatchList(List<long> mList, bool force)
        {
            try
            {
                var forceUpdate = force;

                if (mList.Count <= 0)
                {
                    return "";
                }

                object[] outputParam = new object[] { };
                var results = CacheTool.GetOrAdd<string>("InitMatchList",
                    () =>
                    {
                        return JsonConvert.SerializeObject(mList);
                    }
                   , 43200, forceUpdate);
                if (results == null)
                {
                    results = "";
                }
                return results;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Cannot get check list: {0}", e.Message));
            }
        }
        public static string GetInitMatchList()
        {
            try
            {
                var results = CacheTool.Get<string>("InitMatchList");
                if (results == null)
                {
                    results = "";
                }
                return results;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Cannot get check list: {0}", e.Message));
            }
        }
    }
}