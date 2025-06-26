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
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;

namespace MCS.Controllers
{
    public class TaskListController : Controller
    {

        // GET: Factory
        public ActionResult Index()
        {
            return View("~/Views/Task/TaskList.cshtml");
        }
        #region XML上傳
        public ActionResult MapUpload(HttpPostedFileBase file)
        {
            if (file != null)
            {
                var fileValid = true;
                // Limit File Szie Below : 5MB
                //if (file.ContentLength <= 0 || file.ContentLength > 5242880)
                //{
                //    fileValid = false;
                //}

                if (fileValid == true)
                {
                    string extension = Path.GetExtension(file.FileName);
                    string fileName = $"{Guid.NewGuid()}{extension}";
                    string savePath_tmp = ConfigurationManager.AppSettings["AGV_MapUpload"];
                    string savePath = Path.Combine(Server.MapPath(savePath_tmp), fileName);
                    file.SaveAs(savePath);
                    AGVMAPXMLToDB(fileName);
                    ViewBag.Message = "檔案上傳成功。";
                }
                else
                {
                    ViewBag.Message = "Failed 😛";
                }
            }
            return RedirectToAction("Index");
        }
        public void MAPXMLToBASE(string filename)
        {

            XmlDocument xmlDoc = new XmlDocument();
            //XmlDocument xmlDoc2 = new XmlDocument();
            string savePath_tmp = ConfigurationManager.AppSettings["AGV_MapUpload"];
            xmlDoc.Load(Server.MapPath(savePath_tmp + "\\" + filename));//載入xml檔
            foreach (XmlNode node in xmlDoc)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    xmlDoc.RemoveChild(node);
                }
            }
            //string xml_head = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            string json = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.None, true);
            JObject jsonob = JObject.Parse(json);
            DAO dao = new DAO();

