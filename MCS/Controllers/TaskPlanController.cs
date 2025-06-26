using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MCS.Controllers
{
    /// <summary>
    /// 生產計劃
    /// </summary>
    public class TaskPlanController : Controller
    {
        // GET: TaskPlan
        public ActionResult Index()
        {
            return View("~/Views/Task/Plan.cshtml");
        }
    }

    public class ApiTaskPlanController : ApiController, IJqOneTable
    {
        public JObject Count(JObject obj)
        {
            Transaction sqlCreator = new Transaction();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.QueryMIPlan(obj,true);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        public JObject Delete(JObject obj)
        {
            throw new NotImplementedException();
        }

        public JObject Export(JObject obj)
        {
            throw new NotImplementedException();
        }

        public JObject GetOneByGUID(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 從API匯入
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Import(JObject obj)
        {
            DAO dao = new DAO();
            LogYageo log = new LogYageo();


            JObject ApiLogData = new JObject();
            JObject sandData = new JObject();
            string responseData = "";

            try
            {
                //正式環境抓API資料
                sandData.Add("PID", "C60B1778F80A3252E050B50A9F1C1D9F");
                sandData.Add("VALUE", "");

                var myContent = JsonConvert.SerializeObject(sandData);
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //byteContent.Headers.ContentType = new MediaTypeHeaderValue("charset=UTF-8");
                byteContent.Headers.Add("TID", "C4C633CE0D10B61BE050B50A9F1C62B6");
                byteContent.Headers.Add("FACTORYID", "1040_YP");
                byteContent.Headers.Add("SERVERTYPE", "PROD");
                byteContent.Headers.Add("USERNAME", "");
                byteContent.Headers.Add("PASSWORD", "");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["YAGEO_API_URL"]);
                    var responseTask = client.PostAsync("CALLPROCESS", byteContent);
                    responseTask.Wait();

                    var response = responseTask.Result;
                    var contents = response.Content.ReadAsStringAsync();
                    contents.Wait();

                    if (contents.Result != "")
                    {
                        responseData = contents.Result;
                    }
                    else
                    {
                        return new JObject() { new JProperty("result", "faild") };
                    }
                }

                //讀實體文字檔
                string textFilePath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "App_Data");
                textFilePath += "\\MI_PLAN_JSON.txt";

                //讀取前先寫入文字檔(正式環境)
                StreamWriter sw = new StreamWriter(textFilePath);
                sw.WriteLine(responseData);
                sw.Close();

                string text = File.ReadAllText(textFilePath);
                JObject origin = JObject.Parse(text);

                bool tryGetResult = origin.TryGetValue("OUT_MSG_RESULT", out var planData);
                
                string sqlStr = string.Empty;
                if (tryGetResult)
                {
                    Transaction tran = new Transaction();
                    sqlStr = tran.AddPlan();

                    JArray ary = JArray.Parse(planData.Value<string>());

                    foreach (var item in ary)
                    {
                        JObject plan = new JObject();
                        plan.Add("INSERT_USER", "SYSTEM");
                        plan.Add("DATE_KEY", item["DATE_KEY"]);
                        plan.Add("DATA_COUNT", item["DATA_COUNT"]);
                        plan.Add("SUM_HOURS", item["SUM_HOURS"]);
                        plan.Add("STEPID", item["STEPID"]);
                        plan.Add("FOIL_TYPE", item["FOIL_TYPE"]);
                        plan.Add("REEL_NO", item["REEL_NO"]);
                        plan.Add("REEL_STATUS", item["REEL_STATUS"]);
                        plan.Add("PORITY", item["PORITY"]);
                        plan.Add("EQPID", item["EQPID"]);
                        plan.Add("RCNO", item["RCNO"]);
                        plan.Add("RCNO_STATUS", item["RCNO_STATUS"]);
                        plan.Add("PRODID", item["PRODID"]);
                        plan.Add("QTY_KPC", item["QTY_KPC"]);
                        plan.Add("PRESTEPOUTQTY", item["PRESTEPOUTQTY"]);
                        plan.Add("NEED_HOURS", item["NEED_HOURS"]);
                        plan.Add("TOTAL_M", item["TOTAL_M"]);

                        dao.AddExecuteItem(sqlStr, tran.CreateParameterAry(plan));
                    }

                    dao.Execute();
                }
                else
                {
                    throw new Exception();
                }

                Transaction sqlCreator = new Transaction();

                sqlStr = sqlCreator.GetPlan();
                dao.AddExecuteItem(sqlStr, null);
                var temp = dao.Query().Tables[0];
                if (temp.Rows.Count > 0)
                {
                    for (int i = 0; i < temp.Rows.Count; i++)
                    {
                        JObject parm = new JObject();
                        string planTime = string.Empty;
                        var GUID = temp.Rows[i]["GUID"].ToString();
                        var EQPID = temp.Rows[i]["EQPID"].ToString();
                        var FOIL_TYPE = temp.Rows[i]["FOIL_TYPE"].ToString();
                        parm.Add("GUID", GUID);
                        parm.Add("EQPID", EQPID);
                        parm.Add("FOIL_TYPE", FOIL_TYPE);
                        var sqlParms = sqlCreator.CreateParameterAry(parm);
                        sqlStr = sqlCreator.GetPlanTime();
                        dao.AddExecuteItem(sqlStr, sqlParms);
                        var planTimeLast = dao.Query().Tables[0];
                        if (planTimeLast.Rows.Count > 0)
                        {
                            var NEED_HOURS = Convert.ToDouble(planTimeLast.Rows[0]["NEED_HOURS"].ToString());
                            //var NEED_MINUTE = (Convert.ToDouble(NEED_HOURS) * 60).ToString("#0");
                            var PLAN_TIME = Convert.ToDateTime(planTimeLast.Rows[0]["PLAN_TIME"].ToString());
                            var PLAN_TIME_END = PLAN_TIME.AddHours(NEED_HOURS);
                            if (PLAN_TIME_END > DateTime.Now)
                            {
                                planTime = PLAN_TIME_END.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                planTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }
                        else
                        {
                            planTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        sqlStr = sqlCreator.UpdatePlanTime();
                        parm.Add("PLaN_TIME", planTime);
                        sqlParms = sqlCreator.CreateParameterAry(parm);
                        dao.AddExecuteItem(sqlStr, sqlParms);
                        dao.Execute();
                    }
                }
            }
            catch (Exception ex)
            {
                switch (ex.HResult)
                {
                    case -2146233088:
                        return new JObject() { new JProperty("result", "getDataFaild") };
                    default:
                        return new JObject() { new JProperty("result", "dataReadError") };
                }
            }
            finally
            {
                //Save API Log
                ApiLogData.Add("TID", ConfigurationManager.AppSettings["YAGEO_API_URL"]);
                ApiLogData.Add("FACTORY_ID", "CALLPROCESS");
                ApiLogData.Add("SERVER_TYPE", "PROD");
                ApiLogData.Add("PID", "C60B1778F80A3252E050B50A9F1C1D9F");
                ApiLogData.Add("SEND_VALUE", "");
                ApiLogData.Add("RESPONSE_VALUE", responseData);
                var parm = log.CreateParameterAry(ApiLogData);
                var sqlStr = log.GetSqlStr("INSERT", ApiLogData);
                dao.AddExecuteItem(sqlStr, parm);
                dao.Execute();
            }

            return new JObject() { new JProperty("result", "ok") };
        }

        public JObject Insert(JObject obj)
        {
            throw new NotImplementedException();
        }

        [System.Web.Http.HttpPost]
        public JObject Query(JObject obj)
        {
            Transaction sqlCreator = new Transaction();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            var sqlStr = sqlCreator.QueryMIPlan(obj,false);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);
            var data = dao.Query().Tables[0];

            //處理前段任務資訊
            foreach (DataRow dr in data.Rows)
            {
                string FEseq = dr["ASE_SEQ"].ToString();
                int FECount = 0;
                while (true)
                {
                    string sqlfe = $"select Next_Seq,Next_Job_Name from t_holding_info where Pre_Seq = '{FEseq}'";
                    dao.AddExecuteItem(sqlfe,null);
                    var dataFE = dao.Query().Tables[0];
                    if (dataFE.Rows.Count > 0)
                    {
                        FEseq = dataFE.Rows[0]["Next_Seq"].ToString();
                        FECount++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (FECount > 0 && dr["ASE_JOB_NAME"].ToString()!="暫存")//如果搜尋次數大於0，表示前段分段任務，更新最新任務資訊
                {
                    string sqlfenew = sqlCreator.QueryFEdata(FEseq);
                    dao.AddExecuteItem(sqlfenew,null);
                    var dataNewFE = dao.Query().Tables[0];
                    if (dataNewFE.Rows.Count > 0)
                    {
                        //更新前段任務資訊
                        dr["ASE_JOB_NAME"] = dr["ASE_JOB_NAME"].ToString() + "之" + FECount;
                        dr["JOB_STATUS"] = dataNewFE.Rows[0]["JOB_STATUS"].ToString();
                        dr["ASE_START_LOC"] = dataNewFE.Rows[0]["ASE_START_LOC"].ToString();
                        dr["ASE_TARGET_LOC"] = dataNewFE.Rows[0]["ASE_TARGET_LOC"].ToString();
                        dr["AGVCODE"] = dataNewFE.Rows[0]["AGVCODE"].ToString();
                        dr["ASE_CAR_NO"] = dataNewFE.Rows[0]["ASE_CAR_NO"].ToString();
                        dr["WEIGHTING"] = dataNewFE.Rows[0]["WEIGHTING"].ToString();
                        dr["STATUS"] = dataNewFE.Rows[0]["STATUS"].ToString();
                        dr["SUBTASK_NAME"] = dataNewFE.Rows[0]["SUBTASK_NAME"].ToString();
                        dr["START_TIME"] = dataNewFE.Rows[0]["START_TIME"].ToString();
                        dr["WAIT_TIME"] = dataNewFE.Rows[0]["WAIT_TIME"].ToString();
                    }

                }
   
            }

            var returnVal = extensions.ConvertDataTableToJObject(data);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }
        public JObject Update(JObject obj)
        {
            throw new NotImplementedException();
        }
        [System.Web.Http.HttpPost]
        public JObject GetParamsByFunction()
        {
            DAO dao = new DAO();
            JObject result = new JObject();
            string sqlGetRefreshTime = $"select APIMethod,Hour,Minute,Second from base_timer where APIMethod = 'TaskBoard'";
            dao.AddExecuteItem(sqlGetRefreshTime, null);
            var data = dao.Query().Tables[0];
            if (data.Rows.Count > 0)
            {
                result.Add("Hour", data.Rows[0]["Hour"].ToString());
                result.Add("Minute", data.Rows[0]["Minute"].ToString());
                result.Add("Second", data.Rows[0]["Second"].ToString());
            }
            return result;
        }

    }
}