using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MCS.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class LogLowerRCSController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Log/LogLowerRCS.cshtml");
        }
    }

    public class ApiLogLowerRCSController : ApiController
    {
        public JObject Export(JObject obj)
        {
            throw new NotImplementedException();
        }

        public JObject Query(JObject obj)
        {
            JObject result = new JObject();
            LogRCS sqlCreator = new LogRCS();
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

            result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            //3.Update Response Count Data
            result["total"] = count;

            return result;
        }
    }
}