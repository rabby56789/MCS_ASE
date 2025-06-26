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
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;

namespace MCS.Controllers
{
    public class elvtCallbackServiceController : ApiController
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [System.Web.Http.HttpPost]
        public JObject elvtCallback(JObject callBack)   //
        {
            log.Info($"elvtCallback callBack : {callBack}");
            /// task queue 收到電梯任務的就緒通知("taskSpaceStatus": "0")[電梯在 起始樓層 就緒]，即更新，，，，
            /// task queue 收到電梯任務的就緒通知("taskSpaceStatus": "1")[電梯在 目標樓層 就緒]，即更新，，，，
            /// 
            /// callBack: 
            //{
            //  "code": null,
            //  "taskStatusGuid": "3672aa51-c356-11ec-b24e-0800274ef241",
            //  "elvtTaskStatusGuid": "c6c40f89-fa14-11ec-b24e-0800274ef241",
            //  "taskSpaceStatus": "0",
            //  "elvtSpaceInfoList": [
            //    {
            //      "ELVT_ID": "Elevator1",
            //      "ELVT_SPACE_ID": "E1",
            //      "ELVT_SPACE_INDEX": "1",
            //      "ELVT_SPACE_QRCODE": "966dae63-8cc6-11ec-902a-0800274ef241",
            //      "ELVT_SPACE_STATUS_TROLLEY_ID": "T1008",
            //      "ELVT_SPACE_STATUS_GUID": "966dae63-8cc6-11ec-902a-0800274ef241"
            //    }
            //  ]
            //}

            //callBack["code"].ToString();

            JObject result = new JObject();
            JObject parm = new JObject();
            bool update_success = false;//20230419判斷更新是否成功
            try
            {
                var taskStatusGuid = callBack["taskStatusGuid"].ToString();
                var elvtTaskStatusGuid = callBack["elvtTaskStatusGuid"].ToString();
                var taskSpaceStatus = callBack["taskSpaceStatus"].ToString();
                var elvtSpaceInfoList = callBack["elvtSpaceInfoList"].ToArray();
                parm = JsonConvert.DeserializeObject<JObject>(elvtSpaceInfoList[0].ToString());
                parm.Add("GUID", taskStatusGuid);
                parm.Add("ELVT_TASK_GUID", elvtTaskStatusGuid);
                parm.Add("SUBTASK_TYPE", "1");
                parm.Add("SERVER_FUNCTION", "elvtCallback");
                parm.Add("TASKCODE", taskStatusGuid);
                parm.Add("SUBTASK_FUNCTION","");

                log.Info($"elvtCallback parm {parm} ");

                switch (taskSpaceStatus)
                {
                    case "0":
                        //parm.Add("SUBTASK_FUNCTION", "ELVT_START");
                        parm["SUBTASK_FUNCTION"] = "ELVT_START";
                        log.Info($"elvtCallback case {taskSpaceStatus} ");
                        UpdateElvtParm(parm, "0");

                        //20230419 檢查 ELVT_START_FLOOR 是否更新 1
                        DAO daocheck_s = new DAO();
                        for (int i = 0; i < 10; i++)
                        {
                            string sqlStr = $"select ELVT_START_FLOOR from t_subtask_status where guid = '{taskStatusGuid}'";
                            daocheck_s.AddExecuteItem(sqlStr,null);
                            var datacheck = daocheck_s.Query().Tables[0];
                            string elvt_start_floor = datacheck.Rows[0]["ELVT_START_FLOOR"].ToString();
                            log.Info($"elvtCallback Check elvt_start_floor : {elvt_start_floor}  tryCount : {i}");
                            if (elvt_start_floor != "1")
                            {
                                log.Error($"ELVT_START_FLOOR UPDATE FAIL");
                                try { Thread.Sleep(2000); } catch (Exception exception) { log.Error("Sleep exception", exception); }
                            }
                            else 
                            {
                                log.Info($"ELVT_START_FLOOR UPDATE SUCCESS");
                                update_success = true;
                                break;
                            }
                        }

                        break;
                    case "1":
                        //parm.Add("SUBTASK_FUNCTION", "ELVT_TARGET");
                        parm["SUBTASK_FUNCTION"] = "ELVT_TARGET";
                        log.Info($"elvtCallback case {taskSpaceStatus} ");
                        UpdateElvtParm(parm, "1");

                        //20230419 檢查 ELVT_TARGET_FLOOR 是否更新 1
                        DAO daocheck_t = new DAO();
                        for (int i = 0; i < 10; i++)
                        {
                            string sqlStr = $"select ELVT_TARGET_FLOOR from t_subtask_status where guid = '{taskStatusGuid}'";
                            daocheck_t.AddExecuteItem(sqlStr,null);
                            var datacheck = daocheck_t.Query().Tables[0];
                            string elvt_target_floor = datacheck.Rows[0]["ELVT_TARGET_FLOOR"].ToString();
                            log.Info($"elvtCallback Check elvt_target_floor : {elvt_target_floor}  tryCount : {i}");
                            if (elvt_target_floor != "1")
                            {
                                log.Error($"ELVT_TARGET_FLOOR UPDATE FAIL");
                                try { Thread.Sleep(2000); } catch (Exception exception) { log.Error("Sleep exception", exception); }
                            }
                            else
                            {
                                log.Info($"ELVT_TARGET_FLOOR UPDATE SUCCESS");
                                update_success = true;
                                break;
                            }
                        }

                        break;
                }
                if (update_success == true)
                {
                    result.Add("code", "0");
                    result.Add("message", "成功");
                    parm.Add("TYPE", "0");
                    parm.Add("RESPONSE_DATA", JsonConvert.SerializeObject(result));
                    log.Info($"elvtCallback result {result} ");
                    log.Info($"elvtCallback result parm {parm} ");
                    return result;
                }
                else 
                {
                    result.Add("code", "1");
                    result.Add("message", "Update Elvt Floor Fail");
                    parm.Add("TYPE", "0");
                    parm.Add("RESPONSE_DATA", JsonConvert.SerializeObject(result));
                    parm["SUBTASK_FUNCTION"] = "";
                    log.Info($"elvtCallback result {result} ");
                    log.Info($"elvtCallback result parm {parm} ");
                    return result;
                }

            }
            catch (Exception ex)
            {
                result.Add("code", "1");
                result.Add("message", $"錯誤參數 {ex}");
                parm.Add("TYPE", "0");
                parm.Add("RESPONSE_DATA", JsonConvert.SerializeObject(result));
                //parm.Add("SUBTASK_FUNCTION", "");
                parm["SUBTASK_FUNCTION"] = "";
                log.Error($"elvtCallback result error {result} ");
                log.Error($"elvtCallback result parm {parm} ");
                return result;
            }
            finally
            {
                DAO dao = new DAO();
                SubTask sqlCreator = new SubTask();
                //string HostUrl = $"{Request.RequestUri}{Request.Method}";
                parm.Add("SEND_DATA", JsonConvert.SerializeObject(callBack));
                var sqlParms = sqlCreator.CreateParameterAry(parm);
                var sqlStr = sqlCreator.InsertRCSAPI();
                dao.AddExecuteItem(sqlStr, sqlParms);
                dao.Execute();
            }

            

        }
        //
        public void UpdateElvtParm(JObject parm, string status)
        {
            try
            {
                log.Info($"elvtCallback UpdateElvtParm : parm = {parm} status = {status}");
                DAO dao = new DAO();
                SubTask sqlCreator = new SubTask();
                string sqlStr = string.Empty;
                if (status == "0")
                {
                   
                    parm.Add("ELVT_START_FLOOR", "1");
                    sqlStr = sqlCreator.UPDATE_SUBTASK_STATUS_ELVT_FLOOR("ELVT_START_FLOOR");
                    //parm.Add("ELVT_TARGET_FLOOR", "0");
                    log.Info($"elvtCallback START_floor  : parm = {parm} status = {status}");
                }
                else
                {
                    //parm.Add("ELVT_START_FLOOR", "0");
                    parm.Add("ELVT_TARGET_FLOOR", "1");
                    sqlStr = sqlCreator.UPDATE_SUBTASK_STATUS_ELVT_FLOOR("ELVT_TARGET_FLOOR");
                    log.Info($"elvtCallback TARGET_floor  : parm = {parm} status = {status}");
                }
                //Update 電梯資訊
                //parm.Add("ELVT_TASK_GUID", parm["ELVT_TASK_GUID"]);
                //parm.Add("ELVT_ID", parm["ELVT_ID"]);
                //parm.Add("ELVT_SPACE_ID", parm["ELVT_SPACE_ID"]);
                //parm.Add("ELVT_SPACE_INDEX", parm["ELVT_SPACE_INDEX"]);
                //parm.Add("ELVT_SPACE_QRCODE", parm["ELVT_SPACE_QRCODE"]);
                //parm.Add("ELVT_SPACE_STATUS_TROLLEY_ID", parm["ELVT_SPACE_STATUS_TROLLEY_ID"]);
                //parm.Add("ELVT_SPACE_STATUS_GUID", parm["ELVT_SPACE_STATUS_GUID"]);
                var sqlParms = sqlCreator.CreateParameterAry(parm);            
                dao.AddExecuteItem(sqlStr, sqlParms);
                //dao.Execute();
                //log.Info($"elvtCallback Update t_subtask_status ELVT_flag OK");
                var sqlStrTravel = sqlCreator.InsertTaskTravel();
                dao.AddExecuteItem(sqlStrTravel, sqlParms);
                dao.Execute();
                log.Info($"elvtCallback Update t_subtask_travel ELVT_flag OK");
            }
            catch (Exception ex)
            {
                log.Error($"elvtCallback Error {ex}");
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 更新電梯狀態舊版暫停使用
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="status"></param>
        public void UpdateElvtParm_OLD(JObject parm, string status)
        {
            try
            {
                DAO dao = new DAO();
                SubTask sqlCreator = new SubTask();
                var sqlParms = sqlCreator.CreateParameterAry(parm);
                var sqlStr = sqlCreator.GetTaskFunction_GUID();
                int i = 0;
                DataTable data;
                dao.AddExecuteItem(sqlStr, sqlParms);
                data = dao.Query().Tables[0];
                while (data.Rows.Count == 0 && i < 60)
                {
                    Thread.Sleep(1000);
                    dao.AddExecuteItem(sqlStr, sqlParms);
                    data = dao.Query().Tables[0];
                    i++;

                };

                if (data.Rows.Count == 0)
                {
                    throw new Exception("此GUID流程非eltvCallback");
                }
                JObject indexParm = new JObject();
                indexParm.Add("GUID", data.Rows[0]["GUID"].ToString());
                int PROGRESS = Convert.ToInt32(data.Rows[0]["PROGRESS"].ToString());
                indexParm.Add("PROGRESS", PROGRESS + 1);
                indexParm.Add("TASK_GUID", data.Rows[0]["TASK_GUID"].ToString());
                //Select SUBTASK_TYPE
                sqlParms = sqlCreator.CreateParameterAry(indexParm);
                sqlStr = sqlCreator.GetSubTaskType();
                dao.AddExecuteItem(sqlStr, sqlParms);
                data = dao.Query().Tables[0];
                if (data.Rows.Count == 0)
                {
                    //無下一個任務
                    sqlStr = sqlCreator.UpdateTaskMCSStatus();
                    dao.AddExecuteItem(sqlStr, sqlParms);
                    dao.Execute();
                    return;
                }
                indexParm.Add("SUBTASK_TYPE", data.Rows[0]["SUBTASK_TYPE"].ToString());
                //Update 電梯資訊
                indexParm.Add("ELVT_TASK_GUID", parm["ELVT_TASK_GUID"]);
                indexParm.Add("ELVT_ID", parm["ELVT_ID"]);
                indexParm.Add("ELVT_SPACE_ID", parm["ELVT_SPACE_ID"]);
                indexParm.Add("ELVT_SPACE_INDEX", parm["ELVT_SPACE_INDEX"]);
                indexParm.Add("ELVT_SPACE_QRCODE", parm["ELVT_SPACE_QRCODE"]);
                indexParm.Add("ELVT_SPACE_STATUS_TROLLEY_ID", parm["ELVT_SPACE_STATUS_TROLLEY_ID"]);
                indexParm.Add("ELVT_SPACE_STATUS_GUID", parm["ELVT_SPACE_STATUS_GUID"]);
                //Update INDEX, SUBTASK_TYPE
                sqlParms = sqlCreator.CreateParameterAry(indexParm);
                sqlStr = sqlCreator.UpdateTaskELVTInfo();
                dao.AddExecuteItem(sqlStr, sqlParms);
                dao.Execute();
                var sqlStrTravel = sqlCreator.InsertTaskTravel();
                dao.AddExecuteItem(sqlStrTravel, sqlParms);
                dao.Execute();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }

}