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
    public class BaseAirshowerSpaceController : Controller
    {

        // GET: Factory
        public ActionResult Index()
        {
            return View("~/Views/Base/AirshowerSpace.cshtml");
        }

    }

    public class ApiAirshowerSpaceController : ApiController, IJqOneTable
    {
        [System.Web.Http.HttpPost]
        public JObject Query(JObject obj)
        {
            AirshowerSpace sqlCreator = new AirshowerSpace();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            //先查Airshower GUID
            var sqlStrAirshower = sqlCreator.SearchAirshower(obj);
            dao.AddExecuteItem(sqlStrAirshower, sqlParms);
            var airshowerTables = dao.Query().Tables[0];
            if (airshowerTables.Rows.Count > 0)
            {
                parm.AIRSHOWER_GUID = airshowerTables.Rows[0]["GUID"].ToString();
            }

            sqlParms = sqlCreator.CreateParameterAry(parm);
            var sqlStr = sqlCreator.Search(parm, false);
            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }

        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            AirshowerSpace sqlCreator = new AirshowerSpace();
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
            AirshowerSpace sqlCreator = new AirshowerSpace();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.Delete();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        [System.Web.Http.HttpPost]
        public JObject GetOneByGUID(JObject obj)
        {
            AirshowerSpace sqlCreator = new AirshowerSpace();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetOneByGUID();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        public JObject Insert(JObject obj)
        {
            AirshowerSpace sqlCreator = new AirshowerSpace();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic returnMsg = new JObject();
            dynamic parm = obj as dynamic;

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

        public JObject Update(JObject obj)
        {
            AirshowerSpace sqlCreator = new AirshowerSpace();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.Update();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        public JObject Import(JObject obj)
        {
            throw new NotImplementedException();
        }

        [System.Web.Http.HttpPost]
        public JObject Export(JObject obj)
        {
            AirshowerSpace sqlCreator = new AirshowerSpace();
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

        /// <summary>
        /// 取風淋門資料
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetDataListAirshower(JObject obj)
        {
            AirshowerSpace sqlCreator = new AirshowerSpace();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;
            var sqlStr = sqlCreator.GetAirshowerList(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);

            return returnVal;
        }

        /// <summary>
        /// 取得風淋門總數量
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetAirshowerCount(JObject obj)
        {
            AirshowerSpace sqlCreator = new AirshowerSpace();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchAirshowerCount(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }
    }
}