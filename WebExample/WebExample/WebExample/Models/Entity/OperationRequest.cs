using System.Collections.Generic;

namespace WebExample.Models.Entity
{
    public class OperationRequest
    {
        public int OperationCode { get; set; }
        public int ActionCode { get; set; }
        public string Token { get; set; }
        /// <summary>
        /// Key: 1 ~ 100 Client 傳送API所需要的參數
        /// Key: 101 ~ 200 Server 回應本次操作後所回傳的參數
        /// Key: 201 ~ 250 Client 夾帶所需的資料到伺服器，伺服器會再送回 Client
        /// Key: 251 ~ 255 保留未使用
        /// </summary>
        public Dictionary<byte, object> Parameters { get; set; }
    }
}