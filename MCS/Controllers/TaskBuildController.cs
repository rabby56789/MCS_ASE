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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;

namespace MCS.Controllers
{
    public class TaskBuildController : Controller 
    {
        
        // GET: Factory
        public ActionResult Index()
        {
            return View("~/Views/Tablet/TaskBuild.cshtml");
        }
    }
}

public class ApiTaskBuildController : ApiController, IJqOneTable
    {
    static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    [System.Web.Http.HttpPost]
    public JObject Build(JObject obj)
    {
        #region OLD_Build
        //switch (obj["PRIORITY"].ToString())
        //{
        //    case "1":
        //        obj["PRIORITY"] = "121";
        //        break;
        //    case "2":
        //        obj["PRIORITY"] = "120";
        //        break;
        //    case "3":
        //        obj["PRIORITY"] = "119";
        //        break;
        //}
        //TaskBuild sqlCreator = new TaskBuild();
        //DataTableExtensions extensions = new DataTableExtensions();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();
        ////目的地未輸入
        //if (obj["POSITIONCODE2"].ToString() == "")
        //{
        //    returnMsg.result = false;
        //    returnMsg.errorMessage = "請輸入目標儲區";
        //    return returnMsg;
        //}
        ////先找目的地有沒有貨架
        //var sqlTrolley = sqlCreator.GetTrolley();
        //var TrolleyParms = sqlCreator.CreateParameterAry(obj);
        //dao.AddExecuteItem(sqlTrolley, TrolleyParms);
        //var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
        //var TrolleyCount = result["rows"][0].Value<int>("Count");
        //if (TrolleyCount != 0)
        //{
        //    returnMsg.result = false;
        //    returnMsg.errorMessage = "目標儲位已有貨架";
        //    return returnMsg;
        //}
        ////檢查是否有搬運任務正前往目標儲位
        //var sqlLocationing = sqlCreator.GetLocationing();
        //var LocationingParms = sqlCreator.CreateParameterAry(obj);
        //dao.AddExecuteItem(sqlLocationing, LocationingParms);
        //var Locationingresult = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
        //var LocationingCount = Locationingresult["rows"][0].Value<int>("Count");
        //if (LocationingCount != 0)
        //{
        //    returnMsg.result = false;
        //    returnMsg.errorMessage = "已有貨架前往目標儲位中";
        //    return returnMsg;
        //}
        ////檢查有無此台車
        //var sqlSearchTrolley = sqlCreator.SearchTrolley();
        //var SearchParms = sqlCreator.CreateParameterAry(obj);
        //dao.AddExecuteItem(sqlSearchTrolley, SearchParms);
        ////var Searchdata = dao.Query();
        //var Searchresult = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
        //var SearchCount = Searchresult["rows"][0].Value<int>("Count");
        //if (SearchCount == 0)
        //{
        //    returnMsg.result = false;
        //    returnMsg.errorMessage = "查無此台車，請至基本資料管理-台車 建立台車基本資料";
        //    return returnMsg;
        //}
        ////找LOCATION_ID、TYPE
        //var sqlType = sqlCreator.GetLocation();
        //var LocalParms = sqlCreator.CreateParameterAry(obj);
        //dao.AddExecuteItem(sqlType, LocalParms);
        //var data = dao.Query();
        //var Location_ID = data.Tables[0].Rows[0]["LOCATION_ID"].ToString();
        //var TaskType = data.Tables[0].Rows[0]["TYPE"].ToString();

        //switch (TaskType)
        //{
        //    case "mutil01":
        //        TaskType = "F01";
        //        break;
        //    case "samlltary":
        //        TaskType = "P01";
        //        break;
        //}
        //obj.Add("TASKSTATUS", "0");
        //obj.Add("POSITIONCODE1", Location_ID);
        //obj.Add("TASKTYP", TaskType);
        ////
        //var sqlParms = sqlCreator.CreateParameterAry(obj);
        //var sqlStruuid = sqlCreator.GetUUID();
        //dao.AddExecuteItem(sqlStruuid, sqlParms);
        //var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();
        ////var sqlStr = sqlCreator.Chuck();
        ////dao.AddExecuteItem(sqlStr, sqlParms);
        ////取得權重
        //string weightingID = ConfigurationManager.AppSettings["Weighting_ID"];
        //obj.Add("WEIGHTING_ID", weightingID);
        //var sqlStr = sqlCreator.GetWeighting();
        //sqlParms = sqlCreator.CreateParameterAry(obj);
        //dao.AddExecuteItem(sqlStr, sqlParms);
        //var weighting = dao.Query().Tables[0].Rows[0]["PRIORITY"].ToString();
        //obj.Add("WEIGHTING", weighting);
        //sqlParms = sqlCreator.CreateParameterAry(obj);
        ////
        //sqlStr = sqlCreator.Insert(uuid, obj);
        //dao.AddExecuteItem(sqlStr, sqlParms);
        returnMsg.result = "";
        #endregion
        //算出目前貨架位置，再將參數傳送給A01(呼叫空車不指定貨架start_loc = "")
        var trolley = obj["PODCODE"];
        var start_loc = "";
        if (obj["JOB_NAME"].ToString().Equals("呼叫空車") == false)
        {
            var sqlStr = $@"SELECT bs.ID AS start_loc,ts.LOCATION_ID,ts.TROLLEY_ID FROM t_trolley_status ts
                        left join base_storage bs on ts.LOCATION_ID = bs.QRCODE 
                        where TROLLEY_ID = (select ID from base_trolley where name = '{trolley}')";
            dao.AddExecuteItem(sqlStr, null);
            var dataloc = dao.Query().Tables[0];
            if (dataloc.Rows.Count > 0)
            {
                start_loc = dataloc.Rows[0]["start_loc"].ToString();
                if (string.IsNullOrEmpty(start_loc))
                {
                    returnMsg.Add("msg", "查無此貨架位置");
                    returnMsg.result = false;
                    return returnMsg;
                }
            }
            else
            {
                returnMsg.Add("msg", "查無此貨架");
                returnMsg.result = false;
                return returnMsg;
            }
        }
 
        //傳送給A01參數
        JObject parms = new JObject();
        parms.Add("function","A01");
        parms.Add("seq","W_"+ DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        parms.Add("job_name",obj["JOB_NAME"]);
        parms.Add("start_loc",start_loc);
        parms.Add("start_area", obj["POSITIONCODE1"]);
        parms.Add("target_loc","");
        parms.Add("target_area",obj["POSITIONCODE2"]);//目的地
        parms.Add("car_no", obj["PODCODE"]);
        parms.Add("priority",obj["PRIORITY"]);
        string json = JsonConvert.SerializeObject(parms);

        var sqlurl = $"select SERVER_IP,SERVER_PORT,URL from base_server where server_function = 'API_A01'";
        dao.AddExecuteItem(sqlurl,null);
        var dataurl = dao.Query().Tables[0];
        string IP = dataurl.Rows[0]["SERVER_IP"].ToString();
        string PORT = dataurl.Rows[0]["SERVER_PORT"].ToString();
        string URL = dataurl.Rows[0]["URL"].ToString();
        var APIurl = "http://" + IP + ":" + PORT + "/" + URL;
        Task<string> task = PostAsyncJson(APIurl, json);
        String resultData = task.Result;
        log.Info("TaskCenter Result:" + task.Result);
        string jsonResult = task.Result.Replace("\"{", "{").Replace("}\"", "}").Replace("\\", "");
        JObject result = JsonConvert.DeserializeObject<JObject>(jsonResult);
        if (result["return_code"].ToString() == "S")
        {
            returnMsg.result = true;
        }
        else
        {
            returnMsg.Add("msg", result["return_msg"].ToString());
            returnMsg.result = false;
        }

        return returnMsg;
    }
    [System.Web.Http.HttpPost]
    public JObject Count2()
    {
        JObject obj = new JObject();
        TaskBuild sqlCreator = new TaskBuild();
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
    public JObject Count(JObject obj)
    {
        TaskBuild sqlCreator = new TaskBuild();
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
        TaskBuild sqlCreator = new TaskBuild();
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
        TaskBuild sqlCreator = new TaskBuild();
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
        TaskBuild sqlCreator = new TaskBuild();
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();

        var sqlStr = sqlCreator.GetOneByGUID();
        var sqlParms = sqlCreator.CreateParameterAry(obj);

        dao.AddExecuteItem(sqlStr, sqlParms);

        var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

        return returnVal;
    }
    [System.Web.Http.HttpPost]
    public JObject GetOption()//取得優先權選單
    {
        //BaseSubTask sqlCreator = new BaseSubTask();
        DAO dao = new DAO();
        //var sqlStr = sqlCreator.GetOption(obj);
        //var sqlParms = sqlCreator.CreateParameterAry(obj);

        DataTableExtensions extensions = new DataTableExtensions();
        //dao.AddExecuteItem(sqlStr, sqlParms);
        var sqlString = $"select name AS `VALUE`,priority AS `KEY` from base_weighting where enable = 1";
        dao.AddExecuteItem(sqlString,null);

        var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
        return returnVal;
    }

    public JObject Import(JObject obj)
    {
        throw new NotImplementedException();
    }
    [System.Web.Http.HttpPost]
    public JObject Insert(JObject obj)
    {
        TaskBuild sqlCreator = new TaskBuild();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();
        var sqlParms = sqlCreator.CreateParameterAry(obj);
        var sqlStruuid = sqlCreator.GetUUID();
        dao.AddExecuteItem(sqlStruuid, sqlParms);
        var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();
        //var sqlStr = sqlCreator.Chuck();
        //dao.AddExecuteItem(sqlStr, sqlParms);

        var sqlStr = sqlCreator.Insert(uuid, obj);
        dao.AddExecuteItem(sqlStr, sqlParms);
        returnMsg.result = dao.Execute();

        return returnMsg;
    }
    [System.Web.Http.HttpPost]
    public JObject Query(JObject obj)
    {
        TaskBuild sqlCreator = new TaskBuild();
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();
        dynamic parm = obj as dynamic;

        var sqlStrCount = sqlCreator.Search(obj,true);
        var sqlParmsCount = sqlCreator.CreateParameterAry(obj);
        dao.AddExecuteItem(sqlStrCount, sqlParmsCount);
        var count = dao.Query().Tables[0].Rows[0][0].ToString();

        var sqlStr = sqlCreator.Search(obj, false);
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
    public JObject Query2(JObject obj)
    {
        TaskBuild sqlCreator = new TaskBuild();
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();
        dynamic parm = obj as dynamic;
        obj.Add("TASKSTATUS", "2");
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
    public JObject Query3(JObject obj)
    {
        TaskBuild sqlCreator = new TaskBuild();
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();
        dynamic parm = obj as dynamic;
        obj.Add("TASKSTATUS", "1");
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
            TaskBuild sqlCreator = new TaskBuild();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.Update();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }
        [System.Web.Http.HttpPost]
        public JObject GetDataList(JObject obj)
        {
            TaskBuild sqlCreator = new TaskBuild();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            var sqlStr = sqlCreator.GetDataList();
            var sqlParms = sqlCreator.CreateParameterAry(obj);


            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

    /// <summary>
    /// 檢查目標儲位是否是沒有貨架。
    /// </summary>
    /// <returns>
    /// 若沒有貨架，回傳 true；
    /// 若有，回傳 false；
    /// </returns>
    public bool CheckIfTargetLocationIsEmpty(string locationCode)
    {
        string mapShortName;
        if (locationCode.Contains("FD"))
        {
            mapShortName = "4F";
        }
        else if (locationCode.Contains("FB"))
        {
            mapShortName = "2F";
        }
        else
        {
            throw new Exception("未定義的樓層資訊。");
        }

        PodBerthAndMat targetLocationData = QueryPodBerthAndMatFromRCS(mapShortName);

        //檢查對應樓層中的所有貨架所在是否包含特定的儲位
        foreach (PodBerthAndMatData record in targetLocationData.data)
        {
            if (record.mapDataCode == locationCode)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 使用 RCS API: queryPodBerthAndMat 來查詢特定樓層的所有貨架/儲位/物料等關連資料。
    /// </summary>
    /// <returns>
    /// 回傳自定義類別 PodBerthAndMat。
    /// </returns>
    public PodBerthAndMat QueryPodBerthAndMatFromRCS(string mapShortName)
    {
        try
        {
            string RCS_IP = ConfigurationManager.AppSettings["HikAPI_rest"];
            string RCS_DPS_Port = ConfigurationManager.AppSettings["RCS_DPS_Port"];            

            //string json = File.ReadAllText(_JsonPath);
            var APIurl = RCS_IP + "queryPodBerthAndMat";
            JObject parameter = new JObject();
            parameter.Add("reqCode", "QAS" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            parameter.Add("reqTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            parameter.Add("clientCode", "");
            parameter.Add("positionCode", "");
            parameter.Add("mapShortName", mapShortName);
            string json = JsonConvert.SerializeObject(parameter);

            Task<string> task = PostAsyncJson(APIurl, json);
            String resultData = task.Result;
            PodBerthAndMat podBerthAndMat = JsonConvert.DeserializeObject<PodBerthAndMat>(task.Result);
            return podBerthAndMat;            
        }
        catch (Exception exception)
        {
            //Logger.ErrorFormat("QueryAgvStatus: ", ex);
            throw exception;
        }
    }
    public async Task<string> PostAsyncJson(string url, string json)
    {
        HttpClient client = new HttpClient();
        HttpContent content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        Task< HttpResponseMessage> _task =  client.PostAsync(url, content);
        HttpResponseMessage response = _task.Result;
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    /// <summary>
    /// 檢查是否有搬運任務正前往目標儲位。
    /// </summary>
    /// <returns>
    /// 若有任務，回傳 true；
    /// 若沒有，回傳 false；
    /// </returns>
    public bool CheckIfExistsMovingTaskWithPosition2(JObject obj)
    {
        try
        {
            TaskBuild sqlCreator = new TaskBuild();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.FindWorkingTaskWithPosition2();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);
            var data = dao.Query().Tables[0];
            //DataSet ds = dao.Query();
            //var data = ds.Tables[0];
            if (data.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }            
        }
        catch (Exception exception)
        {
            throw exception;
        }
    }
}
    