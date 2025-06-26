using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using MQTTnet.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;

using MCS.Controllers;

namespace MCS.Controllers
{

    public class Lower_MqttController : Controller
    {       
        public ActionResult Index()
        {
            return View("~/Views/Cmd/Mqtt.cshtml");            
        }        
    }

    /// <summary>
    /// MQTT連線物件
    /// </summary>
    public class CmdMqttTools
    {
        public static List<MQTTnet.MqttApplicationMessageReceivedEventArgs> _subscribedList = new List<MQTTnet.MqttApplicationMessageReceivedEventArgs>();
        string _clientId = "";

        string MQTT_IP = ConfigurationManager.AppSettings["MQTT_IP"];
        int MQTT_Port = int.Parse(ConfigurationManager.AppSettings["MQTT_Port"]);
        string MQTT_User = ConfigurationManager.AppSettings["MQTT_Usr"];
        string MQTT_Password = ConfigurationManager.AppSettings["MQTT_Psw"];
        int QualityOfServiceLevel = 0;
        bool Retained = false;


        public delegate void SubscribeMessageReceived(MQTTnet.MqttApplicationMessageReceivedEventArgs e);

        /// <summary>
        /// 發布MQTT
        /// </summary>
        /// <param name="Topic">主題</param>
        /// <param name="Message">訊息</param>
        public async System.Threading.Tasks.Task<Object> Publish(string Topic, string Message)
        {
            dynamic returnMsg = new Newtonsoft.Json.Linq.JObject();


            try
            {
                // 檢查 MQTT Client 是否有執行實體
                MQTTnet.Client.MqttClient client = new MQTTnet.MqttFactory().CreateMqttClient() as MQTTnet.Client.MqttClient;

                // 建立 MQTT Options
                var options = new MQTTnet.Client.Options.MqttClientOptionsBuilder()
                    .WithTcpServer(MQTT_IP, MQTT_Port)
                    .WithCredentials(MQTT_User, MQTT_Password)
                    .Build();
                MQTTnet.Client.Options.MqttClientOptions mqttOptions = options as MQTTnet.Client.Options.MqttClientOptions;

                // 建立連線
                await client.ConnectAsync(mqttOptions);


                // 建立 MqttApplicationMessage
                MQTTnet.MqttApplicationMessageBuilder mamb = new MQTTnet.MqttApplicationMessageBuilder()
                    .WithTopic(Topic)
                    .WithPayload(Message).WithRetainFlag(Retained);
                if (QualityOfServiceLevel == 0)
                {
                    mamb = mamb.WithAtMostOnceQoS();
                }
                else if (QualityOfServiceLevel == 1)
                {
                    mamb = mamb.WithAtLeastOnceQoS();
                }
                else if (QualityOfServiceLevel == 2)
                {
                    mamb = mamb.WithExactlyOnceQoS();
                }

                // 發布訊息
                await client.PublishAsync(mamb.Build());
                await client.DisconnectAsync();

                returnMsg.result = true;
                returnMsg.Add("msg", "Published.");
                return returnMsg;
            }
            catch (Exception ex)
            {
                returnMsg.result = false;
                returnMsg.Add("msg", ex.Message);
                return returnMsg;
            }
        }

