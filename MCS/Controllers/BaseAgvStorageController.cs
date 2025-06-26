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
    public class BaseAgvStorageController : Controller
    {

        // GET: Factory
        public ActionResult Index()
        {
            return View("~/Views/Base/AgvStorage.cshtml");
        }

    }
    public class ApiAgvStorageController : ApiController, IJqOneTable
    {
        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            AgvStorage sqlCreator = new AgvStorage();
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
        //取得機台總數量
        [System.Web.Http.HttpPost]
        public JObject GetEquipmentCount(JObject obj)
        {
            AgvStorage sqlCreator = new AgvStorage();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchEquipmentAllCount(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }
        //取得 機台未配對 數量
        [System.Web.Http.HttpPost]
        public JObject GetEquipmentLimitCount(JObject obj)
        {
            AgvStorage sqlCreator = new AgvStorage();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchEquipmentLimitCount(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }
        //取得儲位總數量
        [System.Web.Http.HttpPost]
        public JObject GetStorageCount(JObject obj)
        {
            AgvStorage sqlCreator = new AgvStorage();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchStorageAllCount(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }
        //取得 儲位未配對 數量
        [System.Web.Http.HttpPost]
        public JObject GetStorageLimitCount(JObject obj)
        {
            AgvStorage sqlCreator = new AgvStorage();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.SearchStorageLimitCount(obj);
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
            AgvStorage sqlCreator = new AgvStorage();
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
            AgvStorage sqlCreator = new AgvStorage();
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
            AgvStorage sqlCreator = new AgvStorage();
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
            AgvStorage sqlCreator = new AgvStorage();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            dynamic objData = obj as dynamic;
            returnMsg.result = dao.Execute();
            //先找資料庫有無此Equipment的NAME
            var sqlStrEquipment = sqlCreator.SearchEquipment(obj);
            var sqlSearchEquipmentParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrEquipment, sqlSearchEquipmentParms);
            var EquipmentTables = dao.Query().Tables[0];
            int count = EquipmentTables.Rows.Count;
            if (count == 0)
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "查無機台");
                return returnMsg;
            }

            var EquipmentGUID = EquipmentTables.Rows[0].ItemArray[0];
            objData.EQUIPMENT_GUID = EquipmentGUID;
            //////////////////////////////////////////////////////////////////
            /////先找資料庫有無此Storage的NAME
            var sqlStrStorage = sqlCreator.SearchStorage(obj);
            var sqlSearchStorageParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrStorage, sqlSearchStorageParms);
            var StorageTables = dao.Query().Tables[0];
            int Storagecount = StorageTables.Rows.Count;
            if (Storagecount == 0)
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "查無儲位");
                return returnMsg;
            }

            var StorageGUID = StorageTables.Rows[0].ItemArray[0];
            objData.STORAGE_GUID = StorageGUID;
            //以上機台跟儲位都有的話，insert 進資料庫
            var sqlParms = sqlCreator.CreateParameterAry(objData);
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
            AgvStorage sqlCreator = new AgvStorage();
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
            AgvStorage sqlCreator = new AgvStorage();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            dynamic objData = obj as dynamic;
            /////先找資料庫有無此Storage的NAME
            var sqlStrStorage = sqlCreator.SearchStorage(obj);
            var sqlSearchStorageParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrStorage, sqlSearchStorageParms);
            var StorageTables = dao.Query().Tables[0];
            int Storagecount = StorageTables.Rows.Count;
            if (Storagecount == 0)
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "查無儲位");
                return returnMsg;
            }

            var StorageGUID = StorageTables.Rows[0].ItemArray[0];
            objData.STORAGE_GUID = StorageGUID;

            var sqlStr = sqlCreator.Update();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }
        //GetDataListEquipment 取機台資料
        [System.Web.Http.HttpPost]
        public JObject GetDataList(JObject obj)
        {
            AgvStorage sqlCreator = new AgvStorage();
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
        //沒有被選取的equipment
        [System.Web.Http.HttpPost]
        public JObject GetDataListLimit(JObject obj)
        {
            AgvStorage sqlCreator = new AgvStorage();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            var sqlStr = sqlCreator.GetDataListLimit(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dynamic parm = obj as dynamic;

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);

            return returnVal;
        }
        //GetDataListStorage 取全部儲位資料
        [System.Web.Http.HttpPost]
        public JObject GetDataListStorage(JObject obj)
        {
            AgvStorage sqlCreator = new AgvStorage();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;
            var sqlStr = sqlCreator.GetDataListStorage(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);

            return returnVal;
        }
        //GetDataListStorage 沒有被選取的storage
        [System.Web.Http.HttpPost]
        public JObject GetDataListStorageLimit(JObject obj)
        {
            AgvStorage sqlCreator = new AgvStorage();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;
            var sqlStr = sqlCreator.GetDataListStorageLimit(obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);

            return returnVal;
        }
    }
}