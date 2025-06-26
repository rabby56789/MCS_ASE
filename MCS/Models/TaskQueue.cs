using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Data;

namespace MCS.Models
{
    /// <summary>
    /// 使用者設定功能頁面相關資料操作:繼承資料存取通用介面
    /// </summary>
    public class TaskQueue : ISqlCreator
    {
        public string MasterTable { get { return "T_TASK_QUEUE"; } }
        public string TaskTable { get { return "BASE_TASK"; } }
        public string CarrierTable { get { return "BASE_CARRIER"; } }
        public string ElvtTaskStatusTable { get { return "T_ELVTTASK_STATUS"; } }
        public string ElvtTaskTravelTable { get { return "T_ELVTTASK_TRAVEL"; } }
        public string ElvtINFOTable { get { return "t_elvt_info"; } }
        /// <summary>
        /// 用JObject生成對應的SQL參數陣列
        /// </summary>
        /// <param name="input">前端輸入值</param>
        /// <returns></returns>
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
                case "Delete":
                    sqlStr = Delete();
                    break;
                default:
                    break;
            }

            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--查詢
        /// </summary>
        /// <param name="parm">查詢條件參數</param>
        /// <param name="getCount">是否為查詢數量</param>
        /// <returns></returns>
        public string Search(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;

            //是否為查詢資料筆數
            if (getCount)
            {
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} TTQ ", MasterTable);                
                sqlStr += "WHERE TTQ.SEQ IS NOT NULL  ";
            }
            else
            {
                sqlStr = string.Format(@"SELECT GUID,SEQ,JOB_NAME,CAR_NO,START_LOC,START_AREA,SEC_START_AREA,TARGET_LOC,TARGET_AREA,SEC_TARGET_AREA,");
                sqlStr += string.Format(@"CASE IS_MIXED_AREA WHEN 0 THEN '否' ELSE '是' END AS IS_MIXED_AREA,");
                sqlStr += string.Format(@"CASE `STATUS` WHEN 0 THEN '未建立' WHEN 1 THEN '已建立' WHEN 2 THEN '不可執行' WHEN 3 THEN '取消' END AS `STATUS`,");
                sqlStr += string.Format(@"MESSAGE,TRY_COUNT,INSERT_TIME,REMARK FROM {0} TTQ ",MasterTable);
                sqlStr += "WHERE TTQ.SEQ IS NOT NULL  ";
            }

            //任務單號
            if (!string.IsNullOrEmpty((string)conditions.SEQ))
            {
                sqlStr += "And TTQ.SEQ LIKE CONCAT('%' ,@SEQ, '%') ";
            }

            //任務名稱            
            if (!string.IsNullOrEmpty((string)conditions.JOB_NAME))
            {
                sqlStr += "And TTQ.JOB_NAME LIKE CONCAT('%' ,@JOB_NAME, '%') ";
            }            
            //貨架編號
            if (!string.IsNullOrEmpty((string)conditions.CAR_NO))
            {
                sqlStr += "And TTQ.CAR_NO LIKE CONCAT('%' ,@CAR_NO, '%') ";
            }
            //是否為混和儲位
            if (!string.IsNullOrEmpty((string)conditions.IS_MIXED_AREA))
            {
                sqlStr += "And TTQ.IS_MIXED_AREA LIKE CONCAT('%' ,@IS_MIXED_AREA, '%') ";
            }
            //狀態
            if (!string.IsNullOrEmpty((string)conditions.STATUS))
            {
                sqlStr += "And TTQ.STATUS = @STATUS ";
            }            

            //查詢數量不需換頁
            if (getCount)
            {
                return sqlStr;
            }

            if (parm.TryGetValue("sort", out _))
            {
                sqlStr += string.Format("ORDER BY {0} {1} ", (string)conditions.sort, (string)conditions.order);
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

        /// <summary>
        /// 取得唯一值
        /// </summary>
        /// <returns></returns>
        //public string GetOneByGUID()
        //{
        //    string sqlStr = string.Format(@"SELECT D.GUID,D.ID,D.NAME,D.REMARK FROM {0} D ", MasterTable);
        //    sqlStr += "WHERE ENABLE = 1 AND GUID = @GUID";

        //    return sqlStr;
        //}
        //public string GetUUID()
        //{
        //    string sqlStr = "SELECT UUID() from dual;";

        //    return sqlStr;
        //}
        /// <summary>
        /// 回傳SQL指令--刪除
        /// </summary>
        /// <returns></returns>
        public string Delete()
        {
            string sqlStr = string.Format("UPDATE {0} SET " +
                "STATUS = 3," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                "WHERE GUID = @GUID; ", MasterTable);

            return sqlStr;
        }
        /// <summary>
        /// 取消電梯任務
        /// </summary>
        /// <returns></returns>
        public string CancelElvtTask()
        {
            string sqlStr = $"UPDATE {ElvtTaskStatusTable} SET " +
                "TASKSTATUS = 7," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                "WHERE SUBTASK_STATUS_GUID = @GUID; ";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,ELVT_ID,SUBTASK_STATUS_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,ELVTTASK_STATUS_GUID,ELVTTASK_TYPE,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,TASK_TIME,TASK_QTY,IS_EXECUTE,TARGET_FLOOR,START_FLOOR,DIRECTION,PRIORITY,TASKCODE,CARRIER_ID,TROLLEY_ID,TASKSTATUS,CODE,MESSAGE,WEIGHTING,MQSERVER,COMMAND_JSON,COMMAND_ID,TASK_NAME,SUBTASK_NAME)", ElvtTaskTravelTable);
            sqlStr += string.Format(@"SELECT UUID(),ELVT_ID,SUBTASK_STATUS_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,GUID,ELVTTASK_TYPE,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,TASK_TIME,TASK_QTY,IS_EXECUTE,TARGET_FLOOR,START_FLOOR,DIRECTION,PRIORITY,TASKCODE,CARRIER_ID,TROLLEY_ID,TASKSTATUS,CODE,MESSAGE,WEIGHTING,MQSERVER,COMMAND_JSON,COMMAND_ID,TASK_NAME,SUBTASK_NAME FROM {0} ", ElvtTaskStatusTable);
            sqlStr += $@"WHERE SUBTASK_STATUS_GUID = @GUID;";

            return sqlStr;
        }
        //public string CancelElvt_INFO() 
        //{
        //    string sqlStr = $"UPDATE {ElvtINFOTable} EI " +
        //        $"LEFT JOIN {ElvtTaskStatusTable} ES ON ES.GUID = EI.ELVTTASK_STATUS_GUID " +
        //        $"LEFT JOIN {MasterTable} TS ON TS.GUID = ES.SUBTASK_STATUS_GUID " +
        //        "SET ELVT_FLOOR_X = '' , " +
        //        "ELVT_TargetFloor = 0 ," +
        //        "EI.TROLLEY_ID = '' " +
        //        "WHERE TS.GUID = @GUID;";
        //    return sqlStr;
        //}
        //public string GetDataList()
        //{
        //    string sqlStr = string.Format(@"SELECT GUID AS 'Key', ID , NAME AS 'Value' FROM {0} D ", CarrierTable);
        //    sqlStr += "WHERE D.ENABLE = 1 ";
        //    return sqlStr;
        //}

    }
}