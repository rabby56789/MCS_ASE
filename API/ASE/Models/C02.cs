
using JQWEB.Models;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ASE.Models
{
    /// <summary>
    /// 指派運送任務
    /// </summary>
    public class C02 : IModel
    {
        /// <summary>
        /// DB連線字串
        /// </summary>
        string connStr { get; set; }
        /// <summary>
        /// 海康API
        /// </summary>
        string hikApiUrl { get; set; }
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Boolean pass = false;
        /// <summary>
        /// 建構式,取得設定檔資料
        /// </summary>
        public C02()
        {

        }
        /// <summary>
        /// 執行方法
        /// </summary>
        /// <param name="parm">帶入參數</param>
        /// <returns></returns>
        public JObject Run(dynamic obj)
        {
            if (A04.A04isRun == true)
            {
                log.Debug($"C02 Function parameter: {obj}  A04isRun : {A04.A04isRun} ");
                throw new Exception($"A04執行中，請稍後");
            }
            log.Debug($"C02 Function parameter: {obj}");

            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            DataTable data;
            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, null);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();
            JObject parameter = new JObject();

            //取得權重
            string weightingID = ConfigurationManager.AppSettings["Weighting_ID"];
            parameter.Add("WEIGHTING_ID", weightingID);
            var sqlStr = sqlCreator.GetWeighting();
            var sqlParms = sqlCreator.CreateParameterAry(parameter);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var weighting = dao.Query().Tables[0].Rows[0]["PRIORITY"].ToString();

            //設定起點終點
            string startArea = string.Empty;
            string firstStartArea = obj["start_area"];
            //string secondStartArea = obj["second_start_area"];
            string targetArea = string.Empty;
            string firstTargetArea = obj["target_area"];
            //string secondTargetArea = obj["second_target_area"];
            string start = string.Empty;
            string target = string.Empty;
            string jobName = obj["job_name"];
            string startLoctionQrcode = string.Empty;
            string targetLocationQrcode = string.Empty;
            string taskName = string.Empty;

            //找起終點loc和qrcode(不檢查是否有任務和貨架)
            //string sqlGetStart = $"select ID,QRCODE from base_storage where GROUP_GUID = (select guid from base_area where ID = '{firstStartArea}')";
            //dao.AddExecuteItem(sqlGetStart,null);
            //var dataStart = dao.Query().Tables[0];
            //if (dataStart.Rows.Count > 0)
            //{
            //    start = dataStart.Rows[0]["ID"].ToString();
            //    startLoctionQrcode = dataStart.Rows[0]["QRCODE"].ToString();
            //}
            //else 
            //{
            //    throw new Exception($"查無出發區{firstStartArea}");
            //}
            //string sqlGetTarget = $"select ID,QRCODE from base_storage where GROUP_GUID = (select guid from base_area where ID = '{firstTargetArea}')";
            //dao.AddExecuteItem(sqlGetTarget, null);
            //var dataTarget = dao.Query().Tables[0];
            //if (dataTarget.Rows.Count > 0)
            //{
            //    target = dataStart.Rows[0]["ID"].ToString();
            //    targetLocationQrcode = dataStart.Rows[0]["QRCODE"].ToString();
            //}
            //else
            //{
            //    throw new Exception($"查無目的區{firstTargetArea}");
            //}

            //將job_name、開始區域、目的地區域組合成路徑(要確認日月光的區域組合MB2F-FE)
            if (false == string.IsNullOrEmpty(firstStartArea) && false == string.IsNullOrEmpty(firstTargetArea))
            {
                taskName = jobName + "_" + firstStartArea + "_" + firstTargetArea;
            }
            else if (false == string.IsNullOrEmpty(firstStartArea))
            {
                taskName = jobName + "_" + firstStartArea + "_" + targetArea;
            }
            else if (false == string.IsNullOrEmpty(firstTargetArea)) 
            {
                taskName = jobName + "_" + startArea + "_" + firstTargetArea;
            }
            else
            {
                taskName = jobName + "_" + startArea + "_" + targetArea;
            }
            
            
            //用起點終點決定TASK_ID
            string TASK_ID = string.Empty;            
            TASK_ID = GetTask_ID(taskName);

            //用TASK_ID查詢任務資訊
            sqlStr = sqlCreator.GetSubTaskInfo();
            parameter.Add("TASK_ID", TASK_ID);
            parameter.Add("PROGRESS", "1");
            //parameter.Add("SUB_INDEX", "1");
            sqlParms = sqlCreator.CreateParameterAry(parameter);
            dao.AddExecuteItem(sqlStr, sqlParms);
            data = dao.Query().Tables[0];
            //站點集設定或更改
            //string POSITIONCODEPATH = string.Empty;
            if (data.Rows.Count == 0)
            {
                string message = taskName + "無此任務，請先至主任務新增";
                throw new Exception(message);
            }

            

            //建立任務
            parameter.Add("GUID", uuid);
            parameter.Add("TASK_GUID", data.Rows[0]["TASK_GUID"].ToString());
            parameter.Add("WEIGHTING", weighting);
            parameter.Add("SUBTASK_TYPE", data.Rows[0]["SUBTASK_TYPE"].ToString());
            //parameter.Add("TASKTYP", data.Rows[0]["TASK_TYPE"].ToString());
            //parameter.Add("POSITIONCODEPATH", POSITIONCODEPATH);
            parameter.Add("TASKTYP","");
            parameter.Add("POSITIONCODEPATH", "");
            parameter.Add("PODCODE", "");//待確認
            parameter.Add("PODDIR", "");
            parameter.Add("MATERIALLOT", "");
            //parameter.Add("PRIORITY", obj["priority"]);//20230302Ruby加入優先權
            parameter.Add("PRIORITY", "");//20230302Ruby加入優先權
            parameter.Add("AGVCODE", obj["agvcode"]);
            parameter.Add("INSERT_USER", "API_C02");
            //ASE專用
            parameter.Add("ASE_START_LOC", "");
            parameter.Add("ASE_START_QRCODE", obj["start_qrcode"]);
            parameter.Add("ASE_TARGET_LOC", "");
            parameter.Add("ASE_TARGET_QRCODE", obj["target_qrcode"]);
            parameter.Add("ASE_JOB_NAME", obj["job_name"]);
            parameter.Add("ASE_SEQ", obj["seq"]);
            //parameter.Add("ASE_CAR_NO", obj["car_no"]);
            parameter.Add("ASE_CAR_NO","");
            parameter.Add("ASE_A01_TIME",DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            log.Debug("C02  優先級20230331" + " parameter : " + parameter);

            
            //比對base_TASK
            //
            sqlStr = sqlCreator.GetTaskName(taskName);
            var taskNameParms = sqlCreator.CreateParameterAry(null);
            dao.AddExecuteItem(sqlStr, taskNameParms);
            data = dao.Query().Tables[0];
            var taskNameCount = Int32.Parse(data.Rows[0][0].ToString());
            if (taskNameCount == 0)
            {
                throw new Exception("查無此任務，請先至'任務設定'設置 ，JOB_NAME: " + taskName);
            }
            
            sqlStr = sqlCreator.Insert();
            var sqlParm = sqlCreator.CreateParameterAry(parameter);
            dao.AddExecuteItem(sqlStr, sqlParm);
            dao.Execute();
            try
            {
                var sqlStrTravel = sqlCreator.InsertTaskTravel();
                dao.AddExecuteItem(sqlStrTravel, sqlParm);
                dao.Execute();
            }
            catch (Exception ex)
            {
                log.Error($"C02 travel GUID :{parameter["GUID"]}  " +
                    $"TASK_GUID: {parameter["TASK_GUID"]}" +
                    $"ASE_JOB_NAME :{parameter["ASE_JOB_NAME"]}" +
                    $"ASE_SEQ :{parameter["ASE_SEQ"]}" +
                    $"error_msg:{ex}");
            }
            return new JObject() { new JProperty("result", "ok") };


        }
        /// <summary>
        /// 日月光儲位轉換RCS儲位
        /// </summary>
        /// <param name="Location_ID">儲位ID</param>
        /// <returns></returns>
        public string ASEToRCS(string Location_ID)
        {
            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            JObject parm = new JObject();
            parm.Add("Location_ID", Location_ID);
            var sqlStr = sqlCreator.ASEToRCSGetLocation();
            var sqlParm = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParm);
            var data = dao.Query().Tables[0];
            if (data.Rows.Count == 0)
            {
                string message = Location_ID + "無此儲位 (RemovedFromQueue)";
                throw new Exception(message);
            }
            return data.Rows[0]["QRCODE"].ToString();
        }
        /// <summary>
        /// 日月光起點(貨架來源)儲位轉換RCS地碼
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="type">0:區域,1:LOC</param>
        /// <returns></returns>
        public JObject StartToRCS(string start,int type, string parentGuid = "")
        {
            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            JObject parm = new JObject();
            string sqlStr = string.Empty;
            if (type == 0)
            {
                parm.Add("STARTAREA", start);
                sqlStr = sqlCreator.GetStartAreaLocation(parentGuid);
            }
            else
            {
                parm.Add("START", start);
                sqlStr = sqlCreator.GetStartLocation(parentGuid);
            }
            
            var sqlParm = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParm);
            var data = dao.Query().Tables[0];
            if (data.Rows.Count == 0)
            {
                string message = start + "查無起點資訊，或已有任務，或被鎖定";
                throw new Exception(message);
            }
            JObject result = new JObject();
            result.Add("loc", data.Rows[0]["ID"].ToString());
            result.Add("qrcode", data.Rows[0]["LOCATION_ID"].ToString());
            result.Add("podcode", data.Rows[0]["TROLLEY_ID"].ToString());
            result.Add("car_no", data.Rows[0]["CAR_NO"].ToString());
            return result;
        }
        /// <summary>
        /// 日月光終點(貨架來源)儲位轉換RCS地碼
        /// </summary>
        /// <param name="target">目地位置</param>
        /// <param name="type">0:區域,1:LOC</param>
        /// <returns></returns>
        public JObject TargetToRCS(string target,int type)
        {
            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            JObject parm = new JObject();
            string sqlStr = string.Empty;
            if (type == 0)
            {
                parm.Add("TARGETAREA", target);
                sqlStr = $@"SELECT ST.ID,ST.QRCODE FROM mcs.BASE_STORAGE ST ,mcs.BASE_AREA AT";
                sqlStr += " WHERE ST.ENABLE = 1 AND AT.ENABLE = 1 ";
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT LOCATION_ID FROM mcs.T_TROLLEY_STATUS) ";
                //sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_TARGET_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN(0,1) AND ASE_TARGET_QRCODE IS NOT NULL ) ";
                //sqlStr += " AND ST.ID LIKE CONCAT(@TARGETAREA, '%') ";
                sqlStr += " AND ST.GROUP_GUID = AT.GUID ";
                sqlStr += " AND AT.ID = @TARGETAREA ";
            }
            else
            {
                parm.Add("TARGET", target);
                sqlStr = $@"SELECT ST.ID,ST.QRCODE FROM mcs.BASE_STORAGE ST ";
                sqlStr += " WHERE ST.ENABLE = 1 ";
                sqlStr += " AND ST.ID =@TARGET ";
            }
            
            if (!pass)
            {
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT LOCATION_ID FROM mcs.T_TROLLEY_STATUS ) ";
                //sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_TARGET_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN(0,1) AND ASE_TARGET_QRCODE IS NOT NULL ) ";
                
            }
            sqlStr += " ORDER BY ID LIMIT 1 ;";
            var sqlParm = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParm);
            var data = dao.Query().Tables[0];

            if (!pass && data.Rows.Count == 0)
            {
                string message = target + "查無空儲位";
                throw new Exception(message);
            }

            JObject result = new JObject();
            result.Add("loc", data.Rows[0]["ID"].ToString());
            result.Add("qrcode", data.Rows[0]["QRCODE"].ToString());
            return result;
        }
        /// <summary>
        /// 日月光終點(貨架來源)儲位轉換RCS地碼, 專給「呼叫空車」無空儲位時使用
        /// </summary>
        /// <param name="target">目地位置</param>
        /// <param name="type">0:區域,1:LOC</param>
        /// <returns></returns>
        public JObject TargetAreaToRCSWithRunningTask(string target)
        {
            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            JObject parm = new JObject();
            string sqlStr = string.Empty;
            parm.Add("TARGETAREA", target);
            sqlStr = $@"SELECT ST.ID,ST.QRCODE FROM mcs.BASE_STORAGE ST ,mcs.BASE_AREA AT";
            sqlStr += " WHERE ST.ENABLE = 1 AND AT.ENABLE = 1 ";
            sqlStr += " AND AT.ID = @TARGETAREA ";
            sqlStr += " AND ST.GROUP_GUID = AT.GUID ";           
            sqlStr += $" AND ST.QRCODE IN (SELECT ASE_START_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
            //sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_TARGET_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN(0,1) AND ASE_TARGET_QRCODE IS NOT NULL ) ";
            //sqlStr += " AND ST.ID LIKE CONCAT(@TARGETAREA, '%') ";
            

            sqlStr += " ORDER BY ID LIMIT 1 ;";
            var sqlParm = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParm);
            var data = dao.Query().Tables[0];

            if (data.Rows.Count == 0)
            {
                string message = target + "查無空儲位";
                throw new Exception(message);
            }

            JObject result = new JObject();
            result.Add("loc", data.Rows[0]["ID"].ToString());
            result.Add("qrcode", data.Rows[0]["QRCODE"].ToString());
            return result;
        }
        /// <summary>
        /// 日月光終點(貨架來源)儲位轉換RCS地碼, 專給「呼叫空車」無空儲位時使用
        /// </summary>
        /// <param name="target">目地位置</param>
        /// <param name="type">0:區域,1:LOC</param>
        /// <returns></returns>
        public JObject TargetLocToRCSWithRunningTask(string target)
        {
            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            JObject parm = new JObject();
            string sqlStr = string.Empty;
            parm.Add("TARGET", target);
            sqlStr = $@"SELECT ST.ID,ST.QRCODE FROM mcs.BASE_STORAGE ST ";
            sqlStr += " WHERE ST.ENABLE = 1 ";
            sqlStr += " AND ST.ID =@TARGET ";
            //sqlStr += $" AND ST.QRCODE NOT IN (SELECT LOCATION_ID FROM mcs.T_TROLLEY_STATUS ) ";
            sqlStr += $" AND ST.QRCODE IN (SELECT ASE_START_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
            //sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_TARGET_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN(0,1) AND ASE_TARGET_QRCODE IS NOT NULL ) ";
            sqlStr += " ORDER BY ID LIMIT 1 ;";

            var sqlParm = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParm);
            var data = dao.Query().Tables[0];

            if (!pass && data.Rows.Count == 0)
            {
                string message = target + "查無空儲位";
                throw new Exception(message);
            }

            JObject result = new JObject();
            result.Add("loc", data.Rows[0]["ID"].ToString());
            result.Add("qrcode", data.Rows[0]["QRCODE"].ToString());
            return result;
        }
        /// <summary>
        /// 站點集合起點更換
        /// </summary>
        /// <param name="POSITIONCODEPATH"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public string POSITIONCODEPATH_START(string POSITIONCODEPATH, string start)
        {
            string path = POSITIONCODEPATH.Substring(14);
            path = start + path;
            return path;
        }
        /// <summary>
        /// 站點集合終點更換
        /// </summary>
        /// <param name="POSITIONCODEPATH"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public string POSITIONCODEPATH_TARGET(string POSITIONCODEPATH, string target)
        {
            string path = POSITIONCODEPATH.Substring(0, (POSITIONCODEPATH.Length-14));
            path = path+target;
            return path;
        }
        //static string conn = ConnectionStringSettings.connectionStrings["MQTT_IP"];
        public string GetTask_ID(string start, string target, string jobName)
        {
            string task_id = string.Empty;
            
            //20220222 用job_name來撈取 task_id
            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            JObject parm = new JObject();
            var sqlStr = sqlCreator.GetTaskId(jobName);
            var sqlParm = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParm);
            DataSet ds = dao.Query();
            DataTable dt = ds.Tables[0];
            if (dt.Rows.Count == 0)
            {
                var sqlStrB2F = sqlCreator.GetTaskId("B2F平面移動");
                var sqlParmB2F = sqlCreator.CreateParameterAry(parm);
                dao.AddExecuteItem(sqlStrB2F, sqlParmB2F);
                dt = dao.Query().Tables[0];
                task_id = dt.Rows[0].ItemArray[0].ToString();
                return task_id;

            }
            task_id = dt.Rows[0].ItemArray[0].ToString();

            
            // START AlexHung說註解，使用job_name判斷，對應Base_Task_Name 20220222
            //if (start.StartsWith("M08F"))
            //{
            //    if (target.StartsWith("MB2F-FER"))
            //    {
            //        task_id = "B2FOutToB2FIn";
            //    }
            //    else if (target.StartsWith("MB2F-FE"))
            //    {
            //        task_id = "B2FOutToB2FIn";
            //    }
            //}
            //else if (start.StartsWith("MB2F-FE"))
            //{
            //    if (target.StartsWith("M08F"))
            //    {
            //        task_id = "B2FInToB2FOut";
            //    }
            //    else
            //    {
            //        task_id = "B2FToB2F";
            //    }
            //}
            // START AlexHung說註解，使用job_name判斷，對應Base_Task_Name 20220222

            //下面不知道是誰註解
            //    switch (start)
            //{
            //    case "M11F-FE-01":
            //        if (end == "MB2F-FE-01")
            //        {
            //            task_id = "B2FOutToB2FIn";
            //        }
            //        else
            //        {
            //            task_id = "B2F_Test1";
            //        }

            //        break;
            //    default:
            //        if (end != "M11F-FE-01")
            //        {
            //            task_id = "B2FOutToB2FIn";
            //        }
            //        else
            //        {
            //            task_id = "B2FToB2F";
            //        }
            //        break;
            //}
            return task_id;
        }
        public string GetTask_ID(string taskName)
        {
            string task_id = string.Empty;

            //20220222 用job_name來撈取 task_id
            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            JObject parm = new JObject();
            string sqlStr = $@"SELECT TASK_ID from mcs.base_task BT";
            sqlStr += " WHERE BT.ENABLE = 1 ";
            sqlStr += string.Format(" AND TASK_NAME = '{0}' ; ", taskName);
            //var sqlStr = sqlCreator.GetTaskId(taskName);
            var sqlParm = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParm);
            DataSet ds = dao.Query();
            DataTable dt = ds.Tables[0];
            if (dt.Rows.Count == 0)
            {
                //var sqlStrB2F = sqlCreator.GetTaskId("B2F平面移動");
                //var sqlParmB2F = sqlCreator.CreateParameterAry(parm);
                //dao.AddExecuteItem(sqlStrB2F, sqlParmB2F);
                //dt = dao.Query().Tables[0];
                //task_id = dt.Rows[0].ItemArray[0].ToString();
                //return task_id;
                throw new Exception("貨架不在適當的出發區，或查無對應" + taskName + " 路線");

            }
            task_id = dt.Rows[0].ItemArray[0].ToString();

            return task_id;
        }
        /// <summary>
        /// 執行方法
        /// </summary>
        /// <param name="parm">帶入參數</param>
        /// <returns></returns>
        //public JObject Run(dynamic obj)
        //{

        //    Task sqlCreator = new Task();
        //    DAO dao = new DAO();
        //    var sqlStruuid = sqlCreator.GetUUID();
        //    dao.AddExecuteItem(sqlStruuid, null);
        //    var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString().Replace("-", "");
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(ConfigurationManager.AppSettings["HikAPI_rest"]);
        //        genAgvSchedulingTask parameter = new genAgvSchedulingTask();
        //        parameter.reqCode = uuid;
        //        parameter.reqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //        parameter.clientCode = "";
        //        parameter.tokenCode = "";
        //        parameter.taskTyp = "F01";
        //        parameter.sceneTyp = "";
        //        parameter.ctnrTyp = "";
        //        parameter.ctnrCode = "";
        //        parameter.wbCode = "";
        //        PositionCodePath temp1 = new PositionCodePath();
        //        temp1.positionCode = obj["start_loc"];
        //        temp1.type = "00";
        //        PositionCodePath temp2 = new PositionCodePath();
        //        temp2.positionCode = obj["traget_loc"];
        //        temp2.type = "00";
        //        List<PositionCodePath> positionCodePath = new List<PositionCodePath>();
        //        positionCodePath.Add(temp1);
        //        positionCodePath.Add(temp2);
        //        parameter.positionCodePath = positionCodePath;
        //        parameter.podCode = obj["car_no"];
        //        parameter.podDir = "";
        //        parameter.podTyp = "";
        //        parameter.materialLot = "";
        //        parameter.priority = obj["priority"];
        //        parameter.agvCode = "";
        //        parameter.taskCode = "";
        //        parameter.data = "";
        //        var myContent = JsonConvert.SerializeObject(parameter);
        //        var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
        //        var byteContent = new ByteArrayContent(buffer);
        //        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //        var responseTask = client.PostAsync("genAgvSchedulingTask", byteContent);
        //        responseTask.Wait();

        //        var response = responseTask.Result;
        //        var contents = response.Content.ReadAsStringAsync();
        //        contents.Wait();

        //        var result = JObject.Parse(contents.Result) as dynamic;
        //        string message = result["message"];

        //        if (result.code == "0")
        //        {
        //            return new JObject() { new JProperty("result", "ok") };
        //        }
        //        else
        //        {
        //            throw new Exception(message);
        //        }

        //    }
        //}
        public class TaskException : Exception
        {
            public TaskException()
            {
            }

            public TaskException(string message)
                : base(message)
            {
            }

            public TaskException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
    }
}
