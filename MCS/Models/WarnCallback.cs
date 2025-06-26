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
    public class WarnCallback : ISqlCreator
    {
        public string AlarmStatus { get { return "t_alarm_status"; } }
        public string AlarmTravel { get { return "t_alarm_travel"; } }

        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,REQCODE,AGVCODE,WARNCONTENT,TASKCODE,BEGINTIME,INSERT_USER,INSERT_TIME) ", AlarmStatus);
            sqlStr += $@"VALUES ('{uuid}',@REQCODE,@AGVCODE,@WARNCONTENT,@TASKCODE,@BEGINTIME,@INSERT_USER,now());";
            
            sqlStr += string.Format(@"INSERT INTO {0} (GUID,GUID_STATUS,REQCODE,AGVCODE,WARNCONTENT,TASKCODE,BEGINTIME,INSERT_USER,INSERT_TIME) ", AlarmTravel);
            sqlStr += string.Format(@"SELECT UUID(),GUID,REQCODE,AGVCODE,WARNCONTENT,TASKCODE,BEGINTIME,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME FROM {0} ", AlarmStatus);
            sqlStr += $@"WHERE GUID='{uuid}';";
            return sqlStr;
        }
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
                    case "REQCODE":
                    case "AGVCODE":
                    case "WARNCONTENT":
                    case "TASKCODE":
                    case "BEGINTIME":
                        parm.Value = value;
                        parm.DbType = System.Data.DbType.String;
                        break;
                    default:
                        parm.Value = value;
                        parm.DbType = System.Data.DbType.String;
                        break;
                }

                parmList.Add(parm);
            }
            return parmList.ToArray();
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual";

            return sqlStr;
        }

        public string GetSqlStr(string noData, JObject parm)
        {
            return noData;
        }

        public string Search(JObject parm, string getCount)
        {
            return getCount;
        }


    }
}