using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MCS.Controllers
{
    public class BaseAgvController : Controller 
    {
        
        // GET: Factory
        public ActionResult Index()
        {
            return View("~/Views/Base/Agv.cshtml");
        }
                
    }
    public class ApiAgvController : ApiController, IJqOneTable
    {
        [System.Web.Http.HttpPost]
        public JObject Query(JObject obj)
        {
            Agv sqlCreator = new Agv();
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

        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            Agv sqlCreator = new Agv();
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
            Agv sqlCreator = new Agv();
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
            Agv sqlCreator = new Agv();
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
            Agv sqlCreator = new Agv();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic returnMsg = new JObject();
            dynamic parm = obj as dynamic;

            var sqlStrQuery = sqlCreator.Search(obj, false);
            var sqlParmsQuery = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrQuery, sqlParmsQuery);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            //新guid
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, sqlParms);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString(); 

            //新增
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
            Agv sqlCreator = new Agv();
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
            throw new NotImplementedException();
        }

        //取群組資料
        [System.Web.Http.HttpPost]
        public JObject GetDataList(JObject obj)
        {
            Agv sqlCreator = new Agv();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            var sqlStr = sqlCreator.GetDataList(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }

        //取得群組總數量
        [System.Web.Http.HttpPost]
        public JObject GetGroupIdCount(JObject obj)
        {
            Agv sqlCreator = new Agv();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchGroupAllCount(obj);
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