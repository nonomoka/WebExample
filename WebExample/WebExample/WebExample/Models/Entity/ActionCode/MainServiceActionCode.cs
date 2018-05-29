using System;

namespace WebExample.Models.Entity.ActionCode
{
    [Serializable]
    public enum MainServiceActionCode
    {
        /// <summary>
        /// MainService 廣播新賠率
        /// </summary>
        BroadcastOdds = 4,
        /// <summary>
        /// MainService 通知 Server 中介轉傳呼叫指定網址上的 API
        /// </summary>
        IntermediaryApiService = 5
    }
}