            Map sqlCreator = new Map();
            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, null);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();

            //XmlNode xn = xmlDoc.SelectSingleNode("MapCfg");
            string MapID = jsonob["MapID"].ToString();
            string MapName = jsonob["MapName"].ToString();
            string MapQRCode = jsonob["MapQRCode"].ToString();
            string MapType = jsonob["MapType"].ToString();
            string Row = jsonob["Row"].ToString();
            string Col = jsonob["Col"].ToString();
            string sqlStr = "INSERT INTO BASE_MAP (GUID,FLOOR_GUID,ID,NAME,QRCODE,TYPE,ROW,COL,IMPORT_FILE,INSERT_USER,INSERT_TIME) VALUES"
                        + "('"
                        + uuid + "','"
                        + MapID + "','"
                         + MapName + "','"
                          + MapQRCode + "','"
                           + MapType + "','"
                            + Row + "','"
                            + Col + "','"
                            + filename + "',now()"
                        + ");";
            dao.AddExecuteItem(sqlStr, null);

            var aaa = jsonob["PointInfo"].ToArray();
            for (var i = 0; i < aaa.Length; i++)
            {
                string point_id = aaa[i]["id"].ToString();
                string xpos = aaa[i]["xpos"].ToString();
                string ypos = aaa[i]["ypos"].ToString();
                string width = aaa[i]["width"].ToString();
                string height = aaa[i]["height"].ToString();
                string value = aaa[i]["value"]["#text"].ToString();
                string RoadProperty = aaa[i]["RoadProperty"].ToString();
                string Rot = aaa[i]["Rot"].ToString();
                string alldirRot = aaa[i]["alldirRot"].ToString();
                string allDir = aaa[i]["allDir"].ToString();
                string EleDir = aaa[i]["EleDir"].ToString();
                string ElePre = aaa[i]["ElePre"].ToString();
                string RotRad = aaa[i]["RotRad"].ToString();
                string robottype = aaa[i]["robottype"].ToString();
                string RotUnderPod = aaa[i]["RotUnderPod"].ToString();
                string rotMech = aaa[i]["rotMech"].ToString();
                string evit = aaa[i]["evit"].ToString();
                string rotByRobotType = aaa[i]["rotByRobotType"].ToString();
                string rotForPodType = aaa[i]["rotForPodType"].ToString();
                string sensorswitchPoint = aaa[i]["sensorswitchPoint"].ToString();
                string pallet = aaa[i]["pallet"].ToString();
                string TranZoneType = GetJsonVal(aaa[i]["TranZoneType"].ToString(), "TranZoneType");
                string RotBarrierArea = aaa[i]["RotBarrierArea"].ToString();
                string IsUpdateMap = aaa[i]["IsUpdateMap"].ToString();
                sqlStr = "INSERT INTO `jqweb`.`agv_pointinfo`(`GUID`,`MapCfg_ID`,`Point_ID`,`xpos`,"
                        + "`ypos`,`width`,`height`,`value`,`RoadProperty`,`Rot`,`alldirRot`,`allDir`,`EleDir`,`ElePre`,`RotRad`,"
                        + "`robottype`,`RotUnderPod`,`rotMech`,`evit`,`rotByRobotType`,`rotForPodType`,`sensorswitchPoint`,`pallet`,"
                        + "`TranZoneType`,`RotBarrierArea`,`IsUpdateMap`)VALUES"
                        + "(UUID(),'"
                        + MapID + "','"
                        + point_id + "','"
                        + xpos + "','"
                        + ypos + "','"
                        + width + "','"
                        + height + "','"
                        + value + "','"
                        + RoadProperty + "','"
                        + Rot + "','"
                        + alldirRot + "','"
                        + allDir + "','"
                        + EleDir + "','"
                        + ElePre + "','"
                        + RotRad + "','"
                        + robottype + "','"
                        + RotUnderPod + "','"
                        + rotMech + "','"
                        + evit + "','"
                        + rotByRobotType + "','"
                        + rotForPodType + "','"
                        + sensorswitchPoint + "','"
                        + pallet + "','"
                        + TranZoneType + "','"
                        + RotBarrierArea + "','"
                        + IsUpdateMap + "'"
                        + ")";
                dao.AddExecuteItem(sqlStr, null);


                var bbb = aaa[i]["NeighbInfo"].ToArray();
                string Neighb_ID = GetJsonVal(bbb[0].ToString(), "id");
                string distance = GetJsonVal(bbb[1].ToString(), "distance");
                string Rever = GetJsonVal(bbb[2].ToString(), "Rever");
                string Speed = GetJsonVal(bbb[3].ToString(), "Speed");
                string Ultrasonic = GetJsonVal(bbb[4].ToString(), "Ultrasonic");
                string LeftWidth = GetJsonVal(bbb[5].ToString(), "LeftWidth");
                string RightWidth = GetJsonVal(bbb[6].ToString(), "RightWidth");
                string RoadLeftWidth = GetJsonVal(bbb[7].ToString(), "RoadLeftWidth");
                string RoadRightWidth = GetJsonVal(bbb[8].ToString(), "RoadRightWidth");
                string robottype2 = GetJsonVal(bbb[9].ToString(), "robottype");
                string PodDir = GetJsonVal(bbb[10].ToString(), "PodDir");
                string StartDir = GetJsonVal(bbb[11].ToString(), "StartDir");
                string StopDir = GetJsonVal(bbb[12].ToString(), "StopDir");
                string AlarmVoice = GetJsonVal(bbb[13].ToString(), "AlarmVoice");
                string PreForkLift = GetJsonVal(bbb[14].ToString(), "PreForkLift");
                string rotate = GetJsonVal(bbb[15].ToString(), "rotate");
                string LaserType = GetJsonVal(bbb[16].ToString(), "LaserType");
                string PodLaserType = GetJsonVal(bbb[17].ToString(), "PodLaserType");
                string SensorSwitch = GetJsonVal(bbb[18].ToString(), "SensorSwitch");
                string carryType = GetJsonVal(bbb[19].ToString(), "carryType");
                string ctnrtype = GetJsonVal(bbb[20].ToString(), "ctnrtype");
                string CtrlPoint = GetJsonVal(bbb[21].ToString(), "CtrlPoint");
                if (Neighb_ID.IndexOf(",") > 0)
                {
                    for (var j = 0; j < Neighb_ID.Split(',').Length; j++)
                    {
                        sqlStr = "INSERT INTO `jqweb`.`agv_neighbinfo`(`GUID`,`MapCfg_ID`,`PointInfo_ID`,`Neighb_ID`,`distance`,"
                               + "`Rever`,`Speed`,`Ultrasonic`,`LeftWidth`,`RightWidth`,`RoadLeftWidth`,`RoadRightWidth`,`robottype`,`PodDir`,"
                               + "`StartDir`,`StopDir`,`AlarmVoice`,`PreForkLift`,`rotate`,`LaserType`,`PodLaserType`,`SensorSwitch`,`carryType`,"
                               + "`ctnrtype`,`CtrlPoint`)VALUES"
                               + "(UUID(),'"
                               + MapID + "','"
                               + point_id + "','"
                               + Neighb_ID.Split(',')[j] + "','"
                               + distance.Split(',')[j] + "','"
                               + Rever.Split(',')[j] + "','"
                               + Speed.Split(',')[j] + "','"
                               + Ultrasonic.Split(',')[j] + "','"
                               + LeftWidth.Split(',')[j] + "','"
                               + RightWidth.Split(',')[j] + "','"
                               + RoadLeftWidth.Split(',')[j] + "','"
                               + RoadRightWidth.Split(',')[j] + "','"
                               + robottype2.Split(',')[j] + "','"
                               + PodDir.Split(',')[j] + "','"
                               + StartDir.Split(',')[j] + "','"
                               + StopDir.Split(',')[j] + "','"
                               + AlarmVoice.Split(',')[j] + "','"
                               + PreForkLift.Split(',')[j] + "','"
                               + rotate.Split(',')[j] + "','"
                               + LaserType.Split(',')[j] + "','"
                               + PodLaserType.Split(',')[j] + "','"
                               + SensorSwitch.Split(',')[j] + "','"
                               + carryType.Split(',')[j] + "','"
                               + ctnrtype.Split(',')[j] + "','"
                               + CtrlPoint.Split(',')[j] + "'"
                               + ")";
                        dao.AddExecuteItem(sqlStr, null);
                    }

                }
                else
                {
                    sqlStr = "INSERT INTO `jqweb`.`agv_neighbinfo`(`GUID`,`MapCfg_ID`,`PointInfo_ID`,`Neighb_ID`,`distance`,"
                               + "`Rever`,`Speed`,`Ultrasonic`,`LeftWidth`,`RightWidth`,`RoadLeftWidth`,`RoadRightWidth`,`robottype`,`PodDir`,"
                               + "`StartDir`,`StopDir`,`AlarmVoice`,`PreForkLift`,`rotate`,`LaserType`,`PodLaserType`,`SensorSwitch`,`carryType`,"
                               + "`ctnrtype`,`CtrlPoint`)VALUES"
                               + "(UUID(),'"
                               + MapID + "','"
                               + point_id + "','"
                               + Neighb_ID + "','"
                               + distance + "','"
                               + Rever + "','"
                               + Speed + "','"
                               + Ultrasonic + "','"
                               + LeftWidth + "','"
                               + RightWidth + "','"
                               + RoadLeftWidth + "','"
                               + RoadRightWidth + "','"
                               + robottype2 + "','"
                               + PodDir + "','"
                               + StartDir + "','"
                               + StopDir + "','"
                               + AlarmVoice + "','"
                               + PreForkLift + "','"
                               + rotate + "','"
                               + LaserType + "','"
                               + PodLaserType + "','"
                               + SensorSwitch + "','"
                               + carryType + "','"
                               + ctnrtype + "','"
                               + CtrlPoint + "'"
                               + ")";
                    dao.AddExecuteItem(sqlStr, null);
                }
            }
            dao.Execute();
        }
        public void AGVMAPXMLToDB(string filename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //XmlDocument xmlDoc2 = new XmlDocument();
            string savePath_tmp = ConfigurationManager.AppSettings["AGV_MapUpload"];
            xmlDoc.Load(Server.MapPath(savePath_tmp + "\\" + filename));//載入xml檔
            foreach (XmlNode node in xmlDoc)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    xmlDoc.RemoveChild(node);
                }
            }
            //string xml_head = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            string json = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.None, true);
            JObject jsonob = JObject.Parse(json);
            DAO dao = new DAO();
            //XmlNode xn = xmlDoc.SelectSingleNode("MapCfg");
            string MapID = jsonob["MapID"].ToString();
            string MapName = jsonob["MapName"].ToString();
            string MapQRCode = jsonob["MapQRCode"].ToString();
            string MapType = jsonob["MapType"].ToString();
            string Row = jsonob["Row"].ToString();
            string Col = jsonob["Col"].ToString();
            string sqlStr = "INSERT INTO `jqweb`.`agv_mapcfg` (`MapID`,`MapName`,`MapQRCode`,`MapType`,`Row`,`Col`,`agv_upload_name`,`create_dt`) VALUES"
                        + "('"
                        + MapID + "','"
                         + MapName + "','"
                          + MapQRCode + "','"
                           + MapType + "','"
                            + Row + "','"
                            + Col + "','"
                            + filename + "',now()"
                        + ");";
            dao.AddExecuteItem(sqlStr, null);

            var aaa = jsonob["PointInfo"].ToArray();
            for (var i = 0; i < aaa.Length; i++)
            {
                string point_id = aaa[i]["id"].ToString();
                string xpos = aaa[i]["xpos"].ToString();
                string ypos = aaa[i]["ypos"].ToString();
                string width = aaa[i]["width"].ToString();
                string height = aaa[i]["height"].ToString();
                string value = aaa[i]["value"]["#text"].ToString();
                string RoadProperty = aaa[i]["RoadProperty"].ToString();
                string Rot = aaa[i]["Rot"].ToString();
                string alldirRot = aaa[i]["alldirRot"].ToString();
                string allDir = aaa[i]["allDir"].ToString();
                string EleDir = aaa[i]["EleDir"].ToString();
                string ElePre = aaa[i]["ElePre"].ToString();
                string RotRad = aaa[i]["RotRad"].ToString();
                string robottype = aaa[i]["robottype"].ToString();
                string RotUnderPod = aaa[i]["RotUnderPod"].ToString();
                string rotMech = aaa[i]["rotMech"].ToString();
                string evit = aaa[i]["evit"].ToString();
                string rotByRobotType = aaa[i]["rotByRobotType"].ToString();
                string rotForPodType = aaa[i]["rotForPodType"].ToString();
                string sensorswitchPoint = aaa[i]["sensorswitchPoint"].ToString();
                string pallet = aaa[i]["pallet"].ToString();
                string TranZoneType = GetJsonVal(aaa[i]["TranZoneType"].ToString(), "TranZoneType");
                string RotBarrierArea = aaa[i]["RotBarrierArea"].ToString();
                string IsUpdateMap = aaa[i]["IsUpdateMap"].ToString();
                sqlStr = "INSERT INTO `jqweb`.`agv_pointinfo`(`GUID`,`MapCfg_ID`,`Point_ID`,`xpos`,"
                        + "`ypos`,`width`,`height`,`value`,`RoadProperty`,`Rot`,`alldirRot`,`allDir`,`EleDir`,`ElePre`,`RotRad`,"
                        + "`robottype`,`RotUnderPod`,`rotMech`,`evit`,`rotByRobotType`,`rotForPodType`,`sensorswitchPoint`,`pallet`,"
                        + "`TranZoneType`,`RotBarrierArea`,`IsUpdateMap`)VALUES"
                        + "(UUID(),'"
                        + MapID + "','"
                        + point_id + "','"
                        + xpos + "','"
                        + ypos + "','"
                        + width + "','"
                        + height + "','"
                        + value + "','"
                        + RoadProperty + "','"
                        + Rot + "','"
                        + alldirRot + "','"
                        + allDir + "','"
                        + EleDir + "','"
                        + ElePre + "','"
                        + RotRad + "','"
                        + robottype + "','"
                        + RotUnderPod + "','"
                        + rotMech + "','"
                        + evit + "','"
                        + rotByRobotType + "','"
                        + rotForPodType + "','"
                        + sensorswitchPoint + "','"
                        + pallet + "','"
                        + TranZoneType + "','"
                        + RotBarrierArea + "','"
                        + IsUpdateMap + "'"
                        + ")";
                dao.AddExecuteItem(sqlStr, null);


                var bbb = aaa[i]["NeighbInfo"].ToArray();
                string Neighb_ID = GetJsonVal(bbb[0].ToString(), "id");
                string distance = GetJsonVal(bbb[1].ToString(), "distance");
                string Rever = GetJsonVal(bbb[2].ToString(), "Rever");
                string Speed = GetJsonVal(bbb[3].ToString(), "Speed");
                string Ultrasonic = GetJsonVal(bbb[4].ToString(), "Ultrasonic");
                string LeftWidth = GetJsonVal(bbb[5].ToString(), "LeftWidth");
                string RightWidth = GetJsonVal(bbb[6].ToString(), "RightWidth");
                string RoadLeftWidth = GetJsonVal(bbb[7].ToString(), "RoadLeftWidth");
                string RoadRightWidth = GetJsonVal(bbb[8].ToString(), "RoadRightWidth");
                string robottype2 = GetJsonVal(bbb[9].ToString(), "robottype");
                string PodDir = GetJsonVal(bbb[10].ToString(), "PodDir");
                string StartDir = GetJsonVal(bbb[11].ToString(), "StartDir");
                string StopDir = GetJsonVal(bbb[12].ToString(), "StopDir");
                string AlarmVoice = GetJsonVal(bbb[13].ToString(), "AlarmVoice");
                string PreForkLift = GetJsonVal(bbb[14].ToString(), "PreForkLift");
                string rotate = GetJsonVal(bbb[15].ToString(), "rotate");
                string LaserType = GetJsonVal(bbb[16].ToString(), "LaserType");
                string PodLaserType = GetJsonVal(bbb[17].ToString(), "PodLaserType");
                string SensorSwitch = GetJsonVal(bbb[18].ToString(), "SensorSwitch");
                string carryType = GetJsonVal(bbb[19].ToString(), "carryType");
                string ctnrtype = GetJsonVal(bbb[20].ToString(), "ctnrtype");
                string CtrlPoint = GetJsonVal(bbb[21].ToString(), "CtrlPoint");
                if (Neighb_ID.IndexOf(",") > 0)
                {
                    for (var j = 0; j < Neighb_ID.Split(',').Length; j++)
                    {
                        sqlStr = "INSERT INTO `jqweb`.`agv_neighbinfo`(`GUID`,`MapCfg_ID`,`PointInfo_ID`,`Neighb_ID`,`distance`,"
                               + "`Rever`,`Speed`,`Ultrasonic`,`LeftWidth`,`RightWidth`,`RoadLeftWidth`,`RoadRightWidth`,`robottype`,`PodDir`,"
                               + "`StartDir`,`StopDir`,`AlarmVoice`,`PreForkLift`,`rotate`,`LaserType`,`PodLaserType`,`SensorSwitch`,`carryType`,"
                               + "`ctnrtype`,`CtrlPoint`)VALUES"
                               + "(UUID(),'"
                               + MapID + "','"
                               + point_id + "','"
                               + Neighb_ID.Split(',')[j] + "','"
                               + distance.Split(',')[j] + "','"
                               + Rever.Split(',')[j] + "','"
                               + Speed.Split(',')[j] + "','"
                               + Ultrasonic.Split(',')[j] + "','"
                               + LeftWidth.Split(',')[j] + "','"
                               + RightWidth.Split(',')[j] + "','"
                               + RoadLeftWidth.Split(',')[j] + "','"
                               + RoadRightWidth.Split(',')[j] + "','"
                               + robottype2.Split(',')[j] + "','"
                               + PodDir.Split(',')[j] + "','"
                               + StartDir.Split(',')[j] + "','"
                               + StopDir.Split(',')[j] + "','"
                               + AlarmVoice.Split(',')[j] + "','"
                               + PreForkLift.Split(',')[j] + "','"
                               + rotate.Split(',')[j] + "','"
                               + LaserType.Split(',')[j] + "','"
                               + PodLaserType.Split(',')[j] + "','"
                               + SensorSwitch.Split(',')[j] + "','"
                               + carryType.Split(',')[j] + "','"
                               + ctnrtype.Split(',')[j] + "','"
                               + CtrlPoint.Split(',')[j] + "'"
                               + ")";
                        dao.AddExecuteItem(sqlStr, null);
                    }

                }
                else
                {
                    sqlStr = "INSERT INTO `jqweb`.`agv_neighbinfo`(`GUID`,`MapCfg_ID`,`PointInfo_ID`,`Neighb_ID`,`distance`,"
                               + "`Rever`,`Speed`,`Ultrasonic`,`LeftWidth`,`RightWidth`,`RoadLeftWidth`,`RoadRightWidth`,`robottype`,`PodDir`,"
                               + "`StartDir`,`StopDir`,`AlarmVoice`,`PreForkLift`,`rotate`,`LaserType`,`PodLaserType`,`SensorSwitch`,`carryType`,"
                               + "`ctnrtype`,`CtrlPoint`)VALUES"
                               + "(UUID(),'"
                               + MapID + "','"
                               + point_id + "','"
                               + Neighb_ID + "','"
                               + distance + "','"
                               + Rever + "','"
                               + Speed + "','"
                               + Ultrasonic + "','"
                               + LeftWidth + "','"
                               + RightWidth + "','"
                               + RoadLeftWidth + "','"
                               + RoadRightWidth + "','"
                               + robottype2 + "','"
                               + PodDir + "','"
                               + StartDir + "','"
                               + StopDir + "','"
                               + AlarmVoice + "','"
                               + PreForkLift + "','"
                               + rotate + "','"
                               + LaserType + "','"
                               + PodLaserType + "','"
                               + SensorSwitch + "','"
                               + carryType + "','"
                               + ctnrtype + "','"
                               + CtrlPoint + "'"
                               + ")";
                    dao.AddExecuteItem(sqlStr, null);
                }
            }
            dao.Execute();
        }

        public string GetJsonVal(string JsonStr, string Jsonkey)
        {
            var re_bbb = "";
            var str3 = Jsonkey;
            if (Jsonkey == "id" || Jsonkey == "TranZoneType")
            {
                str3 = "#text";
            }
            if (Jsonkey == "TranZoneType")
            {
                if (JsonStr.Length == 1)
                {
                    re_bbb = JsonStr;
                    return re_bbb;
                }
                else
                {
                    JObject js2 = JObject.Parse(JsonStr);
                    re_bbb = js2[str3].ToString();
                    return re_bbb;
                }

            }
            JObject jsonob = JObject.Parse("{" + JsonStr + "}");
            int js_coun = jsonob[Jsonkey].Count();
            var aaa = jsonob[Jsonkey].ToArray();


            if (js_coun == 0)
            {
                re_bbb = jsonob[str3].ToString();
                return re_bbb;
            }
            else
            {
                if (Jsonkey == "id")
                {
                    if (aaa[0].ToString().Length < 15)
                    {
                        re_bbb = aaa[1].ToString().Replace("\"", "").Split(':')[1];
                    }
                    else
                    {
                        for (var i = 0; i < js_coun; i++)
                        {
                            re_bbb += aaa[i][str3].ToString();
                            if (i < js_coun - 1)
                            {
                                re_bbb += ",";
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < js_coun; i++)
                    {
                        re_bbb += aaa[i].ToString();
                        if (i < js_coun - 1)
                        {
                            re_bbb += ",";
                        }
                    }
                }

            }

            return re_bbb;
        }
    }
}
#endregion
public class ApiTaskListController : ApiController, IJqOneTable
{
    [System.Web.Http.HttpPost]
    public JObject Count(JObject obj)
    {
        TaskList sqlCreator = new TaskList();
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();

        obj.Add("TASKSTATUS", "0");


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
        TaskList sqlCreator = new TaskList();
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
        TaskList sqlCreator = new TaskList();
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
        TaskList sqlCreator = new TaskList();
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
        TaskList sqlCreator = new TaskList();
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
        TaskList sqlCreator = new TaskList();
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
    public JObject Cancel(JObject obj)
    {
        TaskList sqlCreator = new TaskList();
        DAO dao = new DAO();
        var sqlStruuid = sqlCreator.GetUUID();
        dao.AddExecuteItem(sqlStruuid, null);
        var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString().Replace("-", "");
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["HikAPI_rest"]);
            JObject parm = new JObject();
            parm.Add("reqCode", uuid);
            parm.Add("reqTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            parm.Add("ClientCode", "");
            parm.Add("tokenCode", "");
            parm.Add("forceCancel", "0");
            parm.Add("matterArea", "");
            parm.Add("agvCode", "");
            parm.Add("taskCode", obj["TASKCODE"]);
            var myContent = JsonConvert.SerializeObject(parm);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var responseTask = client.PostAsync("cancelTask", byteContent);
            responseTask.Wait();

            var response = responseTask.Result;
            var contents = response.Content.ReadAsStringAsync();
            contents.Wait();

            var result = JObject.Parse(contents.Result) as dynamic;

            if (result.code == "0")
            {
                return new JObject() { new JProperty("result", "ok") };
            }
            else
            {
                return new JObject() { new JProperty("result", "faild") };
            }
        }
    }
    [System.Web.Http.HttpPost]
    public JObject Create(JObject obj)
    {
        //寫入組合任務至T_SUBTASK_STATUS
        try
        {            
            DAO dao = new DAO();
            SubTask sqlCreator = new SubTask();
            JObject parm = new JObject();
            //取得權重
            string weightingID = ConfigurationManager.AppSettings["Weighting_ID"];
            parm.Add("WEIGHTING_ID", weightingID);
            var sqlStr = sqlCreator.GetWeighting();
            var sqlParms = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var weighting = dao.Query().Tables[0].Rows[0]["PRIORITY"].ToString();
            //用TASK_ID查詢任務資訊
            sqlStr = sqlCreator.GetSubTaskInfo();
            parm.Add("TASK_ID", obj["TASK_ID"]);
            parm.Add("PROGRESS", "1");
            //parm.Add("SUB_INDEX", "1");
            sqlParms = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var data = dao.Query().Tables[0];
            if (data.Rows.Count == 0)
            {
                return new JObject() { new JProperty("result", "faild") };
            }
            //建立任務
            sqlStr = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStr, null);
            string uuid = dao.Query().Tables[0].Rows[0]["UUID"].ToString();
            parm.Add("GUID", uuid);
            parm.Add("TASK_GUID", data.Rows[0]["TASK_GUID"].ToString());
            parm.Add("WEIGHTING", weighting);
            parm.Add("SUBTASK_TYPE", data.Rows[0]["SUBTASK_TYPE"].ToString());//待
            parm.Add("TASKTYP","");//待
            parm.Add("POSITIONCODEPATH", "");//待
            parm.Add("PODCODE", "");
            parm.Add("PODDIR", "");
            parm.Add("MATERIALLOT", "");
            parm.Add("PRIORITY", "");
            parm.Add("AGVCODE", "");
            parm.Add("INSERT_USER", obj["INSERT_USER"]);
            sqlStr = sqlCreator.Insert();
            var sqlParm = sqlCreator.CreateParameterAry(parm);
            dao.AddExecuteItem(sqlStr, sqlParm);
            dao.Execute();
            var sqlStrTravel = sqlCreator.InsertTaskTravel();
            dao.AddExecuteItem(sqlStrTravel, sqlParm);
            dao.Execute();
            return new JObject() { new JProperty("result", "ok") };
        }
        catch
        {
            return new JObject() { new JProperty("result", "faild") };
        }


    }
    public JObject Update(JObject obj)
    {
        TaskList sqlCreator = new TaskList();
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
        TaskList sqlCreator = new TaskList();
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();
        var sqlStr = sqlCreator.GetDataList();
        var sqlParms = sqlCreator.CreateParameterAry(obj);


        dao.AddExecuteItem(sqlStr, sqlParms);

        var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

        return returnVal;
    }
}
