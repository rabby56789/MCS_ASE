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
    public class Transaction : ISqlCreator
    {
        /// <summary>
        /// 台車狀態
        /// </summary>
        public string Trolley { get { return "t_trolley_status"; } }
        /// <summary>
        /// 任務清單列表
        /// </summary>
        public string Task { get { return "t_task_status"; } }
        /// <summary>
        /// 生產計劃表
        /// </summary>
        public string Plan { get { return "t_production_plan"; } }
        public string SubTask { get { return "t_subtask_status"; } }

        public IDataParameter[] CreateParameterAry(JObject input)
        {
            if (input is null)
            {
                return new IDataParameter[] { };
            }

            List<MySqlParameter> parmList = new List<MySqlParameter>();
            List<string> toVarchar = new List<string> {
                "QUERY_DATE_KEY","DATE_KEY", "STEPID", "FOIL_TYPE", "REEL_NO", "REEL_STATUS", "EQPID", "RCNO", "RCNO_STATUS", "PRODID"
            };
            List<string> toDouble = new List<string> {
                "DATA_COUNT", "SUM_HOURS", "PORITY", "QTY_KPC", "PRESTEPOUTQTY", "NEED_HOURS", "TOTAL_M"
            };
            List<string> likeCondition = new List<string> { "QUERY_DATE_KEY", "QUERY_EQP_ID", "QUERY_FOIL_TYPE" };

            //JSON項目逐一加入參數表中
            foreach (var x in input)
            {
                MySqlParameter parm = new MySqlParameter();
                string name = x.Key;
                JToken value = x.Value;

                parm.ParameterName = "@" + name;
                parm.Value = value;

                if (parm.Value.ToString() != "")
                {
                    //指定參數資料型態
                    if (toVarchar.Contains(name))
                    {
                        parm.DbType = System.Data.DbType.String;
                    }
                    else if (toDouble.Contains(name))
                    {
                        parm.DbType = System.Data.DbType.Double;
                    }
                    else if (name == "PLAN_TIME")
                    {
                        parm.DbType = System.Data.DbType.String;
                    }
                }
                else
                {
                    parm.Value = DBNull.Value;
                    parm.IsNullable = true;
                }

                //模糊查詢條件
                if (likeCondition.Contains(name))
                {
                    parm.Value += "%";
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
                case "QueryTrollyStatus": //取得一台目前為空的台車
                    sqlStr = GetOneEmptyTrolly();
                    break;
                case "AddTask":
                    sqlStr = AddTask();
                    break;
                case "GetCurrentTrollyStatus":
                    sqlStr = GetCurrentTrollyStatus();
                    break;
                case "GetMaterialLotInfo":
                    sqlStr = GetMaterialLotInfo();
                    break;
                case "AddPlan":
                    sqlStr = AddPlan();
                    break;
                default:
                    break;
            }

            return sqlStr;
        }

        /// <summary>
        /// 取得一台空台車資訊
        /// </summary>
        /// <returns></returns>
        public string GetOneEmptyTrolly()
        {
            string sqlStr = $"SELECT TROLLEY_ID,LOCATION_ID FROM {Trolley} ";
            sqlStr += $"WHERE ENABLE = 1 AND TROLLEY_STATUS = '1' ";
            sqlStr += "AND MATERIALLOT IN (NULL,'') LIMIT 1;";

            return sqlStr;
        }

        /// <summary>
        /// 新增任務
        /// </summary>
        /// <returns></returns>
        public string AddTask()
        {
            string sqlStr = $"INSERT INTO {Task} ";
            sqlStr += "(GUID,INSERT_USER,POSITIONCODE1,POSITIONCODE2,PODCODE,PRIORITY,TASKSTATUS) ";
            sqlStr += "VALUE(UUID(),@INSERT_USER,@POSITIONCODE1,@POSITIONCODE2,@PODCODE,@PRIORITY,@TASKSTATUS);";

            return sqlStr;
        }

        /// <summary>
        /// 取得台車狀態
        /// </summary>
        /// <returns></returns>
        public string GetCurrentTrollyStatus()
        {
            string sqlStr = $"SELECT TROLLEY_ID,LOCATION_ID,MATERIALLOT,TROLLEY_STATUS FROM {Trolley} WHERE ENABLE = 1;";
            return sqlStr;
        }

        /// <summary>
        /// 依物料批號查詢狀態
        /// </summary>
        /// <returns></returns>
        public string GetMaterialLotInfo()
        {
            string sqlStr = $"SELECT TROLLEY_ID,LOCATION_ID,MATERIALLOT,TROLLEY_STATUS FROM {Trolley} WHERE ENABLE = 1 AND MATERIALLOT = @MATERIAL_ID;";
            return sqlStr;
        }

        /// <summary>
        /// 加入生產計劃表
        /// </summary>
        /// <returns></returns>
        public string AddPlan()
        {
            string sqlStr = $"INSERT INTO {Plan} (GUID,INSERT_USER,INSERT_TIME,DATE_KEY,DATA_COUNT,SUM_HOURS,STEPID,FOIL_TYPE,REEL_NO,REEL_STATUS,PORITY,EQPID,RCNO,RCNO_STATUS,PRODID,QTY_KPC,PRESTEPOUTQTY,NEED_HOURS,TOTAL_M)";
            sqlStr += "VALUES(uuid(),@INSERT_USER,NOW(),@DATE_KEY,@DATA_COUNT,@SUM_HOURS,@STEPID,@FOIL_TYPE,@REEL_NO,@REEL_STATUS,@PORITY,@EQPID,@RCNO,@RCNO_STATUS,@PRODID,@QTY_KPC,@PRESTEPOUTQTY,@NEED_HOURS,@TOTAL_M);";

            return sqlStr;
        }
        public string GetPlanTime()
        {
            string sqlStr = $"SELECT PLAN_TIME,NEED_HOURS FROM {Plan} WHERE EQPID=@EQPID AND FOIL_TYPE=@FOIL_TYPE AND PLAN_TIME IS NOT NULL ORDER BY PLAN_TIME DESC LIMIT 1";

            return sqlStr;
        }
        public string UpdatePlanTime()
        {
            string sqlStr = $"UPDATE {Plan} SET PLAN_TIME=@PLAN_TIME WHERE GUID=@GUID ";

            return sqlStr;
        }
        public string GetPlan()
        {
            string sqlStr = $"SELECT P.GUID,P.EQPID,P.FOIL_TYPE,P.REEL_NO,P.RCNO,P.PRODID,P.NEED_HOURS FROM {Plan} P ";
            sqlStr += "WHERE P.ENABLE = 1 ";
            sqlStr += "AND P.PLAN_STATUS = 0 ";
            sqlStr += "AND P.PLAN_TIME IS NULL ";
            sqlStr += "ORDER BY P.DATE_KEY, ";
            sqlStr += "P.PORITY ";

            return sqlStr;
        }
        /// <summary>
        /// 查詢生產計畫
        /// </summary>
        /// <returns></returns>
        public string QueryMIPlan(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Empty;

            if (getCount)
            {
                sqlStr = $"SELECT COUNT(*) AS Count ";
            }
            else
            {
                //sqlStr = $"SELECT * FROM {Plan} ";
                //sqlStr = $"SELECT TASKCODE,ASE_JOB_NAME,JOB_STATUS,ASE_START_LOC,ASE_TARGET_LOC,AGVCODE,ASE_CAR_NO,WEIGHTING,JOB_STATUS,PROGRESS,INSERT_TIME,UPDATE_TIME FROM  {SubTask} ";
                sqlStr = $@"SELECT ASE_SEQ,ASE_JOB_NAME,JOB_STATUS,ASE_START_LOC,ASE_TARGET_LOC,AGVCODE,ASE_CAR_NO,WEIGHTING,
                            CASE TSS.JOB_STATUS  
                            WHEN '0' THEN '建立任務中'  
                            WHEN '1' THEN '執行中'  
                            WHEN '2' THEN '完成'  
                            WHEN '3' THEN '失敗'  
                            WHEN '4' THEN '取消任務'  
                            END AS `STATUS` , 
                            SUBTASK_NAME,TSS.INSERT_TIME AS START_TIME,
                            CASE TSS.JOB_STATUS 
                            WHEN '0' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,now())
                            WHEN '1' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,now()) 
                            WHEN '2' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,TSS.UPDATE_TIME)
                            WHEN '3' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,TSS.UPDATE_TIME)
                            WHEN '4' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,TSS.UPDATE_TIME)
                            END AS WAIT_TIME ";
            }
            sqlStr += $@"FROM  {SubTask} TSS
                        LEFT JOIN base_task BT ON TSS.TASK_GUID = BT.GUID
                        LEFT JOIN base_task_subtask BTS ON BTS.PROGRESS = TSS.PROGRESS AND BTS.TASK_GUID = TSS.TASK_GUID 
                        LEFT JOIN base_subtask BS ON BS.GUID = BTS.SUBTASK_GUID 
                        WHERE TSS.ENABLE = 1 AND TSS.ASE_JOB_NAME != '小車調度' AND TSS.ASE_SEQ not like 'M_%'";
            
            //搜尋當日任務
            if (!string.IsNullOrEmpty((string)conditions.PLAN_TIME))
            {
                sqlStr += "AND ( TSS.INSERT_TIME BETWEEN @PLAN_TIME AND NOW() OR TSS.UPDATE_TIME BETWEEN @PLAN_TIME AND NOW() ) ";
            }
            if (!string.IsNullOrEmpty((string)conditions.JOB_STATUS) && (string)conditions.JOB_STATUS == "1")
            {
                sqlStr += "AND tss.JOB_STATUS in (0,1) ";
            }
            if (!string.IsNullOrEmpty((string)conditions.JOB_STATUS) && (string)conditions.JOB_STATUS == "2")
            {
                sqlStr += "AND tss.JOB_STATUS in (2) ";
            }
            if (!string.IsNullOrEmpty((string)conditions.JOB_STATUS) && (string)conditions.JOB_STATUS == "3")
            {
                sqlStr += "AND tss.ASE_JOB_NAME = '暫存'  ";
            }

            //查詢數量不需換頁
            if (getCount)
            {
                return sqlStr;
            }

            //if (parm.TryGetValue("sort", out _))
            //{
            //    //sqlStr += $@"ORDER BY {(string)conditions.sort} {(string)conditions.order} ";
                
            //}
            sqlStr += $@"ORDER BY TSS.INSERT_TIME desc ";
            ////含排序 or 換頁
            //if (parm.TryGetValue("page", out _))
            //{
            //    int offset = (int)conditions.rows * ((int)conditions.page - 1);
            //    sqlStr += $"LIMIT {conditions.rows} ";
            //    sqlStr += $"OFFSET {offset} ";
            //}
            sqlStr += ";";

            return sqlStr;
        }
        public string QueryFEdata(string FEseq)
        {
            
            string sqlStr = string.Empty;

            sqlStr = $@"SELECT ASE_SEQ,ASE_JOB_NAME,JOB_STATUS,ASE_START_LOC,ASE_TARGET_LOC,AGVCODE,ASE_CAR_NO,WEIGHTING,
                            CASE TSS.JOB_STATUS  
                            WHEN '0' THEN '建立任務中'  
                            WHEN '1' THEN '執行中'  
                            WHEN '2' THEN '完成'  
                            WHEN '3' THEN '失敗'  
                            WHEN '4' THEN '取消任務'  
                            END AS `STATUS` , 
                            SUBTASK_NAME,TSS.INSERT_TIME AS START_TIME,
                            CASE TSS.JOB_STATUS 
                            WHEN '0' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,now())
                            WHEN '1' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,now()) 
                            WHEN '2' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,TSS.UPDATE_TIME)
                            WHEN '3' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,TSS.UPDATE_TIME)
                            WHEN '4' then TIMESTAMPDIFF(SECOND,TSS.INSERT_TIME,TSS.UPDATE_TIME)
                            END AS WAIT_TIME ";
            sqlStr += $@"FROM  {SubTask} TSS
                        LEFT JOIN base_task BT ON TSS.TASK_GUID = BT.GUID
                        LEFT JOIN base_task_subtask BTS ON BTS.PROGRESS = TSS.PROGRESS AND BTS.TASK_GUID = TSS.TASK_GUID 
                        LEFT JOIN base_subtask BS ON BS.GUID = BTS.SUBTASK_GUID 
                        WHERE TSS.ENABLE = 1 AND TSS.ASE_SEQ = '{FEseq}' ";
            return sqlStr;
        }
    }
}