        public async System.Threading.Tasks.Task<Object> Subscribe(string ClientId, string Topic)
        {
            dynamic returnMsg = new JObject();

            try
            {
                // 檢查 MQTT Client 是否有執行實體
                MQTTnet.Client.MqttClient client = new MQTTnet.MqttFactory().CreateMqttClient() as MQTTnet.Client.MqttClient;
                client.ApplicationMessageReceivedHandler = new MQTTnet.Client.Receiving.MqttApplicationMessageReceivedHandlerDelegate(new Action<MQTTnet.MqttApplicationMessageReceivedEventArgs>(ApplicationMessageReceived));
                client.DisconnectedHandler = new MQTTnet.Client.Disconnecting.MqttClientDisconnectedHandlerDelegate(new Func<MQTTnet.Client.Disconnecting.MqttClientDisconnectedEventArgs, System.Threading.Tasks.Task>(Disconnected));


                // 建立 MQTT Options
                var options = new MQTTnet.Client.Options.MqttClientOptionsBuilder()
                     .WithTcpServer(MQTT_IP, MQTT_Port)
                     .WithCredentials(MQTT_User, MQTT_Password)
                     .WithKeepAlivePeriod(TimeSpan.FromMinutes(5))
                     .Build();
                MQTTnet.Client.Options.MqttClientOptions mqttOptions = options as MQTTnet.Client.Options.MqttClientOptions;
                if (ClientId.Length > 0) { mqttOptions.ClientId = ClientId; }

                // 建立連線
                await client.ConnectAsync(mqttOptions);


                // 建立 Topic List
                List<MQTTnet.MqttTopicFilter> listTopic = new List<MQTTnet.MqttTopicFilter>();
                if (listTopic.Count() <= 0)
                {
                    var topicFilterBulder = new MQTTnet.MqttTopicFilterBuilder().WithTopic(Topic).Build();
                    listTopic.Add(topicFilterBulder);
                }

                // 訂閱 Topic List
                _clientId = options.ClientId;
                var subscribeResult = await client.SubscribeAsync(listTopic.ToArray());


                returnMsg.result = true;
                returnMsg.Add("msg", "Subscribed.");
                returnMsg.Add("clientId", client.Options.ClientId);
                return returnMsg;
            }
            catch (Exception ex)
            {
                returnMsg.result = false;
                returnMsg.Add("msg", ex.Message);
                return returnMsg;
            }
        }

        // 離線時執行此 Function
        public async System.Threading.Tasks.Task Disconnected(MQTTnet.Client.Disconnecting.MqttClientDisconnectedEventArgs e)
        {
            try
            {
                // 等待 5 秒
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));


                if (_subscribedList.Count > 0)
                {
                    List<MQTTnet.MqttApplicationMessageReceivedEventArgs> loop = new List<MQTTnet.MqttApplicationMessageReceivedEventArgs>(_subscribedList);
                    foreach (MQTTnet.MqttApplicationMessageReceivedEventArgs args in loop)
                    {
                        string ClientId = args.ClientId;

                        if (args.ClientId.Equals(this._clientId))
                        {
                            _subscribedList.Remove(args);
                        }
                    }
                }

                // 重新連線
                try
                {
                    //await mqttClient.ConnectAsync(mqttOptions);
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

        // 收到訊息時執行此 Function
        public void ApplicationMessageReceived(MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                _subscribedList.Add(e);


                string Status = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            }
            catch (Exception ex)
            {

            }
        }
    }

    public class CmdMqttCommands
    {
        public string GetCommandJson(JObject obj)
        {
            dynamic inputs = obj as dynamic;
            JObject message = new JObject();
            string command_json = "";
            //string snvalue = !string.IsNullOrEmpty((string)inputs.IOTDEVICE_IP) ? (string)inputs.IOTDEVICE_IP : "";
            string divalue = !string.IsNullOrEmpty((string)inputs.DI_COMMAND) ? (string)inputs.DI_COMMAND : "";
            string dovalue = !string.IsNullOrEmpty((string)inputs.DO_COMMAND) ? (string)inputs.DO_COMMAND : "";
            //string datetimevalue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //D0
            if (!string.IsNullOrEmpty(dovalue))
            {
                message.Add("DO", "[" + dovalue + "]");
                message.Add("mcsdatetime", "MCSDATETIME");
                command_json = JsonConvert.SerializeObject(message);
                command_json = command_json.Replace("\"[", "[").Replace("]\"", "]");
            }
            else
            {
                command_json = "["+divalue+"]";
            }

            
            //message.Add("SN", snvalue);
            //message.Add("DI", "[" + divalue + "]");
           // message.Add("DO", "[" + dovalue + "]");
            //message.Add("datetime", datetimevalue);

            //string command_json = JsonConvert.SerializeObject(message);

            return command_json;
        }

