using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;

namespace MCS.Controllers
{
    public class elvtNotifyServiceController : ApiController
    {
        [System.Web.Http.HttpPost]
        public JObject elvtNotify(JObject notify)   // 
        {
            /// elvt queue 收到樓層任務的完成通知("taskSpaceStatus": "1")，即更新 電梯任務狀態: [t_elvttask_status].TASKSTATUS = 3.貨架已移入 
            ///                                                                                                                                        以及 實際移入 的電梯儲位與貨架編號 ( 1個貨架, 1個JObject): [t_elvt_status].XXXX
            /// elvt queue 收到樓層任務的完成通知("taskSpaceStatus": "2")，即更新 電梯任務狀態: [t_elvttask_status].TASKSTATUS = 5.貨架已移出 
            ///                                                                                                                                        以及 實際移出 的電梯儲位與貨架編號 ( 1個貨架, 1個JObject): [t_elvt_status].XXXX
            /// notify: 
            //{
            //  "code": null,
            //  "taskStatusGuid": "3672aa51-c356-11ec-b24e-0800274ef241",
            //  "elvtTaskStatusGuid": "c6c40f89-fa14-11ec-b24e-0800274ef241",
            //  "taskSpaceStatus": "1",
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

            //notify["code"].ToString();

            JObject result = new JObject();

            try
            {
                var taskSpaceStatus = notify["taskSpaceStatus"].ToString();
                if (string.IsNullOrEmpty(taskSpaceStatus) || (taskSpaceStatus != "1" && taskSpaceStatus != "2"))
                {
                    if (string.IsNullOrEmpty(taskSpaceStatus))
                    { throw new Exception("no 'taskSpaceStatus' data."); }
                    else { throw new Exception("'taskSpaceStatus' data is error: " + taskSpaceStatus); }
                }
                var elvtTaskStatusGuid = notify["elvtTaskStatusGuid"].ToString();
                if (string.IsNullOrEmpty(elvtTaskStatusGuid))
                { throw new Exception("no 'elvtTaskStatusGuid' data."); }


                // Update [t_elvttask_status]  => 更新 Task: TASKSTATUS = 3.貨架已移入
                JObject msg = new JObject();
                msg.Add("ELVTTASK_STATUS_GUID", elvtTaskStatusGuid);

                if (taskSpaceStatus == "1")
                {
                    msg["TASKSTATUS"] = "3";
                }
                else if (taskSpaceStatus == "2")
                {
                    msg["TASKSTATUS"] = "5";
                }
                UpdateElvtTaskStatus(msg);

                // Update [t_elvt_space_status]  => 更新 電梯儲格狀態: 
                // .. 

            }
            catch (Exception ex)
            {
                result.Add("code", "1");
                result.Add("message", ex.Message);
                return result;
            }

            result.Add("code", "0");
            result.Add("message", "Success");
            return result;

        }

        public JObject UpdateElvtTaskStatus(JObject obj)
        {
            dynamic returnMsg = new JObject();

            DAO dao = new DAO();
            ElevatorStatus sqlCreator = new ElevatorStatus();

            var sqlStr = sqlCreator.UpdateElvtTaskStatus();

            obj.Add("GUID", obj["ELVTTASK_STATUS_GUID"].ToString());
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);
            returnMsg.result = dao.Execute();

            return returnMsg;
        }

    }

}