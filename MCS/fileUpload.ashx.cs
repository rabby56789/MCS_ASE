using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace MCS
{
    /// <summary>
    /// Handler1 的摘要描述
    /// </summary>
    public class fileUpload : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            //string fileName = "";
            
            JObject parms = new JObject();
            string fileType = context.Request.Params.Get("fileType") == null ? "" :
                context.Request.Params.Get("fileType");

            string INSERT_USER = context.Request.Params.Get("INSERT_USER") == null ? "" :
            context.Request.Params.Get("INSERT_USER");
            string FLOOR_GUID = context.Request.Params.Get("FLOOR_GUID") == null ? "" :
                        context.Request.Params.Get("FLOOR_GUID");
            string QRCODE = context.Request.Params.Get("QRCODE") == null ? "" :
                    context.Request.Params.Get("QRCODE");
            parms.Add("INSERT_USER", INSERT_USER);
            parms.Add("UPDATE_USER", INSERT_USER);
            parms.Add("REMARK", "");
            switch (fileType)
            {
                case "mapXML":
                    
                    parms.Add("FLOOR_GUID", FLOOR_GUID);
                    break;
                case "MapRepeat":
                    parms.Add("QRCODE", QRCODE);
                    parms.Add("FLOOR_GUID", FLOOR_GUID);
                    break;
                case "taskList":

                    break;
            }
            //判斷傳過來的fileData中有沒有夾帶檔案
            if (context.Request.Files.Count > 0)
            {
                HttpFileCollection files = context.Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFile file = files[i];
                    //string fname = context.Server.MapPath("~/uploads/" + file.FileName);
                    //file.SaveAs(fname);
                    string extension = Path.GetExtension(file.FileName);
                    string fileName = $"{Guid.NewGuid()}{extension}";
                    string savePath_tmp = ConfigurationManager.AppSettings["TempUpload"];
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath(savePath_tmp)))
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(savePath_tmp));
                    }
                    string savePath = Path.Combine(HttpContext.Current.Server.MapPath(savePath_tmp), fileName);
                    file.SaveAs(savePath);
                    switch (fileType)
                    {
                        case "mapXML":
                            MapXMLToBase(fileName, parms);
                            break;
                        case "MapRepeat":
                            DeleteQrCode(parms);
                            parms.Remove("QRCODE");
                            MapXMLToBase(fileName, parms);
                            break;
                        case "taskList":
                            TaskListToBase(fileName, parms);
                            break;
                        case "check":
                            //context.Response.Write("repeat");
                            context.Response.Write(Check(fileName, parms));
                            return;
                            
                    }
                    
                    //AGVMAPXMLToDB(fileName);
                    //ViewBag.Message = "檔案上傳成功。";
                }
            }
            context.Response.ContentType = "text/plain";
            context.Response.Write("File(s) Uploaded Successfully!");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        public void DeleteQrCode(JObject parms)
        {
            DAO dao = new DAO();
            Map sqlCreator = new Map();            
            dynamic returnMsg = new JObject();
            var sqlStr = sqlCreator.GetRepeatGUID();
            var sqlParms = sqlCreator.CreateParameterAry(parms);
            dao.AddExecuteItem(sqlStr, sqlParms);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();
            sqlStr = sqlCreator.DeleteRepeat(uuid);
            dao.AddExecuteItem(sqlStr, sqlParms);
            dao.Execute();
        }
        public string Check(string filename, JObject parms)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //XmlDocument xmlDoc2 = new XmlDocument();

            string savePath_tmp = ConfigurationManager.AppSettings["TempUpload"];
            xmlDoc.Load(HttpContext.Current.Server.MapPath(savePath_tmp + "\\" + filename));//載入xml檔
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
            string qrCode = string.Empty;
            JObject mapParms = new JObject();
            mapParms = (JObject)parms.DeepClone();
            qrCode = jsonob["MapQRCode"].ToString();
            mapParms.Add("QRCODE", qrCode);
            Map sqlCreator = new Map();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            var sqlStr = sqlCreator.Check();
            var sqlParms = sqlCreator.CreateParameterAry(mapParms);
            dao.AddExecuteItem(sqlStr, sqlParms);
            string result=string.Empty;
            if (dao.Query().Tables[0].Rows.Count == 0)
            {
                result = "";

            }
            else
            {
                result = qrCode;

            }
            return result;
        }
        public void TaskListToBase(string filename,JObject parms)
        {
            string savePath_tmp = ConfigurationManager.AppSettings["TempUpload"] + @"\" + filename;
            List<Task> content = File.ReadAllLines(HttpContext.Current.Server.MapPath(savePath_tmp), Encoding.Default).Select(line => new Task(line)).Skip(1).ToList();
            DAO dao = new DAO();
            DAO daouuid = new DAO();
            foreach (Task item in content)
            {
                JObject taskParms = new JObject();
                taskParms = (JObject)parms.DeepClone();
                taskParms.Add("REQCODE", item.REQCODE);
                taskParms.Add("REQTIME", item.REQTIME);
                taskParms.Add("CLIENTCODE", item.CLIENTCODE);
                taskParms.Add("TASKTYP", item.TASKTYP);
                TaskList sqlCreator = new TaskList();
                var sqlStruuid = sqlCreator.GetUUID();
                var sqlParms = sqlCreator.CreateParameterAry(taskParms);
                daouuid.AddExecuteItem(sqlStruuid, sqlParms);
                var uuid = daouuid.Query().Tables[0].Rows[0].ItemArray[0].ToString();

                
                
                var sqlStr = sqlCreator.Insert(uuid, taskParms);
                dao.AddExecuteItem(sqlStr, sqlParms);
            }
            dao.Execute();
        }
        public void MapXMLToBase(string filename,JObject parms)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //XmlDocument xmlDoc2 = new XmlDocument();
            
            string savePath_tmp = ConfigurationManager.AppSettings["TempUpload"];
            xmlDoc.Load(HttpContext.Current.Server.MapPath(savePath_tmp + "\\" + filename));//載入xml檔
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

            //MAP
            
            JObject mapParms = new JObject();
            mapParms = (JObject)parms.DeepClone();
            //XmlNode xn = xmlDoc.SelectSingleNode("MapCfg");
            //mapParms.Add("ID", jsonob["MapID"].ToString());
            mapParms.Add("ID", jsonob["MapName"].ToString());
            mapParms.Add("NAME", jsonob["MapName"].ToString());
            mapParms.Add("QRCODE", jsonob["MapQRCode"].ToString());
            mapParms.Add("TYPE", jsonob["MapType"].ToString());
            mapParms.Add("ROW", jsonob["Row"].ToString());
            mapParms.Add("COL", jsonob["Col"].ToString());
            mapParms.Add("IMPORT_FILE", filename);
            

            
            DAO dao = new DAO();
            DAO daouuid = new DAO();
            Map sqlCreator = new Map();
            dynamic returnMsg = new JObject();
            var sqlStruuid = sqlCreator.GetUUID();
            var sqlParms = sqlCreator.CreateParameterAry(mapParms);
            daouuid.AddExecuteItem(sqlStruuid, sqlParms);
            var uuid = daouuid.Query().Tables[0].Rows[0].ItemArray[0].ToString();

            
            
            var sqlStr = sqlCreator.Insert(uuid);
            dao.AddExecuteItem(sqlStr, sqlParms);
            //dao.Execute();
            //returnMsg.result = dao.Execute();
            //if (returnMsg.result == true)
            //{
            //    returnMsg.guid = uuid;
            //}
            //return returnMsg;

            //Node
            parms.Add("MAP_GUID", uuid);
            var pointInfo = jsonob["PointInfo"].ToArray();
            for (var i = 0; i < pointInfo.Length; i++)
            {
                JObject nodeParms = new JObject();
                nodeParms = (JObject)parms.DeepClone();
                //nodeParms.Add("MAP_GUID", uuid);
                nodeParms.Add("ID", pointInfo[i]["id"].ToString());
                nodeParms.Add("NAME", pointInfo[i]["id"].ToString());
                nodeParms.Add("XPOS", pointInfo[i]["xpos"].ToString());
                nodeParms.Add("YPOS", pointInfo[i]["ypos"].ToString());
                string qrCodeX=Convert.ToString(Convert.ToInt32(Convert.ToDouble(pointInfo[i]["xpos"].ToString()) * 1000));
                
                while(qrCodeX.Length < 6)
                {
                    qrCodeX = "0" + qrCodeX;
                }
                string qrCodeY = Convert.ToString(Convert.ToInt32(Convert.ToDouble(pointInfo[i]["ypos"].ToString()) * 1000));
                
                while (qrCodeY.Length < 6)
                {
                    qrCodeY = "0" + qrCodeY;
                }
                nodeParms.Add("QRCODE", qrCodeX+ jsonob["MapQRCode"].ToString()+qrCodeY);
                nodeParms.Add("WIDTH", pointInfo[i]["width"].ToString());
                nodeParms.Add("HEIGHT", pointInfo[i]["height"].ToString());
                //nodeParms.Add("Value", pointInfo[i]["value"]["#text"].ToString());
                nodeParms.Add("VALUE", pointInfo[i]["value"].ToString());
                nodeParms.Add("ROADPROPERTY", pointInfo[i]["RoadProperty"].ToString());
                //nodeParms.Add("ROT", pointInfo[i]["Rot"].ToString());
                //nodeParms.Add("ALLDIRROT", pointInfo[i]["alldirRot"].ToString());
                nodeParms.Add("ROT", "");
                nodeParms.Add("ALLDIRROT", "");
                nodeParms.Add("ALLDIR", pointInfo[i]["allDir"].ToString());
                nodeParms.Add("ELEDIR", pointInfo[i]["EleDir"].ToString());
                nodeParms.Add("ELEPRE", pointInfo[i]["ElePre"].ToString());
                nodeParms.Add("ROTRAD", pointInfo[i]["RotRad"].ToString());
                nodeParms.Add("ROBOTTYPE", pointInfo[i]["robottype"].ToString());
                nodeParms.Add("ROTUNDERPOD", pointInfo[i]["RotUnderPod"]);
                nodeParms.Add("ROTMECH", pointInfo[i]["rotMech"].ToString());
                nodeParms.Add("EVIT", pointInfo[i]["evit"].ToString());
                nodeParms.Add("ROTBYROBOTTYPE", pointInfo[i]["rotByRobotType"].ToString());
                nodeParms.Add("ROTFORPODTYPE", pointInfo[i]["rotForPodType"].ToString());
                //nodeParms.Add("SENSORSWITCHPOINT", pointInfo[i]["sensorswitchPoint"].ToString());
                //nodeParms.Add("PALLET", pointInfo[i]["pallet"].ToString());
                nodeParms.Add("SENSORSWITCHPOINT", "");
                nodeParms.Add("PALLET", "");
                nodeParms.Add("TRANZONETYPE", GetJsonVal(pointInfo[i]["TranZoneType"].ToString(), "TranZoneType"));
                nodeParms.Add("ROTBARRIERAREA", pointInfo[i]["RotBarrierArea"].ToString());
                //nodeParms.Add("ISUPDATEMAP", pointInfo[i]["IsUpdateMap"].ToString());
                nodeParms.Add("ISUPDATEMAP", "");
                Node sqlNodeCreator = new Node();
                var sqlStrNodeuuid = sqlNodeCreator.GetUUID();
                var sqlNodeParms = sqlNodeCreator.CreateParameterAry(nodeParms);
                daouuid.AddExecuteItem(sqlStrNodeuuid, sqlNodeParms);
                var nodeuuid = daouuid.Query().Tables[0].Rows[0].ItemArray[0].ToString();                                
                //var sqlNodeStr = sqlNodeCreator.Insert(nodeuuid);
                var sqlNodeStr = sqlNodeCreator.Insert(nodeuuid, nodeParms);
                dao.AddExecuteItem(sqlNodeStr, sqlNodeParms);
                //dao.Execute();

                //Edge
                //var NeighbInfo = pointInfo[i]["NeighbInfo"].ToArray();
                var NeighbInfo = pointInfo[i]["NeighbInfo"].ToList();
                string Neighb_ID = GetJsonVal(NeighbInfo[0].ToString(), "id");
                string distance = GetJsonVal(NeighbInfo[1].ToString(), "distance");
                string Rever = GetJsonVal(NeighbInfo[2].ToString(), "Rever");
                string Speed = GetJsonVal(NeighbInfo[3].ToString(), "Speed");
                string Ultrasonic = GetJsonVal(NeighbInfo[4].ToString(), "Ultrasonic");
                string LeftWidth = GetJsonVal(NeighbInfo[5].ToString(), "LeftWidth");
                string RightWidth = GetJsonVal(NeighbInfo[6].ToString(), "RightWidth");
                string RoadLeftWidth = GetJsonVal(NeighbInfo[7].ToString(), "RoadLeftWidth");
                string RoadRightWidth = GetJsonVal(NeighbInfo[8].ToString(), "RoadRightWidth");
                string robottype2 = GetJsonVal(NeighbInfo[9].ToString(), "robottype");
                string PodDir = GetJsonVal(NeighbInfo[10].ToString(), "PodDir");
                string StartDir = GetJsonVal(NeighbInfo[11].ToString(), "StartDir");
                string StopDir = GetJsonVal(NeighbInfo[12].ToString(), "StopDir");
                string AlarmVoice = GetJsonVal(NeighbInfo[13].ToString(), "AlarmVoice");
                string PreForkLift = GetJsonVal(NeighbInfo[14].ToString(), "PreForkLift");
                string rotate = GetJsonVal(NeighbInfo[15].ToString(), "rotate");
                string LaserType = GetJsonVal(NeighbInfo[16].ToString(), "LaserType");
                string PodLaserType = GetJsonVal(NeighbInfo[17].ToString(), "PodLaserType");
                string SensorSwitch = GetJsonVal(NeighbInfo[18].ToString(), "SensorSwitch");
                string carryType = GetJsonVal(NeighbInfo[19].ToString(), "carryType");
                //string ctnrtype = GetJsonVal(NeighbInfo[20].ToString(), "ctnrtype");
                string ctnrtype = "-1";
                //string CtrlPoint = GetJsonVal(NeighbInfo[21].ToString(), "CtrlPoint
                string CtrlPoint = "";
                if (Neighb_ID.IndexOf(",") > 0)
                {
                    for (var j = 0; j < Neighb_ID.Split(',').Length; j++)
                    {
                        JObject edgeParms = new JObject();
                        edgeParms = (JObject)parms.DeepClone();
                        //edgeParms.Add("MAP_GUID", uuid);
                        edgeParms.Add("NODE_ID", pointInfo[i]["id"].ToString());
                        edgeParms.Add("NEIGHB_ID", Neighb_ID.Split(',')[j]);
                        edgeParms.Add("DISTANCE", distance.Split(',')[j]);
                        edgeParms.Add("REVER", Rever.Split(',')[j]);
                        edgeParms.Add("SPEED", Speed.Split(',')[j]);
                        edgeParms.Add("ULTRASONIC", Ultrasonic.Split(',')[j]);
                        edgeParms.Add("LEFTWIDTH", LeftWidth.Split(',')[j]);
                        edgeParms.Add("RIGHTWIDTH", RightWidth.Split(',')[j]);
                        edgeParms.Add("ROADLEFTWIDTH", RoadLeftWidth.Split(',')[j]);
                        edgeParms.Add("ROADRIGHTWIDTH", RoadRightWidth.Split(',')[j]);
                        edgeParms.Add("ROBOTTYPE", robottype2.Split(',')[j]);
                        edgeParms.Add("PODDIR", PodDir.Split(',')[j]);
                        edgeParms.Add("STARTDIR", StartDir.Split(',')[j]);
                        edgeParms.Add("STOPDIR", StopDir.Split(',')[j]);
                        edgeParms.Add("ALARMVOICE", AlarmVoice.Split(',')[j]);
                        edgeParms.Add("PREFORKLIFT", PreForkLift.Split(',')[j]);
                        edgeParms.Add("ROTATE", rotate.Split(',')[j]);
                        edgeParms.Add("LASERTYPE", LaserType.Split(',')[j]);
                        edgeParms.Add("PODLASERTYPE", PodLaserType.Split(',')[j]);
                        edgeParms.Add("SENSORSWITCH", SensorSwitch.Split(',')[j]);
                        edgeParms.Add("CARRYTYPE", carryType.Split(',')[j]);
                        //edgeParms.Add("CTNRTYPE", ctnrtype.Split(',')[j]);
                        edgeParms.Add("CTNRTYPE", "-1");
                        //edgeParms.Add("CTRLPOINT", CtrlPoint.Split(',')[j]);
                        edgeParms.Add("CTRLPOINT", "");
                        Edge sqlEdgeCreator = new Edge();
                        var sqlStrEdgeuuid = sqlEdgeCreator.GetUUID();
                        var sqlEdgeParms = sqlEdgeCreator.CreateParameterAry(edgeParms);
                        daouuid.AddExecuteItem(sqlStrEdgeuuid, sqlEdgeParms);
                        var edgeuuid = daouuid.Query().Tables[0].Rows[0].ItemArray[0].ToString();
                        //var sqlEdgeStr = sqlEdgeCreator.Insert(edgeuuid);
                        var sqlEdgeStr = sqlEdgeCreator.Insert(edgeuuid, edgeParms);
                        dao.AddExecuteItem(sqlEdgeStr, sqlEdgeParms);
                        //dao.Execute();
                    }

                }
                else
                {
                    JObject edgeParms = new JObject();
                    edgeParms = (JObject)parms.DeepClone();
                    //edgeParms.Add("MAP_GUID", uuid);
                    edgeParms.Add("NODE_ID", pointInfo[i]["id"].ToString());
                    edgeParms.Add("NEIGHB_ID", Neighb_ID);
                    edgeParms.Add("DISTANCE", distance);
                    edgeParms.Add("REVER", Rever);
                    edgeParms.Add("SPEED", Speed);
                    edgeParms.Add("ULTRASONIC", Ultrasonic);
                    edgeParms.Add("LEFTWIDTH", LeftWidth);
                    edgeParms.Add("RIGHTWIDTH", RightWidth);
                    edgeParms.Add("ROADLEFTWIDTH", RoadLeftWidth);
                    edgeParms.Add("ROADRIGHTWIDTH", RoadRightWidth);
                    edgeParms.Add("ROBOTTYPE", robottype2);
                    edgeParms.Add("PODDIR", PodDir);
                    edgeParms.Add("STARTDIR", StartDir);
                    edgeParms.Add("STOPDIR", StopDir);
                    edgeParms.Add("ALARMVOICE", AlarmVoice);
                    edgeParms.Add("PREFORKLIFT", PreForkLift);
                    edgeParms.Add("ROTATE", rotate);
                    edgeParms.Add("LASERTYPE", LaserType);
                    edgeParms.Add("PODLASERTYPE", PodLaserType);
                    edgeParms.Add("SENSORSWITCH", SensorSwitch);
                    edgeParms.Add("CARRYTYPE", carryType);
                    edgeParms.Add("CTNRTYPE", ctnrtype);
                    edgeParms.Add("CTRLPOINT", CtrlPoint);
                    Edge sqlEdgeCreator = new Edge();
                    var sqlStrEdgeuuid = sqlEdgeCreator.GetUUID();
                    var sqlEdgeParms = sqlEdgeCreator.CreateParameterAry(edgeParms);
                    daouuid.AddExecuteItem(sqlStrEdgeuuid, sqlEdgeParms);
                    var edgeuuid = daouuid.Query().Tables[0].Rows[0].ItemArray[0].ToString();
                    //var sqlEdgeStr = sqlEdgeCreator.Insert(edgeuuid);
                    var sqlEdgeStr = sqlEdgeCreator.Insert(edgeuuid, edgeParms);
                    dao.AddExecuteItem(sqlEdgeStr, sqlEdgeParms);
                    //dao.Execute();
                }
            }
            dao.Execute();
            //匯入儲位
            JObject storage = new JObject();
            storage.Add("MAP_GUID", uuid);
            var parmStorage= sqlCreator.CreateParameterAry(storage);
            var sqlStrStorage = sqlCreator.InsertStorage(); ;
            dao.AddExecuteItem(sqlStrStorage, parmStorage);
            dao.Execute();
        }
        public void AGVMAPXMLToDB(string filename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //XmlDocument xmlDoc2 = new XmlDocument();
            string savePath_tmp = ConfigurationManager.AppSettings["AGV_MapUpload"];
            xmlDoc.Load(HttpContext.Current.Server.MapPath(savePath_tmp + "\\" + filename));//載入xml檔
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
                //string ctnrtype = GetJsonVal(bbb[20].ToString(), "ctnrtype");
                string ctnrtype = "-1";
                //string CtrlPoint = GetJsonVal(bbb[21].ToString(), "CtrlPoint");
                string CtrlPoint = "";
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
                               //+ CtrlPoint.Split(',')[j] + "'"
                               + CtrlPoint + "'"
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
            //if (Jsonkey == "id" || Jsonkey == "TranZoneType")
            if (Jsonkey == "TranZoneType")
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
            ///var aaa = jsonob[Jsonkey].ToList();

            if (js_coun == 0)
            {
                re_bbb = jsonob[str3].ToString();
                return re_bbb;
            }
            else
            {
                //if (Jsonkey == "id")
                //{
                //    if (aaa[0].ToString().Length < 15)
                //    {
                //        re_bbb = aaa[1].ToString().Replace("\"", "").Split(':')[1];
                //    }
                //    else
                //    {
                //        for (var i = 0; i < js_coun; i++)
                //        {
                //            re_bbb += aaa[i][str3].ToString();
                //            if (i < js_coun - 1)
                //            {
                //                re_bbb += ",";
                //            }
                //        }
                //    }
                //}
                //else
                //{
                    for (var i = 0; i < js_coun; i++)
                    {
                        re_bbb += aaa[i].ToString();
                        if (i < js_coun - 1)
                        {
                            re_bbb += ",";
                        }
                    }
                //}

            }
            re_bbb = re_bbb.Replace(" ", "");
            return re_bbb;
        }
    }
}