using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;
using System.Web.Mvc;

namespace MCS.Controllers
{
    public class StorageStatusController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Task/StorageStatus.cshtml");
        }
    }
    public class apiStorageStatusController : ApiController, IJqOneTable
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            StorageStatus sqlCreator = new StorageStatus();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            //先撈取任務數量
            var sqlStrCount = sqlCreator.Search(obj,true);
            dao.AddExecuteItem(sqlStrCount, sqlParms);
            var total = dao.Query().Tables[0].Rows[0][0].ToString();
            //再找任務
            var sqlStr = sqlCreator.Search(obj, false);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = total;
            //returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }
        public JObject Update(JObject obj)
        {
            throw new NotImplementedException();
        }

        public JObject execute(JObject obj)
        {
            StorageStatus sqlCreator = new StorageStatus();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            log.Info($"Unlock : {obj}");
            if ("1" == obj["isLock"].ToString())
            {
                throw new Exception("Not supported to lock with web UI");
            }
            else
            {
                string _guid = obj["GUID"].ToString();
                string _sqlString = $@"delete from mcs.t_storage_status where GUID = '{_guid}';";
                dao.AddExecuteItem(_sqlString, null);
                log.Info($"Unlock sql : {_sqlString}");
                returnMsg.result = dao.Execute();
                return returnMsg;
            }

            //var sqlStr = sqlCreator.executeLock();
            //var sqlParms = sqlCreator.CreateParameterAry(obj);

            //dao.AddExecuteItem(sqlStr, sqlParms);

            //returnMsg.result = dao.Execute();

            //return returnMsg;
        }

        /// <summary>
        /// 頁面載入時下拉選單取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetOption(JObject obj)
        {
            StorageStatus sqlCreator = new StorageStatus();
            DAO dao = new DAO();
            var sqlStr = sqlCreator.GetOption();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            DataTableExtensions extensions = new DataTableExtensions();
            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            return returnVal;
        }
    }
}