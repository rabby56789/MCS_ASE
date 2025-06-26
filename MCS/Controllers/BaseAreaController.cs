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
    public class BaseAreaController : Controller
    {
        //回傳單表或關聯表View
        public ActionResult Index()
        {
            return View("~/Views/Base/Area.cshtml");
        }
    }

    //範本API--可選擇繼承介面IJqOneTable(單表UI),IJqBindTable(關聯表UI)

    public class ApiAreaController : ApiController, IJqOneTable, IJqBindTable
    {
        //實例通用BD存取物件DAO與資料轉換工具DataTableExtensions
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();

        [System.Web.Http.HttpPost]
        public JObject Query(JObject obj)
        {
            Area sqlCreator = new Area();
            dynamic parm = obj as dynamic;

            var sqlStr = sqlCreator.GetSqlStr("Query", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);


            return returnVal;
        }

        //找區域
        [System.Web.Http.HttpPost]
        public JObject QueryA(JObject obj)
        {
            Area sqlCreator = new Area();
            dynamic parm = obj as dynamic;
            //計算儲區數量
            var sqlStrCount = sqlCreator.GetSqlStr("Count", obj);
            var sqlParmsCount = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrCount, sqlParmsCount);
            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            //找儲區
            var sqlStr = sqlCreator.GetSqlStr("Query", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            //returnVal["total"] = parm.total;
            returnVal["total"] = count;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);

            return returnVal;
        }

        //找還未加入區域的儲位
        [System.Web.Http.HttpPost]
        public JObject QueryB(JObject obj)
        {
            Area sqlCreator = new Area();
            dynamic parm = obj as dynamic;
            //先找未加入區域的儲位有幾個
            var sqlStrCount = sqlCreator.GetSqlStr("CountB", obj);
            var sqlParmsCount = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrCount, sqlParmsCount);
            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            //再找未加入區域的儲位
            var sqlStr = sqlCreator.GetSqlStr("QueryB", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            //total = 區域裡的儲位
            returnVal["total"] = count;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }


        /// <summary>
        /// 查詢A表資料筆數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            Area sqlCreator = new Area();

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
        public JObject CountA(JObject obj)
        {
            Area sqlCreator = new Area();

            var sqlStr = sqlCreator.Search(obj, true);

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
        [System.Web.Http.HttpPost]
        public JObject GetOneByGUID(JObject obj)
        {
            Area sqlCreator = new Area();
            DataTableExtensions extensions = new DataTableExtensions();
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
        [System.Web.Http.HttpPost]
        public JObject Insert(JObject obj)
        {
            Area sqlCreator = new Area();
            DAO dao = new DAO();
            dynamic parm = obj as dynamic;
            DataTableExtensions extensions = new DataTableExtensions();
            var sqlStr = sqlCreator.GetSqlStr("Add", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }
        
        //A表insert
        [System.Web.Http.HttpPost]
        public JObject InsertA(JObject obj)
        {
            Area sqlCreator = new Area();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            dynamic objData = obj as dynamic;
            //returnMsg.result = dao.Execute();
            //判斷是否為輸入空值
            if(objData.ID == "" || objData.NAME == "")
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "輸入資料不完整");
                return returnMsg;
            }

            //先找有無重複的區域ID
            var sqlStrAreaID = sqlCreator.SearchAreaID(obj);
            var sqlSearchAreaIDParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrAreaID, sqlSearchAreaIDParms);
            var AreaIDTables = dao.Query().Tables[0];
            int IDcount = AreaIDTables.Rows.Count;
            if (IDcount != 0)//有重複就return錯誤訊息
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "已新增過的區域編碼");
                return returnMsg;
            }
            //再找有無重複的區域NAME
            var sqlStrAreaNAME = sqlCreator.SearchAreaNAME(obj);
            var sqlSearchAreaNameParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrAreaNAME, sqlSearchAreaNameParms);
            var AreaNameTables = dao.Query().Tables[0];
            int NameCount = AreaNameTables.Rows.Count;
            if (NameCount != 0)//有重複就return錯誤訊息
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "已新增過的區域名稱");
                return returnMsg;
            }

            //都沒有在insert
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

        /// <summary>
        /// 按下主表編輯畫面確認按鈕
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject Update(JObject obj)
        {
            Area sqlCreator = new Area();
            DAO dao = new DAO();
            dynamic parm = obj as dynamic;
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetSqlStr("Edit", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }
        //A表 update
        [System.Web.Http.HttpPost]
        public JObject UpdateA(JObject obj)
        {
            Area sqlCreator = new Area();
            DAO dao = new DAO();
            dynamic returnMsg = CheckColumnA(obj);
            if (returnMsg.result == false)
            {
                return returnMsg;
            }

            //dynamic returnMsg = new JObject();
            //判斷更動的資料是否都一樣
            //var sqlStrArea = sqlCreator.SearchArea(obj);
            //var sqlSearchAreaParms = sqlCreator.CreateParameterAry(obj);
            //dao.AddExecuteItem(sqlStrArea, sqlSearchAreaParms);
            //var AreaTables = dao.Query().Tables[0];
            //int count = AreaTables.Rows.Count;
            //if (count != 0)//輸入資料和資料庫資料都一樣就return未變動訊息
            //{
            //    returnMsg.result = false;
            //    returnMsg.Add("msg", "區域名稱和備註未更改");
            //    return returnMsg;
            //}
            var sqlStr = sqlCreator.GetSqlStr("EditA", obj);
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
        public JObject Delete(JObject obj)
        {
            Area sqlCreator = new Area();
            DAO dao = new DAO();
            dynamic parm = obj as dynamic;
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetSqlStr("Delete", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);


            return returnVal;
        }
        //A表 DeleteA
        [System.Web.Http.HttpPost]
        public JObject DeleteA(JObject obj)
        {
            Area sqlCreator = new Area();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            var sqlStr = sqlCreator.GetSqlStr("Delete", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
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
            Area sqlCreator = new Area();
            DAO dao = new DAO();
            dynamic parm = obj as dynamic;
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetSqlStr("DeleteBind", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);


            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        /// <summary>
        /// 按下主表某筆資料後,觸發查詢關聯
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject QueryBind(JObject obj)
        {
            Area sqlCreator = new Area();
            DAO dao = new DAO();
            dynamic parm = obj as dynamic;
            DataTableExtensions extensions = new DataTableExtensions();
            //先查區域裡的儲位有幾個
            var sqlStrCount = sqlCreator.GetSqlStr("CountR", obj);
            var sqlParmsCount = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStrCount, sqlParmsCount);
            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");

            //再找該區域裡的儲位
            var sqlStr = sqlCreator.GetSqlStr("QueryR", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            //total = 區域裡的儲位
            returnVal["total"] = count;
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// 按下關聯表新增按鈕後彈出關聯選擇視窗,選擇關聯後按下確定
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject InsertBind(JObject obj)
        {
            Area sqlCreator = new Area();
            DAO dao = new DAO();
            dynamic parm = obj as dynamic;
            
            var sqlStr = sqlCreator.GetSqlStr("InsertBind", obj);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }
        //檢查資料輸入
        public dynamic CheckColumnA(JObject obj)
        {
            dynamic returnMsg = new JObject();
            dynamic objData = obj as dynamic;
            returnMsg.result = true;
            //判斷是否為輸入空值
            if (objData.ID == "" || objData.NAME == "")
            {
                returnMsg.result = false;
                returnMsg.Add("msg", "輸入資料不完整");
                return returnMsg;
            }

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
    }
}