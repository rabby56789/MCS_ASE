using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using log4net;
using System.Text;

namespace MCS.Controllers
{

    public class AGVData
    {
        public string robotCode { get; set; }
        public string robotDir { get; set; }
        public string robotIp { get; set; }
        public string battery { get; set; }
        public string posX { get; set; }
        public string posY { get; set; }
        public string mapCode { get; set; }
        public string speed { get; set; }
        public string status { get; set; }
        public string exclType { get; set; }
        public string stop { get; set; }
        public string podCode { get; set; }
        public string podDir { get; set; }
        //public List<path> path { get; set; }
    }


    public class AGVRootObject
    {
        public string code { get; set; }
        public string reqCode { get; set; }
        public string message { get; set; }
        public List<AGVData> data { get; set; }
    }
    public class agvCallbackServiceController : ApiController
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //public static JObject AGVstatus = new JObject();

        [System.Web.Http.HttpPost]
        public JObject agvCallback(JObject callBack)
        {
            log.Info($"RCS Function parameter: {callBack}");

            JObject result = new JObject();
            SQL sql = new SQL();
            JObject parm = new JObject();

            #region//20221004 KJ  RCS warnCallback
            /*
            if (callBack.ContainsKey("method"))
            {
                string[] _allMethods = {"task_start", "task_finish", "airshower_in","airshower_inside", "airshower_out", "elevator_in","elevator_inside", "elevator", "elevator_out", "temp_bin","autodoor_in","autodoor_out",
                "start", "outbin", "cancel", "end","arrive","lockdoor_in","lockdoor_out"};

                string _method = callBack["method"].ToString();

                if (false == _allMethods.Contains(_method))
                {
                    log.Info($"Function warnCallback parameter 1 : {_method}");
                    log.Info($"Function warnCallback parameter 1 callBack  : {callBack}");

                    log.Info($"Function warnCallback parameter 1 20221024");
                    DateTime dt_now;
                    dt_now = DateTime.Now;
                    string datetime_Str = dt_now.ToString("yyyy-MM-ddHH:mm:ss");
                    string datetime_Str2 = dt_now.ToString("yyyyMMddHHmmss");
                    string reqCode_Str = "MCS_" + datetime_Str2;

                    JObject Test_callBack = new JObject();
                    //Test_callBack.Add("function", "ABCD");
                    Test_callBack.Add("reqCode", reqCode_Str);
                    Test_callBack.Add("reqTime", datetime_Str);
                    Test_callBack.Add("clientCode", "");
                    Test_callBack.Add("tokenCode", "");
                    Test_callBack.Add("mapShortName", "MB");
                    return Test_callBack;
                    //return ForwardMessage_TEST(Test_callBack, "http://192.168.56.82:8083/rcms-dps/rest/queryAgvStatus/"); //測試打API queryAgvStatus


                }
            }
            else
            {
                log.Info($"Function warnCallback parameter 2 ");
                log.Info($"Function warnCallback parameter 2 callBack  : {callBack}");
                log.Info($"Function warnCallback parameter 2 Ruby");
                DateTime dt_now;
                dt_now = DateTime.Now;
                string datetime_Str = dt_now.ToString("yyyy-MM-ddHH:mm:ss");
                string datetime_Str2 = dt_now.ToString("yyyyMMddHHmmss");
                string reqCode_Str = "MCS_" + datetime_Str2;

                JObject Test_callBack = new JObject();
                //Test_callBack.Add("function", "ABCD");
                Test_callBack.Add("reqCode", reqCode_Str);
                Test_callBack.Add("reqTime", datetime_Str);
                Test_callBack.Add("clientCode", "");
                Test_callBack.Add("tokenCode", "");
                Test_callBack.Add("mapShortName", "MB");
                return Test_callBack;
                // return ForwardMessage_TEST(Test_callBack, "http://192.168.56.82:8083/rcms-dps/rest/queryAgvStatus/"); //測試打API queryAgvStatus

            }
            //
            */

            #endregion//DataTableExtensions extensions = new DataTableExtensions();
            try
            {

                #region
                /*
                log.Info($"Function get agv status test send RCS");
                DateTime dt_now;
                dt_now = DateTime.Now;
                string datetime_Str = dt_now.ToString("yyyy-MM-ddHH:mm:ss");
                string datetime_Str2 = dt_now.ToString("yyyyMMddHHmmss");
                string reqCode_Str = "MCS_" + datetime_Str2;

                JObject Test_callBack = new JObject();
                //Test_callBack.Add("function", "ABCD");
                Test_callBack.Add("reqCode", reqCode_Str);
                Test_callBack.Add("reqTime", datetime_Str);
                Test_callBack.Add("clientCode", "");
                Test_callBack.Add("tokenCode", "");
                Test_callBack.Add("mapShortName", "MB");

                JObject _result = new JObject();
                //_result =  ForwardMessage_TEST(Test_callBack, "http://192.168.56.82:8083/rcms-dps/rest/queryAgvStatus/"); //測試打API queryAgvStatus
                _result = ForwardMessage_TEST(Test_callBack, "http://192.168.56.98/eztiot_EV01/API/select_machine_state_all?gkey=ffb3bd2319d4f6a061f38882bb504e7f"); //測試打API queryAgvStatus

                log.Info($"Function warnCallback AGVMessage_json :  {_result}");


                //string AGVMessage_json = JsonConvert.SerializeObject(_result);
                //log.Info($"Function warnCallback AGVMessage_json :  {AGVMessage_json}");


                //AGVRootObject AGV_RootObj = JsonConvert.DeserializeObject<AGVRootObject>(AGVMessage_json);
                //log.Info($"Function warnCallback  AGV_RootObj :  {AGVMessage_json}");

                */
                #endregion


                var SUBTASK_FUNCTION = callBack["method"].ToString();
                log.Info($"callBack method (SUBTASK_FUNCTION): {SUBTASK_FUNCTION}");

                parm.Add("TASKCODE", callBack["taskCode"].ToString());
                parm.Add("SUBTASK_FUNCTION", SUBTASK_FUNCTION);
                parm.Add("SUBTASK_TYPE", "1");
                parm.Add("SERVER_FUNCTION", "agvCallback");
                //B01紀錄TEMP_BIN
                parm.Add("QRCODE", callBack["mapDataCode"].ToString());
                parm.Add("podCode", callBack["podCode"].ToString());
                switch (SUBTASK_FUNCTION)
                {
                    case "task_start":
                    case "task_mid":
                    case "task_finish":                        
                        //sql.UpdateIndex(parm);
                        sql.UpdateIndexByThread(parm);
                        break;
                    case "airshower_in":
                    case "airshower_inside":
                    case "airshower_out":
                    case "lockdoor_in":
                    case "lockdoor_out":
                        //sql.UpdateIndex(parm);
                        sql.UpdateIndexByThread(parm);
                        break;                                            
                    case "elevator_in":
                    case "elevator_inside":
                    case "elevator_out":
                        //sql.UpdateIndex(parm);
                        sql.UpdateIndexByThread(parm);
                        break;                   
                    case "autodoor_in":                       
                    case "autodoor_out":
                        //sql.UpdateIndex(parm);
                        sql.UpdateIndexByThread(parm);
                        break;
                    //B01紀錄貨架位置用
                    case "temp_bin":
                        sql.B01_TEMP_BIN(parm);
                        break;
                        //default:
                        //sql.UpdateIndex(parm);
                        //break;
                }

                result.Add("code", "0");
                result.Add("message", "成功");
                result.Add("reqCode", callBack["reqCode"]);
                parm.Add("TYPE", "0");
                parm.Add("RESPONSE_DATA", JsonConvert.SerializeObject(result));
                
                //AGVstatus = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
                //AGVstatus = parm;
                log.Info($"Function result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                result.Add("code", "0");
                result.Add("message", ex.Message);
                result.Add("reqCode", callBack["reqCode"]);
                parm.Add("TYPE", "1");
                parm.Add("RESPONSE_DATA", JsonConvert.SerializeObject(result));

                log.Info($"Function result: {result}");
                return result;
            }
            finally
            {
                try
                {
                    DAO dao = new DAO();
                    SubTask sqlCreator = new SubTask();
                    //string HostUrl = $"{Request.RequestUri}{Request.Method}";
                    parm.Add("SEND_DATA", JsonConvert.SerializeObject(callBack));
                    var sqlParms = sqlCreator.CreateParameterAry(parm);
                    var sqlStr = sqlCreator.InsertRCSAPI();
                    dao.AddExecuteItem(sqlStr, sqlParms);
                    if (false == dao.Execute())
                    {
                        log.Error($@"agvCallback: 新增資料失敗: {sqlStr}");                        
                    }
                }
                catch (Exception _exception)
                {
                    log.Error(_exception);
                }
                
            }
        }
        private JObject ForwardMessage_TEST(JObject _originalMessage, string url)
        {
            JObject _result = null;
            bool isOk = false;
            log.Info("ForwardMessage_TEST  JObject = " + _originalMessage);

            string reqCodestr = _originalMessage["reqCode"].ToString();

            ThreadPool.QueueUserWorkItem(sendRcsqueryAgvStatus, _originalMessage);

            void sendRcsqueryAgvStatus(object _object)
            {
                JObject _jObject = (JObject)_object;
                try
                {

                    if (!string.IsNullOrEmpty(url))
                    {
                        string _jsonString = JsonConvert.SerializeObject(_jObject);

                        //Task<string> task = PostAsyncJson(url, _jsonString);
                        Task<string> task = PostAsyncNoParm(url);


                        log.Info("ForwardMessage_TEST Result : " + task.Result);

                        //_result = JObject.Parse(task.Result);

                        //log.Info("ForwardMessage_TEST Result Parse : " + _result);

                        //_jsonString = JsonConvert.SerializeObject(task.Result);
                        _result = JsonConvert.DeserializeObject<JObject>(task.Result);

                        log.Info("ForwardMessage_TEST Result DeserializeObject : " + _result);

                        // _result = JsonConvert.DeserializeObject<JObject>(task.Result);

                        //  System.Diagnostics.Debug.Write("ForwardMessage_TEST:" + task.Result);
                    }

                }
                catch (Exception ex)
                {
                    isOk = true;
                    log.Info("ForwardMessage_TEST ex : ", ex);
                }

                if (_result != null)
                {
                    isOk = true;
                }

            } //end send



            int t = 0;

            while (isOk == false)
            {
                log.Info("Main thread  while");
                try { Thread.Sleep(300); } catch { }
                if (_result == null)
                {
                    System.Diagnostics.Debug.Write("rcsResult is null ");
                    if (t > 20)
                    {
                        log.Info("Main thread  while t>20 ");
                        isOk = true;  //結束迴圈
                    }
                    t++;
                }
                else
                {
                    log.Info("result is NOT null ");
                    isOk = true;
                }
            }

            if (_result != null)
            {
                string tempString_ = _result.ToString();
                log.Info($"END ForwardMessage._result in thread: {tempString_} \n");
            }
            else
            {
                //_result = NULL
                _result = new JObject();
                _result.Add("code", "1");
                _result.Add("message", "TEST失敗");
                _result.Add("reqCode", reqCodestr);
                string tempString = _result.ToString();
                log.Info($"END ForwardMessage._result in thread: NULL Fail  {tempString} \n");
            }
            return _result;
        }



        //[System.Web.Http.HttpPost]
        //public JObject warnCallback(JObject _jObject)
        //{
        //    log.Info($"Function get warncallback RCS");
        //    return agvCallback(_jObject);
        //}
        [System.Web.Http.HttpPost]
        public JObject warnCallback(JObject _jObject)
        {
            log.Info($"MCS Get WarnCallback  {_jObject}");
            JObject result = new JObject();
            JObject B02TaskParm = new JObject();
            B02TaskParm = _jObject;

            if (B02TaskParm.Property("function") != null)
            {
                B02TaskParm["function"] = "B02";
                B02TaskParm["seq"] = "MCS2API_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }
            else
            {
                B02TaskParm.Add("function", "B02");
                B02TaskParm.Add("seq", "MCS2API_" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            }

            string _jsonString = JsonConvert.SerializeObject(B02TaskParm);

            log.Info($"MCS to API  B02 DATA : " + _jsonString);

            PostAsyncJson("http://192.168.56.81:81/api/Ase/CallFunc", _jsonString);

            //warncallback 一律回傳成功 //20221103
            result.Add("code", "0");
            result.Add("message", "成功");
            result.Add("reqCode", _jObject["reqCode"]);

            log.Info($"WarnCallback Function result: {result}");
            return result;
        }


        private async Task<string> PostAsyncJson(string url, string json)
        {
            HttpClient client = new HttpClient();
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        private async Task<string> PostAsyncNoParm(string url)
        {
            HttpClient client = new HttpClient();
            using (MemoryStream ms = new MemoryStream())
            {
                string content = "";
                byte[] bytes = Encoding.Unicode.GetBytes(content);
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                HttpContent NullContent = new StreamContent(ms);
                HttpResponseMessage response = await client.PostAsync(url, NullContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }


        }


    }

    public class SQL
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void UpdateIndex(JObject parm)
        {
            try
            {
                log.Debug($@"agvCallback UpdateIndex:  {parm}");
                ////////////////////用直接連DB，不要用DAO試試//////////////////////
                //SqlConnection conn = new SqlConnection(" data source =192.168.56.81,3306; initial catalog=mcs; user id = sa; password =1qaz@WSX; ");
                //SqlConnection conn = new SqlConnection("server=192.168.56.101;port=3306;database=mcs;uid=sa;pwd=1qaz@WSX;");
                //啟用連線
                //conn.Open();
                //SqlCommand cmd = new SqlCommand(sqlStr, conn);
                //SqlDataReader可以讀取資料 而ExecuteReader()執行查詢指令
                //SqlDataReader reader = cmd.ExecuteReader();
                //task_id = reader["欄位名稱"].ToString();
                //conn.Close();
                //////////////////////////////////////////
                DAO dao = new DAO();
                SubTask sqlCreator = new SubTask();
                string taskcode = parm["TASKCODE"].ToString();
                string server_function = parm["SERVER_FUNCTION"].ToString();
                string subtask_type = parm["SUBTASK_TYPE"].ToString();
                var sqlParms = sqlCreator.CreateParameterAry(parm);
                //var sqlStr = sqlCreator.GetTaskFunction();
                string sqlStr = $@"SELECT T1.GUID,T1.TASK_GUID,T1.PROGRESS,T1.POSITIONCODEPATH,T1.ASE_JOB_NAME,
                               T1.AGVCODE,T1.PODCODE,T1.PRIORITY,T1.TASKTYP,T1.MATERIALLOT,T1.TASKCODE,                               
                               T3.SUBTASK_ID,T3.SUBTASK_TYPE,T3.SERVER_FUNCTION 
                               FROM T_SUBTASK_STATUS T1,BASE_TASK_SUBTASK T2,BASE_SUBTASK T3 ";
                sqlStr += "WHERE T1.ENABLE = 1 ";
                sqlStr += "AND T1.TASK_GUID=T2.TASK_GUID ";

                sqlStr += "AND T1.PROGRESS=T2.PROGRESS ";
                sqlStr += "AND T2.SUBTASK_GUID=T3.GUID ";
                sqlStr += $"AND T1.TASKCODE= '{taskcode}' ";
                sqlStr += $"AND T3.SERVER_FUNCTION= '{server_function}' ";
                sqlStr += $"AND T3.SUBTASK_TYPE='{subtask_type}' ;";
                log.Debug($@"agvCallback sql:  {sqlStr}");

                int i = 0;
                //DataTable data;
                dao.AddExecuteItem(sqlStr, null);
                //DataSet ds = dao.Query();
                //DataTable dt = ds.Tables[0];
                var data = dao.Query().Tables[0];
                //while (data.Rows.Count == 0 && i < 60)
                //{
                //    Thread.Sleep(1000);
                //    dao.AddExecuteItem(sqlStr, sqlParms);
                //    data = dao.Query().Tables[0];
                //    i++;

                //};
               
                if (data.Rows.Count == 0)
                {
                    //log.Error($@"此任務ID流程非agvCallback: TASK_GUID = {TASK_GUID}, PROGRESS = {PROGRESS}");
                    throw new Exception("此任務ID流程非agvCallback");
                }

                int PROGRESS = Convert.ToInt32(data.Rows[0]["PROGRESS"].ToString());
                log.Debug($@"agvCallback PROGRESS:  {PROGRESS}");
                string jobname = data.Rows[0]["ASE_JOB_NAME"].ToString();
                log.Debug($@"agvCallback jobname:  {jobname}");
                string TASK_GUID = data.Rows[0]["TASK_GUID"].ToString();
                log.Debug($@"agvCallback TASK_GUID:  {TASK_GUID}");

                JObject indexParm = new JObject();
                indexParm.Add("GUID", data.Rows[0]["GUID"].ToString());
                indexParm.Add("PROGRESS", PROGRESS + 1);
                indexParm.Add("TASK_GUID", TASK_GUID);
                log.Debug($@"agvCallback GUID:  {data.Rows[0]["GUID"].ToString()}");
                //Select SUBTASK_TYPE
                sqlParms = sqlCreator.CreateParameterAry(indexParm);
                sqlStr = sqlCreator.GetSubTaskType();
                dao.AddExecuteItem(sqlStr, sqlParms);
                //DataSet ds2 = dao.Query();
                //DataTable dt2 = ds2.Tables[0];
                data = dao.Query().Tables[0];
                log.Debug($@"agvCallback data count:  {data.Rows.Count}");
                if (data.Rows.Count == 0)
                {
                    if (jobname.Contains("小車") == false)
                    {
                        //等待 UpdateTrolleyStatus 更新
                        string podCode = parm["podCode"].ToString();
                        var statusOldStr = $@"select BaseTrolley.ID as LOCATION_ID from T_TROLLEY_STATUS TrolleyStatus, base_trolley BaseTrolley  where BaseTrolley.ID = '{podCode}'And TrolleyStatus.TROLLEY_ID = BaseTrolley.ID";
                        for (int t = 0; t < 20; t++)
                        {
                            dao.AddExecuteItem(statusOldStr, sqlParms);
                            DataTable dt = dao.Query().Tables[0];
                            string location = dt.Rows[0]["LOCATION_ID"].ToString();
                            if (string.IsNullOrEmpty(location))
                            {
                                log.Debug($@"等待 UpdateTrolleyStatus 更新 ({t})");
                                try { Thread.Sleep(1000); } catch { };
                            }
                            else
                            {
                                log.Debug($@"等待 UpdateTrolleyStatus 更新, 新位置: ({location})");
                                break;
                            }
                        }
                    }
                    
                    //無下一個任務
                    sqlStr = sqlCreator.UpdateTaskMCSStatus();
                    dao.AddExecuteItem(sqlStr, sqlParms);
                    if (false == dao.Execute())
                    {
                        log.Error($@"agvCallback: 更新資料失敗: {sqlStr}");
                        throw new Exception($@"agvCallback: 更新資料失敗: {sqlStr}");
                    }
                    log.Info("agvCallback next progress: " + sqlStr);
                    return;
                }
                indexParm.Add("SUBTASK_TYPE", data.Rows[0]["SUBTASK_TYPE"].ToString());
                log.Debug($@"agvCallback SUBTASK_TYPE:  {data.Rows[0]["SUBTASK_TYPE"].ToString()}");
                //Update INDEX,SUBTASK_TYPE
                sqlParms = sqlCreator.CreateParameterAry(indexParm);
                sqlStr = sqlCreator.UpdateTaskIndex();
                dao.AddExecuteItem(sqlStr, sqlParms);
                if (false == dao.Execute())
                {
                    log.Error($@"agvCallback: 更新資料失敗: {sqlStr}");
                    throw new Exception($@"agvCallback: 更新資料失敗: {sqlStr}");
                }
                var sqlStrTravel = sqlCreator.InsertTaskTravel();
                dao.AddExecuteItem(sqlStrTravel, sqlParms);
                dao.Execute();
                log.Info("agvCallback next progress: " + indexParm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public void UpdateIndexByThread(JObject parm_)
        {
            ThreadPool.QueueUserWorkItem(_updateIndex, parm_);

            log.Info($"UpdateIndexByThread parm : {parm_}");

            void _updateIndex(object _parm)
            {
                JObject parm = (JObject)_parm;
                bool isOk = false;
                int maxTryCount = 100;
                string RCS_method = parm["SUBTASK_FUNCTION"].ToString().Trim();
                log.Info($"_updateIndex parm : {_parm}");
                for (int tryCount = 0; !isOk && tryCount < maxTryCount; tryCount++)
                {
                    log.Info($"_updateIndex parm : {_parm} tryCount :({tryCount})");
                    try
                    {
                        DAO dao = new DAO();
                        SubTask sqlCreator = new SubTask();
                        var sqlParms = sqlCreator.CreateParameterAry(parm);
                        var sqlStr = sqlCreator.GetTaskFunction();
                        int i = 0;
                        //DataTable data;
                        //log.Debug($"sqlStr_01: {sqlStr}");
                        dao.AddExecuteItem(sqlStr, sqlParms);
                        //DataSet ds = dao.Query();
                        //DataTable dt = ds.Tables[0];
                        var data = dao.Query().Tables[0];
                        log.Info($"RCS 回傳 callback : {RCS_method}");
                        if (data.Rows.Count == 0)
                        {
                            isOk = false;
                            log.Error($"RCS_method : {RCS_method}  {parm["TASKCODE"]} 此任務ID流程非agvCallback ({tryCount})");
                            try { Thread.Sleep(5000); } catch (Exception exception) { log.Error("Sleep exception", exception); }
                            continue;
                        }
                        else//檢查RCS回傳method 與任務需要的是否相同
                        {
                            string SUBTASK_FUNCTION = data.Rows[0]["SUBTASK_FUNCTION"].ToString().Trim();//任務要收到的callback
                            log.Info($"此任務需要收到的callback類型 : {SUBTASK_FUNCTION}");
                            //SUBTASK_FUNCTION如果為空，跳過檢查
                            if (RCS_method != SUBTASK_FUNCTION && !string.IsNullOrEmpty(SUBTASK_FUNCTION))
                            {
                                isOk = false;
                                log.Error($"RCS_method : {RCS_method}  {parm["TASKCODE"]} 此任務ID流程 要收到的 callback method 為 : {SUBTASK_FUNCTION}");
                                try { Thread.Sleep(5000); } catch (Exception exception) { log.Error("Sleep exception", exception); }
                                continue;
                            }
                            log.Info($"Task need callback : {SUBTASK_FUNCTION}  Rcs callback : {RCS_method}");
                        }
                        string GUID = data.Rows[0]["GUID"].ToString();
                        string jobname = data.Rows[0]["ASE_JOB_NAME"].ToString();
                        int PROGRESS = Convert.ToInt32(data.Rows[0]["PROGRESS"].ToString());

                        JObject indexParm = new JObject();
                        indexParm.Add("GUID", GUID);
                        indexParm.Add("PROGRESS", PROGRESS + 1);
                        indexParm.Add("TASK_GUID", data.Rows[0]["TASK_GUID"].ToString());

                        log.Info($@"indexParm: {indexParm}");

                        //Select SUBTASK_TYPE
                        sqlParms = sqlCreator.CreateParameterAry(indexParm);
                        sqlStr = sqlCreator.GetSubTaskType();
                        //log.Debug($"sqlStr_02: {sqlStr}");
                        dao.AddExecuteItem(sqlStr, sqlParms);
                        //DataSet ds2 = dao.Query();
                        //DataTable dt2 = ds2.Tables[0];
                        data = dao.Query().Tables[0];

                        log.Info($@"GetSubTaskType Count {data.Rows.Count}");

                        if (data.Rows.Count == 0)
                        {
                            log.Info($@"GetSubTaskType jobname : {jobname}");

                            if (jobname.Contains("小車") == false)
                            {
                                //等待JOBS  UpdateTrolleyStatus 更新     
                                string podCode = parm["podCode"].ToString();
                                var statusOldStr = string.Format($@"select LOCATION_ID from T_TROLLEY_STATUS where TROLLEY_ID = '{podCode}'");
                                for (int t = 0; t < 40; t++)
                                {
                                    dao.AddExecuteItem(statusOldStr, sqlParms);
                                    DataTable dt = dao.Query().Tables[0];
                                    string location = dt.Rows[0]["LOCATION_ID"].ToString();
                                    if (string.IsNullOrEmpty(location))
                                    {
                                        log.Warn($@"Waite UpdateTrolleyStatus Update ({t}) , Location: ({location})");
                                        try { Thread.Sleep(1000); } catch { };
                                    }
                                    else
                                    {
                                        log.Warn($@"Waite UpdateTrolleyStatus Update, New Location: ({location})");
                                        break;
                                    }
                                }

                            }

                            log.Info($@"UpdateTrolleyStatus Finish");

                            //無下一個任務                             

                            bool _isOk = false;
                            for (int t = 0; _isOk == false && t < 10; t++)
                            {
                                sqlStr = sqlCreator.UpdateTaskMCSStatus();
                                //log.Debug($"sqlStr_03: {sqlStr}");
                                dao.AddExecuteItem(sqlStr, sqlParms);
                                dao.Execute();

                                sqlStr = $"SELECT JOB_STATUS FROM mcs.t_subtask_status where GUID = '{GUID}';";
                                dao.AddExecuteItem(sqlStr, null);
                                data = dao.Query().Tables[0];
                                string jobStatus = data.Rows[0]["JOB_STATUS"].ToString();
                                if ("2" == jobStatus)
                                {
                                    _isOk = true;
                                    log.Warn("DB does be updated.");
                                }
                                else
                                {
                                    _isOk = false;
                                    log.Error("DB does not be updated.");
                                    try { Thread.Sleep(3000); } catch { }
                                }
                            }
                        }
                        else
                        {
                            indexParm.Add("SUBTASK_TYPE", data.Rows[0]["SUBTASK_TYPE"].ToString());
                            //Update INDEX,SUBTASK_TYPE
                            sqlParms = sqlCreator.CreateParameterAry(indexParm);
                            sqlStr = sqlCreator.UpdateTaskIndex();
                            //log.Debug($"sqlStr_04: {sqlStr}");
                            dao.AddExecuteItem(sqlStr, sqlParms);
                            //dao.Execute();
                            var sqlStrTravel = sqlCreator.InsertTaskTravel();
                            //log.Debug($"sqlStr_05: {sqlStr}");
                            dao.AddExecuteItem(sqlStrTravel, sqlParms);
                            dao.Execute();
                        }

                        isOk = true;
                    }
                    catch (Exception ex)
                    {
                        isOk = false;
                        log.Error("UpdateIndex() fails", ex);
                        try { Thread.Sleep(5000); } catch (Exception exception) { log.Error("Sleep exception", exception); }
                        continue;
                    }
                }
                log.Info(" agvCallback End ");
            }
        }
        public void B01_TEMP_BIN(JObject parm)
        {
            try
            {
                DAO dao = new DAO();
                SubTask sqlCreator = new SubTask();
                var sqlParms = sqlCreator.CreateParameterAry(parm);
                var sqlStr = sqlCreator.GetTempBin();
                dao.AddExecuteItem(sqlStr, sqlParms);
                var data = dao.Query().Tables[0];                 
                 
                if (data.Rows.Count == 0)
                {
                    return;
                }
                parm.Add("ASE_TEMP_BIN", data.Rows[0]["ID"].ToString());
                sqlStr= sqlCreator.UpdateB01TempBin();
                sqlParms = sqlCreator.CreateParameterAry(parm);
                dao.AddExecuteItem(sqlStr, sqlParms);
                if (false == dao.Execute())
                {
                    log.Error($@"agvCallback: 更新資料失敗: {sqlStr}");
                    throw new Exception($@"agvCallback: 更新資料失敗: {sqlStr}");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        public void ASE_B01(JObject parm)
        {
            //取得權重
            string ASEAPI = ConfigurationManager.AppSettings["ASE_API_B01"];
            if (string.IsNullOrEmpty(ASEAPI))
                return;
            DAO dao = new DAO();
            SubTask sqlCreator = new SubTask();
            var sqlParms = sqlCreator.CreateParameterAry(parm);
            var sqlStr = sqlCreator.GetB01Task();
            dao.AddExecuteItem(sqlStr, sqlParms);
            var data = dao.Query().Tables[0];
            if (data.Rows.Count == 0)
            {
                return;
            }
            JObject B01parm = new JObject();
            B01parm.Add("function", "B01");
            B01parm.Add("seq", data.Rows[0]["ASE_SEQ"].ToString());//更改為A01seq
            B01parm.Add("start_loc", data.Rows[0]["ASE_START_LOC"].ToString());//更改為A01
            B01parm.Add("task_id", data.Rows[0]["ASE_SEQ"].ToString());//更改為A01seq
            B01parm.Add("temp_bin", "");
            B01parm.Add("target_loc", data.Rows[0]["ASE_TARGET"].ToString());//更改為A01seq
            B01parm.Add("car_no", data.Rows[0]["PODCODE"].ToString());//更改為A01
            B01parm.Add("job_name", data.Rows[0]["ASE_JOB_NAME"].ToString());//更改為A01
            B01parm.Add("job_status", "完成");//發送完成
            string json = JsonConvert.SerializeObject(parm);
            string url = ASEAPI;
            Task<string> task = PostAsyncJson(url, json);
        }

        public async Task<string> PostAsyncJson(string url, string json)
        {
            HttpClient client = new HttpClient();
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        public void UpdateNextSubTask(JObject parm)
        {
            try
            {
                DAO dao = new DAO();
                SubTask sqlCreator = new SubTask();
                var sqlParms = sqlCreator.CreateParameterAry(parm);
                //查詢agvCallback方法
                var sqlStr = sqlCreator.GetTaskFunction();
                dao.AddExecuteItem(sqlStr, sqlParms);
                var data = dao.Query().Tables[0];
                if (data.Rows.Count == 0)
                {
                    throw new Exception("此任務ID流程非agvCallback");
                }
                JObject indexParm = new JObject();
                int SUB_INDEX = Convert.ToInt32(data.Rows[0]["SUB_INDEX"].ToString());
                var TASK_ID = data.Rows[0]["TASK_ID"].ToString();
                var PODCODE = data.Rows[0]["PODCODE"].ToString();
                var AGVCODE = data.Rows[0]["AGVCODE"].ToString();
                //ASE 增加資料
                var ASE_START_LOC = data.Rows[0]["ASE_START_LOC"].ToString();
                var ASE_START_QRCODE = data.Rows[0]["ASE_START_QRCODE"].ToString();
                var ASE_TARGET_LOC = data.Rows[0]["ASE_TARGET_LOC"].ToString();
                var ASE_TARGET_QRCODE = data.Rows[0]["ASE_TARGET_QRCODE"].ToString();
                var ASE_JOB_NAME = data.Rows[0]["ASE_JOB_NAME"].ToString();
                var ASE_SEQ = data.Rows[0]["ASE_SEQ"].ToString();

                indexParm.Add("SUB_INDEX", SUB_INDEX);
                indexParm.Add("TASK_ID", TASK_ID);
                //ASE_B01增加GUID
                indexParm.Add("GUID", data.Rows[0]["GUID"].ToString());
                sqlParms = sqlCreator.CreateParameterAry(indexParm);
                //查詢子任務INDEX數量並比較
                sqlStr = sqlCreator.GetSubTaskMaxIndex();
                dao.AddExecuteItem(sqlStr, sqlParms);
                data = dao.Query().Tables[0];
                if (data.Rows.Count == 0 || data.Rows[0]["MAX_INDEX"] == null)
                {
                    return;
                }
                int MAX_INDEX = Convert.ToInt32(data.Rows[0]["MAX_INDEX"].ToString());
                if (SUB_INDEX >= MAX_INDEX)
                {
                    return;
                }
                JObject taskParm = new JObject();
                taskParm.Add("TASK_ID", TASK_ID);
                taskParm.Add("SUB_INDEX", SUB_INDEX + 1);
                taskParm.Add("INDEX", "1");

                taskParm.Add("PODCODE", PODCODE);
                taskParm.Add("AGVCODE", AGVCODE);
                //ASE 增加資料
                taskParm.Add("ASE_START_LOC",ASE_START_LOC);
                taskParm.Add("ASE_START_QRCODE", ASE_START_QRCODE);
                taskParm.Add("ASE_TARGET_LOC", ASE_TARGET_LOC);
                taskParm.Add("ASE_TARGET_QRCODE", ASE_TARGET_QRCODE);
                taskParm.Add("ASE_JOB_NAME", ASE_JOB_NAME);
                taskParm.Add("ASE_SEQ", ASE_SEQ);

                sqlParms = sqlCreator.CreateParameterAry(taskParm);
                //查詢下個子任務資訊
                sqlStr = sqlCreator.GetSubTaskInfo();
                dao.AddExecuteItem(sqlStr, sqlParms);
                data = dao.Query().Tables[0];
                if (data.Rows.Count == 0)
                {
                    ASE_B01(parm);
                    return;
                }
                taskParm.Add("POSITIONCODEPATH", data.Rows[0]["POSITIONCODEPATH"].ToString());
                taskParm.Add("TASKTYP", data.Rows[0]["TASK_TYPE"].ToString());
                taskParm.Add("SUBTASK_TYPE", data.Rows[0]["SUBTASK_TYPE"].ToString());
                sqlParms = sqlCreator.CreateParameterAry(taskParm);
                //組合新任務寫入
                string sqluuid = sqlCreator.GetUUID();
                dao.AddExecuteItem(sqluuid, null);
                var uuid = dao.Query().Tables[0].Rows[0]["UUID"].ToString();
                taskParm.Add("GUID", uuid);
                sqlParms = sqlCreator.CreateParameterAry(taskParm);
                sqlStr = sqlCreator.InsertTask();
                dao.AddExecuteItem(sqlStr, sqlParms);
                dao.Execute();
                sqlStr = sqlCreator.InsertTaskTravel();
                dao.AddExecuteItem(sqlStr, sqlParms);
                if (false == dao.Execute())
                {
                    log.Error($@"agvCallback: 新增資料失敗: {sqlStr}");
                    throw new Exception($@"agvCallback: 更新資料失敗: {sqlStr}");
                }
            }
            catch
            {
                throw new Exception();
            }
        }
        public enum RCS_TYPE
        {
            INFO,
            ERROR
        }
    }
}