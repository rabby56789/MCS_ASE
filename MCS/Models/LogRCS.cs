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
    /// 國巨廠內API發送紀錄
    /// </summary>
    public class LogRCS : ISqlCreator
    {
        string LogTable { get { return "log_lower_rcs"; } }
        string TSubTaskStatusTable { get { return "t_subtask_status"; } }

        public IDataParameter[] CreateParameterAry(JObject input)//帶參數
        {
            if (input is null)
            {
                return new IDataParameter[] { };
            }

            List<MySqlParameter> parmList = new List<MySqlParameter>();
            string[] strType = new string[] {
                "TID" , "FACTORY_ID", "SERVER_TYPE", "USER_NAME", "PID", "SEND_VALUE", "RESPONSE_VALUE"
            };


            //JSON項目逐一加入參數表中
            foreach (var x in input)
            {
                MySqlParameter parm = new MySqlParameter();
                string name = x.Key;
                JToken value = x.Value;

                parm.ParameterName = "@" + name;
                parm.Value = value;

                if (strType.Contains(name))
                {
                    parm.DbType = System.Data.DbType.String;
                }
                else
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
                default:
                    return "";
            }
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        private string Query(JObject parm, bool getCount)
        {
            string sqlStr;
            var conditions = parm as dynamic;

            //是否為查詢資料筆數
            if (getCount)
            {
                sqlStr = $"SELECT COUNT(*) AS count ";
            }
            else
            {
                sqlStr = $@"SELECT SEQ,TASKCODE,JOB_NAME,START_AREA,START_LOC,TARGET_AREA,TARGET_LOC,CAR_NO,
                            CASE CODE WHEN '0' THEN '成功' ELSE '失敗' END AS STATUS,MESSAGE,RETIME ";
            }

            sqlStr += $@"FROM {LogTable} WHERE ENABLE = 1 ";

            if (!string.IsNullOrEmpty((string)conditions.SEQ))
            {
                sqlStr += "AND SEQ = @SEQ ";
            }

            if (!string.IsNullOrEmpty((string)conditions.CAR_NO))
            {
                sqlStr += "AND CAR_NO = @CAR_NO ";
            }

            if (!string.IsNullOrEmpty((string)conditions.STATUS))
            {
                sqlStr += "AND CODE = @STATUS ";
            }

            //LOG查詢時間:開始
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_START))
            {
                sqlStr += "AND INSERT_TIME > @INSERT_TIME_START ";
            }

            //LOG查詢時間:結束
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_END))
            {
                sqlStr += "AND INSERT_TIME < @INSERT_TIME_END ";
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
    }
}