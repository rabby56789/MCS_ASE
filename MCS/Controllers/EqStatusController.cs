using JQWEB.Controllers;
using JQWEB.Models;
using log4net;
using MCS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace MCS.Controllers
{
    public class EqStatusController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Task/EqStatus.cshtml");
        }
    }
    public class apiEqptStatusController : ApiController, IJqOneTable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string JOB_QueryIOTStatus = "[JOBS_QueryIOTStatus] ";
        EqptStatus sqlCreator = new EqptStatus();
        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            EqptStatus sqlCreator = new EqptStatus();
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
            throw new NotImplementedException();
        }

        public JObject Export(JObject obj)
        {
            throw new NotImplementedException();
        }

        [System.Web.Http.HttpPost]
        public JObject GetOneByGUID(JObject obj)
        {
            throw new NotImplementedException();
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
        public JObject QueryDIDO(JObject obj)
        {
            DAO dao = new DAO();
            JObject returnVal = new JObject();
            DataTableExtensions extensions = new DataTableExtensions();
            string sqlDIDO = sqlCreator.DIDO();
            var parm = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlDIDO, parm);

            returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }
        public JObject Update(JObject obj)
        {
            throw new NotImplementedException();
        }

        public JObject execute(JObject obj)
        {
            StorageStatus sqlCreator = new StorageStatus();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();            

            if ("1" == obj["isLock"].ToString())
            {
                throw new Exception("Not supported to lock with web UI");
            }
            else
            {
                string _guid = obj["GUID"].ToString();
                string _sqlString = $@"delete from mcs.t_storage_status where GUID = '{_guid}';";
                dao.AddExecuteItem(_sqlString, null);
                returnMsg.result = dao.Execute();
                return returnMsg;
            }

            //var sqlStr = sqlCreator.executeLock();
            //var sqlParms = sqlCreator.CreateParameterAry(obj);

            //dao.AddExecuteItem(sqlStr, sqlParms);

            //returnMsg.result = dao.Execute();

            //return returnMsg;
        }

        /// <summary>
        /// 頁面載入時下拉選單取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetOption(JObject obj)
        {
            EqptStatus sqlCreator = new EqptStatus();
            DAO dao = new DAO();
            var sqlStr = sqlCreator.GetOption();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            DataTableExtensions extensions = new DataTableExtensions();
            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            return returnVal;
        }

        public JObject ImportMQTT()//從MQTT API 匯入IOT資訊
        {
            dynamic returnMsg = new JObject();
            returnMsg.result = true;
            try
            {
                string IOT_IP = ConfigurationManager.AppSettings["IOT_IP"];
                string IOT_gkey = ConfigurationManager.AppSettings["IOT_gkey"];
                //http://192.168.56.98/eztiot_EV01/API/select_machine_state_all?gkey=ffb3bd2319d4f6a061f38882bb504e7f


                var APIurl = "http://" + IOT_IP + "/eztiot_EV01/API/select_machine_state_all?gkey=" + IOT_gkey;
                

                //test
                //APIurl = "http://localhost:44314/api/ProductsApp/CallFunc";


                if (string.IsNullOrEmpty(APIurl))
                {
                    Logger.Info($"{JOB_QueryIOTStatus}  url is null or empty");
                    returnMsg.Add("msg", $"url is null or empty");
                    returnMsg.result = false;
                    return returnMsg;
                }



                Logger.Info($"{JOB_QueryIOTStatus}   URL : " + APIurl);

                JObject parameter = new JObject();
                string IOTMessage_result = null;

                parameter.Add("reqCode", "MCS_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")); //no use


                //發IOT API 取得IOT設備
                IOTMessage_result = Forward_IOTMessage(parameter, APIurl);

                if (IOTMessage_result == null)
                {
                    Logger.Info($"{JOB_QueryIOTStatus}  IOTMessage_result : null ");
                    returnMsg.Add("msg", "IOTMessage_result : null ");
                    returnMsg.result = false;
                    return returnMsg;
                }
                else if (IOTMessage_result.Contains("errmessage"))
                {
                    Logger.Info($"{JOB_QueryIOTStatus}  IOTMessage_result errmessage :  " + IOTMessage_result);
                    returnMsg.Add("msg", $"IOTMessage_result errmessage : " + IOTMessage_result);
                    returnMsg.result = false;
                    return returnMsg;
                }

                Logger.Info($"{JOB_QueryIOTStatus}   IOTMessage_result errmessage :  \n" + IOTMessage_result);

                #region 寫入DB
                dynamic[] IOTdata = JsonConvert.DeserializeObject<JObject[]>(IOTMessage_result.ToString());
                int IOTdata_count = IOTdata.Count();

                for (int i = 0; i < IOTdata_count; i++) //巡迴每一筆IOT
                {
                    JObject IOTObj = new JObject();
                    DAO dao = new DAO();

                    string id_str = "";
                    string machine_title_str = "";
                    string snkey_str = "";
                    string area_str = "";
                    string areasn_str = "";
                    string ip_str = "";
                    string internet_str = "";
                    string select_time_str = "";
                    string state_str = "";
                    string error_state_str = "";

                    IOTObj = IOTdata[i];

                    Logger.Info($"{JOB_QueryIOTStatus}  COUNT : {IOTdata_count}  IOTObj :  \n" + IOTObj);

                    if (IOTObj.Property("snkey") != null)
                    {
                        snkey_str = IOTObj.Property("snkey").Value.ToString();
                    }
                    else
                    {
                        //SN KEY 沒有直接跳下筆資料
                        Logger.Info($"{JOB_QueryIOTStatus} IOTObj not correct ! ");
                        continue;
                    }

                    //再檢查一次snkey
                    if (string.IsNullOrEmpty(snkey_str))
                    {
                        Logger.Info($"{JOB_QueryIOTStatus} snkey is Null ! ");
                        continue;
                    }

                    if (IOTObj.Property("id") != null)
                    {
                        id_str = IOTObj.Property("id").Value.ToString();
                    }

                    if (IOTObj.Property("machine_title") != null)
                    {
                        machine_title_str = IOTObj.Property("machine_title").Value.ToString();
                    }

                    if (IOTObj.Property("area") != null)
                    {
                        area_str = IOTObj.Property("area").Value.ToString();
                    }

                    if (IOTObj.Property("areasn") != null)
                    {
                        areasn_str = IOTObj.Property("areasn").Value.ToString();
                    }

                    if (IOTObj.Property("ip") != null)
                    {
                        ip_str = IOTObj.Property("ip").Value.ToString();
                    }

                    if (IOTObj.Property("internet") != null)
                    {
                        internet_str = IOTObj.Property("internet").Value.ToString();
                    }

                    if (IOTObj.Property("select_time") != null)
                    {
                        select_time_str = IOTObj.Property("select_time").Value.ToString();
                    }

                    if (IOTObj.Property("state") != null)
                    {
                        state_str = IOTObj.Property("state").Value.ToString();
                    }

                    if (IOTObj.Property("error_state") != null)
                    {
                        error_state_str = IOTObj.Property("error_state").Value.ToString();
                    }
                    //20221208 Ruby 存pin_array
                    if (IOTObj.Property("pin_array") != null)
                    {
                        dynamic[] pindata = JsonConvert.DeserializeObject<JObject[]>(IOTObj.Property("pin_array").Value.ToString());

                        Logger.Info($"{JOB_QueryIOTStatus}  pindataT : \n" + pindata);

                        int pincount = pindata.Count();
                        //循每一筆pin_array
                        for (int j = 0; j < pincount; j++)
                        {
                            JObject pinObj = new JObject();
                            pinObj = pindata[j];

                            Logger.Info($"{JOB_QueryIOTStatus}  PIN_COUNT : {j}  pinObj :  \n" + pinObj);

                            string pin_title = "";
                            string pin_type_tag = "";
                            string pin_tag = "";
                            if (pinObj.Property("pin_title") != null)
                            {
                                pin_title = pinObj.Property("pin_title").Value.ToString();
                            }
                            if (pinObj.Property("pin_type_tag") != null)
                            {
                                pin_type_tag = pinObj.Property("pin_type_tag").Value.ToString();
                            }
                            if (pinObj.Property("pin_tag") != null)
                            {
                                pin_tag = pinObj.Property("pin_tag").Value.ToString();
                            }

                            string sql_insert_pin_array = $"INSERT INTO mcs.base_pin_array (GUID,SNKEY,pin_title,pin_type_tag,pin_tag) ";
                            sql_insert_pin_array += $"VALUE('{snkey_str + "-" + pin_type_tag + "-" + pin_tag}','{snkey_str}','{pin_title}','{pin_type_tag}','{pin_tag}') ";
                            sql_insert_pin_array += $" ON DUPLICATE KEY UPDATE  pin_title = '{pin_title}' ,pin_type_tag = '{pin_type_tag}',pin_tag = '{pin_tag}' ";

                            Logger.Info("IOTSTATUS_PINTAG INSERT OR UPDATE :  \n" + sql_insert_pin_array);

                            dao.AddExecuteItem(sql_insert_pin_array, null);
                            dao.Execute();
                        }
                    }


                    string sqlStr_insert_IOT_status = $" INSERT INTO mcs.base_iot_status ( GUID,UPDATE_USER,UPDATE_TIME,IOT_ID,machine_title,snkey,area,areasn,ip,internet,select_time,state,error_state) ";
                    sqlStr_insert_IOT_status += $" VALUE( uuid(),'MCS', now() ,'{id_str}','{machine_title_str}','{snkey_str}','{area_str}','{areasn_str}','{ip_str}','{internet_str}','{select_time_str}','{state_str}','{error_state_str}')  ";
                    sqlStr_insert_IOT_status += $" ON DUPLICATE KEY UPDATE UPDATE_USER ='MCS' , UPDATE_TIME = now() , IOT_ID = '{id_str}' , machine_title ='{machine_title_str}', area = '{area_str}' ,areasn = '{areasn_str}' ,ip = '{ip_str}' ,  ";
                    sqlStr_insert_IOT_status += $" internet ='{internet_str}' ,select_time = '{select_time_str}' , state ='{state_str}',error_state ='{error_state_str}' ";

                    Logger.Info("IOTSTATUS INSERT OR UPDATE :  \n" + sqlStr_insert_IOT_status);

                    dao.AddExecuteItem(sqlStr_insert_IOT_status, null);
                    dao.Execute();

                }// end  for (int i = 0; i < IOTdata_count; i++)
                #endregion

                return returnMsg;

            } //end try
            catch (Exception ex)
            {
                Logger.Error($"{ JOB_QueryIOTStatus}  Exception ex: " + ex);
                returnMsg.Add("msg", $"Exception ex: " + ex);
                returnMsg.result = false;
                return returnMsg;
            }
        }
        private string Forward_IOTMessage(JObject _originalMessage, string url)
        {
            string _result = null;
            bool isOk = false;
            string exString = "";

            ThreadPool.QueueUserWorkItem(sendQueryIOTStatus, _originalMessage);

            void sendQueryIOTStatus(object _object)
            {
                JObject _jObject = (JObject)_object;
                try
                {

                    if (!string.IsNullOrEmpty(url))
                    {
                        string _jsonString = JsonConvert.SerializeObject(_jObject);

                        Logger.Info($"{JOB_QueryIOTStatus} sendQueryIOTStatus send :" + url);

                        Task<string> task = PostAsyncNoParm(url);

                        Logger.Info($"{JOB_QueryIOTStatus} sendQueryIOTStatus:" + task.Result);

                        _result = task.Result;

                    }
                    else
                    {
                        Logger.Error($"{JOB_QueryIOTStatus} url is null or empty");
                        return;
                    }

                }
                catch (Exception ex)
                {
                    isOk = true;
                    exString = ex.ToString();
                    Logger.Error($"{JOB_QueryIOTStatus} ForwardMessage  Exception :", ex);
                }

                if (_result != null)
                {
                    isOk = true;
                }
            } //end send



            //  int t = 0; //JOBS用不讓TIMEOUT造成中斷
            while (isOk == false)
            {
                Logger.Info($"{JOB_QueryIOTStatus} Main thread  while");
                try { Thread.Sleep(300); } catch (Exception exception) { Logger.Error($"{JOB_QueryIOTStatus} Sleep exception", exception); }
                if (_result == null)
                {
                    /*
                    if (t > 100)
                    {
                        Logger.Info("Main thread while time out ");
                        isOk = true;  //結束迴圈
                    }
                    t++;
                    */
                }
                else
                {
                    Logger.Info($"{JOB_QueryIOTStatus} result is not null ");
                    isOk = true;
                }
            }

            if (_result != null)
            {
                string tempString_ = _result.ToString();
                Logger.Info($"{JOB_QueryIOTStatus} END ForwardMessage._result in thread: {tempString_} \n");
            }
            else
            {
                if (!string.IsNullOrEmpty(exString))
                {
                    _result = "errmessage : " + exString;
                }
                else
                {
                    _result = "result = NULL";
                }

                string tempString = _result.ToString();
                Logger.Info($"{JOB_QueryIOTStatus} END ForwardMessage._result in thread: NULL Failed \n");
            }

            return _result;
        }
        private async Task<string> PostAsyncNoParm(string url)
        {
            HttpClient client = new HttpClient();
            using (MemoryStream ms = new MemoryStream())
            {
                string content = "";
                client.Timeout = TimeSpan.FromSeconds(6); //
                byte[] bytes = Encoding.Unicode.GetBytes(content);  //Encoding.UTF8
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                HttpContent hc = new StreamContent(ms);
                HttpResponseMessage response = await client.PostAsync(url, hc);
                //HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }

        }
    }
}