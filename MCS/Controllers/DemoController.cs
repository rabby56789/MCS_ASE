using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JQWEB.Models;
using JQWEB.Controllers;
using Newtonsoft.Json.Linq;
using MCS.Models;
using Newtonsoft.Json;
using System.IO;

namespace MCS.Controllers
{
    //控制器範本
    public class DemoController : Controller
    {
        //回傳單表或關聯表View
        public ActionResult Index()
        {
            return View("~/Views/Demo_SingleTable.cshtml"); 
            //return View("~/Views/Demo_BindTable.cshtml");
        }
    }

    //範本API--可選擇繼承介面IJqOneTable(單表UI),IJqBindTable(關聯表UI)
    public class ApiDemoController : ApiController, IJqOneTable,IJqBindTable
    {
        //實例通用BD存取物件DAO與資料轉換工具DataTableExtensions
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();

        public JObject Query(JObject obj)
        {
            //Demo sqlCreator = new Demo();
            //dynamic parm = obj as dynamic;

            //var sqlStr = sqlCreator.GetSqlStr("Query",obj);
            //var sqlParms = sqlCreator.CreateParameterAry(obj);

            //dao.AddExecuteItem(sqlStr, sqlParms);

            //var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            //returnVal["total"] = parm.total;
            //returnVal.Add("order", parm.order);
            //returnVal.Add("page", parm.page);
            //returnVal.Add("sort", parm.sort);
            JObject o1 = JObject.Parse(File.ReadAllText(HttpContext.Current.Server.MapPath("/Views/Tablet/tasklist1.json")));
            return o1;
        }

        public JObject Query2(JObject obj)
        {
            JObject o2 = JObject.Parse(File.ReadAllText(HttpContext.Current.Server.MapPath("/Views/Tablet/tasklist2.json")));
            return o2;
        }
        public JObject Query3(JObject obj)
        {
            JObject o3 = JObject.Parse(File.ReadAllText(HttpContext.Current.Server.MapPath("/Views/Tablet/tasklist3.json")));
            return o3;
        }
        public JObject Taskbuild(JObject obj)
        {
            JObject o4 = JObject.Parse(File.ReadAllText(HttpContext.Current.Server.MapPath("/Views/Tablet/taskbuild.json")));
            return o4;
        }
        public JObject Taskcookies(JObject obj)
        {
            JObject o5 = JObject.Parse(File.ReadAllText(HttpContext.Current.Server.MapPath("/Views/Tablet/taskcookies.json")));
            return o5;
        }

        /// <summary>
        /// 查詢A表資料筆數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Count(JObject obj)
        {
            Demo sqlCreator = new Demo();

            var sqlStr = sqlCreator.GetSqlStr("Count", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        /// <summary>
        /// 按下編輯時取得單筆資料內容
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject GetOneByGUID(JObject obj)
        {
            Demo sqlCreator = new Demo();

            var sqlStr = sqlCreator.GetSqlStr("GetOneByGUID", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        /// <summary>
        /// 按下主表新增按鈕
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Insert(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按下主表編輯畫面確認按鈕
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Update(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按下主表刪除確認按鈕
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Delete(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按下主表某筆資料後,觸發查詢關聯
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject QueryBind(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按下主表某筆資料後,觸發查詢關聯資料筆數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject CountBind(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按下關聯表新增按鈕後彈出關聯選擇視窗,選擇關聯後按下確定
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject InsertBind(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 關聯表刪除關聯
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject DeleteBind(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 主表匯入
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Import(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 關聯表匯入
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject ImportBind(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 主表匯出
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Export(JObject obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 關聯表匯出
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject ExportBind(JObject obj)
        {
            throw new NotImplementedException();
        }
    }
}