        public JObject GetCmdTopicsByName(JObject obj)
        {
            dynamic returnMsg = new JObject();

            CmdMqtt sqlCreator = new CmdMqtt();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetTopicInfoByNAME();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            dynamic vals = returnVal as dynamic;
            JObject topic = new JObject();
            if (vals.rows.Count > 0)
            {
                string floorId = !string.IsNullOrEmpty((string)vals.rows[0].FLOOR_ID) ? (string)vals.rows[0].FLOOR_ID : "";
                string floorName = !string.IsNullOrEmpty((string)vals.rows[0].FLOOR_NAME) ? (string)vals.rows[0].FLOOR_NAME : "";
                string iotDeviceSnkey = !string.IsNullOrEmpty((string)vals.rows[0].IOTDEVICE_SNKEY) ? (string)vals.rows[0].IOTDEVICE_SNKEY : "";

                if (floorName.Length > 0 && iotDeviceSnkey.Length > 0)
                {
                    string cmdType = !string.IsNullOrEmpty((string)vals.rows[0].TYPE) ? (string)vals.rows[0].TYPE : "";
                    string publishTopic = "MCS/" + floorName + "/" + iotDeviceSnkey + "/" + (cmdType.ToUpper().Equals("DO") ? "control" : "read");
                    string subscribeTopic = "MCS/" + floorName + "/" + iotDeviceSnkey + "/state";
                    string sTopic = "MCS/" + floorName + "/" + iotDeviceSnkey;
                    topic.Add("PTOPIC", publishTopic);
                    topic.Add("STOPIC", subscribeTopic);
                    topic.Add("TOPIC", sTopic);
                }
            }

            returnVal.Add("topic", topic);
            return returnVal;
        }

        public JObject GetCmdTopicsById(JObject obj)
        {
            dynamic returnMsg = new JObject();

            CmdMqtt sqlCreator = new CmdMqtt();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetTopicInfoByID();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            dynamic vals = returnVal as dynamic;
            JObject topic = new JObject();
            if (vals.rows.Count > 0)
            {
                string floorId = !string.IsNullOrEmpty((string)vals.rows[0].FLOOR_ID) ? (string)vals.rows[0].FLOOR_ID : "";
                string floorName = !string.IsNullOrEmpty((string)vals.rows[0].FLOOR_NAME) ? (string)vals.rows[0].FLOOR_NAME : "";
                string iotDeviceSnkey = !string.IsNullOrEmpty((string)vals.rows[0].IOTDEVICE_SNKEY) ? (string)vals.rows[0].IOTDEVICE_SNKEY : "";

                if (floorName.Length > 0 && iotDeviceSnkey.Length > 0)
                {
                    string cmdType = !string.IsNullOrEmpty((string)vals.rows[0].TYPE) ? (string)vals.rows[0].TYPE : "";
                    string publishTopic = "MCS/" + floorName + "/" + iotDeviceSnkey + "/" + (cmdType.ToUpper().Equals("DO") ? "control" : "read");
                    string subscribeTopic = "MCS/" + floorName + "/" + iotDeviceSnkey + "/state";
                    string sTopic = "MCS/" + floorName + "/" + iotDeviceSnkey;
                    topic.Add("PTOPIC", publishTopic);
                    topic.Add("STOPIC", subscribeTopic);
                    topic.Add("TOPIC", sTopic);
                }
            }

            returnVal.Add("topic", topic);
            return returnVal;
        }

