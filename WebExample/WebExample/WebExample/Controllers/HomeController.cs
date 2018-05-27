using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebExample.Models.Resp;

namespace WebExample.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查詢賽式列表
        /// </summary>
        /// <param name="matchDate"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MatchListGet(string matchDate)
        {
            ResponesJson jsonResp = new ResponesJson();
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("MatchID");
                dt.Columns.Add("Team1Zh");
                dt.Columns.Add("Team2Zh");
                dt.Columns.Add("MatchDate");

                DataRow dr1 = dt.NewRow();
                dr1["MatchID"] = "13590020";
                dr1["Team1Zh"] = "美國洛杉磯";
                dr1["Team2Zh"] = "台灣台北";
                dr1["MatchDate"] = "2018-05-25 18:00:00";
                DataRow dr2 = dt.NewRow();
                dr2["MatchID"] = "13590077";
                dr2["Team1Zh"] = "南非綜合隊";
                dr2["Team2Zh"] = "阿根廷";
                dr2["MatchDate"] = "2018-05-25 18:40:00";
                DataRow dr3 = dt.NewRow();
                dr3["MatchID"] = "13590657";
                dr3["Team1Zh"] = "新疆猛獸隊";
                dr3["Team2Zh"] = "亞馬遜火龍隊";
                dr3["MatchDate"] = "2018-05-25 18:50:00";
                dt.Rows.Add(dr1);
                dt.Rows.Add(dr2);
                dt.Rows.Add(dr3);

                jsonResp.ResultData = dt;
                jsonResp.Success = true;
            }
            catch (Exception ex)
            {
                jsonResp.ResultData = ex.Message;
                jsonResp.Success = false;
            }
            return Content(JsonConvert.SerializeObject(jsonResp));
        }

    }
}