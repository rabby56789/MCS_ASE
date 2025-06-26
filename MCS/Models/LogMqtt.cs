using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Data;

namespace MCS.Models
{
    /// <summary>
    /// ASE廠內MQTT發送紀錄
    /// </summary>
    public class LogMqtt : ISqlCreator
    {
        string LogTable { get { return "log_lower_mqtt"; } }
        string SubTaskTable { get { return "t_subtask_status"; } }
        string ElvtTaskTable { get { return "t_elvttask_status"; } }

        public IDataParameter[] CreateParameterAry(JObject input)
        {
            if (input is null)
            {
                return new IDataParameter[] { };
            }

            List<MySqlParameter> parmList = new List<MySqlParameter>();

            string[] strInsertParms = new string[] {
                "SEND_TIME","TYPE" , "CONTENT"
            };

            string[] strQueryParms = new string[] {
                "QUERY_TYPE" , "QUERY_CONTENT"
            };

            string[] timeType = new string[] {
                "QUERY_SEND_TIME_START" , "QUERY_SEND_TIME_END"
            };

            //JSON項目逐一加入參數表中
            foreach (var x in input)
            {
                MySqlParameter parm = new MySqlParameter();
                string name = x.Key;
                JToken value = x.Value;

                parm.ParameterName = "@" + name;
                parm.Value = value;

                if (strInsertParms.Contains(name))
                {
                    parm.DbType = System.Data.DbType.String;
                }

                if (strQueryParms.Contains(name))
                {
                    parm.DbType = System.Data.DbType.String;
                    parm.Value += "%";
                }

                if (timeType.Contains(name))
                {
                    parm.DbType = System.Data.DbType.String;
                }

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        public string GetSqlStr(string actionName, [Optional] JObject parm)
        {
            switch (actionName)
            {
                case "COUNT":
                    return Query(parm, true);
                case "QUERY":
                    return Query(parm, false);
                case "QueryLog":
                    return QueryLog();
                case "ElvtTask":
                    return QueryElvtTask();
                default:
                    return "";
            }
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        public string Query(JObject parm, bool getCount)
        {
            string sqlStr;
            var conditions = parm as dynamic;

            //是否為查詢資料筆數
            if (getCount)
            {
                sqlStr = "SELECT count(*) as COUNT ";
            }
            else
            {
                sqlStr = $@"SELECT ASE_SEQ,TASKCODE,ASE_JOB_NAME,
            ASE_START_LOC,ASE_START_QRCODE,ASE_TARGET_LOC,ASE_TARGET_QRCODE,
            ASE_CAR_NO,INSERT_TIME,UPDATE_TIME,GUID ";
            }

            sqlStr += $"FROM {SubTaskTable} WHERE ENABLE = 1 ";

            //類
            if (!string.IsNullOrEmpty((string)conditions.SEQ))
            {
                sqlStr += "AND ASE_SEQ LIKE @SEQ ";
            }

            //內容
            if (!string.IsNullOrEmpty((string)conditions.CAR_NO))
            {
                sqlStr += "AND ASE_CAR_NO LIKE @CAR_NO ";
            }

            //任務開始時間(開始)
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_START))
            {
                sqlStr += "AND INSERT_TIME >= @INSERT_TIME_START ";
            }

            //任務結束時間(結束)
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_END))
            {
                sqlStr += "AND UPDATE_TIME <= @INSERT_TIME_END ";
            }

            //查詢數量不需換頁
            if (getCount)
            {
                return sqlStr;
            }

            //排序
            if (parm.TryGetValue("sort", out _))
            {
                sqlStr += string.Format("ORDER BY {0} {1} ", (string)conditions.sort, (string)conditions.order);
            }

            //換頁
            if (parm.TryGetValue("page", out _))
            {
                int offset = (int)conditions.rows * ((int)conditions.page - 1);
                sqlStr += string.Format("LIMIT {0} ", conditions.rows);
                sqlStr += string.Format("OFFSET {0}", offset);
            }
            sqlStr += ";";

            return sqlStr;
        }
        public string QueryLog()
        {
            string sqlStr = $@"select SEND_TIME,TOPIC,CONTENT from {LogTable} where TASK_GUID = @TASK_GUID ";
            return sqlStr;
        }
        public string QueryElvtTask()
        {
            string sqlStr = $@"select GUID from {ElvtTaskTable} where SUBTASK_STATUS_GUID = @TASK_GUID ";
            return sqlStr;
        }
        public string Insert()//JOB LogTODB
        {
            string sqlStr = string.Empty;
            sqlStr = $"INSERT IGNORE INTO {LogTable} (GUID,INSERT_TIME,SEND_TIME,TYPE,CONTENT) ";
            sqlStr += "VALUES (UUID(),NOW(),@SEND_TIME,@TYPE,@CONTENT);";
            return sqlStr;
        }
       
    }
}