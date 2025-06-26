using JQWEB.Models;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace ASE.Models
{
    public class Task 
    {
        public string MasterTable { get { return "T_SUBTASK_STATUS"; } }
        public string HistoryTable { get { return "BASE_MAP_HISTORY"; } }
        public string SubTable { get { return "AGV_TASK_POSITION"; } }
        public string DataTable { get { return "BASE_FACTORY"; } }
        /// <summary>
        /// 用JObject生成對應的SQL參數陣列
        /// </summary>
        /// <param name="input">前端輸入值</param>
        /// <returns></returns>
        public MySqlParameter[] CreateParameterAry(JObject input)
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
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual;";

            return sqlStr;
        }

        public string GetTASKCODE()
        {
            string sqlStr = $"SELECT TASKCODE FROM {MasterTable} WHERE ASE_SEQ=@ASE_SEQ ;";

            return sqlStr;
        }
    }
    public class PositionCodePath
    {
        public string positionCode { get; set; }
        public string type { get; set; }
    }

    public class genAgvSchedulingTask
    {
        public string reqCode { get; set; }
        public string reqTime { get; set; }
        public string clientCode { get; set; }
        public string tokenCode { get; set; }
        public string taskTyp { get; set; }
        public string sceneTyp { get; set; }
        public string ctnrTyp { get; set; }
        public string ctnrCode { get; set; }
        public string wbCode { get; set; }
        public List<PositionCodePath> positionCodePath { get; set; }
        public string podCode { get; set; }
        public string podDir { get; set; }
        public string podTyp { get; set; }
        public string materialLot { get; set; }
        public string priority { get; set; }
        public string agvCode { get; set; }
        public string taskCode { get; set; }
        public string data { get; set; }
    }
}