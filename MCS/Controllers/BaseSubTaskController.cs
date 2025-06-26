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

namespace MCS.Controllers
{
    public class BaseSubTaskController : Controller 
    {
        
        // GET: Factory
        public ActionResult Index()
        {
            return View("~/Views/Task/SubTask.cshtml");
        }
                
    }
    public class ApiSubTaskController : ApiController, IJqOneTable
    {
        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

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
            BaseSubTask sqlCreator = new BaseSubTask();
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
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            EXCEL excel = new EXCEL();
            JObject returnMessage = new JObject();
            string controllerName = ControllerContext.RouteData.Values["controller"].ToString().Replace("Api", null);

            var sqlStr = sqlCreator.Search(obj, false);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);

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
            BaseSubTask sqlCreator = new BaseSubTask();
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
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic returnMsg = new JObject();
            returnMsg = CheckColumn(obj);
            if (returnMsg.result == false)
            {
                return returnMsg;
            }
            dynamic parm = obj as dynamic;

            //
            var sqlStrQuery = sqlCreator.Search(obj, false);
            var sqlParmsQuery = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrQuery, sqlParmsQuery);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, sqlParms);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString(); 
            var sqlStr = sqlCreator.Insert(uuid);
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
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            var sqlStr = sqlCreator.Search(obj, false);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }

        public JObject Update(JObject obj)
        {
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            returnMsg = CheckColumn(obj);
            if (returnMsg.result == false)
            {
                return returnMsg;
            }
            var sqlStr = sqlCreator.Update();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        /// <summary>
        /// 頁面載入時下拉選單取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetOption(JObject obj)
        {
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            var sqlStr = sqlCreator.GetOption(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            DataTableExtensions extensions = new DataTableExtensions();
            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            return returnVal;
        }
        /// <summary>
        /// 依照SUBTASK_TYPE不同，更新SERVER_FUNCTION下拉選項
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetServerFunctionOption(JObject obj)
        {
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            var sqlStr = sqlCreator.GetServerFunctionOption(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            DataTableExtensions extensions = new DataTableExtensions();
            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            return returnVal;
        }
        /// <summary>
        /// 取得IOT設備數量Count
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetIOTCount(JObject obj)
        {
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchIOTAllCount(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        /// <summary>
        /// 取得下位命令總數量
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetCmdCount(JObject obj)
        {
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchCmdAllCount(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        /// <summary>
        /// 取得下位命令
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetDataList(JObject obj)
        {
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            var sqlStr = sqlCreator.GetDataList(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        /// <summary>
        /// 取得下位命令
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetIOTList(JObject obj)
        {
            BaseSubTask sqlCreator = new BaseSubTask();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            var sqlStr = sqlCreator.GetIOTList(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        public dynamic CheckColumn(JObject obj) 
        {
            dynamic ObjData = obj as dynamic;
            dynamic returnMsg = new JObject();
            returnMsg.result = true;


            if (ObjData.SUBTASK_ID == "") 
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "子任務編號未輸入");
                return returnMsg;
            }
            if (ObjData.SUBTASK_TYPE == "") 
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "子任務類型未輸入");
                return returnMsg;
            }
            if (ObjData.SERVER_FUNCTION == "")
            {
                if (ObjData.SUBTASK_TYPE != "5" && ObjData.SUBTASK_TYPE != "6") //Booking 和 Holding不用輸入
                {
                    returnMsg.result = false;
                    returnMsg.Add("msg", "api類型/SN_KEY 未輸入");
                    return returnMsg;
                }
            }
            if (ObjData.SUBTASK_FUNCTION == "")
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "命令/method 未輸入");
                return returnMsg;
            }
            if (obj["SERVER_FUNCTION"].ToString().Contains("genAgvSchedulingTask"))
            {
                if (ObjData.TASK_TYPE == "")
                {
                    returnMsg.result = false;
                    returnMsg.Add("msg", "請輸入RCS模板");
                    return returnMsg;
                }
            }
            return returnMsg;

        }
    }
}