        public JObject GetCmdTopicsByGUID(JObject obj)
        {
            dynamic returnMsg = new JObject();

            CmdMqtt sqlCreator = new CmdMqtt();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetTopicInfoByGUID();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            dynamic vals = returnVal as dynamic;
            JObject topic = new JObject();
            if (vals.rows.Count > 0)
            {
                string floorId = !string.IsNullOrEmpty((string)vals.rows[0].FLOOR_ID) ? (string)vals.rows[0].FLOOR_ID : "";
                string floorName = !string.IsNullOrEmpty((string)vals.rows[0].FLOOR_NAME) ? (string)vals.rows[0].FLOOR_NAME : "";
                string iotDeviceSnkey = !string.IsNullOrEmpty((string)vals.rows[0].IOTDEVICE_SNKEY) ? (string)vals.rows[0].IOTDEVICE_SNKEY : "";

                if (floorName.Length > 0 && iotDeviceSnkey.Length > 0)
                {
                    string cmdType = !string.IsNullOrEmpty((string)vals.rows[0].TYPE) ? (string)vals.rows[0].TYPE : "";
                    string publishTopic = "MCS/" + floorName + "/" + iotDeviceSnkey + "/" + (cmdType.ToUpper().Equals("DO") ? "control" : "read");
                    string subscribeTopic = "MCS/" + floorName + "/" + iotDeviceSnkey + "/state";
                    string sTopic = "MCS/" + floorName + "/" + iotDeviceSnkey;
                    topic.Add("PTOPIC", publishTopic);
                    topic.Add("STOPIC", subscribeTopic);
                    topic.Add("TOPIC", sTopic);
                }
            }

            returnVal.Add("topic", topic);
            return returnVal;
        }
    }
}

