using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MCS.Models
{
    /// <summary>
    /// 及時狀態Table存取
    /// </summary>
    public class Alarm : ISqlCreator
    {
        /// <summary>
        /// 歷史警報訊息
        /// </summary>
        public string AlarmMessage { get { return "t_alarm_message"; } }

        public IDataParameter[] CreateParameterAry(JObject input)
        {
            if (input is null)
            {
                return new IDataParameter[] { };
            }

            List<MySqlParameter> parmList = new List<MySqlParameter>();

            //JSON項目逐一加入參數表中
            foreach (var x in input)
            {
                MySqlParameter parm = new MySqlParameter();
                string name = x.Key;
                JToken value = x.Value;

                parm.ParameterName = "@" + name;

                switch (name)
                {
                    case "INSERT_TIME_START":
                        parm.Value = value;
                        parm.DbType = System.Data.DbType.String;
                        break;
                    case "INSERT_TIME_END":
                        parm.Value = value;
                        parm.DbType = System.Data.DbType.String;
                        break;
                    default:
                        parm.Value = value + "%";
                        parm.DbType = System.Data.DbType.String;
                        break;
                }

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        public string GetSqlStr(string actionName, JObject parm)
        {
            string sqlStr = string.Empty;

            switch (actionName)
            {
                case "Count":
                    sqlStr = Search(parm, true);
                    break;
                case "Query": 
                    sqlStr = Search(parm, false);
                    break;
                default:
                    break;
            }

            return sqlStr;
        }

        public string Search(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;

            //是否為查詢資料筆數
            if (getCount)
            {
                sqlStr = $"SELECT COUNT(*) `Count` FROM {AlarmMessage} A ";
            }
            else
            {
                sqlStr = $"SELECT GUID,FACTORY,FLOOR,MAP,EQUIPMENT,EQUIPMENT_NO,MESSAGE,FUNCTION_NAME,TYPE,SEND_ENABLE,REMARK,INSERT_TIME FROM {AlarmMessage} A ";

            }

            sqlStr += "WHERE A.ENABLE = 1 ";



            //FACTORY
            if (!string.IsNullOrEmpty((string)conditions.FACTORY))
            {
                sqlStr += "AND FACTORY LIKE @FACTORY ";
            }
            //FLOOR
            if (!string.IsNullOrEmpty((string)conditions.FLOOR))
            {
                sqlStr += "AND FLOOR LIKE @FLOOR ";
            }
            //MAP
            if (!string.IsNullOrEmpty((string)conditions.MAP))
            {
                sqlStr += "AND MAP LIKE @MAP ";
            }
            //EQUIPMENT
            if (!string.IsNullOrEmpty((string)conditions.EQUIPMENT))
            {
                sqlStr += "AND EQUIPMENT LIKE @EQUIPMENT ";
            }
            //EQUIPMENT_NO
            if (!string.IsNullOrEmpty((string)conditions.EQUIPMENT_NO))
            {
                sqlStr += "AND EQUIPMENT_NO LIKE @EQUIPMENT_NO ";
            }
            //FUNCTION_NAME
            if (!string.IsNullOrEmpty((string)conditions.FUNCTION_NAME))
            {
                sqlStr += "AND FUNCTION_NAME LIKE @FUNCTION_NAME ";
            }

            //建立時間 - 起
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_START))
            {
                sqlStr += "AND INSERT_TIME >= @INSERT_TIME_START ";
            }
            //建立時間 - 終
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_END))
            {
                sqlStr += "AND INSERT_TIME <= @INSERT_TIME_END ";
            }


            if (getCount)
            {
                return sqlStr;
            }

            //排序
            if (parm.TryGetValue("sort", out _))
            {
                sqlStr += $"ORDER BY {(string)conditions.sort} {(string)conditions.order} ";
            }

            //換頁
            if (parm.TryGetValue("page", out _))
            {
                int offset = (int)conditions.rows * ((int)conditions.page - 1);
                sqlStr += $"LIMIT {conditions.rows} ";
                sqlStr += $"OFFSET {offset}";
            }
            sqlStr += ";";

            return sqlStr;
        }


    }
}