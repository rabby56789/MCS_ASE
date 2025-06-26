using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace MCS.Models
{
    /// <summary>
    /// 及時狀態Table存取
    /// </summary>
    public class Tablet : ISqlCreator
    {
        public string Log_Api_Hik { get { return "log_api_hik"; } }

        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,API_URL,ACTION_NAME,SEND_DATA,RESPONSE_DATA,REMARK,INSERT_USER,INSERT_TIME) ", Log_Api_Hik);
            sqlStr += $@"VALUES ('{uuid}',@API_URL,@ACTION_NAME,@SEND_DATA,@RESPONSE_DATA,@REMARK,@INSERT_USER,now());";
            
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
                    case "API_URL":
                    case "ACTION_NAME":
                    case "SEND_DATA":
                    case "RESPONSE_DATA":
                    case "REMARK":
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