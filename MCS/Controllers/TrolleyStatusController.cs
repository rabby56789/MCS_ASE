using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;
using System.Web.Mvc;

namespace MCS.Controllers
{
    public class TrolleyStatusController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Task/TrolleyStatus.cshtml");
        }
    }
    public class apiTrolleyStatusController : ApiController, IJqOneTable
    {
        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            StorageStatus sqlCreator = new StorageStatus();
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
            throw new NotImplementedException();
        }

        public JObject Export(JObject obj)
        {
            throw new NotImplementedException();
        }

        [System.Web.Http.HttpPost]
        public JObject GetOneByGUID(JObject obj)
        {
            throw new NotImplementedException();
        }

        public JObject Import(JObject obj)
        {
            throw new NotImplementedException();
        }

        public JObject Insert(JObject obj)
        {
            throw new NotImplementedException();
        }

        [System.Web.Http.HttpPost]
        public JObject Query(JObject obj)
        {
            TrolleyStatus sqlCreator = new TrolleyStatus();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            //先撈取任務的數量
            var sqlStrCount = sqlCreator.Search(obj, true);
            var sqlParmsCount = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrCount, sqlParmsCount);
            var total = dao.Query().Tables[0].Rows[0][0].ToString();

            var sqlStr = sqlCreator.Search(obj, false);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }
        [System.Web.Http.HttpPost]        
        public JObject Update(JObject obj)
        {
            throw new NotImplementedException();
        }
    }
}