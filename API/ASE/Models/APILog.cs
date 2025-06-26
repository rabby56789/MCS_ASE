using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using JQWEB.Models;
using log4net;

namespace ASE.Models
{
    public class APILog
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        DAO dao = new DAO();
        public void InsertAPILog(JObject parm,string return_code,string return_msg)
        {
            log.Info("--------------------Start Logging APILog--------------------");
            try
            {
                //所有API參數(目前只記錄A開頭A01~A05,上位紀錄)20221107
                log.Info($"APILog parm : {parm}  return_code : {return_code}  return_msg : {return_msg}");
                if (parm["function"].ToString().Contains("A"))
                {
                    //parm.Add("api_status",return_code[0]);
                    JObject api_parm = new JObject();
                    api_parm.Add("function", parm["function"]);
                    api_parm.Add("seq", parm["seq"]);
                    api_parm.Add("job_seq", parm["job_seq"]);//A05
                    api_parm.Add("job_name", parm["job_name"]);//A01
                    api_parm.Add("start_area", parm["start_area"]);//A01
                    api_parm.Add("start_loc", parm["start_loc"]);//A01
                    api_parm.Add("target_area", parm["target_area"]);//A01
                    api_parm.Add("target_loc", parm["target_loc"]);//A01
                    api_parm.Add("car_no", parm["car_no"]);//A01,A03,A05                
                    api_parm.Add("location", parm["location"]);//A02,A03
                    api_parm.Add("lock_status", parm["lock_status"]);//A03
                    api_parm.Add("data_type", parm["data_type"]);//A04
                    api_parm.Add("file_data", parm["file_data"]);//A04
                    api_parm.Add("api_status", return_code);//API 命令 成功:S 失敗:F
                    api_parm.Add("api_msg", return_msg);//回傳訊息

                    //新增至DB，欄位順序依照DB內表格排序
                    string InsertSql = $@"insert into log_upper_ase value (uuid(),";
                    foreach (var col in api_parm)//加入參數字串
                    {
                        InsertSql += $"'{api_parm[col.Key]}',";
                    }
                    InsertSql += "'APILog',now());";
                    dao.AddExecuteItem(InsertSql, null);
                    dao.Execute();
                }
                else { return; }
            }
            catch(Exception ex)
            {
                log.Error("APILog : " + ex);
            }
            log.Info("--------------------End Logging APILog--------------------");
        }
    }
}