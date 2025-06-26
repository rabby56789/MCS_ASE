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
    public class TaskOnline : ISqlCreator
    {
        public string MasterTable { get { return "T_SUBTASK_STATUS"; } }
        public string TaskTable { get { return "BASE_TASK"; } }
        public string CarrierTable { get { return "BASE_CARRIER"; } }
        public string ElvtTaskStatusTable { get { return "T_ELVTTASK_STATUS"; } }
        public string ElvtTaskTravelTable { get { return "T_ELVTTASK_TRAVEL"; } }
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
                case "Add":
                    sqlStr = Insert();
                    break;
                case "Edit":
                    sqlStr = Update();
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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} TSS ", MasterTable);
                sqlStr += string.Format(@"left join {0} AS BT on TSS.TASK_GUID = BT.GUID ", TaskTable);
                sqlStr += "WHERE TSS.ENABLE = 1  ";
            }
            else
            {
                sqlStr = string.Format(@"SELECT ASE_SEQ,TSS.GUID,TASKTYP,AGVCODE,ASE_CAR_NO,WEIGHTING,ASE_START_LOC,ASE_TARGET_LOC,ASE_JOB_NAME,TSS.PROGRESS ,SUBTASK_NAME,TASKCODE,MESSAGE, ");
                sqlStr += string.Format(@"CASE TSS.JOB_STATUS  ");
                sqlStr += string.Format(@"WHEN '0' THEN '建立任務中'  ");
                sqlStr += string.Format(@"WHEN '1' THEN '執行中'  ");
                sqlStr += string.Format(@"WHEN '2' THEN '完成'  ");
                sqlStr += string.Format(@"WHEN '3' THEN '失敗'  ");
                sqlStr += string.Format(@"WHEN '4' THEN '取消任務'  ");
                sqlStr += string.Format(@"END AS `STATUS` , ");
                sqlStr += string.Format(@"TSS.INSERT_TIME ,  ");
                sqlStr += string.Format(@"TSS.UPDATE_TIME , ");
                //等待時間 = 任務建立時間 - 任務最後更新時間
                sqlStr += string.Format(@"CASE TSS.JOB_STATUS ");
                sqlStr += string.Format(@"WHEN '0' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,now()) ");
                sqlStr += string.Format(@"WHEN '1' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,now()) ");
                sqlStr += string.Format(@"WHEN '2' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,TSS.UPDATE_TIME) ");
                sqlStr += string.Format(@"WHEN '3' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,TSS.UPDATE_TIME) ");
                sqlStr += string.Format(@"WHEN '4' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,TSS.UPDATE_TIME) ");
                sqlStr += string.Format(@"END AS WAIT_TIME, ");

                //到達時間  (B01已送達的時間
                sqlStr += string.Format(@"ASE_B01_TIME AS ARRIVE_TIME, ");

                //花費時間 (成功= B01已送達時間 - A01給指令時間  //沒完成 = 00:00:00  取消又不知道何時取消的所以不計
                sqlStr += string.Format(@"CASE TSS.JOB_STATUS ");
                sqlStr += string.Format(@"WHEN '0' then '' ");
                sqlStr += string.Format(@"WHEN '1' then '' ");
                sqlStr += string.Format(@"WHEN '2' then TIMESTAMPDIFF(SECOND,TSS.ASE_A01_TIME,TSS.ASE_B01_TIME) ");
                sqlStr += string.Format(@"WHEN '3' then '' ");
                sqlStr += string.Format(@"WHEN '4' then '' ");
                sqlStr += string.Format(@"END AS SPEND_TIME ");

                sqlStr += string.Format(@"FROM T_SUBTASK_STATUS AS TSS ");
                sqlStr += string.Format(@"left join BASE_TASK AS BT on BT.GUID = TSS.TASK_GUID and BT.ENABLE = 1 ");
                sqlStr += string.Format(@"left join BASE_TASK_SUBTASK AS BTST on BTST.TASK_GUID = TSS.TASK_GUID and TSS.Progress = BTST.PROGRESS and BTST.ENABLE = 1 ");
                sqlStr += string.Format(@"left join BASE_SUBTASK AS BST on BTST.SUBTASK_GUID = BST.GUID and BST.ENABLE = 1 ");




                //sqlStr += string.Format(@"FROM T_SUBTASK_STATUS AS TSS ,BASE_TASK AS BT ,BASE_SUBTASK AS BST,BASE_TASK_SUBTASK AS BTST ");
                
                sqlStr += string.Format(@"WHERE TSS.ENABLE = 1 ");
                //sqlStr += string.Format(@"AND TSS.PROGRESS = BTST.PROGRESS ");
                //sqlStr += string.Format(@"AND TSS.TASK_GUID = BTST.TASK_GUID ");
                //sqlStr += string.Format(@"AND BTST.SUBTASK_GUID = BST.GUID ");
                //sqlStr += string.Format(@"AND TSS.TASK_GUID = BT.GUID  ");


            }

            //主任務單號
            if (!string.IsNullOrEmpty((string)conditions.ASE_SEQ))
            {
                sqlStr += "And TSS.ASE_SEQ LIKE CONCAT('%' ,@ASE_SEQ, '%') ";
            }

            //貨架編號
            if (!string.IsNullOrEmpty((string)conditions.ASE_CAR_NO))
            {
                sqlStr += "And TSS.ASE_CAR_NO LIKE CONCAT('%' ,@ASE_CAR_NO, '%') ";
            }
            //任務名稱
            if (!string.IsNullOrEmpty((string)conditions.ASE_JOB_NAME))
            {
                sqlStr += "And TSS.ASE_JOB_NAME LIKE CONCAT('%' ,@ASE_JOB_NAME, '%') ";
            }
            //狀態
            if (!string.IsNullOrEmpty((string)conditions.JOB_STATUS))
            {
                sqlStr += $"And TSS.JOB_STATUS in ( {(string)conditions.JOB_STATUS} ) ";
            }
            //建立時間-起
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_START))
            {
                sqlStr += "AND TSS.INSERT_TIME >= @INSERT_TIME_START ";
            }
            //建立時間-終
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_END))
            {
                sqlStr += "AND TSS.INSERT_TIME <= @INSERT_TIME_END ";
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
        public string GetOneByGUID()
        {
            string sqlStr = string.Format(@"SELECT D.GUID,D.ID,D.NAME,D.REMARK FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND GUID = @GUID";

            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual;";

            return sqlStr;
        }
        /// <summary>
        /// 擴充欄位
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        /// <summary>
        /// 回傳SQL指令--新增
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            string sqlStr = "";

            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--更新
        /// </summary>
        /// <returns></returns>
        public string Update()
        {
            string sqlStr = " ";
            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--刪除
        /// </summary>
        /// <returns></returns>
        public string Delete()
        {
            string sqlStr = string.Format("UPDATE {0} SET " +
                "JOB_STATUS = 4," +
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
        public string ToUnbooking() 
        {
            string sqlStr = $"UPDATE T_BOOKING_INFO SET " +
                "Booking_STATUS = 0,"+
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                "WHERE SUBTASK_STATUS_GUID = @GUID; ";
            return sqlStr;
        }
        public string GetDataList()
        {
            string sqlStr = string.Format(@"SELECT GUID AS 'Key', ID , NAME AS 'Value' FROM {0} D ", CarrierTable);
            sqlStr += "WHERE D.ENABLE = 1 ";
            return sqlStr;
        }

    }
}