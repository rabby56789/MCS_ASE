
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
    public class C03 : IModel
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
        public C03()
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
                log.Debug($"C03 Function parameter: {obj}  A04isRun : {A04.A04isRun} ");
                throw new Exception($"A04執行中，請稍後");
            }
            log.Debug($"!!!!!C03 Function parameter: {obj} !!!!!");

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
            //取得PASS目地的JobName
            string PassJobName = ConfigurationManager.AppSettings["PASS_JobName"];

            //找出該貨架正在執行的任務，並取該任務的target_loc為起點
            string Location_ID = string.Empty;
            string storage = string.Empty;
            string car_no = obj["car_no"];
            string podcode = string.Empty;
            if (string.IsNullOrEmpty(car_no) == false)
            {
                string sqlTarget = $"select * from t_subtask_status where job_status in (0,1) and ASE_CAR_NO = '{car_no}'";
                dao.AddExecuteItem(sqlTarget,null);
                var dataTarget = dao.Query().Tables[0];
                if (dataTarget.Rows.Count > 0)
                {
                    Location_ID = dataTarget.Rows[0]["ASE_TARGET_QRCODE"].ToString();
                    storage = dataTarget.Rows[0]["ASE_TARGET_LOC"].ToString();
                    podcode = dataTarget.Rows[0]["PODCODE"].ToString();
                }
                else
                {
                    //若該貨架原任務在派發(下一個任務)C03前結束，則找該貨架目前位置
                    var sqlType = sqlCreator.GetLocation();
                    var LocalParms = sqlCreator.CreateParameterAry(obj);
                    dao.AddExecuteItem(sqlType, LocalParms);
                    data = dao.Query().Tables[0];
                    if (data.Rows.Count != 0)
                    {
                        Location_ID = data.Rows[0]["LOCATION_ID"].ToString();
                        storage = data.Rows[0]["ID"].ToString();
                        podcode = data.Rows[0]["TROLLEY_ID"].ToString();
                    }
                    //throw new Exception($"貨架號碼{car_no}，已無執行中任務，查無起點位置");
                }
                
                log.Debug("C03  " + "Location_ID : " + Location_ID);
                log.Debug("C03  " + "storage : " + storage);
                log.Debug("C03  " + "podcode : " + podcode);
            }            

            //設定或找尋起點終點
            string startArea = string.Empty;
            string firstStartArea = obj["start_area"];
            string secondStartArea = obj["second_start_area"];
            string targetArea = string.Empty;
            string firstTargetArea = obj["target_area"];
            string secondTargetArea = obj["second_target_area"];
            string start = string.Empty;
            string target = string.Empty;
            string jobName = obj["job_name"];
            string startLoctionQrcode = Location_ID;
            string targetLocationQrcode = string.Empty;
            string taskName = string.Empty;
            
            if (false == string.IsNullOrEmpty(secondStartArea))
            {
                startArea = secondStartArea;
            }
            else if (firstStartArea == "StartArea")
            {
                startArea = string.Empty;
            }
            else
            {
                startArea = firstStartArea;
            }

            if (false == string.IsNullOrEmpty(secondTargetArea))
            {
                targetArea = secondTargetArea;
            }
            else
            {
                targetArea = firstTargetArea;
            }

            start = obj["start_loc"];            
            target = obj["target_loc"];

            if (string.IsNullOrEmpty(car_no) && string.IsNullOrEmpty(start) && string.IsNullOrEmpty(firstStartArea) && string.IsNullOrEmpty(secondStartArea))
            {
                throw new Exception("無起點位置資訊 (RemovedFromQueue) C03");
            }

            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(storage) && string.IsNullOrEmpty(firstStartArea) && string.IsNullOrEmpty(secondStartArea))
            {
                throw new Exception("無起點位置資訊_C03");
            }
            else if (string.IsNullOrEmpty(start) && false == string.IsNullOrEmpty(storage))
            {
                start = storage;
            }
            else if (false == string.IsNullOrEmpty(start) && true == string.IsNullOrEmpty(storage))
            {
                //同初始經設定 start = obj["start_loc"]; 
            }
            else if (false == string.IsNullOrEmpty(start) && false == string.IsNullOrEmpty(storage))
            {
                if (start != storage)
                {
                    throw new Exception("貨架所在位置與start_loc不相符_C03");
                }
            }
            //判斷任務是否為目的型(20220824 Ruby 特殊鎖定任務)
            string ismixedarea = obj["ismixedarea"];
            // 檢查出發儲位是否被鎖定(20220823 Ruby 特殊鎖定任務)
            string _guid = string.Empty;
            string allow_job = string.Empty;
            bool isEuualJob = false;
            if (false == string.IsNullOrEmpty(start))
            {
                string location = start;
                string _sqlString = $@"select GUID,GROUP_GUID from mcs.base_storage where ID = '{location}' || NAME = '{location}' || QRCODE = '{location}'";
                dao.AddExecuteItem(_sqlString, null);
                data = dao.Query().Tables[0];
                if (data.Rows.Count == 0)
                {
                    throw new Exception($@"查無儲位 {location} (RemovedFromQueue) C03");
                }

                string _storageGuid = data.Rows[0]["GUID"].ToString();
                _sqlString = $@"select GUID,JOB_NAME from mcs.t_storage_status where STORAGE_GUID = '{_storageGuid}'";
                dao.AddExecuteItem(_sqlString, null);
                data = dao.Query().Tables[0];
                if (data.Rows.Count != 0)
                {
                    _guid = data.Rows[0]["GUID"].ToString();
                    allow_job = data.Rows[0]["JOB_NAME"].ToString();
                    if (allow_job.Equals(jobName))
                    {
                        //出發儲位和可執行任務符合，儲位解鎖
                        //var sqlUnlock = $"delete from t_storage_status where GUID = '{_guid}';";
                        //dao.AddExecuteItem(sqlUnlock,null);
                        //dao.Execute();
                        isEuualJob = true;
                        log.Debug($"C01 檢查出發點有鎖定且JOBNAME有相同 C03 : {_guid}");
                    }
                    else 
                    {
                        log.Debug($"C01 檢查出發點有鎖定但JOBNAME不相同 C03: {_guid}");
                        throw new Exception($@"出發儲位 {start} 已被鎖定 C03");
                        
                    }

                }
            }

            #region 檢查 start_loc 是否屬於 start_area
            if (false == string.IsNullOrEmpty(start))
            {
                var sqlArea = sqlCreator.GetStartArea(start);
                var AreaParms = sqlCreator.CreateParameterAry(null);
                dao.AddExecuteItem(sqlArea, AreaParms);
                data = dao.Query().Tables[0];
                if (data.Rows.Count == 0)
                {
                    throw new Exception(@"查無此儲位 start_loc 所在儲位: " + start + " (RemovedFromQueue)_C03");
                }
                string startAreaFromStart = data.Rows[0][0].ToString();
                if (string.IsNullOrEmpty(startArea))
                {
                    startArea = startAreaFromStart;
                }
                else
                {
                    if (startArea != startAreaFromStart)
                    {
                        throw new Exception($@"出發儲位 {start} 不屬於出發儲區 {startArea} C03");
                    }
                }
            }
            #endregion

            //檢查並找出 target_loc 和 target_area
            if (string.IsNullOrEmpty(target))
            {
                if (string.IsNullOrEmpty(targetArea))
                {
                    throw new Exception("沒有輸入目標位置資訊 (RemovedFromQueue) C03");
                }
                else
                {
                    // 沒有 target_loc 但有 target_area
                }
            }
            else
            {
                string sqlArea = sqlCreator.GetTargetArea(target);
                var AreaParms = sqlCreator.CreateParameterAry(null);
                dao.AddExecuteItem(sqlArea, AreaParms);
                data = dao.Query().Tables[0];
                if (data.Rows.Count == 0)
                {
                    throw new Exception(@"查無此儲位 ""target_loc"" 所在儲位: " + target + " (RemovedFromQueue) C03");
                }
                string targetAreaFromTarget = data.Rows[0][0].ToString();
                if (string.IsNullOrEmpty(targetArea))
                {
                    targetArea = targetAreaFromTarget;
                }
                else
                {
                    if (targetArea != targetAreaFromTarget)
                    {
                        throw new Exception($@"目標儲位 {target} 不屬於目標儲區 {targetArea} (RemovedFromQueue) C03");
                    }
                }
            }
            

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
            
            //檢查JobName與設定的PassJobName是否相同後續用作目地檢查條件
            if (PassJobName == taskName)
            {
                pass = true;
            }

            //找出 start location qurcode 
            JObject startObj = new JObject();
            if (string.IsNullOrEmpty(startLoctionQrcode))
            {
                if (false == string.IsNullOrEmpty(start))
                {
                    //透過 start 找 qrcode
                    startObj = StartToRCS(start, 1);
                    startLoctionQrcode = startObj["qrcode"].ToString();
                    car_no = startObj["car_no"].ToString();
                    podcode = startObj["podcode"].ToString();
                }
                else
                {
                    //透過 startAra 找 qrcode
                    string parentGuid = obj["parent_guid"];
                    startObj = StartToRCS(startArea, 0, parentGuid);
                    startLoctionQrcode = startObj["qrcode"].ToString();
                    start = startObj["loc"].ToString();
                    car_no = startObj["car_no"].ToString();
                    podcode = startObj["podcode"].ToString();
                }                
            }
            log.Debug("C03  " + " startLoctionQrcode : " + startLoctionQrcode);

            // 找出 target location qrcode
            JObject targetObj = new JObject();
            if (false == string.IsNullOrEmpty(target))
            {
                // 透過 target 找 qrcode
                try
                {
                    targetObj = TargetToRCS(target, 1);
                    targetLocationQrcode = targetObj["qrcode"].ToString();
                }
                catch (Exception exception)
                {
                    throw exception;                    
                }
            }
            else
            {
                // 透過 targetArea 找 qrcode
                try
                {
                    targetObj = TargetToRCS(targetArea, 0);
                    targetLocationQrcode = targetObj["qrcode"].ToString();
                    target = targetObj["loc"].ToString();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }                        
            log.Debug("C03  " + " targetLocationQrcode : " + targetLocationQrcode);


            log.Debug($"混和任務類型: {ismixedarea}");
            
            

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
                string message = taskName + "無此任務，請先至主任務新增 (RemovedFromQueue) C03";
                throw new Exception(message);
            }

            //如果該任務為目的型，結束點要上鎖，job_name同名(20220824 Ruby)
            if (ismixedarea == "1" || ismixedarea == "3")
            {
                var sqlGetTargetInfo = $"select guid ,group_guid from base_storage where qrcode = '{targetLocationQrcode}'";
                dao.AddExecuteItem(sqlGetTargetInfo, null);
                var dataRow = dao.Query().Tables[0].Rows[0];
                string _storageGuid = dataRow["guid"].ToString();
                string _groupGuid = dataRow["group_guid"].ToString();
                //已鎖定的儲位不重複鎖定
                string sqlcheckout = $"select * from t_storage_status where guid = (select guid from base_storage where qrcode = '{targetLocationQrcode}')";
                dao.AddExecuteItem(sqlcheckout, null);
                string nolock = obj["nolock"];
                if (dao.Query().Tables[0].Rows.Count == 0 && (nolock != "退空車") )
                {
                    
                    var sqlLock = $@"INSERT INTO t_storage_status 
                    (GUID, STORAGE_GUID, AREA_GUID, JOB_NAME, IS_LOCK, INSERT_USER, UPDATE_USER, ENABLE) 
                    VALUES ((SELECT uuid()), '{_storageGuid}', '{_groupGuid}','{nolock}', '1', 'C01', 'C01', '1');";
                    try
                    {
                        dao.AddExecuteItem(sqlLock, null);
                        dao.Execute();
                        log.Debug($"C01  nolock:   {nolock} "  );
                    }
                    catch (Exception exception)
                    { throw exception; }
                    
                    
                    //JObject _jObject = new JObject() { new JProperty("result", "ok") };
                    log.Debug($"C01 儲位特定任務搬運目的型鎖定(寫入t_storage_status JOB_NAME): {sqlLock}");
                }
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
            parameter.Add("PODCODE", podcode);//待確認
            parameter.Add("PODDIR", "");
            parameter.Add("MATERIALLOT", "");
            parameter.Add("PRIORITY", obj["priority"]);//20230302Ruby加入優先權
            parameter.Add("AGVCODE", "");
            parameter.Add("INSERT_USER", "API_C03");
            //ASE專用
            parameter.Add("ASE_START_LOC",start);
            parameter.Add("ASE_START_QRCODE", startLoctionQrcode);
            parameter.Add("ASE_TARGET_LOC", target);
            parameter.Add("ASE_TARGET_QRCODE", targetLocationQrcode);
            parameter.Add("ASE_JOB_NAME", obj["job_name"]);
            parameter.Add("ASE_SEQ", obj["seq"]);
            parameter.Add("ASE_CAR_NO", car_no);
            parameter.Add("ASE_A01_TIME", obj["a01_time"]);
            log.Debug("C03  " + " parameter : " + parameter);

            
            //比對base_TASK
            //
            sqlStr = sqlCreator.GetTaskName(taskName);
            var taskNameParms = sqlCreator.CreateParameterAry(null);
            dao.AddExecuteItem(sqlStr, taskNameParms);
            data = dao.Query().Tables[0];
            var taskNameCount = Int32.Parse(data.Rows[0][0].ToString());
            if (taskNameCount == 0)
            {
                throw new Exception("查無此任務，請先至【任務設定】設置 ，JOB_NAME: " + taskName + " (RemovedFromQueue) C03");
            }
            //最後如果符合特殊搬運，可以建立t_subtask_status才清
            if (isEuualJob)
            {
                var sqlUnlock = $"delete from t_storage_status where GUID = '{_guid}'";
                try
                {
                    dao.AddExecuteItem(sqlUnlock, null);
                    dao.Execute();
                    log.Debug($"C03  清t_storage_status   {_guid}   jobnmae {allow_job}");
                }
                catch (Exception ex)
                {
                    log.Debug($"C03  清t_storage_status   {ex} ");
                    throw ex;                    
                }
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
                log.Error($"C03 travel GUID :{parameter["GUID"]}  " +
                    $"TASK_GUID: {parameter["TASK_GUID"]}" +
                    $"ASE_JOB_NAME :{parameter["ASE_JOB_NAME"]}" +
                    $"ASE_SEQ :{parameter["ASE_SEQ"]}" +
                    $"error_msg:{ex}");
            }
            return new JObject() { new JProperty("result", "ok") };


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
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_TARGET_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN(0,1) AND ASE_TARGET_QRCODE IS NOT NULL ) ";                
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
