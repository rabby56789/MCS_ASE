using JQWEB.Models;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ASE.Models
{
    public class A05 : IModel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public JObject Run(dynamic obj)
        {
            if (A04.A04isRun == true)
            {
                log.Debug($"A05 Function parameter: {obj}  A04isRun : {A04.A04isRun} ");
                throw new Exception($"A04執行中，請稍後");
            }
            log.Debug($"A05 Function parameter: {obj}");

            string job_seq = obj["job_seq"].ToString();

            JObject QueueResult = CancelQueue(job_seq);//取消Queue
            if (QueueResult["code"].ToString() == "F")//Queue已建立，才處理在線任務管理的取消
            {
                JObject SubTaskResult = CancelSubTaskStatus(job_seq);
                return SubTaskResult;
            }
            else {
                return QueueResult;
            }

            #region 20230410 OLD A05
            //Task sqlCreator = new Task();
            //DAO dao = new DAO();
            //var sqlStruuid = sqlCreator.GetUUID();
            //dao.AddExecuteItem(sqlStruuid, null);
            //var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString().Replace("-", "");
            ////var sqlStr = sqlCreator.GetTASKCODE();

            ////var STATUS = string.Empty;
            ////var QueGUID = string.Empty;
            //var JOB_STATUS = string.Empty;
            //var GUID = string.Empty;
            //var ELVT_TASK_GUID = string.Empty;

            ////Queue
            ////string QueueTable = "T_TASK_QUEUE";
            ////string sqlQue = $"SELECT GUID, STATUS FROM {QueueTable} WHERE SEQ='{job_seq}';";
            ////dao.AddExecuteItem(sqlQue, null);
            ////var dataQue = dao.Query().Tables[0];

            ////Subtask
            //string SubtaskTable = "T_SUBTASK_STATUS";
            //var sqlSub = $"SELECT GUID, JOB_STATUS,ELVT_TASK_GUID FROM {SubtaskTable} WHERE ASE_SEQ='{job_seq}' AND ENABLE = 1;";
            //dao.AddExecuteItem(sqlSub, null);
            //var dataSub = dao.Query().Tables[0];

            ////if (dataQue.Rows.Count > 0)
            ////{
            ////    STATUS = dataQue.Rows[0]["STATUS"].ToString();
            ////    QueGUID = dataQue.Rows[0]["GUID"].ToString();
            ////}
            //if (dataSub.Rows.Count > 0)
            //{
            //    JOB_STATUS = dataSub.Rows[0]["JOB_STATUS"].ToString();
            //    GUID = dataSub.Rows[0]["GUID"].ToString();
            //    ELVT_TASK_GUID = dataSub.Rows[0]["ELVT_TASK_GUID"].ToString();
            //}

            ////if (dataSub.Rows.Count == 0 && dataQue.Rows.Count == 0)
            ////{
            ////    throw new Exception($"無此任務: {job_seq}");
            ////}

            //string sqlStr = string.Empty;
            //string QueueMsg = string.Empty;
            //string SubtaskMsg = string.Empty;
            //bool isCancel = false;//判斷是否取消任務，沒有取消任務才回傳exception
            ////更新Queue STATUS
            ////if (STATUS == "0")
            ////{
            ////    sqlStr = $@"UPDATE {QueueTable} SET 
            ////        STATUS=3,                    
            ////        UPDATE_USER='A05:{job_seq}', 
            ////        UPDATE_TIME=now() 
            ////        WHERE GUID = '{QueGUID}'";
            ////    dao.AddExecuteItem(sqlStr, null);
            ////    dao.Execute();

            ////    isCancel = true;

            ////    JObject _jObject = new JObject() { new JProperty("result", "ok") };
            ////    log.Debug($"A05 result: {_jObject}");

            ////}
            ////else if (STATUS == "1")
            ////{
            ////    //QueueMsg = $"任務 {job_seq} 在Queue中已建立";//已建立t_subtask_status才有資料，t_subtask_status沒取消不顯示Queue已建立               
            ////}
            ////else if (STATUS == "2")
            ////{
            ////    QueueMsg = $"任務 {job_seq} 在Queue中不可執行";
            ////}
            ////else if (STATUS == "3")
            ////{
            ////    QueueMsg = $"任務 {job_seq} 在Queue中已取消 ";
            ////}
            ////else
            ////{
            ////    if (dataQue.Rows.Count == 0) { QueueMsg = $""; }
            ////    else { QueueMsg = $"任務 {job_seq} 異常狀態值 "; }
            ////}

            ////更新Subtask
            //if (JOB_STATUS == "0" || JOB_STATUS == "1")
            //{
            //    //取消t_subtask_status
            //    sqlStr = $@"UPDATE {SubtaskTable} SET 
            //        JOB_STATUS=4,                    
            //        UPDATE_USER='A05:{job_seq}', 
            //        UPDATE_TIME=now() 
            //        WHERE ENABLE = 1 
            //        AND GUID = '{GUID}' ;";
            //    dao.AddExecuteItem(sqlStr, null);
            //    //取消booking
            //    string sqlUnbooking = $@" UPDATE t_booking_info SET 
            //        Booking_STATUS = 0,
            //        UPDATE_USER = 'A05:{job_seq}',
            //        UPDATE_TIME = now() 
            //        WHERE SUBTASK_STATUS_GUID = '{GUID}';";
            //    dao.AddExecuteItem(sqlUnbooking, null);
            //    //取消電梯任務(如果電梯任務已派) TASKSTATUS = 7 失敗
            //    if (string.IsNullOrEmpty(ELVT_TASK_GUID) == false)
            //    {
            //        string sqlCancelELVT = $@" UPDATE t_elvttask_status SET 
            //        TASKSTATUS = 7,
            //        UPDATE_USER = 'A05:{job_seq}',
            //        UPDATE_TIME = now() 
            //        WHERE GUID = '{ELVT_TASK_GUID}';";
            //        dao.AddExecuteItem(sqlCancelELVT, null);
            //    }
            //    dao.Execute();

            //    var TASKCODE = string.Empty;
            //    //取消RCS
            //    for (int i = 0; i < 5; i++) {
            //        string sqlTaskCode = $"SELECT TASKCODE FROM {SubtaskTable} WHERE ASE_SEQ='{job_seq}' AND ENABLE = 1;";
            //        dao.AddExecuteItem(sqlTaskCode, null);
            //        var datataskcode = dao.Query().Tables[0];
            //        TASKCODE = datataskcode.Rows[0]["TASKCODE"].ToString();
            //        if (string.IsNullOrEmpty(TASKCODE)) {
            //            try { Thread.Sleep(1000); } catch { }
            //        }
            //    }
            //    if (string.IsNullOrEmpty(TASKCODE) == false)
            //    {
            //        JObject parm = new JObject();
            //        parm.Add("reqCode", uuid);
            //        parm.Add("reqTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //        parm.Add("ClientCode", "");
            //        parm.Add("tokenCode", "");
            //        parm.Add("forceCancel", "0");
            //        parm.Add("matterArea", "");
            //        parm.Add("agvCode", "");
            //        parm.Add("taskCode", TASKCODE);
            //        string json = JsonConvert.SerializeObject(parm);
            //        //發RCS API
            //        JObject rcsResult = null;
            //        ThreadPool.QueueUserWorkItem(sendRcsCancelApi, json);

            //        void sendRcsCancelApi(object _parm)
            //        {
            //            string parm_ = (string)_parm;
            //            log.Info("cancelTask:" + parm_);

            //            var APIurl = ConfigurationManager.AppSettings["HikAPI_rest"] + "cancelTask"; //取消RCS任務 

            //            Task<string> task = PostAsyncJson(APIurl, parm_);
            //            string resultString = task.Result;
            //            log.Info("cancelTask:" + resultString);
            //            rcsResult = JsonConvert.DeserializeObject<JObject>(task.Result);
            //        }

            //        while (rcsResult == null)
            //        {
            //            try { Thread.Sleep(300); } catch { }
            //            if (rcsResult == null) { log.Debug("rcsResult is null"); }
            //            else { log.Debug("rcsResult is NOT null"); }
            //        }

            //        string tempString_ = rcsResult.ToString();
            //        log.Debug($"rcsResult in thread: {tempString_}");

            //        if (rcsResult["code"].ToString() == "0")
            //        {

            //        }
            //    }

            //    isCancel = true;

            //    JObject _jObject = new JObject() { new JProperty("result", "ok") };
            //    log.Debug($"A05 result: {_jObject}");
            //    //return _jObject;
            //}
            ////else if (JOB_STATUS == "1")//A05 JOB_STATUS = 1 不取消 > 改成連動RCS
            ////{

            ////    SubtaskMsg = $"任務 {job_seq} 已正在進行中。";
            ////}
            //else if (JOB_STATUS == "2")
            //{
            //    SubtaskMsg = $"任務 {job_seq} 已完成。";
            //    //throw new Exception($"任務 {_seq} 已完成");
            //}
            //else if (JOB_STATUS == "4")
            //{
            //    SubtaskMsg = $"任務 {job_seq} 早已取消。";
            //    //throw new Exception($"任務 {_seq} 早已取消");
            //}
            //else
            //{
            //    if (dataSub.Rows.Count == 0) { SubtaskMsg = $""; }
            //    else { SubtaskMsg = $"任務 {job_seq} 異常狀態值。"; }
            //    //throw new Exception($"任務 {_seq} 異常狀態值");
            //}

            ////都沒更新資料表回傳Exception
            //if (isCancel == false)
            //{
            //    throw new Exception($"{QueueMsg}  {SubtaskMsg}");
            //}

            #endregion
            JObject returnMsg = new JObject() { new JProperty("result", "ok") };
            return returnMsg;
        }

        public JObject CancelQueue(string job_seq) {
            //Queue
            JObject QueueMsg = new JObject();//訊息
            QueueMsg.Add("code", "");
            QueueMsg.Add("msg", "");
            var STATUS = string.Empty;
            var QueGUID = string.Empty;

            DAO dao = new DAO();
            string QueueTable = "T_TASK_QUEUE";
            string sqlQue = $"SELECT GUID, STATUS FROM {QueueTable} WHERE SEQ='{job_seq}';";
            dao.AddExecuteItem(sqlQue, null);
            var dataQue = dao.Query().Tables[0];
            if (dataQue.Rows.Count > 0)
            {
                STATUS = dataQue.Rows[0]["STATUS"].ToString();
                QueGUID = dataQue.Rows[0]["GUID"].ToString();
            }

            //更新Queue STATUS
            string sqlStr = string.Empty;
            if (STATUS == "0")
            {
                sqlStr = $@"UPDATE {QueueTable} SET 
                    STATUS=3,                    
                    UPDATE_USER='A05:{job_seq}', 
                    UPDATE_TIME=now() 
                    WHERE GUID = '{QueGUID}'";
                dao.AddExecuteItem(sqlStr, null);
                dao.Execute();
                JObject _jObject = new JObject() { new JProperty("result", "ok") };
                log.Debug($"A05 result: {_jObject}");
                QueueMsg["code"] = "S";
                QueueMsg["msg"] = "Queue內任務已取消";
                
            }
            else if (STATUS == "1")
            {
                QueueMsg["code"] = "F";
                QueueMsg["msg"] = "Queue內任務已建立";
                //QueueMsg = $"任務 {job_seq} 在Queue中已建立";//已建立t_subtask_status才有資料，t_subtask_status沒取消不顯示Queue已建立               
            }
            else if (STATUS == "2")
            {
                QueueMsg["code"] = "S";
                QueueMsg["msg"] = "Queue內任務不可執行";
            }
            else if (STATUS == "3")
            {
                QueueMsg["code"] = "S";
                QueueMsg["msg"] = "Queue內任務先前已取消";
            }
            else
            {
                QueueMsg["code"] = "S";
                QueueMsg["msg"] = "Queue內任務狀態異常";
            }
            return QueueMsg;
        }


        public JObject CancelSubTaskStatus(string job_seq) {

            JObject SubtaskMsg = new JObject(); //回傳Msg
            SubtaskMsg.Add("code", "");
            SubtaskMsg.Add("msg", "");
            var JOB_STATUS = string.Empty;
            var GUID = string.Empty;
            var ELVT_TASK_GUID = string.Empty;

            DAO dao = new DAO();
            Task sqlCreator = new Task();
            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, null);
            //var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString().Replace("-", "");//RCS reqcode
            var uuid = "TEST";//RCS reqcode

            //Subtask
            string SubtaskTable = "T_SUBTASK_STATUS";
            var sqlSub = $"SELECT GUID, JOB_STATUS,ELVT_TASK_GUID FROM {SubtaskTable} WHERE ASE_SEQ='{job_seq}' AND ENABLE = 1;";
            dao.AddExecuteItem(sqlSub, null);
            var dataSub = dao.Query().Tables[0];
            if (dataSub.Rows.Count > 0)
            {
                JOB_STATUS = dataSub.Rows[0]["JOB_STATUS"].ToString();
                GUID = dataSub.Rows[0]["GUID"].ToString();
                ELVT_TASK_GUID = dataSub.Rows[0]["ELVT_TASK_GUID"].ToString();
            }

            string sqlStr = string.Empty;
            //更新Subtask
            if (JOB_STATUS == "0" || JOB_STATUS == "1")
            {
                //取消t_subtask_status
                sqlStr = $@"UPDATE {SubtaskTable} SET 
                    JOB_STATUS=4,                    
                    UPDATE_USER='A05:{job_seq}', 
                    UPDATE_TIME=now() 
                    WHERE ENABLE = 1 
                    AND GUID = '{GUID}' ;";
                dao.AddExecuteItem(sqlStr, null);
                //取消booking
                string sqlUnbooking = $@" UPDATE t_booking_info SET 
                    Booking_STATUS = 0,
                    UPDATE_USER = 'A05:{job_seq}',
                    UPDATE_TIME = now() 
                    WHERE SUBTASK_STATUS_GUID = '{GUID}';";
                dao.AddExecuteItem(sqlUnbooking, null);
                //取消電梯任務(如果電梯任務已派) TASKSTATUS = 7 失敗
                if (string.IsNullOrEmpty(ELVT_TASK_GUID) == false)
                {
                    string sqlCancelELVT = $@" UPDATE t_elvttask_status SET 
                    TASKSTATUS = 7,
                    UPDATE_USER = 'A05:{job_seq}',
                    UPDATE_TIME = now() 
                    WHERE GUID = '{ELVT_TASK_GUID}';";
                    dao.AddExecuteItem(sqlCancelELVT, null);
                }
                dao.Execute();

                var TASKCODE = string.Empty;
                //取消RCS
                for (int i = 0; i < 5; i++)
                {
                    //找任務RCS TASKCODE
                    string sqlTaskCode = $"SELECT TASKCODE FROM {SubtaskTable} WHERE ASE_SEQ='{job_seq}' AND ENABLE = 1;";
                    dao.AddExecuteItem(sqlTaskCode, null);
                    var datataskcode = dao.Query().Tables[0];
                    TASKCODE = datataskcode.Rows[0]["TASKCODE"].ToString();
                    if (string.IsNullOrEmpty(TASKCODE))//如果是空值等待
                    {
                        try { Thread.Sleep(1000); } catch { }
                    }
                    else {
                        break;
                    }
                }
                if (string.IsNullOrEmpty(TASKCODE) == false)
                {
                    JObject parm = new JObject();
                    parm.Add("reqCode", uuid);
                    parm.Add("reqTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    parm.Add("ClientCode", "");
                    parm.Add("tokenCode", "");
                    parm.Add("forceCancel", "0");
                    parm.Add("matterArea", "");
                    parm.Add("agvCode", "");
                    parm.Add("taskCode", TASKCODE);
                    string json = JsonConvert.SerializeObject(parm);
                    //發RCS API
                    JObject rcsResult = null;
                    ThreadPool.QueueUserWorkItem(sendRcsCancelApi, json);

                    void sendRcsCancelApi(object _parm)
                    {
                        string parm_ = (string)_parm;
                        log.Info("cancelTask:" + parm_);

                        var APIurl = ConfigurationManager.AppSettings["HikAPI_rest"] + "cancelTask"; //取消RCS任務 

                        Task<string> task = PostAsyncJson(APIurl, parm_);
                        string resultString = task.Result;
                        log.Info("cancelTask:" + resultString);
                        rcsResult = JsonConvert.DeserializeObject<JObject>(task.Result);
                    }

                    while (rcsResult == null)
                    {
                        try { Thread.Sleep(300); } catch { }
                        if (rcsResult == null) { log.Debug("rcsResult is null"); }
                        else { log.Debug("rcsResult is NOT null"); }
                    }

                    string tempString_ = rcsResult.ToString();
                    log.Debug($"rcsResult in thread: {tempString_}");

                    if (rcsResult["code"].ToString() == "0")
                    {
                        SubtaskMsg["code"] = "S";
                        SubtaskMsg["msg"] = "MCS任務以及RCS任務已取消";
                    }
                }
                else {
                    SubtaskMsg["code"] = "S";
                    SubtaskMsg["msg"] = "MCS任務已取消，RCS任務取消狀況尚未確認";
                }

                JObject _jObject = new JObject() { new JProperty("result", "ok") };
                log.Debug($"A05 result: {_jObject}");
                //return _jObject;
            }
            else if (JOB_STATUS == "2")
            {
                SubtaskMsg["code"] = "F";
                SubtaskMsg["msg"] = $"任務 {job_seq} 已完成";
                //throw new Exception($"任務 {_seq} 已完成");
            }
            else if (JOB_STATUS == "4")
            {
                SubtaskMsg["code"] = "F";
                SubtaskMsg["msg"] = $"任務 {job_seq} 先前已取消";
                //throw new Exception($"任務 {_seq} 早已取消");
            }
            else
            {
                SubtaskMsg["code"] = "F";
                SubtaskMsg["msg"] = $"任務 {job_seq} 異常狀態值";
                //throw new Exception($"任務 {_seq} 異常狀態值");
            }
            return SubtaskMsg;
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
        //public JObject Run(dynamic obj)
        //{
        //    log.Debug($"A05 Function parameter: {obj}");

        //    Task sqlCreator = new Task();
        //    DAO dao = new DAO();
        //    var sqlStruuid = sqlCreator.GetUUID();
        //    dao.AddExecuteItem(sqlStruuid, null);
        //    var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString().Replace("-", "");
        //    var sqlStr= sqlCreator.GetTASKCODE();
        //    JObject parameter = new JObject();
        //    parameter.Add("TASKCODE", obj["seq"]);
        //    var sqlParms = sqlCreator.CreateParameterAry(parameter);
        //    var data = dao.Query().Tables[0];
        //    if (data.Rows.Count == 0)
        //    {
        //        throw new Exception("無此任務seq");
        //    }
        //    var TASKCODE = data.Rows[0]["TASKCODE"].ToString();
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(ConfigurationManager.AppSettings["HikAPI_rest"]);
        //        JObject parm = new JObject();
        //        parm.Add("reqCode", uuid);
        //        parm.Add("reqTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //        parm.Add("ClientCode", "");
        //        parm.Add("tokenCode", "");
        //        parm.Add("forceCancel", "0");
        //        parm.Add("matterArea", "");
        //        parm.Add("agvCode", "");
        //        parm.Add("taskCode", TASKCODE);
        //        var myContent = JsonConvert.SerializeObject(parm);
        //        var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
        //        var byteContent = new ByteArrayContent(buffer);
        //        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //        var responseTask = client.PostAsync("cancelTask", byteContent);
        //        responseTask.Wait();

        //        var response = responseTask.Result;
        //        var contents = response.Content.ReadAsStringAsync();
        //        contents.Wait();

        //        var result = JObject.Parse(contents.Result) as dynamic;
        //        string message = result["message"];
        //        if (result.code == "0")
        //        {
        //            JObject _jObject = new JObject() { new JProperty("result", "ok") };
        //            log.Debug($"A05 result: {_jObject}");
        //            return _jObject;
        //        }
        //        else
        //        {
        //            throw new Exception(message);
        //        }
        //    }
        //}
    }
    //class TaskException : Exception, ISerializable
    //{
    //    public TaskException()
    //        : base("show message") { }
    //    public TaskException(string message)
    //        : base(message) { }
    //    public TaskException(string message, Exception inner)
    //        : base(message, inner) { }
    //    protected TaskException(SerializationInfo info, StreamingContext context)
    //        : base(info, context) { }
    //}
}
