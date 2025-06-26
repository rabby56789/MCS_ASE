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
    public class IndexChart : ISqlCreator
    {
        public string IndexChartData { get { return "t_index_chart"; } }

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
                    case "start_day":
                        parm.Value = value;
                        parm.DbType = DbType.String;
                        break;
                    case "end_day":
                        parm.Value = value;
                        parm.DbType = DbType.String;
                        break;
                    default:
                        parm.Value = value + "%";
                        parm.DbType = DbType.String;
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

            if (getCount)
            {
                sqlStr = $"SELECT COUNT(*) `Count` FROM {IndexChartData} ";
            }
            else
            {
                sqlStr = $"SELECT INSERT_TIME,`VALUE` FROM {IndexChartData} ";
            }

            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND TYPE LIKE @type ";
            sqlStr += "AND INSERT_TIME BETWEEN @start_day AND @end_day ";
            sqlStr += "GROUP BY DATE(INSERT_TIME) ";
            sqlStr += "ORDER BY INSERT_TIME;";

            return sqlStr;
        }
    }
}