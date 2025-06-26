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
    public class elvtGenSchedulingTaskServiceController : ApiController
    {
        [System.Web.Http.HttpPost]
        public JObject elvtGenSchedulingTask(JObject genSchedulingTask)
        {

            /// genSchedulingTask: 
            //{
            //  "ElvtId": "Elevator1",
            //  "TaskStatusGuid": "00000000-0000-0000-0000-000000000000",
            //  "Remark": "api-test",
            //  "TaskQty": "3",
            //  "TargetFloor": "2",
            //  "StartFloor": "4",
            //  "CarrierId": "",
            //  "TrolleyId": "T9103,T9001,T9002",
            //  "Weighting": "5",
            //  "TaskName": "4F到2F任務",
            //  "SubtaskName": "呼叫電梯到4樓"
            //}

            JObject result = new JObject();

            try
            {
                var elvtId = genSchedulingTask["ElvtId"].ToString();
                var taskStatusGuid = genSchedulingTask["TaskStatusGuid"].ToString();
                var remark = genSchedulingTask["Remark"].ToString();
                var taskQty = genSchedulingTask["TaskQty"].ToString();
                var targetFloor = genSchedulingTask["TargetFloor"].ToString();
                var startFloor = genSchedulingTask["StartFloor"].ToString();
                var carrierId = genSchedulingTask["CarrierId"].ToString();
                var trolleyId = genSchedulingTask["TrolleyId"].ToString();
                var weighting = genSchedulingTask["Weighting"].ToString();
                var taskName = genSchedulingTask["TaskName"].ToString();
                var subtaskName = genSchedulingTask["SubtaskName"].ToString();
                var taskStatus = "0";


                string start = startFloor;
                string target = targetFloor;                
                if (start.IndexOf("B") == 0)
                { start = "-" + start.Substring(1); }
                start = start.Replace("A", "").Replace("B", "");
                if (target.IndexOf("B") == 0)
                { target = "-" + target.Substring(1); }
                target = target.Replace("A", "").Replace("B", "");
                

                var direction = Convert.ToInt32(target) - Convert.ToInt32(start) >= 0 ? "U" : "D";


                // Insert [t_elvttask_status]  => 新增電梯任務
                JObject msg = new JObject();
                msg.Add("ELVT_ID", elvtId);
                msg.Add("TASK_STATUS_GUID", taskStatusGuid);
                msg.Add("REMARK", remark);
                msg.Add("TASK_QTY", taskQty);
                msg.Add("TARGET_FLOOR", targetFloor);
                msg.Add("START_FLOOR", startFloor);
                msg.Add("CARRIER_ID", carrierId);
                msg.Add("TROLLEY_ID", trolleyId);
                msg.Add("WEIGHTING", weighting);
                msg.Add("TASK_NAME", taskName);
                msg.Add("SUBTASK_NAME", subtaskName);
                msg.Add("DIRECTION", direction);
                msg.Add("TASKSTATUS", taskStatus);

                InsertElvtTaskStatus(msg);
            }
            catch (Exception ex)
            {
                result.Add("code", "1");
                result.Add("message", "錯誤參數");
                return result;
            }

            result.Add("code", "0");
            result.Add("message", "成功");
            return result;

        }

        public JObject InsertElvtTaskStatus(JObject obj)
        {
            dynamic returnMsg = new JObject();

            DAO dao = new DAO();
            ElevatorStatus sqlCreator = new ElevatorStatus();

            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, null);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();

            var sqlStr = sqlCreator.InsertElvtTaskStatus(uuid);

            obj.Add("SUBTASK_STATUS_GUID", obj["TASK_STATUS_GUID"].ToString());
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);
            returnMsg.result = dao.Execute();
            if (returnMsg.result == true)
            {
                returnMsg.guid = uuid;
            }

            return returnMsg;
        }

    }
    
}