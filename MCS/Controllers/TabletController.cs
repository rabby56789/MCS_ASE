using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MCS.Controllers
{
    /// <summary>
    /// 平板功能
    /// </summary>
    public class TabletController : Controller
    {
        // GET: Tablet
        public ActionResult Index(string id)
        {
            switch (id)
            {
                case "TaskList": //任務清單
                    return View("~/Views/Tablet/TaskList.cshtml");
                case "TaskBuild": //建立任務
                    return View("~/Views/Tablet/TaskBuild.cshtml");
                case "TaskCookies": //歷史任務
                    return View("~/Views/Tablet/TaskCookies.cshtml");
                case "MaterialBind": //物料/台車綁定解綁
                    return View("~/Views/Tablet/MaterialBind.cshtml");
                case "MaterialStatusSearch": //物料狀態查詢
                    return View("~/Views/Tablet/MaterialStatusSearch.cshtml");
                case "CallTrolley": //呼叫台車
                    return View("~/Views/Tablet/CallTrolley.cshtml");
                case "UnbindTrolley": //解除台車綁定
                    return View("~/Views/Tablet/UnbindTrolley.cshtml");
                case "Testchart":
                    return View("~/Views/Tablet/Testchart.cshtml");
                default: //功能導覽首頁
                    return View("~/Views/Tablet/FrontPage.cshtml");
            }
        }
    }

    /// <summary>
    /// API 呼叫台車
    /// </summary>
    public class CallTrolleyController : ApiController
    {
        [System.Web.Http.HttpPost]
        public JObject CheckQrCode(JObject obj)
        {
            Node sqlCreator = new Node();
            DAO dao = new DAO();

            var queryNodeParm = new JObject(){
                new JProperty("QRCODE", obj.GetValue("POSITIONCODE2").ToString())
            };

            string sqlStr = sqlCreator.GetQrCode();
            var parm = sqlCreator.CreateParameterAry(queryNodeParm);

            dao.AddExecuteItem(sqlStr, parm);

            var nodeData = dao.Query().Tables[0];

            if (nodeData.Rows.Count > 0)
            {
                return new JObject() { new JProperty("result", "ok") };
            }
            else
            {
                return new JObject() { new JProperty("result", "failed") };
            }
        }

        /// <summary>
        /// 執行呼叫命令
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject Execute(JObject obj)
        {
            Transaction sqlCreator = new Transaction();
            DAO dao = new DAO();

            //1.取得空台車位置
            string sqlStr_GetOneEmptyTrolly = sqlCreator.GetOneEmptyTrolly();

            dao.AddExecuteItem(sqlStr_GetOneEmptyTrolly, null);

            var trolleyData = dao.Query().Tables[0];

            if (trolleyData.Rows.Count > 0)
            {
                string PODCODE = trolleyData.Rows[0]["TROLLEY_ID"].ToString();
                string POSITIONCODE1 = trolleyData.Rows[0]["LOCATION_ID"].ToString();

                obj.Add("PODCODE", PODCODE); //台車編號
                obj.Add("POSITIONCODE1", POSITIONCODE1); //台車點位ID
                obj.Add("PRIORITY", "120"); //優先級
                obj.Add("TASKSTATUS", "0"); //任務狀態:0未建立

                //2.將移動台車命令加入任務佇列
                string sqlStr_AddTask = sqlCreator.AddTask();
                var parm = sqlCreator.CreateParameterAry(obj);

                dao.AddExecuteItem(sqlStr_AddTask, parm);
                dao.Execute();

                return new JObject() { new JProperty("result", "OK") };
            }
            else
            {
                return new JObject() { new JProperty("result", "NoEmptyTrolly") };
            }
        }
    }

    /// <summary>
    /// 解除台車綁定
    /// </summary>
    public class UnbindTrolleyController : ApiController
    {
        public JObject CheckTrolleyId(JObject obj)
        {
            return new JObject() { new JProperty("result", "ok") };
        }

        public JObject Execute(JObject obj)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["HikAPI_rest"]);
                var myContent = JsonConvert.SerializeObject(obj);
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseTask = client.PostAsync("bindPodAndBerth", byteContent);
                responseTask.Wait();

                var response = responseTask.Result;
                var contents = response.Content.ReadAsStringAsync();
                contents.Wait();

                var result = JObject.Parse(contents.Result) as dynamic;

                if (result.code == "0")
                {
                    return new JObject() { new JProperty("result", "ok") };
                }
                else
                {
                    return new JObject() { new JProperty("result", "faild") };
                }
            }

        }
    }

    /// <summary>
    /// 綁定解綁物料與台車
    /// </summary>
    public class MaterialBindController : ApiController
    {
        public JObject QueryMaterialAndTrolley(JObject obj)
        {
            Transaction sqlCreator = new Transaction();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            var sqlStr = sqlCreator.GetCurrentTrollyStatus();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }

        public JObject Execute(JObject obj)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["HikAPI_rest"]);
                var myContent = JsonConvert.SerializeObject(obj);
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseTask = client.PostAsync("bindPodAndMat", byteContent);
                responseTask.Wait();

                var response = responseTask.Result;
                var contents = response.Content.ReadAsStringAsync();
                contents.Wait();

                var result = JObject.Parse(contents.Result) as dynamic;

                if (result.code == "0")
                {
                    return new JObject() { new JProperty("result", "ok") };
                }
                else
                {
                    return new JObject() { new JProperty("result", "faild") };
                }
            }
        }
    }

    /// <summary>
    /// 查詢物料狀態
    /// </summary>
    public class MaterialInfoController : ApiController
    {
        [System.Web.Http.HttpPost]
        public string Query(JObject obj)
        {
            Transaction sqlCreator = new Transaction();
            DAO dao = new DAO();

            var sqlStr = sqlCreator.GetMaterialLotInfo();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = dao.Query().Tables[0];

            if (result.Rows.Count > 0)
            {
                var JSONString = JsonConvert.SerializeObject(result);
                return JSONString;
            }
            else
            {
                return "no_data";
            }
        }
    }
}