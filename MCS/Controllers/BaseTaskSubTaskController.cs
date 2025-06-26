using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace MCS.Controllers
{
    public class BaseTaskSubTaskController : Controller
    {

        // GET: Factory
        public ActionResult Index()
        {
            return View("~/Views/Task/TaskSubTask.cshtml");
        }

    }
    public class ApiTaskSubTaskController : ApiController, IJqOneTable
    {
        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            //輸入任務名稱才可查詢
            dynamic ObjectData = obj as dynamic;
            dynamic returnMsg = new JObject();
            returnMsg.result = true;
            if (ObjectData.TASK_NAME == "") 
            {
                returnMsg.Add("msg", "請輸入任務名稱再做查詢");
                returnMsg.result = false;
                return returnMsg;
            }
            var sqlStr = sqlCreator.Search(obj, true);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }
        
        [System.Web.Http.HttpPost]
        public JObject Delete(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.Delete();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        [System.Web.Http.HttpPost]
        public JObject Export(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            EXCEL excel = new EXCEL();
            JObject returnMessage = new JObject();
            string controllerName = ControllerContext.RouteData.Values["controller"].ToString().Replace("Api", null);

            dynamic parm = obj as dynamic;
            dynamic objData = obj as dynamic;
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            //先查TASK GUID
            var sqlStrTask = sqlCreator.SearchTask(obj);
            dao.AddExecuteItem(sqlStrTask, sqlParms);
            var taskTables = dao.Query().Tables[0];
            string taskGuid = "", subTaskGuid = "";
            if (taskTables.Rows.Count > 0)
            {
                taskGuid = taskTables.Rows[0]["GUID"].ToString();
            }

            //再查SUBTASK GUID
            var sqlStrSubTask = sqlCreator.SearchSubTask(obj);
            dao.AddExecuteItem(sqlStrSubTask, sqlParms);
            taskTables = dao.Query().Tables[0];
            if (taskTables.Rows.Count > 0)
            {
                subTaskGuid = taskTables.Rows[0]["GUID"].ToString();
            }

            objData.TASK_GUID = taskGuid;
            objData.SUBTASK_GUID = subTaskGuid;

            var sqlStr = sqlCreator.Search(obj, false);
            var sqlParmsWithGuid = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParmsWithGuid);

            var data = dao.Query().Tables[0];

            //檔案建立與命名
            string filename = controllerName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("/Files/temp/")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("/Files/temp/"));
            }
            string path = Path.Combine(HttpContext.Current.Server.MapPath("/Files/temp/"), filename + ".xlsx");
            //int count = 
            excel.DataTableToExcel(data, controllerName, false, true, path);

            returnMessage.Add("filePath", "/Files/temp/" + filename + ".xlsx");

            return JObject.FromObject(returnMessage);
        }
        [System.Web.Http.HttpPost]
        public JObject GetOneByGUID(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetOneByGUID();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        public JObject Import(JObject obj)
        {
            throw new NotImplementedException();
        }
        public JObject Insert(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            returnMsg = CheckColumn(obj);
            //欄位檢查
            if (returnMsg.result == false) 
            {
                return returnMsg;
            }

            //dynamic objData = obj as dynamic;
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, sqlParms);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();

            var sqlStr = sqlCreator.Insert(uuid,obj);
            dao.AddExecuteItem(sqlStr, sqlParms);
            returnMsg.result = dao.Execute();
            if (returnMsg.result == true)
            {
                returnMsg.guid = uuid;
            }

            return returnMsg;
        }

        [System.Web.Http.HttpPost]
        public JObject Query(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;
            dynamic objData = obj as dynamic;
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            //先查TASK GUID
            var sqlStrTask = sqlCreator.SearchTask(obj);
            dao.AddExecuteItem(sqlStrTask, sqlParms);
            var taskTables = dao.Query().Tables[0];
            string taskGuid="", subTaskGuid="";
            if (taskTables.Rows.Count > 0)
            {
                taskGuid = taskTables.Rows[0]["GUID"].ToString();
            }
            
            //再查SUBTASK GUID
            var sqlStrSubTask = sqlCreator.SearchSubTask(obj);
            dao.AddExecuteItem(sqlStrSubTask, sqlParms);
            taskTables = dao.Query().Tables[0];
            if (taskTables.Rows.Count > 0)
            {
                subTaskGuid = taskTables.Rows[0]["GUID"].ToString();
            }

            objData.TASK_GUID = taskGuid;
            objData.SUBTASK_GUID = subTaskGuid;

            var sqlParmsWithGuid = sqlCreator.CreateParameterAry(objData);
            var sqlStr = sqlCreator.Search(objData, false);
            
            dao.AddExecuteItem(sqlStr, sqlParmsWithGuid);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }

        public JObject Update(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            dynamic objData = obj as dynamic;
            returnMsg = CheckColumn(obj);
            //欄位檢查
            if (returnMsg.result == false)
            {
                return returnMsg;
            }
            //var sqlParms = sqlCreator.CreateParameterAry(obj);            
            var sqlStr = sqlCreator.Update(obj);
            var sqlParmsWithGuid = sqlCreator.CreateParameterAry(objData);

            dao.AddExecuteItem(sqlStr, sqlParmsWithGuid);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        /// <summary>
        /// 取任務資料
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetDataListTask(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            //先抓count 頭
            var sqlStr = sqlCreator.TaskCount(obj);

            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            //先抓count 尾
            
            sqlStr = sqlCreator.GetTaskList(obj);
            sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            returnVal["total"] = count;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }

        /// <summary>
        /// 取得任務總數量
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetTaskCount(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchTaskCount(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        /// <summary>
        /// 取子任務資料
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetDataListSubTask(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            //先抓count 頭
            var sqlStr = sqlCreator.SubTaskCount(obj);

            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            //先抓count 尾


            sqlStr = sqlCreator.GetSubTaskList(obj);
            sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            returnVal["total"] = count;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);

            return returnVal;
        }

        /// <summary>
        /// 取得子任務總數量
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetSubTaskCount(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchSubTaskCount(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        /// <summary>
        /// 取AGV群組資料
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetDataListAgvGroup(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            //先抓count 頭
            var sqlStr = sqlCreator.AgvGroupListCount(obj);

            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            //先抓count 尾

            sqlStr = sqlCreator.GetAgvGroupList(obj);
            sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            returnVal["total"] = count;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);

            return returnVal;
        }

        /// <summary>
        /// 取得AGV群組總數量
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetAgvGroupCount(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchAgvGroupCount(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        /// <summary>
        /// 頁面載入時下拉選單取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetOption(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            var sqlStr = sqlCreator.GetOption();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            DataTableExtensions extensions = new DataTableExtensions();
            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            return returnVal;
        }

        public dynamic CheckColumn(JObject obj) 
        {
            dynamic returnMsg = new JObject();
            dynamic objData = obj as dynamic;
            returnMsg.result = true;
            //判斷必填欄位是否為空
            if (objData.TASK_GUID == "" || objData.SUBTASK_GUID == "" || objData.PROGRESS == "") 
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "輸入資料不完整");
                return returnMsg;
            }
            bool isNumber_PROGRESS = Regex.IsMatch(Convert.ToString(objData.PROGRESS), @"^[0-9]+$");
            float intPROGRESS = objData.PROGRESS;
            if (!isNumber_PROGRESS || intPROGRESS > 127) 
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "輸入的順序不符規格");
                return returnMsg;
            }
            return returnMsg;
        }
        //任務流程Processs Update
        public void ProcessUpdate(JObject obj)
        {
            TaskSubTask sqlCreator = new TaskSubTask();
            DAO dao = new DAO();
            dynamic objData = obj as dynamic;
            //取得TASK_GUID
            string taskguid = objData["TASK_GUID"];
            //取得該任務子任務順序
            string sqlStr = $@"select * from base_task_subtask where TASK_GUID = '{taskguid}' and enable = 1 ";
            //當舊的progress比新增的progress後面
            if (objData["old_progress"] > objData["PROGRESS"])
            {
                sqlStr += "order by PROGRESS,UPDATE_TIME desc";
            }
            else 
            {
                sqlStr += "order by PROGRESS,UPDATE_TIME";
            }
            
            dao.AddExecuteItem(sqlStr, null);
            var data = dao.Query().Tables[0];
            int subCount = data.Rows.Count;//子任務筆數

            string UpdateProgress = string.Empty;
            //有子任務才做排序
            if (subCount > 0) 
            {
                for (int i = 0; i < subCount; i++)
                {
                    UpdateProgress += $@"update base_task_subtask set progress = {i + 1} where enable = 1 and GUID = '{data.Rows[i]["GUID"].ToString()}' ;";
                }
                dao.AddExecuteItem(UpdateProgress, null);
                dao.Execute();
            }
            
            
            //dynamic returnMsg = new JObject();
            
            //returnMsg = CheckColumn(obj);
            ////欄位檢查
            //if (returnMsg.result == false)
            //{
            //    return returnMsg;
            //}
            //var sqlParms = sqlCreator.CreateParameterAry(obj);            
            //var sqlStr = sqlCreator.Update(obj);
            //var sqlParmsWithGuid = sqlCreator.CreateParameterAry(objData);

            //dao.AddExecuteItem(sqlStr, sqlParmsWithGuid);

            //returnMsg.result = dao.Execute();

            //return returnMsg;
        }
    }
}