public class ApiCmdMqttController : ApiController, IJqOneTable
{  
    [System.Web.Http.HttpPost]
    public JObject Count(JObject obj)
    {
        CmdMqtt sqlCreator = new CmdMqtt();
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
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();

        var sqlStr = sqlCreator.Delete();
        var sqlParms = sqlCreator.CreateParameterAry(obj);

        dao.AddExecuteItem(sqlStr, sqlParms);

        returnMsg.result = dao.Execute();

        return returnMsg;
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

    [System.Web.Http.HttpPost]
    public JObject GetOneByGUID(JObject obj)
    {
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();

        var sqlStr = sqlCreator.GetOneByGUID();
        var sqlParms = sqlCreator.CreateParameterAry(obj);

        dao.AddExecuteItem(sqlStr, sqlParms);

        var data = dao.Query().Tables[0];

        var command_json = data.Rows[0]["CONTENT"].ToString();

        /*{"DO":[0,1],"mcsdatetime":"MCSDATETIME"}*/
        command_json = command_json.Replace("\"","");
        command_json = command_json.Replace("{DO:", "");
        command_json = command_json.Replace(",mcsdatetime:MCSDATETIME}", "");
        command_json = command_json.Replace("[", "");
        command_json = command_json.Replace("]", "");
        

        data.Rows[0]["CONTENT"] = command_json;

        var returnVal = extensions.ConvertDataTableToJObject(data);

        return returnVal;
    }

    [System.Web.Http.HttpPost]
    public JObject GetCmdTopicsByGUID(JObject obj)
    {
        dynamic returnMsg = new JObject();

        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        DataTableExtensions extensions = new DataTableExtensions();

        var sqlStr = sqlCreator.GetTopicInfoByGUID();
        var sqlParms = sqlCreator.CreateParameterAry(obj);

        dao.AddExecuteItem(sqlStr, sqlParms);

        var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

        dynamic vals = returnVal as dynamic;        
        JObject topic = new JObject();
        if (vals.rows.Count > 0)
        {
            string floorId = !string.IsNullOrEmpty((string)vals.rows[0].FLOOR_ID) ? (string)vals.rows[0].FLOOR_ID : "";
            string floorName = !string.IsNullOrEmpty((string)vals.rows[0].FLOOR_NAME) ? (string)vals.rows[0].FLOOR_NAME : "";
            string iotDeviceSnkey = !string.IsNullOrEmpty((string)vals.rows[0].IOTDEVICE_SNKEY) ? (string)vals.rows[0].IOTDEVICE_SNKEY : "";

            if (floorName.Length > 0 && iotDeviceSnkey.Length > 0)
            {
                string cmdType = !string.IsNullOrEmpty((string)vals.rows[0].TYPE) ? (string)vals.rows[0].TYPE : "";
                string publishTopic = "MCS/" + floorName + "/" + iotDeviceSnkey + "/" + (cmdType.ToUpper().Equals("DO") ? "control" : "read");
                string subscribeTopic = "MCS/" + floorName + "/" + iotDeviceSnkey + "/state";
                string sTopic = "MCS/" + floorName + "/" + iotDeviceSnkey;
                topic.Add("PTOPIC", publishTopic);
                topic.Add("STOPIC", subscribeTopic);
                topic.Add("TOPIC", sTopic);
            }
        }

        returnVal.Add("topic", topic);
        return returnVal;
    }

    public JObject Import(JObject obj)
    {
        throw new NotImplementedException();
    }

    public JObject Check(JObject obj)
    {
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();
        var sqlStr = sqlCreator.Check();
        var sqlParms = sqlCreator.CreateParameterAry(obj);
        dao.AddExecuteItem(sqlStr, sqlParms);
        if (dao.Query().Tables[0].Rows.Count == 0)
        {
            returnMsg.repeat = true;

        }
        else
        {
            returnMsg.repeat = false;

        }
        return returnMsg;
    }

    public JObject Repeat(JObject obj)
    {
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();
        var sqlStr = sqlCreator.Check();
        var sqlParms = sqlCreator.CreateParameterAry(obj);
        dao.AddExecuteItem(sqlStr, sqlParms);
        if (dao.Query().Tables[0].Rows.Count == 0)
        {
            returnMsg.repeat = true;
        }
        else
        {
            returnMsg.repeat = false;
        }
        return returnMsg;
    }

    public JObject Insert(JObject obj)
    {
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();

        returnMsg = CheckInsert(obj);
        if (returnMsg.result == false)
        {
            return returnMsg;
        }


        var sqlStruuid = sqlCreator.GetUUID();
        dao.AddExecuteItem(sqlStruuid, null);
        var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();

        string command_json = new CmdMqttCommands().GetCommandJson(obj);
        obj.Add("COMMAND_JSON", command_json);

        var sqlStr = sqlCreator.Insert(uuid);
        var sqlParms = sqlCreator.CreateParameterAry(obj);
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
        CmdMqtt sqlCreator = new CmdMqtt();
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
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();

        returnMsg = CheckUpdate(obj);
        if (returnMsg.result == false)
        {
            return returnMsg;
        }


        string command_json = new CmdMqttCommands().GetCommandJson(obj);
        obj.Add("COMMAND_JSON", command_json);

        var sqlStr = sqlCreator.Update();
        var sqlParms = sqlCreator.CreateParameterAry(obj);

        dao.AddExecuteItem(sqlStr, sqlParms);

        returnMsg.result = dao.Execute();

        return returnMsg;
    }    

    public dynamic CheckInsert(JObject obj)
    {
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();

        dynamic inputs = obj as dynamic;
        returnMsg.result = true;


        //檢查未輸入的資料
        var iotDeviceGuid = !string.IsNullOrEmpty((string)inputs.IOTDEVICE_GUID) ? (string)inputs.IOTDEVICE_GUID : "";
        var iotDeviceIP = !string.IsNullOrEmpty((string)inputs.IOTDEVICE_IP) ? (string)inputs.IOTDEVICE_IP : "";
        var command = !string.IsNullOrEmpty((string)inputs.COMMAND) ? (string)inputs.COMMAND : "";
        var cmdName = !string.IsNullOrEmpty((string)inputs.NAME) ? (string)inputs.NAME : "";
        var cmdType = !string.IsNullOrEmpty((string)inputs.TYPE) ? (string)inputs.TYPE : "";
        var content = !string.IsNullOrEmpty((string)inputs.CONTENT) ? (string)inputs.CONTENT : "";

        if (iotDeviceGuid == "" || iotDeviceIP == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "IOT資訊相關資料輸入不完整");
            return returnMsg;
        }
        if (command == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "命令ID尚未輸入");
            return returnMsg;
        }
        if (cmdName == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "命令名稱尚未輸入");
            return returnMsg;
        }
        if (cmdType == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "命令類型尚未選擇");
            return returnMsg;
        }
        if (content == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "命令內容尚未輸入");
            return returnMsg;
        }

        //檢查資料格式




        //檢查資料是否重複
        var sqlStr = sqlCreator.Check();
        var sqlParms = sqlCreator.CreateParameterAry(obj);
        dao.AddExecuteItem(sqlStr, sqlParms);
        if (dao.Query().Tables[0].Rows.Count != 0)
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "命令ID重複，請重新填寫");
            return returnMsg;
        }


        return returnMsg;
    }

    public dynamic CheckUpdate(JObject obj)
    {
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();

        dynamic inputs = obj as dynamic;
        returnMsg.result = true;


        //檢查未輸入的資料
        var iotDeviceGuid = !string.IsNullOrEmpty((string)inputs.IOTDEVICE_GUID) ? (string)inputs.IOTDEVICE_GUID : "";
        var iotDeviceIP = !string.IsNullOrEmpty((string)inputs.IOTDEVICE_IP) ? (string)inputs.IOTDEVICE_IP : "";
        var command = !string.IsNullOrEmpty((string)inputs.COMMAND) ? (string)inputs.COMMAND : "";
        var cmdName = !string.IsNullOrEmpty((string)inputs.NAME) ? (string)inputs.NAME : "";
        var cmdType = !string.IsNullOrEmpty((string)inputs.TYPE) ? (string)inputs.TYPE : "";
        var content = !string.IsNullOrEmpty((string)inputs.CONTENT) ? (string)inputs.CONTENT : "";

        if (iotDeviceGuid == "" || iotDeviceIP == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "IOT資訊相關資料輸入不完整");
            return returnMsg;
        }
        if (command == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "命令ID尚未輸入");
            return returnMsg;
        }
        if (cmdName == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "命令名稱尚未輸入");
            return returnMsg;
        }
        if (cmdType == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "命令類型尚未選擇");
            return returnMsg;
        }
        if (content == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "命令內容尚未輸入");
            return returnMsg;
        }

        //檢查資料格式




        //檢查資料是否重複
        //var sqlStr = sqlCreator.Check();
        //var sqlParms = sqlCreator.CreateParameterAry(obj);
        //dao.AddExecuteItem(sqlStr, sqlParms);
        //if (dao.Query().Tables[0].Rows.Count != 0)
        //{
        //    returnMsg.result = false;
        //    returnMsg.Add("msg", "命令ID重複，請重新填寫");
        //    return returnMsg;
        //}


        return returnMsg;
    }

    public dynamic CheckTest(JObject obj)
    {
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();

        dynamic inputs = obj as dynamic;
        returnMsg.result = true;


        //檢查未輸入的資料
        var topic = !string.IsNullOrEmpty((string)inputs.TOPIC) ? (string)inputs.TOPIC : "";        

        if (topic == "")
        {
            returnMsg.result = false;
            returnMsg.Add("msg", "請輸入頻道");
            return returnMsg;
        }
     

        return returnMsg;
    }

    [System.Web.Http.HttpPost]
    public async System.Threading.Tasks.Task<JObject> PublishCmdMqtt(JObject obj)
    {
        CmdMqtt sqlCreator = new CmdMqtt();
        DAO dao = new DAO();
        dynamic returnMsg = new JObject();

        returnMsg = CheckTest(obj);
        if (returnMsg.result == false)
        {
            return returnMsg;
        }


        DataTableExtensions extensions = new DataTableExtensions();

        dynamic inputs = obj as dynamic;
        var topic = !string.IsNullOrEmpty((string)inputs.TOPIC) ? (string)inputs.TOPIC : "";


        var sqlStr = sqlCreator.GetOneByGUID();
        var sqlParms = sqlCreator.CreateParameterAry(obj);
        dao.AddExecuteItem(sqlStr, sqlParms);
        var getVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
        dynamic vals = getVal as dynamic;
        string command_json = !string.IsNullOrEmpty((string)vals.rows[0].COMMAND_JSON) ? (string)vals.rows[0].COMMAND_JSON : "";
        dynamic commands = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(command_json);

        string snvalue = !string.IsNullOrEmpty((string)commands.SN) ? (string)commands.SN : "";
        string divalue = !string.IsNullOrEmpty((string)commands.DI) ? (string)commands.DI : "";
        string dovalue = !string.IsNullOrEmpty((string)commands.DO) ? (string)commands.DO : "";
        string datetimevalue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        JObject message = new JObject();
        message.Add("SN", snvalue);
        message.Add("DI", divalue);
        message.Add("DO", dovalue);
        message.Add("datetime", datetimevalue);
        command_json = JsonConvert.SerializeObject(message);

        //-- Publish --
        CmdMqttTools objCmdMqtt = new CmdMqttTools();
        //returnMsg = await this.Publish(topic, command_json);
        returnMsg = await objCmdMqtt.Publish(topic, command_json);

        return returnMsg;
    }

    [System.Web.Http.HttpPost]
    public async System.Threading.Tasks.Task<JObject> SubscribeCmdMqtt(JObject obj)
    {
        dynamic returnMsg = new JObject();

        returnMsg = CheckTest(obj);
        if (returnMsg.result == false)
        {
            return returnMsg;
        }


        dynamic inputs = obj as dynamic;
        var clientId = !string.IsNullOrEmpty((string)inputs.CLIENTID) ? (string)inputs.CLIENTID : "";
        var topic = !string.IsNullOrEmpty((string)inputs.TOPIC) ? (string)inputs.TOPIC : "";

        //-- Subscribe --
        CmdMqttTools objCmdMqtt = new CmdMqttTools();
        returnMsg = await objCmdMqtt.Subscribe(clientId, topic);

        return returnMsg;
    }

    [System.Web.Http.HttpPost]
    public JObject GetMsgSubscribed(JObject obj)
    {
        dynamic returnMsg = new JObject();

        try
        {
            dynamic inputs = obj as dynamic;
            string clientId = !string.IsNullOrEmpty((string)inputs.ClientId) ? (string)inputs.ClientId : "";


            string msgDisplay = "";
            
            if (CmdMqttTools._subscribedList.Count > 0)
            {
                List<MQTTnet.MqttApplicationMessageReceivedEventArgs> loop = new List<MQTTnet.MqttApplicationMessageReceivedEventArgs>(CmdMqttTools._subscribedList);
                foreach (MQTTnet.MqttApplicationMessageReceivedEventArgs args in loop)
                {
                    if (args.ClientId.Equals(clientId))
                    {
                        string Topic = args.ApplicationMessage.Topic;
                        string Status = System.Text.Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

                        msgDisplay += "Topic: " + Topic + Environment.NewLine +
                            "Payload: " + Status + Environment.NewLine +
                            "    -------------------------------------    " + Environment.NewLine;
                        CmdMqttTools._subscribedList.Remove(args);
                    }
                }
            }


            returnMsg.result = true;
            returnMsg.Add("msg", msgDisplay.Length > 0 ? msgDisplay : "-- No latest message --" + Environment.NewLine);

            return returnMsg;
        }
        catch (Exception ex)
        {
            returnMsg.result = false;
            returnMsg.Add("msg", ex.Message);

            return returnMsg;
        }
    }
}