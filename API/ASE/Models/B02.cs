using JQWEB.Models;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ASE.Models
{
    public class B02 : IModel
    {
        /// <summary>
        /// DB連線字串
        /// </summary>
       // string connStr { get; set; }


        public class Data
        {
            public string warnContent { get; set; }
            public string taskCode { get; set; }
            public string robotCode { get; set; }
            public string beginDate { get; set; }
        }



        public class RootObject
        {
            public string reqCode { get; set; }
            public string reqTime { get; set; }
            public string clientCode { get; set; }
            public string tokenCode { get; set; }
            public List<Data> data { get; set; }


        }


        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string SERVER_FUNCTION = "B02_ReceiveErrorData";
        public string ASE_B02_API = "";
        private static object LOCKobj = new object();
        public string API_B02 = "[API_B02] ";
        /// <summary>
        /// 建構式,取得設定檔資料
        /// </summary>
        public B02()
        {

        }

        //20221102 KJ
        public JObject Run(dynamic parm)
        {
            lock (LOCKobj)
            {
                log.Info($"{API_B02} B02 Function parameter: {parm}");
                JObject result = new JObject();
                result.Add("result", "");
                string M2Aseq = "";

                if (parm.Property("seq") != null)
                {
                    M2Aseq = parm.Property("seq").Value.ToString();
                }
                else
                {
                    log.Info($"{API_B02} M2Aseq不存在");
                    result["result"] = "M2Aseq不存在";
                    return result;
                }

                log.Info($"{API_B02} B02 Run");
                try
                {
                    DAO dao = new DAO();
                    DataTable dt;

                    //URL
                    string sqlURLStr = $"select * from mcs.base_server where SERVER_FUNCTION = '{SERVER_FUNCTION}' ; ";
                    dao.AddExecuteItem(sqlURLStr, null);
                    //dao.Execute();
                    dt = dao.Query().Tables[0];
                    if (dt.Rows.Count <= 0)
                    {
                        log.Info($"{API_B02}  {M2Aseq} B02 SERVER_FUNCTION NOT FOUND ");
                        result["result"] = M2Aseq + " B02 SERVER_FUNCTION NOT FOUND ";
                        return result;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        string IP = row["SERVER_IP"].ToString();
                        string PORT = row["SERVER_PORT"].ToString();
                        string URL = row["URL"].ToString();
                        string url = string.Empty;
                        //url = "http://" + IP + ":" + PORT + "/" + URL;
                        url = "https://" + IP + PORT + "/" + URL;
                        log.Info($"{API_B02} B02  URL : " + url);
                        ASE_B02_API = url;
                    }

                    if (string.IsNullOrEmpty(ASE_B02_API))
                    {
                        log.Info($"{API_B02} {M2Aseq} B02 url is null or empty");
                        result["result"] = M2Aseq + " B02 url is null or empty";
                        return result;
                    }

                    //KJTEST
                    // ASE_B02_API = "http://localhost:44314/api/ProductsApp/CallFunc";


                    //RUN　B02
                    List<string> AGV_List = new List<string>(); //小車數

                    string reqcode = "";

                    JObject obj = new JObject();
                    // obj = JObject.Parse(parm);  //RCS回傳資料
                    obj = parm;

                    if (obj.Property("reqCode") != null)
                    {
                        log.Info($"{API_B02} reqCode存在!");
                        reqcode = obj.Property("reqCode").Value.ToString();
                    }
                    else
                    {
                        log.Info($"{API_B02} {M2Aseq} reqCode不存在!");
                        result["result"] = M2Aseq + " reqCode不存在";
                        return result;
                    }


                    //取得RCS回傳的DATA 資料
                    string jsonDataString = JsonConvert.SerializeObject(obj);
                    RootObject rb = JsonConvert.DeserializeObject<RootObject>(jsonDataString);

                    if (rb.data is null)
                    {
                        log.Info($"{API_B02} {M2Aseq} RCS data NULL!");
                        result["result"] = M2Aseq + " data NULL";
                        return result;
                    };


                    string taskCode = "";

                    foreach (Data ep in rb.data)  //DATA是有warn的小車訊息 一次一筆小車(任務)資料處理
                    {

                        if (!string.IsNullOrEmpty(ep.taskCode.ToString()))
                        {
                            taskCode = ep.taskCode.ToString();
                        }
                        else
                        {
                            taskCode = "";
                        }

                        //其他資料
                        string agv_no = ep.robotCode.ToString();// 車號
                        string error_msg = ep.warnContent.ToString();//錯誤訊息
                        //string beginTime = ep.beginDate.ToString();//時間戳
                        DateTime DateObject = Convert.ToDateTime(ep.beginDate.ToString());
                        string beginTime = DateObject.ToString("yyyyMMddHHmmss");
                        string error_statusstr = "";//異常位置
                        string agv_warnContent = ""; //資料庫中的原告警訊息紀錄
                        string error_loc = "";
                        string agv_location = ""; //資料庫中的原位置紀錄

                        AGV_List.Add(agv_no); //寫入小車資料

                        //查詢RCS車的位置資料
                        DataTable POSDT;

                        string sqlGetPOSStr = string.Empty;
                        sqlGetPOSStr = $"SELECT * FROM mcs.t_agv_status where ROBOTCODE = '{agv_no}'; ";

                        dao.AddExecuteItem(sqlGetPOSStr, null);
                        //dao.Execute();

                        POSDT = dao.Query().Tables[0];
                        if (POSDT.Rows.Count <= 0)
                        {
                            log.Info($"{API_B02} {M2Aseq} t_agv_status 沒有小車資料不用紀錄 ");
                            error_loc = "";
                        }
                        else
                        {
                            log.Info($"{API_B02} {M2Aseq} t_agv_status 有小車資料，取得位置");

                            //讀取是否有對應的小車， 取得該車位置 

                            string POSX = "";
                            string POSY = "";
                            string MAPCODE = "";

                            if (POSDT.Rows.Count > 1) //同小車最多只會有一筆
                            {
                                log.Error($"{API_B02} {M2Aseq} t_agv_status 小車資料錯誤"); //進來這邊表示資料庫有錯
                                result["result"] = M2Aseq + "  t_agv_status 小車資料錯誤";
                                return result;
                                // break;
                            }
                            else
                            {

                                for (int j = 0; j < POSDT.Rows.Count; j++)
                                {
                                    MAPCODE = POSDT.Rows[j]["MAPCODE"].ToString();
                                    POSX = POSDT.Rows[j]["POSX"].ToString();
                                    POSY = POSDT.Rows[j]["POSY"].ToString();

                                }

                                error_loc = POSX + MAPCODE + POSY;
                            }

                        } //end t_agv_status 有小車資料


                        //error_status //狀態 開始/結束 

                        //查詢t_alarm_status有無該筆小車資料
                        DataTable alarmDT;
                        string sqlStr_read_alarm_status = $" SELECT * FROM mcs.t_alarm_status where AGVCODE = {agv_no} ; ";
                        dao.AddExecuteItem(sqlStr_read_alarm_status, null);
                        // dao.Execute();
                        alarmDT = dao.Query().Tables[0];
                        if (alarmDT.Rows.Count <= 0)
                        {
                            log.Info($"{API_B02} {M2Aseq}  沒有 t_alarm_status 的該筆小車資料 ");
                            error_statusstr = "開始";
                        }
                        else
                        {
                            //有資料的話寫入t_alarm_status前先比較error_msg 是否一樣 
                            log.Info($"{API_B02} {M2Aseq}  t_alarm_status 有相對應的車號資料");
                            int i = 0;

                            if (alarmDT.Rows.Count > 1)//同一台車只有一筆 這邊表示資料庫有誤
                            {
                                log.Error($"{API_B02} {M2Aseq}  t_alarm_status中對應的小車數不正確");
                                result["result"] = M2Aseq + "  t_alarm_status 中對應的小車數不正確";
                                return result;
                            }

                            foreach (DataRow row in alarmDT.Rows)  //正常只會有一筆
                            {
                                agv_location = alarmDT.Rows[i]["LOCATION"].ToString();
                                agv_warnContent = alarmDT.Rows[i]["warnContent"].ToString();
                                i++;
                            }



                            //先比對LOCATION是否一樣
                            if (agv_location == error_loc)
                            {
                                //LOCATION一樣
                                log.Info($"{API_B02} {M2Aseq} : {agv_no} LOCATION一樣");

                                //再比較error_msg 是否一樣
                                if (agv_warnContent != error_msg)
                                {
                                    log.Info($"{API_B02} {M2Aseq} : {agv_no} MSG不一樣");

                                    // parm.Add("error_status", "開始");
                                    error_statusstr = "開始";

                                }
                                else
                                {
                                    log.Info($"{API_B02} {M2Aseq} : {agv_no} MSG一樣");
                                    error_statusstr = ""; //相同表示一樣的錯誤，清空
                                }
                            }
                            else
                            {
                                //LOCATION 不一樣表示是新的錯誤
                                log.Info($"{API_B02} {M2Aseq} : {agv_no} LOCATION不一樣");
                                error_statusstr = "開始";

                            }
                        }


                        //寫入t_alarm_status WARNCONTENT / AGV NO不存在才新增 不然用UPDATE    REMARK註記 開始/結束//不變
                        string sqlStr_insert_alarm_status = $" INSERT INTO mcs.t_alarm_status (GUID,INSERT_USER,INSERT_TIME,AGVCODE,BEGINTIME,WARNCONTENT,TASKCODE,REMARK,REQCODE,LOCATION) VALUE(uuid(),'MCS',now(),'{agv_no}','{beginTime}','{error_msg}','{taskCode}','{error_statusstr}','{reqcode}','{error_loc}') ";
                        sqlStr_insert_alarm_status += $" ON DUPLICATE KEY UPDATE INSERT_USER='MCS',INSERT_TIME = now(), WARNCONTENT = '{error_msg}' ,BEGINTIME= '{beginTime}',TASKCODE='{taskCode}' ,REMARK = '{error_statusstr}' , REQCODE = '{reqcode}' ,LOCATION= '{error_loc}'; ";
                        log.Info($"{API_B02} {M2Aseq}  sqlStr_insert_alarm_status :  {sqlStr_insert_alarm_status} ");
                        dao.AddExecuteItem(sqlStr_insert_alarm_status, null);
                        dao.Execute();

                    } //end 每一台小車foreach


                    //無法判斷結束 因為是等RCS CALLBACK
                    //這邊的結束是指RCS回傳其他小車的時間來改REMARK

                    // 如果REMARK 一開始為'' 第一次即改 開始
                    // 之後比較WARNCONTENT的值，一樣的話改''， 有變化如果RCS回傳 WARNCONTENT ='' 就是結束，否則為'開始'
                    // RCS 回傳沒有該台車即表示WARNCONTENT ='' ，這時比較REMARK ， 如果REMARK是開始 改為結束，如果REMARK是 結束 改為''，如果REMARK是'' 不動作
                    //其他沒有錯誤的小車資料WARNCONTENT要REFALSH為""
                    //
                    string[] AGV_terms = AGV_List.ToArray();
                    int AGV_termsCount = AGV_List.Count();

                    //清空其他小車的WARNCONTENT與REMARK  
                    //string sqlStr_REFALSH_alarm_status = $" UPDATE mcs.t_alarm_status t set WARNCONTENT = '' ,t.TASKCODE = '',  t.BEGINTIME = '' , t.REMARK= CASE  WHEN t.REMARK = '開始' THEN '結束' ELSE '' END  where ";
                    string sqlStr_REFALSH_alarm_status = $" UPDATE mcs.t_alarm_status t set WARNCONTENT = '' , LOCATION = '' , t.REMARK= CASE  WHEN t.REMARK = '開始' THEN '結束' ELSE '' END  where "; //移除結束設定


                    for (int i = 0; i < AGV_termsCount; i++) //巡迴每台沒有錯誤的小車
                    {
                        if (i == 0)
                        {
                            sqlStr_REFALSH_alarm_status += $" AGVCODE <> { AGV_terms[i]} ";
                        }
                        else
                        {
                            sqlStr_REFALSH_alarm_status += $" and AGVCODE <> {AGV_terms[i]} ";
                        }
                    }

                    log.Info($"{API_B02} sqlStr_REFALSH_alarm_status: {sqlStr_REFALSH_alarm_status} ");
                    dao.AddExecuteItem(sqlStr_REFALSH_alarm_status, null);
                    dao.Execute();



                    //error_status 開始或結束判斷 與要不要上傳
                    //error_status 判斷要同時讀DB warnContent跟REMARK 比較
                    DataTable ReadstatusDT;
                    // string sqlStr_Readstatus = $" SELECT * FROM mcs.t_alarm_status where REMARK = '開始' or  REMARK = '結束' ; "; //結束不用傳
                    string sqlStr_Readstatus = $" SELECT * FROM mcs.t_alarm_status where REMARK = '開始'; ";

                    dao.AddExecuteItem(sqlStr_Readstatus, null);
                    //dao.Execute();

                    ReadstatusDT = dao.Query().Tables[0];
                    if (ReadstatusDT.Rows.Count <= 0)
                    {
                        log.Info($"{API_B02} {M2Aseq} REMARK沒有需要上傳的資料 ");
                        result["result"] = M2Aseq + " REMARK沒有需要上傳的資料";
                        return result;
                    }
                    else
                    {

                        System.Diagnostics.Debug.Write("REMARK有要上傳的資料");

                        //讀取要上傳的資料是否有對應的任務， t_alarm_status的車號 對應 RCS回傳有無TASKCODE 
                        string Real_taskcode = "";

                        //   foreach (DataRow row in DT.Rows)  //多筆資料，每一筆去檢查是否有對應的TASK，然後直接打B02
                        for (int i = 0; i < ReadstatusDT.Rows.Count; i++)
                        {
                            JObject ParmTask = new JObject();
                            //ParmTask.Add("function", "ABCD");//KJTEST
                            ParmTask.Add("function", "B02");
                            ParmTask.Add("error_type", "重大");




                            //寫入t_alarm_status回傳JSON資料

                            Real_taskcode = ReadstatusDT.Rows[i]["TASKCODE"].ToString();
                            ParmTask.Add("task_id", Real_taskcode); //TASKCODE
                            ParmTask.Add("agv_no", ReadstatusDT.Rows[i]["AGVCODE"].ToString());
                            ParmTask.Add("error_msg", ReadstatusDT.Rows[i]["WARNCONTENT"].ToString());
                            //20221219 修正回傳的時間格式
                            string time_stamps = "";
                            time_stamps = DateTime.Parse((ReadstatusDT.Rows[i]["BEGINTIME"].ToString())).ToString("yyyyMMddHHmmss");
                            ParmTask.Add("time_stamp", time_stamps);
                            //ParmTask.Add("time_stamp", ReadstatusDT.Rows[i]["BEGINTIME"].ToString());
                            //ParmTask.Add("error_status", DT.Rows[i]["REMARK"].ToString());//拿掉，因為結束時間不準
                            ParmTask.Add("error_status", "開始");//改只要回傳就是開始
                            ParmTask.Add("error_loc", ReadstatusDT.Rows[i]["LOCATION"].ToString());

                            if (string.IsNullOrEmpty(Real_taskcode))
                            {
                                //沒有TASK                     
                                ParmTask.Add("start_loc", "");  //start_loc
                                ParmTask.Add("target_loc", "");  //start_loc
                                ParmTask.Add("job_name", "");  //job_name
                                ParmTask.Add("podCode", "");  //貨架編號
                            }
                            else
                            {
                                //找對應的T_SUBTASK_STATUS  //對應的TASK

                                DataTable SUBTASKDT;
                                string sqlStr = string.Empty;
                                sqlStr = $" SELECT * FROM mcs.t_subtask_status where TASKCODE = '{Real_taskcode}'; ";

                                dao.AddExecuteItem(sqlStr, null);
                                //dao.Execute();

                                SUBTASKDT = dao.Query().Tables[0];
                                if (SUBTASKDT.Rows.Count <= 0)
                                {
                                    log.Info($"{API_B02} {M2Aseq} t_subtask_status 找不到小車對應任務 ");
                                    ParmTask.Add("start_loc", "");  //start_loc
                                    ParmTask.Add("target_loc", "");  //start_loc
                                    ParmTask.Add("job_name", "");  //job_name
                                    ParmTask.Add("podCode", "");  //貨架編號
                                }
                                else
                                {
                                    if (SUBTASKDT.Rows.Count <= 0 || SUBTASKDT.Rows.Count > 1)
                                    {
                                        log.Info($"{M2Aseq} MCS中對應的任務數不正確");
                                        ParmTask.Add("start_loc", "");  //start_loc
                                        ParmTask.Add("target_loc", "");  //start_loc
                                        ParmTask.Add("job_name", "");  //job_name
                                        ParmTask.Add("podCode", "");  //貨架編號                                              
                                    }
                                    else
                                    {
                                        int DTi = 0;
                                        foreach (DataRow row2 in SUBTASKDT.Rows)  //正常只會有一筆                                                                                     
                                        {
                                            // ParmTask.Add("seq", DT2.Rows[DT2i]["ASE_SEQ"].ToString());  //seq
                                            ParmTask.Add("start_loc", SUBTASKDT.Rows[DTi]["ASE_START_LOC"].ToString());  //start_loc
                                            ParmTask.Add("target_loc", SUBTASKDT.Rows[DTi]["ASE_TARGET_LOC"].ToString());  //start_loc
                                            ParmTask.Add("job_name", SUBTASKDT.Rows[DTi]["ASE_JOB_NAME"].ToString());  //job_name
                                            ParmTask.Add("podCode", SUBTASKDT.Rows[DTi]["podCode"].ToString()); //貨架編號
                                            DTi++;
                                        }
                                    }
                                } // end if (reader.HasRows == false)
                            }//end  if (string.IsNullOrEmpty(Real_taskcode))


                            ParmTask.Add("seq", "MCS_" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));

                            log.Info(M2Aseq + " " + ParmTask);

                            //打API
                            //lock (LOCKobj)
                            //{
                            JObject doB02TASK = new JObject();
                            doB02TASK = doB02ByThread(ParmTask, ASE_B02_API);
                            log.Info($"{API_B02} {M2Aseq} " + " doB02ByThread return: " + doB02TASK);
                            //}

                            try { Thread.Sleep(600); } catch (Exception exception) { log.Error("Sleep exception", exception); }

                        } //end foreach

                    }//end else   Console.WriteLine("有資料要上傳");

                }
                catch (Exception ex)
                {
                    log.Error($"{API_B02} {M2Aseq} B02 Exception: " + ex.Message);
                    //result["msg"] = "B02 Exception " + ex.Message;
                    throw new Exception(ex.Message);
                    //return;
                }


                log.Info($"{API_B02} {M2Aseq} API B02 完成");
                result["result"] = M2Aseq + " API B02 完成";
                return result;
            }
        }

        public JObject doB02ByThread(JObject jObject, string url)
        {
            //執行B02 上傳WarnCallback
            log.Info($"{API_B02} doB02ByThread : \n" + jObject);

            JObject _result = null;
            int tryCount = 0;

            int maxTryCount = 1;  //打API的RETRY次數 設定為1

            //string seq = jObject["seq"].ToString(); //MCS_DATETIME
            string seq = "";
            string exString = "";
            string reqString = "";

            bool isOk = false;

            ThreadPool.QueueUserWorkItem(doB02, jObject);

            void doB02(object _jObject)
            {

                for (tryCount = 0; tryCount < maxTryCount; tryCount++)
                {
                    try
                    {
                        // log.Info("doB02 TASK tryCount= " + tryCount);
                        // log.Info($"doB02 Function parameter : {jObject}");
                        log.Info($"{API_B02} doB02 TASK  send ");

                        string _jsonString = JsonConvert.SerializeObject(_jObject);
                        Task<string> task = B02PostAsyncJson(url, _jsonString);

                        log.Info($"{API_B02} TASK: " + task.Result);

                        _result = JsonConvert.DeserializeObject<JObject>(task.Result);
                    }
                    catch (Exception ex)
                    {
                        isOk = true;
                        exString = ex.ToString();
                        log.Error(ex);
                    }


                    if (_result != null)
                    {
                        isOk = true;
                        if (_result["return_code"].ToString().ToUpper() == "S")
                        {
                            seq = _result["seq"].ToString();
                            log.Info($"{API_B02}  doB02 TASK  " + seq + " return S  ,  count= " + tryCount);
                            break;
                        }
                        else
                        {   //return_code != S
                            seq = _result["seq"].ToString();
                            log.Info($"{API_B02}  doB02 TASK " + seq + " return = " + _result["return_code"].ToString().ToUpper() + ", return_msg = " + _result["return_msg"].ToString().ToUpper() + ", count = " + tryCount);
                            if ((tryCount + 1) == maxTryCount)
                            {
                                //retry 結束時其他處理
                            }
                        }
                    }


                    try { Thread.Sleep(300); } catch (Exception exception) { log.Error("Sleep exception", exception); }

                }   //end for  retry

            }   //end for doB02


            while (isOk == false)
            {
                log.Info($"{API_B02} B02 Main thread while");
                try { Thread.Sleep(300); } catch (Exception exception) { log.Error("Sleep exception", exception); }
                if (_result == null)
                {
                    //有設定TIMEOUT的EX會中止while
                }
                else
                {
                    log.Info($"{API_B02} B02 _result is not null ");
                    isOk = true;
                }
            }//end while



            if (_result != null)
            {
                reqString = _result.ToString();
                log.Info($"{API_B02} END doB02 _result in thread: {reqString} \n");
            }
            else
            {
                //_result = NULL
                _result = new JObject();
                //_result.Add("code", "1");

                if (!string.IsNullOrEmpty(exString))
                {
                    _result.Add("result", exString);
                }
                else
                {
                    _result.Add("result", "result = NULL");
                }
                reqString = _result.ToString();
                log.Info($"{API_B02} Failed  END doB02 _result in thread:  {reqString}  \n");
            }

            //  return ;
            return _result;

        }// end doB02thread



        private async Task<string> B02PostAsyncJson(string url, string json)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(1000);
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;

        }


        /* //OLD CODE
        public JObject Run(dynamic parm)
        {
            log.Debug($"B02 Function parameter: {parm}");
            JObject result = new JObject();
            result.Add("function", GetType().Name);
            result.Add("seq", parm.seq);
            result.Add("return_code", "S");
            result.Add("return_msg", "");

            log.Debug("B02 result : " + result);
            return result;
        }
        */

    } //end B02 Class IModel


}


