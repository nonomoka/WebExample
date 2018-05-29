using System;

namespace WebExample.Models.Entity
{
    [Serializable]
    public enum OperationCode
    {
        /// <summary>
        /// Server 與 Client 溝通帳號相關的操作，對應 PlayerActionCode
        /// </summary>
        Player = 1,

        /// <summary>
        /// Server 與 Client 溝通消息相關的操作，對應 ChatActionCode
        /// </summary>
        Chat = 2,

        /// <summary>
        /// BookmakerFeeds 和伺服器溝通的相關操作(Client未使用)
        /// </summary>
        Betredar = 3,

        /// <summary>
        /// MainService 和伺服器溝通的相關操作(Client未使用)
        /// </summary>
        MainService = 4,

        /// <summary>
        /// Server 與 Client 之間溝通投注相關的動作，對應 BetActionCode
        /// </summary>
        Bet = 5,
        /// <summary>
        /// 
        /// </summary>
        Server = 6
    }
}