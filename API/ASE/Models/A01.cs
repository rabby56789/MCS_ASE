
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
    public class A01 : IModel
    {
        /// <summary>
        /// DB連線字串
        /// </summary>
        string connStr { get; set; }
        /// <summary>
        /// 海康API
        /// </summary>
        string hikApiUrl { get; set; }
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Boolean pass = false;
        /// <summary>
        /// 建構式,取得設定檔資料
        /// </summary>
        public A01()
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
                _log.Debug($"A01 Function parameter: {obj}  A04isRun : {A04.A04isRun} ");
                throw new Exception($"A04執行中，請稍後再派");
            }
            _log.Debug($"A01 Function parameter: {obj}");

            string seq = obj["seq"];
            string jobName = obj["job_name"];
            string trolleyId = obj["car_no"];
            string startLoc = obj["start_loc"];
            string startArea = obj["start_area"];
            string secondStartArea = obj["second_start_area"];
            string targetLoc = obj["target_loc"];
            string targetArea = obj["target_area"];
            string secondTargetArea = obj["second_target_area"];
            string priorityStr = obj["priority"];
            //int priorityInt = stringToInt("priority", priorityStr, 1);
            string message = "初建立";
            string insertUser = "A01";

            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            #region 20220921 Ruby             
            //string task_StartArea = string.Empty;
            ////string StartLocArea = string.Empty;
            ////string StartTrolleyArea = string.Empty;
            //string task_TargetArea = targetArea;

            //if (jobName.Contains("Mold") || jobName.Contains("後段") || jobName.Contains("退空車"))//這些任務需要指派貨架和start_loc
            //{
            //    if (string.IsNullOrEmpty(trolleyId) || string.IsNullOrEmpty(startLoc))
            //    {
            //        _log.Debug($"A01 jobname = {jobName} trolleyID = {trolleyId}  startLoc = {startLoc}");
            //        throw new Exception($"非呼叫空車任務，請先指定貨架和start_loc填寫後再派");
            //    }
            //    else
            //    {
            //        _log.Debug($"A01 jobname = {jobName} trolleyID = {trolleyId}  startLoc = {startLoc}");
            //        //檢查該貨架在Queue和Subtask有無任務
            //        string sqlQueue = $"select * from t_task_queue where car_no = '{trolleyId}' and status = 0 and insert_time > date_sub(now() ,INTERVAL 10 minute)";
            //        string sqlSubtask = $"select * from t_subtask_status where ase_car_no ='{trolleyId}' and job_status in (0,1) and insert_time > date_sub(now() ,INTERVAL 10 minute)";
            //        dao.AddExecuteItem(sqlQueue, null);
            //        int QueueCount = dao.Query().Tables[0].Rows.Count;
            //        dao.AddExecuteItem(sqlSubtask, null);
            //        int SubtaskCount = dao.Query().Tables[0].Rows.Count;
            //        _log.Debug($"A01 jobname = {jobName}  QueueCount = {QueueCount}  SubtaskCount = {SubtaskCount}");
            //        if (QueueCount != 0) { throw new Exception($"貨架{trolleyId}在Queue中已有任務"); }
            //        if (SubtaskCount != 0) { throw new Exception($"貨架{trolleyId}已有任務正在執行"); }

            //        //檢查貨架位置和start_loc是否符合
            //        string sqlGetTrolleyLocation = $"select location_id from t_trolley_status where trolley_id = (select ID from base_trolley where name = '{trolleyId}')";
            //        dao.AddExecuteItem(sqlGetTrolleyLocation, null);
            //        string TrolleyLocation = dao.Query().Tables[0].Rows[0][0].ToString();
            //        _log.Debug($"A01 jobname = {jobName} 貨架目前location:  {TrolleyLocation}");
            //        string sqlGetArea = $"select ba.id,bs.id as storageID from base_area ba left join base_storage bs on ba.guid = bs.group_guid where bs.QRCODE = '{TrolleyLocation}';";
            //        dao.AddExecuteItem(sqlGetArea, null);
            //        var _dataArea = dao.Query().Tables[0];
            //        if (_dataArea.Rows.Count != 0)
            //        {
            //            string TrolleyStart_Loc = _dataArea.Rows[0]["storageID"].ToString();
            //            _log.Debug($"A01 jobname = {jobName} 貨架目前start_loc:  {TrolleyStart_Loc}");
            //            task_StartArea = _dataArea.Rows[0]["ID"].ToString();
            //            if (TrolleyStart_Loc.Equals(startLoc) == false)
            //            {
            //                _log.Debug($"A01 jobname = {jobName} | 貨架目前start_loc:  {TrolleyStart_Loc} |  任務下的start_loc : {startLoc}");
            //                throw new Exception($"貨架{trolleyId}位置於{TrolleyStart_Loc}，不在位置{startLoc},請重新下任務");
            //            }
            //        }
            //    }
            //}
            //else
            //{

            //    task_StartArea = startArea;
            //}
            #endregion
            //1.先檢查是否為特殊任務(退空車和呼叫空車)，不是的話沒有輸入貨架編號和start_loc則擋掉

            if (false == jobName.Contains("平面"))
            {
                if ((false == jobName.Contains("呼叫空車") && false == jobName.Contains("TTmove") && false == jobName.Contains("No6B2FtoM1Finoutc") && false == jobName.Contains("1FBE2AS") && false == jobName.Contains("No1M1FtoM11Fc") && false == jobName.Contains("No1M11FtoM1Fc") && false == jobName.Contains("1FAS2BE") && false == jobName.Contains("No6M1FtoB2Finoutc")) && (string.IsNullOrEmpty(trolleyId) || string.IsNullOrEmpty(startLoc)))
                {
                    throw new Exception($"非呼叫空車任務，請先指定貨架和start_loc填寫後再派");
                }
            }
            //2.有指派的貨架，就檢查現在這個貨架在t_task_queue和t_subtask_status建立時間X秒內是否有任務
            if (false == string.IsNullOrEmpty(trolleyId) && (false == jobName.Contains("TTmove") && false == jobName.Contains("No6B2FtoM1Finoutc") && false == jobName.Contains("1FBE2AS") && false == jobName.Contains("No1M1FtoM11Fc") && false == jobName.Contains("No1M11FtoM1Fc") && false == jobName.Contains("1FAS2BE") && false == jobName.Contains("No6M1FtoB2Finoutc")))
            {

                string sqlQueue = $"select * from t_task_queue where car_no = '{trolleyId}' and status = 0 and insert_time > date_sub(now() ,INTERVAL 10 minute)";
                string sqlSubtask = $"select * from t_subtask_status where ase_car_no ='{trolleyId}' and job_status in (0,1) and insert_time > date_sub(now() ,INTERVAL 10 minute)";
                dao.AddExecuteItem(sqlQueue, null);
                int QueueCount = dao.Query().Tables[0].Rows.Count;
                dao.AddExecuteItem(sqlSubtask, null);
                int SubtaskCount = dao.Query().Tables[0].Rows.Count;
                if (QueueCount != 0) { throw new Exception($"貨架{trolleyId}在Queue中已有任務"); }
                if (SubtaskCount != 0) { throw new Exception($"貨架{trolleyId}已有任務正在執行"); }
            }
            //3.檢查Task_Name，先找出起始區域和目的區域
            string task_StartArea = string.Empty;
            string StartLocArea = string.Empty;
            string StartTrolleyArea = string.Empty;
            string task_TargetArea = string.Empty;
            //用start_loc找起始區域
            if (false == string.IsNullOrEmpty(startLoc) && (false == jobName.Contains("TTmove") && false == jobName.Contains("No6B2FtoM1Finoutc") && false == jobName.Contains("1FBE2AS") && false == jobName.Contains("No1M1FtoM11Fc") && false == jobName.Contains("No1M11FtoM1Fc") && false == jobName.Contains("1FAS2BE") && false == jobName.Contains("No6M1FtoB2Finoutc")))
            //if (false == string.IsNullOrEmpty(startLoc))
            {
                string sqlGetArea = sqlCreator.GetStartArea(startLoc);
                dao.AddExecuteItem(sqlGetArea, null);
                var _dataArea = dao.Query().Tables[0];
                if (_dataArea.Rows.Count != 0)
                {
                    task_StartArea = _dataArea.Rows[0]["ID"].ToString();//用start_loc找到start_area
                    if (false == string.IsNullOrEmpty(startArea) && false == task_StartArea.Equals(startArea))
                    {
                        throw new Exception($"儲位{startLoc}不包含在儲區{startArea},請重新下任務");
                    }
                    _log.Debug($"A01 task_StartArea for have startLoc: {task_StartArea}  \n start_loc is : {startLoc}");
                }

            }
            _log.Debug($"A01 task_StartArea for have startLoc: {task_StartArea}  \n start_loc is : {startLoc}");
            //用貨架目前位置(QRCODE)找起始區域
            if (false == string.IsNullOrEmpty(trolleyId) && (false == jobName.Contains("TTmove") && false == jobName.Contains("No6B2FtoM1Finoutc") && false == jobName.Contains("1FBE2AS") && false == jobName.Contains("No1M1FtoM11Fc") && false == jobName.Contains("No1M11FtoM1Fc") && false == jobName.Contains("1FAS2BE") && false == jobName.Contains("No6M1FtoB2Finoutc")))
            {
                string sqlGetLocation_ID = $@"select location_id from t_trolley_status where trolley_id = (select ID from base_trolley where name = '{trolleyId}')";
                dao.AddExecuteItem(sqlGetLocation_ID, null);
                var _dataLoc = dao.Query().Tables[0];
                string location_id = string.Empty;
                string trolley_start_loc = string.Empty;
                if (_dataLoc.Rows.Count != 0)
                {
                    //string storage_ID = string.Empty;
                    location_id = _dataLoc.Rows[0]["LOCATION_ID"].ToString();//貨架目前位置QRCODE
                    string sqlGetArea = $"select ba.id,bs.id as storageID from base_area ba left join base_storage bs on ba.guid = bs.group_guid where bs.QRCODE = '{location_id}';";
                    dao.AddExecuteItem(sqlGetArea, null);
                    var _dataArea = dao.Query().Tables[0];
                    if (_dataArea.Rows.Count != 0)
                    {
                        StartTrolleyArea = _dataArea.Rows[0]["ID"].ToString();//用trolley找到start_Area
                        trolley_start_loc = _dataArea.Rows[0]["storageID"].ToString();//貨架目前位置storage_id
                        if (false == string.IsNullOrEmpty(startArea) && false == task_StartArea.Equals(startArea))
                        {
                            throw new Exception($"貨架{trolleyId}位置於區域{task_StartArea}，不包含在出發儲區{startArea},請重新下任務");
                        }
                        if (false == string.IsNullOrEmpty(startLoc) && false == trolley_start_loc.Equals(startLoc))
                        {
                            throw new Exception($"貨架{trolleyId}位置於{trolley_start_loc}，不在位置{startLoc},請重新下任務");
                        }
                        _log.Debug($"A01 task_StartArea for have trolleId: {StartTrolleyArea}  \n trolley_id is : {trolleyId}");
                    }

                }

            }
            _log.Debug($"A01 task_StartArea for have trolleId: {StartTrolleyArea}  \n trolley_id is : {trolleyId}");

            if (false == string.IsNullOrEmpty(targetLoc) && (false == jobName.Contains("TTmove") && false == jobName.Contains("No6B2FtoM1Finoutc") && false == jobName.Contains("1FBE2AS") && false == jobName.Contains("No1M1FtoM11Fc") && false == jobName.Contains("No1M11FtoM1Fc") && false == jobName.Contains("1FAS2BE") && false == jobName.Contains("No6M1FtoB2Finoutc")))//找目的區域
            {
                string sqlGetArea = sqlCreator.GetTargetArea(targetLoc);
                dao.AddExecuteItem(sqlGetArea, null);
                var _data = dao.Query().Tables[0];
                if (_data.Rows.Count != 0)
                {
                    task_TargetArea = _data.Rows[0]["ID"].ToString();
                    _log.Debug($"A01 task_TargetArea for have targetLoc : {task_TargetArea}");
                }

            }
            if (string.IsNullOrEmpty(task_StartArea))
            {
                task_StartArea = startArea;
            }
            if (string.IsNullOrEmpty(task_TargetArea))
            {
                task_TargetArea = targetArea;
            }
            //拼出TaskName
            string _taskname = jobName + "_" + task_StartArea + "_" + task_TargetArea;
            _log.Debug($"A01 _taskname compile is : {_taskname}");
            string sqlGetTask = sqlCreator.GetTaskName(_taskname);
            dao.AddExecuteItem(sqlGetTask, null);
            var dataTask = dao.Query().Tables[0];
            var taskNameCount = Int32.Parse(dataTask.Rows[0][0].ToString());
            _log.Debug($"A01 _taskname count is : {taskNameCount}");
            if (taskNameCount == 0 && jobName.Contains("平面搬運")==false)
            {
                throw new Exception("查無此任務，請先至【任務設定】設置 ，JOB_NAME: " + _taskname );
            }

            //新增任務

            string sqlStr = $@"INSERT INTO t_task_queue
                     (GUID, JOB_NAME, SEQ, START_LOC, START_AREA, SEC_START_AREA, TARGET_LOC, TARGET_AREA, SEC_TARGET_AREA, CAR_NO, PRIORITY, IS_MIXED_AREA, STATUS, MESSAGE, TRY_COUNT,REMARK, INSERT_USER, INSERT_TIME, UPDATE_USER, UPDATE_TIME) VALUE
                     (UUID(), '{jobName}', '{seq}', '{startLoc}', '{startArea}', '{secondStartArea}', '{targetLoc}', '{targetArea}', '{secondTargetArea}', '{trolleyId}', '{priorityStr}', 0, 0, '{message}', 0, 'N/A', '{insertUser}', NOW(), '{insertUser}', NOW())";
            dao.AddExecuteItem(sqlStr, null);
            if ( true == dao.Execute())
            {
                JObject result = new JObject();
                //出發儲位被鎖定
                string sqlLock = $"select * from t_storage_status where storage_guid = (select guid from base_storage where ID = '{startLoc}')";
                dao.AddExecuteItem(sqlLock, null);
                var lock_data = dao.Query().Tables[0].Rows.Count;
                if (lock_data > 0)
                {
                    result.Add("msg", "已派任務至Queue，出發儲位被鎖定，請記得解鎖");
                }
                else { result.Add("msg", ""); }
                result.Add("result","ok");
                //return new JObject() { new JProperty("result", "ok") };
                return result;
            }
            else
            {
                throw new Exception("新增資料失敗。");
            }

            
        }

        private int stringToInt(string variableName, string inputString, int defaultValue)
        {
            if (false == string.IsNullOrEmpty(inputString))
            {
                try
                {
                    return int.Parse(inputString);
                }
                catch
                {
                    _log.Error($@"{variableName} 轉型失敗: {inputString}，改用預設值 {defaultValue} 。");
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }
        public JObject Old_Run(dynamic obj)
        {
            _log.Debug($"A01 Function parameter: {obj}");

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

            //找出起始位置
            //找LOCATION_ID、TYPE
            string Location_ID = string.Empty;
            string storage = string.Empty;
            string car_no = obj["car_no"];
            string podcode = string.Empty;
            if (!string.IsNullOrEmpty(car_no))
            {
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
                else
                {
                    throw new Exception("貨架號碼查無起點位置或已有任務");
                }

            }
            _log.Debug("A01  " + "Location_ID : " + Location_ID);
            _log.Debug("A01  " + "storage : " + storage);
            _log.Debug("A01  " + "podcode : " + podcode);

            //設定或找尋起點終點
            string startArea = string.Empty;
            string targetArea = string.Empty;
            string start = string.Empty;
            string target = string.Empty;
            string jobName = string.Empty;
            string start_Location_ID = string.Empty;
            string target_Location_ID = string.Empty;
            string taskName = string.Empty;
            startArea = obj["start_area"];
            start = obj["start_loc"];
            targetArea = obj["target_area"];
            target = obj["target_loc"];
            jobName = obj["job_name"];

            if (true == string.IsNullOrEmpty(start) && false == string.IsNullOrEmpty(storage))
            {
                start = storage;
            }

            if (false == string.IsNullOrEmpty(start) && false == string.IsNullOrEmpty(storage))
            {
                if (start != storage)
                {
                    throw new Exception("貨架所在位置與start_loc不相符");
                }
            }

            // 檢查出發儲位是否被鎖定
            if (false == string.IsNullOrEmpty(start))
            {
                string location = start;
                string _sqlString = $@"select GUID,GROUP_GUID from mcs.base_storage where ID = '{location}' || NAME = '{location}' || QRCODE = '{location}'";
                dao.AddExecuteItem(_sqlString, null);
                data = dao.Query().Tables[0];
                if (data.Rows.Count == 0)
                {
                    throw new Exception($@"查無儲位 {location}");
                }

                string _storageGuid = data.Rows[0]["GUID"].ToString();
                _sqlString = $@"select GUID from mcs.t_storage_status where STORAGE_GUID = '{_storageGuid}'";
                dao.AddExecuteItem(_sqlString, null);
                data = dao.Query().Tables[0];
                if (data.Rows.Count != 0)
                {
                    throw new Exception($@"出發儲位 {start} 已被鎖定");
                }
            }


            //
            //將job_name、開始區域、目的地區域組合成路徑(要確認日月光的區域組合MB2F-FE)
            if (startArea == "")
            {
                var sqlArea = sqlCreator.GetStartArea(start);
                var AreaParms = sqlCreator.CreateParameterAry(null);
                dao.AddExecuteItem(sqlArea, AreaParms);
                data = dao.Query().Tables[0];
                startArea = data.Rows[0][0].ToString();
            }
            if (targetArea == "")
            {
                var sqlArea = sqlCreator.GetTargetArea(target);
                var AreaParms = sqlCreator.CreateParameterAry(null);
                dao.AddExecuteItem(sqlArea, AreaParms);
                data = dao.Query().Tables[0];
                if (data.Rows.Count == 0)
                {
                    throw new Exception(@"查無此儲位 ""target_loc"": " + target);
                }
                targetArea = data.Rows[0][0].ToString();
            }

            taskName = jobName + "_" + startArea + "_" + targetArea;
            //檢查JobName與設定的PassJobName是否相同後續用作目地檢查條件
            if (PassJobName == taskName)
            {
                pass = true;
            }

            JObject startObj = new JObject();
            if (string.IsNullOrEmpty(start))
            {
                if (string.IsNullOrEmpty(startArea))
                {
                    if (string.IsNullOrEmpty(Location_ID))
                    {
                        throw new Exception("無起點位置資訊");
                    }
                    else
                    {
                        start = storage;
                        start_Location_ID = Location_ID;
                    }
                }
                else
                {
                    //area找
                    string parentGuid = obj["parent_guid"];
                    startObj = StartToRCS(startArea, 0, parentGuid);
                    start_Location_ID = startObj["qrcode"].ToString();
                    start = startObj["loc"].ToString();
                    car_no = startObj["car_no"].ToString();
                    podcode = startObj["podcode"].ToString();
                }
            }
            else
            {
                startObj = StartToRCS(start, 1);
                start_Location_ID = startObj["qrcode"].ToString();
                car_no = startObj["car_no"].ToString();
                podcode = startObj["podcode"].ToString();
            }



            JObject targetObj = new JObject();
            if (string.IsNullOrEmpty(target))
            {
                if (string.IsNullOrEmpty(targetArea))
                {

                    throw new Exception("查無目標位置資訊");

                }
                else
                {
                    //area找
                    try
                    {
                        targetObj = TargetToRCS(targetArea, 0);
                        target_Location_ID = targetObj["qrcode"].ToString();
                        target = targetObj["loc"].ToString();
                    }
                    catch (Exception exception)
                    {
                        if (!exception.Message.Contains("查無空儲位"))
                            throw exception;

                        if (jobName.Contains("呼叫空車"))
                        {
                            _log.Info($@"呼叫空車找不到 {targetArea} 空儲位，改找正在進行的任務的出發儲位");
                            targetObj = TargetAreaToRCSWithRunningTask(targetArea);
                            target_Location_ID = targetObj["qrcode"].ToString();
                            target = targetObj["loc"].ToString();
                        }
                        else if (targetArea == "MB2F-MD")
                        {
                            _log.Info($@"找不到 {targetArea} 空儲位，改找 MB2F-MDW");
                            targetArea = "MB2F-MDW";
                            targetObj = TargetToRCS(targetArea, 0);
                            target_Location_ID = targetObj["qrcode"].ToString();
                            target = targetObj["loc"].ToString();
                        }
                        else if (targetArea == "MB2F-MDW")
                        {
                            _log.Info($@"找不到 {targetArea} 空儲位，改找 MB2F-MD");
                            targetArea = "MB2F-MD";
                            targetObj = TargetToRCS(targetArea, 0);
                            target_Location_ID = targetObj["qrcode"].ToString();
                            target = targetObj["loc"].ToString();
                        }
                        else
                            throw exception;
                    }


                }
            }
            else
            {
                try
                {
                    targetObj = TargetToRCS(target, 1);
                    target_Location_ID = targetObj["qrcode"].ToString();
                }
                catch (Exception exception)
                {
                    if (!exception.Message.Contains("查無空儲位"))
                        throw exception;

                    if (jobName.Contains("呼叫空車"))
                    {
                        _log.Info($@"呼叫空車發現 {targetArea} 不是空儲位，改找正在進行的任務的出發儲位");
                        targetObj =

                            TargetLocToRCSWithRunningTask(target);
                        target_Location_ID = targetObj["qrcode"].ToString();
                    }
                    else
                    {
                        throw exception;
                    }
                }

            }
            _log.Debug("A01  " + " targetObj : " + targetObj);

            //測試
            //將job_name、開始區域、目的地區域組合成路徑(要確認日月光的區域組合MB2F-FE)
            //var sqlArea = sqlCreator.GetArea(start, target);
            //var AreaParms = sqlCreator.CreateParameterAry(obj);
            //dao.AddExecuteItem(sqlArea, AreaParms);
            //data = dao.Query().Tables[0];
            //taskName = jobName + "_" + data.Rows[0].ItemArray[0].ToString() + "_" + data.Rows[1].ItemArray[0].ToString();
            //


            //Location_ID FOR日月光 轉換

            //用起點終點決定TASK_ID
            string TASK_ID = string.Empty;
            //TASK_ID = GetTask_ID(start, target, jobName);
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
            //POSITIONCODEPATH = data.Rows[0]["POSITIONCODEPATH"].ToString();
            ////只有一個任務(更換終點)
            //if (data.Rows.Count == 1)
            //{                
            //    POSITIONCODEPATH = POSITIONCODEPATH_TARGET(POSITIONCODEPATH, target_Location_ID);
            //}
            ////更換起點
            //POSITIONCODEPATH = POSITIONCODEPATH_START(POSITIONCODEPATH, start_Location_ID);

            //建立任務
            parameter.Add("GUID", uuid);
            parameter.Add("TASK_GUID", data.Rows[0]["TASK_GUID"].ToString());
            parameter.Add("WEIGHTING", weighting);
            parameter.Add("SUBTASK_TYPE", data.Rows[0]["SUBTASK_TYPE"].ToString());
            //parameter.Add("TASKTYP", data.Rows[0]["TASK_TYPE"].ToString());
            //parameter.Add("POSITIONCODEPATH", POSITIONCODEPATH);
            parameter.Add("TASKTYP", "");
            parameter.Add("POSITIONCODEPATH", "");
            parameter.Add("PODCODE", podcode);//待確認
            parameter.Add("PODDIR", "");
            parameter.Add("MATERIALLOT", "");
            parameter.Add("PRIORITY", "");
            parameter.Add("AGVCODE", "");
            parameter.Add("INSERT_USER", "API_A01");
            //ASE專用
            parameter.Add("ASE_START_LOC", start);
            parameter.Add("ASE_START_QRCODE", start_Location_ID);
            parameter.Add("ASE_TARGET_LOC", target);
            parameter.Add("ASE_TARGET_QRCODE", target_Location_ID);
            parameter.Add("ASE_JOB_NAME", obj["job_name"]);
            parameter.Add("ASE_SEQ", obj["seq"]);
            parameter.Add("ASE_CAR_NO", car_no);
            _log.Debug("A01  " + " parameter : " + parameter);


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
            var sqlStrTravel = sqlCreator.InsertTaskTravel();
            dao.AddExecuteItem(sqlStrTravel, sqlParm);
            dao.Execute();
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
                string message = Location_ID + "無此儲位，A01 Function ASEToRCS";
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
        public JObject StartToRCS(string start, int type, string parentGuid = "")
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
        public JObject TargetToRCS(string target, int type)
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
                string message = target + "查無空儲位，Function TargetToRCS";
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
                string message = target + "查無空儲位，Function TargetToRCS";
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
                string message = target + "查無空儲位，Function TargetToRCS";
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
            string path = POSITIONCODEPATH.Substring(0, (POSITIONCODEPATH.Length - 14));
            path = path + target;
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
                throw new Exception("查無此: " + taskName + " 路線");

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
