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
    public class BaseMixedAreaController : Controller
    {

        // GET: Storage
        public ActionResult Index()
        {
            return View("~/Views/Base/MixedArea.cshtml");
        }

    }
    public class ApiMixedAreaController : ApiController, IJqOneTable
    {
        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            MixedArea sqlCreator = new MixedArea();
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
        //刪除混和儲位
        [System.Web.Http.HttpPost]
        public JObject Delete(JObject obj)
        {
            MixedArea sqlCreator = new MixedArea();
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
            MixedArea sqlCreator = new MixedArea();
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
            MixedArea sqlCreator = new MixedArea();
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
            throw new NotImplementedException();
        }
        [System.Web.Http.HttpPost]
        public JObject Query(JObject obj)
        {
            MixedArea sqlCreator = new MixedArea();
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
        //B表查詢區域
        public JObject QueryB(JObject obj)
        {
            MixedArea sqlCreator = new MixedArea();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;
            string ID = parm.ID;
            string NAME = parm.NAME;
            var sqlStrCount = $"select COUNT(*) `Count` from base_area where enable = 1 ";
            var sqlStr = $"select ID,NAME from base_area where enable = 1 ";
            if (string.IsNullOrEmpty(ID) == false)
            {
                sqlStr += $" and ID like '%{ID}%' ";
                sqlStrCount += $" and ID like '%{ID}%' ";
            }
            if (string.IsNullOrEmpty(NAME) == false)
            {
                sqlStr += $" and NAME like '%{NAME}%' ";
                sqlStrCount += $" and NAME like '%{NAME}%' ";
            }
            //var sqlParms = sqlCreator.CreateParameterAry(obj);
            //先計算Count
            dao.AddExecuteItem(sqlStrCount, null);
            var resultC = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = resultC["rows"][0].Value<string>("Count");
            //再查詢資料
            dao.AddExecuteItem(sqlStr,null);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = count;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }
        //新增、編輯混和儲位
        public JObject Update(JObject obj)
        {
            MixedArea sqlCreator = new MixedArea();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            //參數
            string ID = obj["ID"].ToString();
            string FOR_START_AREA = obj["FOR_START_AREA"].ToString();
            string FOR_TARGET_AREA = obj["FOR_TARGET_AREA"].ToString();


            //檢查有無混和儲位設定
            string sqlMix = $"select * FROM base_mixed_area where ORIGINAL_AREA = '{ID}' and Enable = 1";
            dao.AddExecuteItem(sqlMix,null);
            var dataMix = dao.Query().Tables[0];
            if (dataMix.Rows.Count == 0 && (!string.IsNullOrEmpty(FOR_START_AREA) || !string.IsNullOrEmpty(FOR_TARGET_AREA)))
            {
                //無混和儲位設定-->新增
                var sqlStrInsert = sqlCreator.Insert();
                var sqlParms = sqlCreator.CreateParameterAry(obj);
                dao.AddExecuteItem(sqlStrInsert, sqlParms);
            }
            else
            {
                //有混和儲位設定-->更新
                var sqlStrUpdate = sqlCreator.Update();
                var sqlParms = sqlCreator.CreateParameterAry(obj);
                dao.AddExecuteItem(sqlStrUpdate,sqlParms);
            }
            returnMsg.result = dao.Execute();

            return returnMsg;
        }
    }

}