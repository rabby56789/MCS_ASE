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
using System.Text.RegularExpressions;

namespace MCS.Controllers
{
    //控制器範本
    public class BaseIotdeviceController : Controller
    {
        //回傳單表或關聯表View
        public ActionResult Index()
        {
            return View("~/Views/Base/Iotdevice.cshtml"); 
        }
    }

    //範本API--可選擇繼承介面IJqOneTable(單表UI),IJqBindTable(關聯表UI)
    public class ApiIotdeviceController : ApiController, IJqOneTable,IJqBindTable
    {
        //實例通用BD存取物件DAO與資料轉換工具DataTableExtensions
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();

        public JObject QueryA(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            dynamic parm = obj as dynamic;

            var sqlStr = sqlCreator.GetSqlStr("QueryA", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);

            return returnVal;
        }

        public JObject QueryIOT_STATUS(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            dynamic parm = obj as dynamic;

            //查尋筆數
            var sqlStrCount = sqlCreator.SearchIOT_STATUS(obj,true);
            var sqlParmsCount = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStrCount, sqlParmsCount);

            var count = Convert.ToInt32(dao.Query().Tables[0].Rows[0][0].ToString());

            //查詢資料
            var sqlStr = sqlCreator.SearchIOT_STATUS(obj, false);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var data = dao.Query().Tables[0];
            var returnVal = extensions.ConvertDataTableToJObject(data);

            returnVal["total"] = count;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);

            return returnVal;
        }

        public JObject Query(JObject obj)
        {
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

        public JObject Count(JObject obj)
        {
            return null;
        }


        /// <summary>
        /// 查詢A表資料筆數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject CountA(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();

            var sqlStr = sqlCreator.GetSqlStr("CountA", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        public JObject GetOneByGUID(JObject obj)
        {
            return null;
        }
        public JObject Insert(JObject obj)
        {
            return null;
        }
        public JObject Update(JObject obj)
        {
            return null;
        }
        public JObject Delete(JObject obj)
        {
            return null;
        }


        /// <summary>
        /// 按下編輯時取得單筆資料內容
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetAOneByGUID(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            var sqlStr = sqlCreator.GetSqlStr("GetAOneByGUID", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        
        [System.Web.Http.HttpPost]
        public JObject GetBOneByGUID(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            var sqlStr = sqlCreator.GetSqlStr("GetBOneByGUID", obj);
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
        public JObject InsertA(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();            
            //returnMsg = CheckColumnA(obj);
            //判斷是否為輸入空值
            //if (returnMsg.result == false)
            //{
            //    return returnMsg;
            //}
         
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, sqlParms);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();


            var sqlStr = sqlCreator.InsertA(uuid);
            dao.AddExecuteItem(sqlStr, sqlParms);
            returnMsg.result = dao.Execute();
            if (returnMsg.result == true)
            {
                returnMsg.guid = uuid;
            }
            return returnMsg;
        }


        
        /// <summary>
        /// 按下主表編輯畫面確認按鈕
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject UpdateA(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            //returnMsg = CheckColumnA(obj);
            //判斷是否為輸入空值
            //if (returnMsg.result == false)
            //{
            //    return returnMsg;
            //}

            var sqlStr = sqlCreator.UpdateA();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        /// <summary>
        /// 按下主表刪除確認按鈕
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject DeleteA(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.DeleteA();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        /// <summary>
        /// 按下主表某筆資料後,觸發查詢關聯
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject QueryBind(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            JToken CountData = CountBind(obj); //取得資料筆數

            dynamic parm = obj as dynamic;
            parm.total = CountData["count"] == null ? "0" : CountData["count"];

            var sqlStr = sqlCreator.GetSqlStr("QueryBind",obj);
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
        /// 按下主表某筆資料後,觸發查詢關聯資料筆數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject CountBind(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();

            var sqlStr = sqlCreator.GetSqlStr("CountBind", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        /// <summary>
        /// 按下關聯表新增按鈕後彈出關聯選擇視窗,選擇關聯後按下確定
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject InsertBind(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            returnMsg = CheckColumnBind(obj);
            //判斷是否為輸入空值
            if (returnMsg.result == false)
            {
                return returnMsg;
            }

            var sqlParms = sqlCreator.CreateParameterAry(obj);
            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, sqlParms);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();


            var sqlStr = sqlCreator.InsertBind(uuid);
            dao.AddExecuteItem(sqlStr, sqlParms);
            returnMsg.result = dao.Execute();
            if (returnMsg.result == true)
            {
                returnMsg.guid = uuid;
            }
            return returnMsg;
        }

        [System.Web.Http.HttpPost]
        public JObject UpdateBind(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            returnMsg = CheckColumnBind(obj);
            //判斷是否為輸入空值
            if (returnMsg.result == false)
            {
                return returnMsg;
            }
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            var sqlStr = sqlCreator.GetSqlStr("UpdateBind", obj);
            dao.AddExecuteItem(sqlStr, sqlParms);
            returnMsg.result = dao.Execute();
            return returnMsg;
        }

        

        /// <summary>
        /// 關聯表刪除關聯
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject DeleteBind(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.DeleteBind();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
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

        [System.Web.Http.HttpPost]
        public JObject GetListOfMap(JObject obj)
        {
            Iotdevice sqlCreator = new Iotdevice();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            var sqlStr = sqlCreator.GetListOfMap();
            var sqlParms = sqlCreator.CreateParameterAry(obj);


            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        public dynamic CheckColumnA(JObject obj)
        {
            dynamic returnMsg = new JObject();
            dynamic objData = obj as dynamic;
            returnMsg.result = true;
            //判斷是否為輸入空值
            if (objData.DI_COUNT == "" || objData.DO_COUNT == "" || objData.MAP_GUID == "")
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "輸入資料不完整");
                return returnMsg;
            }

            //Check2
            bool isNumber_DI_COUNT = Regex.IsMatch(Convert.ToString(objData.DI_COUNT), @"^[0-9]+$");
            bool isNumber_DO_COUNT = Regex.IsMatch(Convert.ToString(objData.DO_COUNT), @"^[0-9]+$");
            if (!isNumber_DI_COUNT || !isNumber_DO_COUNT)
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "輸入資料格式錯誤 必須為正整數");
                return returnMsg;
            }

            return returnMsg;
        }
        public dynamic CheckColumnBind(JObject obj)
        {
            dynamic returnMsg = new JObject();
            dynamic objData = obj as dynamic;
            returnMsg.result = true;
            //判斷是否為輸入空值
            if (objData.INDEX == "" || objData.SIGNAL_0 == "" || objData.SIGNAL_1 == "")
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "輸入資料不完整");
                return returnMsg;
            }

            //Check 2
            bool isNumber_INDEX = Regex.IsMatch(Convert.ToString(objData.INDEX), @"^[0-9]+$");
            bool isNumber_SIGNAL_0 = Regex.IsMatch(Convert.ToString(objData.SIGNAL_0), @"^[0-9]+$");
            bool isNumber_SIGNAL_1 = Regex.IsMatch(Convert.ToString(objData.SIGNAL_1), @"^[0-9]+$");
            if (!isNumber_INDEX || !isNumber_SIGNAL_0 || !isNumber_SIGNAL_1)
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "訊號輸入資料格式錯誤 必須為正整數");
                return returnMsg;
            }

            return returnMsg;
        }


    }
}