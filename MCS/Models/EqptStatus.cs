using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace MCS.Models
{
    public class EqptStatus : ISqlCreator
    {
        public IDataParameter[] CreateParameterAry(JObject input)
        {
            if (input is null)
            {
                return new MySqlParameter[] { };
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
                    case "ID":
                    case "NAME":
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
        public string GetSqlStr(string actionName, [Optional] JObject parm)
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
                case "QueryDIDO":
                    sqlStr = DIDO();
                    break;
                default:
                    break;
            }

            return sqlStr;
        }
        public string Search(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Empty;
            if (getCount)
            {
                sqlStr = "select count(*) as Count from base_iot_status where 1 = 1 ";
            }
            else
            {
                sqlStr = $@"select AREA,AREASN,SNKEY,IP from base_iot_status where 1 = 1 ";
            }

            //樓層
            if (!string.IsNullOrEmpty((string)conditions.AREA))
            {
                sqlStr += "AND AREA LIKE CONCAT(@AREA , '%') ";
            }
            //I/O模組名稱
            if (!string.IsNullOrEmpty((string)conditions.AREASN))
            {
                sqlStr += "AND AREASN LIKE CONCAT(@AREASN , '%') ";
            }

            if (getCount)
            {
                return sqlStr;
            }

            if (parm.TryGetValue("sort", out _))
            {
                sqlStr += string.Format("ORDER BY {0} ", (string)conditions.sort, (string)conditions.order);
            }

            //含排序 or 換頁
            if (parm.TryGetValue("page", out _))
            {
                int offset = (int)conditions.rows * ((int)conditions.page - 1);
                sqlStr += string.Format("LIMIT {0} ", conditions.rows);
                sqlStr += string.Format("OFFSET {0}", offset);
            }
            sqlStr += ";";

            return sqlStr;
            
        }

        public string DIDO()
        {
            string sqlStr = $@"SELECT pin_type_tag,pin_tag,pin_title FROM mcs.base_pin_array where SNKEY = @SNKEY;";
            return sqlStr;
        }
        public string GetOption()
        {
            //string sqlStr = string.Format(@"SELECT KEY_CODE, KEY_NAME FROM {0} D ", KeyCodeTable);
            //sqlStr += "WHERE D.ENABLE = 1 And TABLE_NAME = 'storage_status' And COL_NAME = @name";
            string sqlStr = "select area as KEY_CODE,area as KEY_NAME  from base_iot_status group by area";

            return sqlStr;
        }

    }
}