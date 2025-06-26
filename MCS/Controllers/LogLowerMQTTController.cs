using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MCS.Controllers
{
    public class LogLowerMQTTController : Controller
    {
        // GET: LogLowerMQTT
        public ActionResult Index()
        {
            return View("~/Views/Log/LogMqtt.cshtml");
        }
    }

    public class ApiLogLowerMQTTController : ApiController
    {
        LogMqtt sqlCreator = new LogMqtt();
        public JObject QueryMQTTLog(JObject obj)
        {
            DAO dao = new DAO();
            JObject returnVal = new JObject();
            DataTableExtensions extensions = new DataTableExtensions();
            string sqlMQTTLog = string.Empty;

            //1.搜尋log_lower_mqtt內包含該任務GUID和電梯任務GUID，再用SEND_TIME排序
            sqlMQTTLog = sqlCreator.GetSqlStr("QueryLog");
            var parms = sqlCreator.CreateParameterAry(obj);
            //2.找該筆TASK 是否有電梯任務,並記錄該電梯任務GUID
            string sqlElvtTask = sqlCreator.GetSqlStr("ElvtTask");
            dao.AddExecuteItem(sqlElvtTask,parms);
            var data = dao.Query().Tables[0];
            if (data.Rows.Count > 0)
            {
                foreach (DataRow row in data.Rows)
                {
                    sqlMQTTLog += $" or TASK_GUID = '{row["GUID"]}' ";//增加電梯任務GUID判斷
                }
            }
            sqlMQTTLog += "Order by SEND_TIME ";

            dao.AddExecuteItem(sqlMQTTLog,parms);
            
            //dao.AddExecuteItem(sqlTSubtaskStatus,null);
            returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            //returnVal["total"] = count;
            return returnVal;
        }
        public JObject Query(JObject obj)
        {
            JObject returnVal = new JObject();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();


            //1.Get Count
            var sqlStr = sqlCreator.GetSqlStr("COUNT", obj);
            var parms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, parms);

            string count = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();

            //2.Get Date
            sqlStr = sqlCreator.GetSqlStr("QUERY", obj);
            parms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, parms);

            returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            //3.Update Response Count Data
            returnVal["total"] = count;

            return returnVal;
        }
    }
}