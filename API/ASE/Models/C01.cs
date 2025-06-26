
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
    public class C01 : IModel
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
        public C01()
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
                log.Debug($"C01 Function parameter: {obj}  A04isRun : {A04.A04isRun} ");
                throw new Exception($"A04執行中，請稍後");
            }
            log.Debug($"C01 Function parameter: {obj}");

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
            //檢查並找出 start_loc 和 start_area
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
                    storage= data.Rows[0]["ID"].ToString();
                    podcode = data.Rows[0]["TROLLEY_ID"].ToString();
                }
                else
                {
                    throw new Exception("貨架號碼查無起點位置或已有任務");
                }

                log.Debug("C01  " + "Location_ID : " + Location_ID);
                log.Debug("C01  " + "storage : " + storage);
                log.Debug("C01  " + "podcode : " + podcode);
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
                throw new Exception("無起點位置資訊 (RemovedFromQueue)");
            }

            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(storage) && string.IsNullOrEmpty(firstStartArea) && string.IsNullOrEmpty(secondStartArea))
            {
                throw new Exception("無起點位置資訊");
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
                    throw new Exception("貨架所在位置與start_loc不相符");
                }
            }
            //判斷任務是否為目的型(20220824 Ruby 特殊鎖定任務)
            string ismixedarea = obj["ismixedarea"];
            //bool isTargetType = false;
            //if (ismixedarea == "1" && string.IsNullOrEmpty(secondStartArea))//目的型會沒有secondStartArea
            //{
            //    isTargetType = true;
            //}
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
                    throw new Exception($@"查無儲位 {location} (RemovedFromQueue)");
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
                        log.Debug($"C01 檢查出發點有鎖定且JOBNAME有相同): {_guid}");
                    }
                    else 
                    {
                        log.Debug($"C01 檢查出發點有鎖定但JOBNAME不相同): {_guid}");
                        throw new Exception($@"出發儲位 {start} 已被鎖定");
                        
                    }

                }
            }

            //檢查 start_loc 是否屬於 start_area
            if (false == string.IsNullOrEmpty(start))
            {
                var sqlArea = sqlCreator.GetStartArea(start);
                var AreaParms = sqlCreator.CreateParameterAry(null);
                dao.AddExecuteItem(sqlArea, AreaParms);
                data = dao.Query().Tables[0];
                if (data.Rows.Count == 0)
                {
                    throw new Exception(@"查無此儲位 start_loc 所在儲位: " + start + " (RemovedFromQueue)");
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
                        throw new Exception($@"出發儲位 {start} 不屬於出發儲區 {startArea}");
                    }
                }
            }
            

            //檢查並找出 target_loc 和 target_area
            if (string.IsNullOrEmpty(target))
            {
                if (string.IsNullOrEmpty(targetArea))
                {
                    throw new Exception("沒有輸入目標位置資訊 (RemovedFromQueue)");
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
                    throw new Exception(@"查無此儲位 ""target_loc"" 所在儲位: " + target + " (RemovedFromQueue)");
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
                        throw new Exception($@"目標儲位 {target} 不屬於目標儲區 {targetArea} (RemovedFromQueue)");
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
            log.Debug("C01  " + " startLoctionQrcode : " + startLoctionQrcode);

            // 找出 target location qrcode
            JObject targetObj = new JObject();
            if (false == string.IsNullOrEmpty(target))
            {
                // 透過 target 找 qrcode
                try
                {

                    //正常找target用TargetToRCS
                    //前段任務用t_holding_info找出最前面的起始任務，再找出一開始派的target_loc(被鎖定的)，任務結束後由子任務API_A03解鎖
                    //20221214 Ruby 在t_holding_info查詢目前任務seq如果為接力任務，找最前面那一筆
                    string FEseq = obj["seq"].ToString();
                    int FECount = 0;//計算查詢次數，若查詢次數小於則表示不是前段最後一段任務
                    while (true)
                    {
                        string sqlseq = $"select Pre_seq from t_holding_info where Next_seq = '{FEseq}'";
                        dao.AddExecuteItem(sqlseq, null);
                        var FEdata = dao.Query().Tables[0];
                        if (FEdata.Rows.Count > 0)
                        {
                            FEseq = FEdata.Rows[0][0].ToString();
                            FECount++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    log.Debug("前段任務 :　" + FEseq + " 第  " + FECount + " 段");

                    if (FECount == 4)//查最前seq 4次的為前段任務最後一段
                    {
                        string sqlFEtarget_loc = $"select ASE_TARGET_LOC,ASE_TARGET_QRCODE from t_subtask_status where ASE_SEQ = '{FEseq}' and ASE_JOB_NAME != '暫存'";
                        dao.AddExecuteItem(sqlFEtarget_loc, null);
                        var _FEdata = dao.Query().Tables[0];
                        target = _FEdata.Rows[0]["ASE_TARGET_LOC"].ToString();
                        targetLocationQrcode = _FEdata.Rows[0]["ASE_TARGET_QRCODE"].ToString();
                        log.Debug("C01 用target_loc 判斷前段  " + " targetLocation : " + target);
                        log.Debug("C01 用target_loc 判斷前段  " + " targetLocationQrcode : " + targetLocationQrcode);
                    }
                    else if (FECount == 0 && targetArea.Contains("FE"))//目的區域包含FE皆為前段任務，用來識別第0段任務用
                    {
                        targetObj = TargetToRCS_Block(target, 1);
                        targetLocationQrcode = targetObj["qrcode"].ToString();
                        log.Debug("C01 用target_loc 判斷前段任務0  " + " targetLocation : " + target);
                        log.Debug("C01 用target_loc 判斷前段任務0  " + " targetLocationQrcode : " + targetLocationQrcode);
                    }
                    else if (FECount > 0 && FECount < 4)//第1-3段
                    {
                        targetObj = TargetToRCS_Block(target, 1);
                        targetLocationQrcode = targetObj["qrcode"].ToString();
                        log.Debug("C01 用target_loc 判斷前段任務" + FECount + " targetLocation : " + target);
                        log.Debug("C01 用target_loc 判斷前段任務" + FECount + "targetLocationQrcode : " + targetLocationQrcode);
                    }
                    else//後段任務和CPD用
                    {
                        targetObj = TargetToRCS(target, 1);
                        targetLocationQrcode = targetObj["qrcode"].ToString();
                        log.Debug("C01 用target_loc 判斷任務" + FECount + " targetLocation : " + target);
                        log.Debug("C01 用target_loc 判斷任務" + FECount + "targetLocationQrcode : " + targetLocationQrcode);
                    }
                    targetObj = TargetToRCS(target, 1);
                    targetLocationQrcode = targetObj["qrcode"].ToString();
                }
                catch (Exception exception)
                {
                    throw exception;
                    //if (!exception.Message.Contains("查無空儲位"))
                    //    throw exception;

                    //if (jobName.Contains("呼叫空車"))
                    //{
                    //    log.Info($@"呼叫空車發現 {targetArea} 不是空儲位，改找正在進行的任務的出發儲位");
                    //    targetObj = TargetLocToRCSWithRunningTask(target);
                    //    targetLocationQrcode = targetObj["qrcode"].ToString();
                    //}
                    //else
                    //{
                    //    throw exception;
                    //}
                }
            }
            else
            {
                // 透過 targetArea 找 qrcode
                try
                {


                    //正常找target用TargetToRCS
                    //前段任務用t_holding_info找出最前面的起始任務，再找出一開始派的target_loc(被鎖定的)，任務結束後由子任務API_A03解鎖
                    //20221214 Ruby 在t_holding_info查詢目前任務seq如果為接力任務，找最前面那一筆
                    string FEseq = obj["seq"].ToString();
                    int FECount = 0;//計算查詢次數，若查詢次數小於則表示不是前段最後一段任務
                    while (true)
                    {
                        string sqlseq = $"select Pre_seq from t_holding_info where Next_seq = '{FEseq}'";
                        dao.AddExecuteItem(sqlseq, null);
                        var FEdata = dao.Query().Tables[0];
                        if (FEdata.Rows.Count > 0)
                        {
                            FEseq = FEdata.Rows[0][0].ToString();
                            FECount++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    log.Debug("前段任務 :　" + FEseq + " 第  " + FECount + " 段");

                    if (FECount == 4)//查最前seq 4次的為前段任務最後一段
                    {
                        string sqlFEtarget_loc = $"select ASE_TARGET_LOC,ASE_TARGET_QRCODE from t_subtask_status where ASE_SEQ = '{FEseq}' and ASE_JOB_NAME != '暫存'";
                        dao.AddExecuteItem(sqlFEtarget_loc, null);
                        var _FEdata = dao.Query().Tables[0];
                        target = _FEdata.Rows[0]["ASE_TARGET_LOC"].ToString();
                        targetLocationQrcode = _FEdata.Rows[0]["ASE_TARGET_QRCODE"].ToString();
                        log.Debug("C01 判斷前段  " + " targetLocation : " + target);
                        log.Debug("C01 判斷前段  " + " targetLocationQrcode : " + targetLocationQrcode);
                    }
                    else if (FECount == 0 && targetArea.Contains("FE"))//目的區域包含FE皆為前段任務，用來識別第0段任務用
                    {
                        targetObj = TargetToRCS_Block(targetArea, 0);//target為鎖定不能派
                        targetLocationQrcode = targetObj["qrcode"].ToString();
                        target = targetObj["loc"].ToString();
                        log.Debug("C01 判斷前段任務0  " + " targetLocation : " + target);
                        log.Debug("C01 判斷前段任務0  " + " targetLocationQrcode : " + targetLocationQrcode);
                    }
                    else if (FECount > 0 && FECount < 4)//第1-3段
                    {
                        targetObj = TargetToRCS_Block(targetArea, 0);//target為鎖定不能派
                        targetLocationQrcode = targetObj["qrcode"].ToString();
                        target = targetObj["loc"].ToString();
                        log.Debug("C01 判斷前段任務"+ FECount  + " targetLocation : " + target);
                        log.Debug("C01 判斷前段任務" + FECount + "targetLocationQrcode : " + targetLocationQrcode);
                    }
                    else//後段任務和CPD用
                    {
                        targetObj = TargetToRCS(targetArea, 0);
                        targetLocationQrcode = targetObj["qrcode"].ToString();
                        target = targetObj["loc"].ToString();
                        log.Debug("C01 判斷任務" + FECount + " targetLocation : " + target);
                        log.Debug("C01 判斷任務" + FECount + "targetLocationQrcode : " + targetLocationQrcode);
                    }
                    //targetObj = TargetToRCS(targetArea, 0);
                    //targetLocationQrcode = targetObj["qrcode"].ToString();
                    //target = targetObj["loc"].ToString();
                }
                catch (Exception exception)
                {
                    throw exception;

                    //if (!exception.Message.Contains("查無空儲位"))
                    //    throw exception;

                    //if (jobName.Contains("呼叫空車"))
                    //{
                    //    log.Info($@"呼叫空車找不到 {targetArea} 空儲位，改找正在進行的任務的出發儲位");
                    //    targetObj = TargetAreaToRCSWithRunningTask(targetArea);
                    //    targetLocationQrcode = targetObj["qrcode"].ToString();
                    //    target = targetObj["loc"].ToString();
                    //}
                    //else if (targetArea == "MB2F-MD")
                    //{
                    //    log.Info($@"找不到 {targetArea} 空儲位，改找 MB2F-MDW");
                    //    targetArea = "MB2F-MDW";
                    //    targetObj = TargetToRCS(targetArea, 0);
                    //    targetLocationQrcode = targetObj["qrcode"].ToString();
                    //    target = targetObj["loc"].ToString();
                    //}
                    //else if (targetArea == "MB2F-MDW")
                    //{
                    //    log.Info($@"找不到 {targetArea} 空儲位，改找 MB2F-MD");
                    //    targetArea = "MB2F-MD";
                    //    targetObj = TargetToRCS(targetArea, 0);
                    //    targetLocationQrcode = targetObj["qrcode"].ToString();
                    //    target = targetObj["loc"].ToString();
                    //}
                    //else
                    //    throw exception;
                }
            }                        
            log.Debug("C01  " + " targetLocationQrcode : " + targetLocationQrcode);


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
                string message = taskName + "無此任務，請先至主任務新增 (RemovedFromQueue)";
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

                //JObject result = new JObject();
                //JObject A03Parms = new JObject();
                //A03Parms.Add("function", "A03");
                //A03Parms.Add("lock_status", "1");
                //A03Parms.Add("location", targetLocationQrcode);
                //A03Parms.Add("job_name", jobName);
                //string json = JsonConvert.SerializeObject(A03Parms);
                //log.Info("發送A03__TaskCenter json:" + json);

                ////SEND API
                //string IP = "192.168.56.81"; //parm["SERVER_IP"].ToString();
                //string PORT = "81"; //parm["SERVER_PORT"].ToString();
                //string URL = "api/Ase/CallFunc"; //parm["URL"].ToString();
                //string url = "http://" + IP + ":" + PORT + "/" + URL;
                //ConnectionTools HikApi = new ConnectionTools();
                //Task<string> task = HikApi.PostAsyncJson(url, json);

                //log.Info("發送A03__TaskCenter Result:" + task.Result);

                ////API回傳判定

                //string jsonResult = task.Result.Replace("\"{", "{").Replace("}\"", "}").Replace("\\", "");
                //result = JsonConvert.DeserializeObject<JObject>(jsonResult);

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
            parameter.Add("INSERT_USER", "API_C01");
            //ASE專用
            parameter.Add("ASE_START_LOC",start);
            parameter.Add("ASE_START_QRCODE", startLoctionQrcode);
            parameter.Add("ASE_TARGET_LOC", target);
            parameter.Add("ASE_TARGET_QRCODE", targetLocationQrcode);
            parameter.Add("ASE_JOB_NAME", obj["job_name"]);
            parameter.Add("ASE_SEQ", obj["seq"]);
            parameter.Add("ASE_CAR_NO", car_no);
            parameter.Add("ASE_A01_TIME", obj["a01_time"]);
            log.Debug("C01  " + " parameter : " + parameter);

            
            //比對base_TASK
            //
            sqlStr = sqlCreator.GetTaskName(taskName);
            var taskNameParms = sqlCreator.CreateParameterAry(null);
            dao.AddExecuteItem(sqlStr, taskNameParms);
            data = dao.Query().Tables[0];
            var taskNameCount = Int32.Parse(data.Rows[0][0].ToString());
            if (taskNameCount == 0)
            {
                throw new Exception("查無此任務，請先至【任務設定】設置 ，JOB_NAME: " + taskName + " (RemovedFromQueue)");
            }
            //最後如果符合特殊搬運，可以建立t_subtask_status才清
            if (isEuualJob)
            {
                var sqlUnlock = $"delete from t_storage_status where GUID = '{_guid}'";
                try
                {
                    dao.AddExecuteItem(sqlUnlock, null);
                    dao.Execute();
                    log.Debug($"C01  清t_storage_status   {_guid}   jobnmae {allow_job}");
                }
                catch (Exception ex)
                {
                    log.Debug($"C01  清t_storage_status   {ex} ");
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
            catch(Exception ex) {
                log.Error($"C01 travel GUID :{parameter["GUID"]}  " +
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
            if (type == 0)//type: 0 用Area找
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
            else//type: 1用loc找
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
        public JObject TargetToRCS_Block(string target, int type)
        {
            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();
            JObject parm = new JObject();
            string sqlStr = string.Empty;
            if (type == 0)//type: 0 用Area找
            {
                parm.Add("TARGETAREA", target);
                sqlStr = $@"SELECT ST.ID,ST.QRCODE FROM mcs.BASE_STORAGE ST ,mcs.BASE_AREA AT";
                sqlStr += " WHERE ST.ENABLE = 1 AND AT.ENABLE = 1 ";
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT LOCATION_ID FROM mcs.T_TROLLEY_STATUS) ";
                //sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_TARGET_QRCODE FROM mcs.T_SUBTASK_STATUS WHERE JOB_STATUS IN(0,1) AND ASE_TARGET_QRCODE IS NOT NULL ) ";
                //sqlStr += " AND ST.ID LIKE CONCAT(@TARGETAREA, '%') ";
                //20221219 Ruby 被鎖定的儲位不可當target
                sqlStr += "AND ST.QRCODE NOT IN (SELECT QRCODE from base_storage where guid in (select storage_guid from t_storage_status)) ";
                sqlStr += " AND ST.GROUP_GUID = AT.GUID ";
                sqlStr += " AND AT.ID = @TARGETAREA ";
            }
            else//type: 1用loc找
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
                sqlStr += "AND ST.QRCODE NOT IN (SELECT QRCODE from base_storage where guid in (select storage_guid from t_storage_status)) ";

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
