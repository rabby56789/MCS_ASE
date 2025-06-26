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
using System.Xml;

namespace MCS.Controllers
{
    public class LogAlarmController : Controller
    {
        // GET: LogAlarm
        public ActionResult Index()
        {
            return View("~/Views/Log/LogAlarm.cshtml");
        }
    }
    public class ApiLogAlarmController : ApiController, IJqOneTable 
    { 
        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            Alarm sqlCreator = new Alarm();
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

        
        public JObject Query(JObject obj)
        {
            Alarm sqlCreator = new Alarm();
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

        public JObject Insert(JObject obj)
        {
            return obj;
        }
        public JObject Update(JObject obj)
        {
            return obj;
        }
        public JObject Delete(JObject obj)
        {
            return obj;
        }

        public JObject Import(JObject obj)
        {
            return obj;
        }

        public JObject Export(JObject obj)
        {
            return obj;
        }

        public JObject GetOneByGUID(JObject obj)
        {
            return obj;
        }
    }
}