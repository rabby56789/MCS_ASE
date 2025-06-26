using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace MCS.Models
{
    /// <summary>
    /// 使用者設定功能頁面相關資料操作:繼承資料存取通用介面
    /// </summary>
    public class ElevatorStatus : ISqlCreator
    {
        public string ElvtTaskStatusTable { get { return "T_ELVTTASK_STATUS"; } }
        public string ElvtTaskTravelTable { get { return "T_ELVTTASK_TRAVEL"; } }      

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
        /// <summary>
        /// 擴充欄位
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public string InsertElvtTaskStatus(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,ELVT_ID,SUBTASK_STATUS_GUID,INSERT_USER,INSERT_TIME,REMARK,TASK_TIME,TASK_QTY,TARGET_FLOOR,START_FLOOR,DIRECTION,CARRIER_ID,TROLLEY_ID,TASKSTATUS,WEIGHTING,TASK_NAME,SUBTASK_NAME)", ElvtTaskStatusTable);
            sqlStr += $@"VALUES ('{uuid}',@ELVT_ID,@SUBTASK_STATUS_GUID,'ElevatorQueue',now(),@REMARK,now(),@TASK_QTY,@TARGET_FLOOR,@START_FLOOR,@DIRECTION,@CARRIER_ID,@TROLLEY_ID,@TASKSTATUS,@WEIGHTING,@TASK_NAME,@SUBTASK_NAME); ";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,ELVT_ID,SUBTASK_STATUS_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,ELVTTASK_STATUS_GUID,ELVTTASK_TYPE,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,TASK_TIME,TASK_QTY,IS_EXECUTE,TARGET_FLOOR,START_FLOOR,DIRECTION,PRIORITY,TASKCODE,CARRIER_ID,TROLLEY_ID,TASKSTATUS,CODE,MESSAGE,WEIGHTING,MQSERVER,COMMAND_JSON,COMMAND_ID,TASK_NAME,SUBTASK_NAME)", ElvtTaskTravelTable);
            sqlStr += string.Format(@"SELECT UUID(),ELVT_ID,SUBTASK_STATUS_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,GUID,ELVTTASK_TYPE,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,TASK_TIME,TASK_QTY,IS_EXECUTE,TARGET_FLOOR,START_FLOOR,DIRECTION,PRIORITY,TASKCODE,CARRIER_ID,TROLLEY_ID,TASKSTATUS,CODE,MESSAGE,WEIGHTING,MQSERVER,COMMAND_JSON,COMMAND_ID,TASK_NAME,SUBTASK_NAME FROM {0} ", ElvtTaskStatusTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
        /// <summary>
        /// 回傳SQL指令--更新
        /// </summary>
        /// <returns></returns>
        public string UpdateElvtTaskStatus()
        {
            string sqlStr = string.Format(@"UPDATE {0} SET                 
                TASKSTATUS = @TASKSTATUS,
                UPDATE_USER = 'ElevatorQueue',
                UPDATE_TIME = now() 
                WHERE GUID = @GUID ; ", ElvtTaskStatusTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,ELVT_ID,SUBTASK_STATUS_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,ELVTTASK_STATUS_GUID,ELVTTASK_TYPE,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,TASK_TIME,TASK_QTY,IS_EXECUTE,TARGET_FLOOR,START_FLOOR,DIRECTION,PRIORITY,TASKCODE,CARRIER_ID,TROLLEY_ID,TASKSTATUS,CODE,MESSAGE,WEIGHTING,MQSERVER,COMMAND_JSON,COMMAND_ID,TASK_NAME,SUBTASK_NAME)", ElvtTaskTravelTable);
            sqlStr += string.Format(@"SELECT UUID(),ELVT_ID,SUBTASK_STATUS_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,GUID,ELVTTASK_TYPE,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,TASK_TIME,TASK_QTY,IS_EXECUTE,TARGET_FLOOR,START_FLOOR,DIRECTION,PRIORITY,TASKCODE,CARRIER_ID,TROLLEY_ID,TASKSTATUS,CODE,MESSAGE,WEIGHTING,MQSERVER,COMMAND_JSON,COMMAND_ID,TASK_NAME,SUBTASK_NAME FROM {0} ", ElvtTaskStatusTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual;";

            return sqlStr;
        }

        public string GetSqlStr(string actionName, JObject parm)
        {
            throw new NotImplementedException();
        }

        IDataParameter[] ISqlCreator.CreateParameterAry(JObject input)
        {
            throw new NotImplementedException();
        }
    }

}