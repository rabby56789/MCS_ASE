using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace ASE.Models
{
    /// <summary>
    /// 使用者設定功能頁面相關資料操作:繼承資料存取通用介面
    /// </summary>
    public class SubTask 
    {
        public string MasterTable { get { return "T_SUBTASK_STATUS"; } }
        public string SubTable { get { return "T_SUBTASK_TRAVEL"; } }
        public string SubTaskTable { get { return "BASE_SUBTASK"; } }
        public string TaskTable { get { return "BASE_TASK"; } }
        public string Task_SubTable { get { return "BASE_TASK_SUBTASK"; } }
        public string ServerTable { get { return "BASE_AGV_SERVER"; } }
        public string IOTTable { get { return "BASE_IOTDEVICE"; } }
        public string CMDTable { get { return "CMD_MQTT"; } }
        public string WeightingTable { get { return "BASE_WEIGHTING"; } }
        public string TTrolleyStatusTable { get { return "T_TROLLEY_STATUS"; } }
        public string StorageTable { get { return "BASE_STORAGE"; } }
        public string AreaTable { get { return "BASE_AREA"; } }
        public string TrolleyTable { get { return "BASE_TROLLEY"; } }
        public string StorageStatusTable { get { return "T_STORAGE_STATUS"; } }

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
                    case "ID": case "NAME":
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
        public string CheckSubTaskType()
        {
            string sqlStr = $@"SELECT T1.GUID,T1.SUBTASK_TYPE 
                               FROM {MasterTable} T1 ";
            sqlStr += "WHERE T1.ENABLE = 1 ";
            sqlStr += "AND T1.SUBTASK_TYPE=@SUBTASK_TYPE ";
            sqlStr += "AND T1.TASKSTATUS IN(0,1,2) ";
            sqlStr += "ORDER BY PRIORITY DESC ;";

            return sqlStr;
        }
        public string GetSubTaskType()
        {
            string sqlStr = $@"SELECT T1.SUBTASK_TYPE 
                               FROM {SubTaskTable} T1 ";
            sqlStr += "WHERE T1.ENABLE = 1 ";
            sqlStr += "AND T1.INDEX=@INDEX ";
            sqlStr += "AND T1.SUBTASK_ID=@SUBTASK_ID ;";

            return sqlStr;
        }
        public string GetTask()
        {
            string sqlStr = $@"SELECT T1.GUID,T1.TASK_ID,T1.SUB_INDEX,T1.`INDEX`,T1.POSITIONCODEPATH,
                               T1.AGVCODE,T1.PODCODE,T1.PRIORITY,T1.TASKTYP,T1.MATERIALLOT,T1.TASKCODE,
                               T3.SUBTASK_ID,T3.SUBTASK_TYPE,T3.SERVER_FUNCTION,T3.SUBTASK_FUNCTION,
                               T4.SERVER_IP,T4.SERVER_PORT,T4.URL 
                               FROM {MasterTable} T1,{Task_SubTable} T2,{SubTaskTable} T3,{ServerTable} T4 ";
            sqlStr += "WHERE T1.ENABLE = 1 ";
            sqlStr += "AND T3.ENABLE = 1 ";
            sqlStr += "AND T1.SUBTASK_TYPE=@SUBTASK_TYPE ";
            sqlStr += "AND T1.TASK_ID=T2.TASK_ID ";
            sqlStr += "AND T1.SUB_INDEX=T2.`INDEX` ";
            sqlStr += "AND T1.`INDEX`=T3.`INDEX` ";
            sqlStr += "AND T2.SUBTASK_ID=T3.SUBTASK_ID ";
            sqlStr += "AND T3.SERVER_FUNCTION=T4.SERVER_FUNCTION ";
            sqlStr += "AND T1.TASKSTATUS IN(0,1,2) ";
            sqlStr += "ORDER BY WEIGHTING  ;";

            return sqlStr;
        }
        public string GetMQTTTask()
        {
            string sqlStr = $@"SELECT T1.GUID,T1.TASK_ID,T1.SUB_INDEX,T1.`INDEX`,
                               T3.SUBTASK_ID,T3.SUBTASK_TYPE,T3.SERVER_FUNCTION,
                               T4.SN_KEY,T4.REMARK,
                               T5.COMMAND,T5.COMMAND_JSON 
                               FROM {MasterTable} T1,{Task_SubTable} T2,{SubTaskTable} T3,{IOTTable} T4,{CMDTable} T5  ";
            sqlStr += "WHERE T1.ENABLE = 1 ";
            sqlStr += "AND T3.ENABLE = 1 ";
            sqlStr += "AND T1.SUBTASK_TYPE=@SUBTASK_TYPE ";
            sqlStr += "AND T1.TASK_ID=T2.TASK_ID ";
            sqlStr += "AND T1.SUB_INDEX=T2.`INDEX` ";
            sqlStr += "AND T1.`INDEX`=T3.`INDEX` ";
            sqlStr += "AND T2.SUBTASK_ID=T3.SUBTASK_ID ";
            sqlStr += "AND T3.SUBTASK_FUNCTION=T5.COMMAND ";
            sqlStr += "AND T3.SERVER_FUNCTION=T4.SN_KEY ";
            sqlStr += "AND T1.TASKSTATUS IN(0,1,2) ";
            sqlStr += "ORDER BY WEIGHTING ;";

            return sqlStr;
        }
        public string GetTaskStatusCount()
        {
            string sqlStr = @"SELECT COUNT(TASKSTATUS)AS STATUS,SUM(TASKSTATUS=1 OR TASKSTATUS=2)AS Queue ,SUM(TASKSTATUS=3)AS Success ,SUM(TASKSTATUS=4)AS Fail ";
            sqlStr += string.Format(@"FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND TO_DAYS(INSERT_TIME) = TO_DAYS(NOW());";
            return sqlStr;
        }
        public string GetUndonTask()
        {
            string sqlStr = string.Format(@"SELECT * FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND (TASKSTATUS = 1 OR TASKSTATUS = 2) ";
            sqlStr += "ORDER BY PRIORITY DESC ,";
            sqlStr += "INSERT_TIME DESC ;";

            return sqlStr;
        }
        public string UpdateTask()
        {
            string sqlStr = string.Format(@"UPDATE {0} SET 
                    REQCODE=@REQCODE,
                    REQTIME=@REQTIME, 
                    CODE=@CODE,
                    MESSAGE=@MESSAGE,
                    TASKSTATUS=@TASKSTATUS, 
                    TASKCODE=@TASKCODE, 
                    `INDEX`=@INDEX, 
                    SUBTASK_TYPE=@SUBTASK_TYPE, 
                    UPDATE_USER='QUEUE', UPDATE_TIME=now() ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND GUID = @GUID ";
            sqlStr += "AND TASKSTATUS = 0 ";
            
            
            return sqlStr;
        }
        public string UpdateTaskIndex()
        {
            string sqlStr = string.Format(@"UPDATE {0} SET                    
                    `INDEX`=@INDEX, 
                    SUBTASK_TYPE=@SUBTASK_TYPE,
                    UPDATE_USER='QUEUE', UPDATE_TIME=now() ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND GUID = @GUID ;";


            return sqlStr;
        }
        public string InsertTaskTravel()
        {
            string sqlStr = string.Format(@"INSERT INTO {0}(GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,
                            TASK_GUID,PROGRESS,SUBTASK_TYPE,POSITIONCODEPATH,WEIGHTING,
                            ASE_START_LOC,ASE_START_QRCODE,ASE_TARGET_LOC,ASE_TARGET_QRCODE,ASE_JOB_NAME,ASE_SEQ,ASE_CAR_NO,ASE_TEMP_BIN,
                            SUBTASK_STATUS_GUID,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,CTNRTYP,CTNRCODE,WBCODE,
                            PODCODE,PODDIR,MATERIALLOT,PRIORITY,TASKCODE,AGVCODE,DATA,TASKSTATUS,CODE,MESSAGE)", SubTable);
            sqlStr += string.Format(@"SELECT UUID(),INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,
                            TASK_GUID,PROGRESS,SUBTASK_TYPE,POSITIONCODEPATH,WEIGHTING,
                            ASE_START_LOC,ASE_START_QRCODE,ASE_TARGET_LOC,ASE_TARGET_QRCODE,ASE_JOB_NAME,ASE_SEQ,ASE_CAR_NO,ASE_TEMP_BIN,
                            GUID,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,CTNRTYP,CTNRCODE,WBCODE,
                            PODCODE,PODDIR,MATERIALLOT,PRIORITY,TASKCODE,AGVCODE,DATA,TASKSTATUS,CODE,MESSAGE FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";

            return sqlStr;
        }
        public string QueryInsertTaskTravel()
        {
            string sqlStr = string.Format(@"INSERT INTO {0}(GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,
                            TASK_ID,SUB_INDEX,`INDEX`,SUBTASK_TYPE,POSITIONCODEPATH,WEIGHTING,
                            SUBTASK_STATUS_GUID,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,CTNRTYP,CTNRCODE,WBCODE,
                            ASE_START_LOC,ASE_START_QRCODE,ASE_TARGET_LOC,ASE_TARGET_QRCODE,ASE_JOB_NAME,ASE_SEQ,
                            PODCODE,PODDIR,MATERIALLOT,PRIORITY,TASKCODE,AGVCODE,DATA,TASKSTATUS,CODE,MESSAGE)", SubTable);
            sqlStr += string.Format(@"SELECT UUID(),INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,
                            TASK_ID,SUB_INDEX,`INDEX`,SUBTASK_TYPE,POSITIONCODEPATH,WEIGHTING,
                            GUID,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,CTNRTYP,CTNRCODE,WBCODE,
                            ASE_START_LOC,ASE_START_QRCODE,ASE_TARGET_LOC,ASE_TARGET_QRCODE,ASE_JOB_NAME,ASE_SEQ,
                            PODCODE,PODDIR,MATERIALLOT,PRIORITY,TASKCODE,AGVCODE,DATA,TASKSTATUS,CODE,MESSAGE FROM {0} ", MasterTable);
            sqlStr += $@"WHERE TASKCODE=@TASKCODE;";

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
        public string GetWeighting()
        {
            string sqlStr = $@"SELECT PRIORITY FROM {WeightingTable} ";
            sqlStr += "WHERE ID=@WEIGHTING_ID ;";
            return sqlStr;
        }
        public string GetSubTaskInfo()
        {
            string sqlStr = $@"SELECT T1.TASK_GUID,T2.SUBTASK_TYPE FROM {Task_SubTable} T1,{SubTaskTable} T2,{TaskTable} T3 ";
            sqlStr += "WHERE T1.SUBTASK_GUID=T2.GUID AND T1.ENABLE=1 AND T2.ENABLE=1 AND T3.ENABLE=1 ";
            sqlStr += "AND T1.TASK_GUID=T3.GUID ";
            sqlStr += "AND T3.TASK_ID=@TASK_ID ";
            sqlStr += "AND T1.PROGRESS=@PROGRESS ;";
            //sqlStr += "AND T1.`INDEX`=@SUB_INDEX LIMIT 1;";
            //sqlStr += "ORDER BY T1.`INDEX` LIMIT 2;";
            return sqlStr;
        }
        //public string GetSubTaskInfo()
        //{
        //    string sqlStr = $@"SELECT T1.TASK_ID,T1.`INDEX` AS SUB_INDEX ,T1.POSITIONCODEPATH,T2.TASK_TYPE,T2.SUBTASK_ID,T2.SUBTASK_TYPE FROM {Task_SubTable} T1,{SubTaskTable} T2 ";
        //    sqlStr += "WHERE T1.SUBTASK_ID=T2.SUBTASK_ID ";
        //    sqlStr += "AND T1.TASK_ID=@TASK_ID ";
        //    sqlStr += "AND T2.`INDEX`=@INDEX ";
        //    //sqlStr += "AND T1.`INDEX`=@SUB_INDEX LIMIT 1;";
        //    sqlStr += "ORDER BY T1.`INDEX` LIMIT 2;";
        //    return sqlStr;
        //}
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() AS UUID from dual;"; 

            return sqlStr;
        }
        public string GetLocation()
        {
            string sqlStr = $@"SELECT TS.LOCATION_ID,TS.TROLLEY_ID,ST.ID,TT.NAME AS CAR_NO FROM {TTrolleyStatusTable} TS ,{StorageTable} ST ,{TrolleyTable} TT ";
            sqlStr += " WHERE TS.ENABLE = 1 AND ST.ENABLE = 1 AND TT.ENABLE = 1 ";
            sqlStr += " AND TS.LOCATION_ID = ST.QRCODE ";
            //sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM {MasterTable} WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
            //sqlStr += " AND TS.TROLLEY_ID = @car_no ;";
            //sqlStr += " AND TS.TROLLEY_ID LIKE CONACAT('%',@car_no) ;";
            sqlStr += " AND TS.TROLLEY_ID = TT.ID ";
            sqlStr += " AND TT.NAME = @car_no ;";
            return sqlStr;
        }
        public string GetStartLocation(string parentGuid = "")
        {
            string sqlStr = $@"SELECT TS.LOCATION_ID,TS.TROLLEY_ID,TT.NAME AS CAR_NO,ST.ID FROM {TTrolleyStatusTable} TS ,{StorageTable} ST ,{TrolleyTable} TT ";
            sqlStr += " WHERE TS.ENABLE = 1 AND ST.ENABLE =1 AND TT.ENABLE = 1 ";
            sqlStr += " AND TS.LOCATION_ID = ST.QRCODE ";
            sqlStr += " AND TS.TROLLEY_ID = TT.ID ";
            if (string.IsNullOrEmpty(parentGuid))
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM {MasterTable} WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
            else
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM {MasterTable} WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL AND GUID <> '{parentGuid}' ) ";
            sqlStr += " AND ST.ID =@START ";
            sqlStr += " ORDER BY ID LIMIT 1 ;";
            return sqlStr;
        }
        public string GetTargetLocation()
        {
            string sqlStr = $@"SELECT ST.ID,ST.QRCODE FROM {StorageTable} ST ";
            sqlStr += " WHERE ST.ENABLE = 1 ";
            sqlStr += $" AND ST.QRCODE NOT IN (SELECT LOCATION_ID FROM {TTrolleyStatusTable} ) ";
            sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM {MasterTable} WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
            sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_TARGET_QRCODE FROM {MasterTable} WHERE JOB_STATUS IN(0,1) AND ASE_TARGET_QRCODE IS NOT NULL ) ";
            sqlStr += " AND ST.ID =@TARGET ";
            sqlStr += " ORDER BY ID LIMIT 1 ;";
            return sqlStr;
        }
        public string GetStartAreaLocation(string parentGuid = "")
        {
            string sqlStr = $@"SELECT TS.LOCATION_ID,TS.TROLLEY_ID,TT.NAME AS CAR_NO,ST.ID FROM {TTrolleyStatusTable} TS ,{StorageTable} ST ,{TrolleyTable} TT ,{AreaTable} AT ";
            sqlStr += " WHERE TS.ENABLE = 1 AND ST.ENABLE =1 AND TT.ENABLE = 1 AND AT.ENABLE = 1 ";
            sqlStr += " AND TS.LOCATION_ID = ST.QRCODE ";
            sqlStr += " AND TS.TROLLEY_ID = TT.ID ";
            if (string.IsNullOrEmpty(parentGuid))
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM {MasterTable} WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
            else
                sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM {MasterTable} WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL AND GUID <> '{parentGuid}') ";
            sqlStr += $" AND ST.QRCODE NOT IN (SELECT BSto.QRCODE FROM mcs.base_storage BSto, mcs.t_storage_status TStoSta where TStoSta.STORAGE_GUID = BSto.GUID) ";
            sqlStr += $" and TT.NAME not in (select car_no from t_task_queue where status in (0)) ";//20230321 Ruby 呼叫空車任務會將Queue內有任務的貨架叫走
            //sqlStr += " AND ST.ID LIKE CONCAT(@STARTAREA, '%') ";
            sqlStr += " AND ST.GROUP_GUID = AT.GUID ";
            sqlStr += " AND AT.ID = @STARTAREA ";
            sqlStr += " ORDER BY ID LIMIT 1 ;";
            return sqlStr;
        }
        public string GetTargetAreaLocation()
        {
            string sqlStr = $@"SELECT ST.ID,ST.QRCODE FROM {StorageTable} ST ,{AreaTable} AT";
            sqlStr += " WHERE ST.ENABLE = 1 AND AT.ENABLE = 1 ";
            sqlStr += $" AND ST.QRCODE NOT IN (SELECT LOCATION_ID FROM {TTrolleyStatusTable} ) ";
            sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_START_QRCODE FROM {MasterTable} WHERE JOB_STATUS IN (0,1) AND ASE_START_QRCODE IS NOT NULL ) ";
            sqlStr += $" AND ST.QRCODE NOT IN (SELECT ASE_TARGET_QRCODE FROM {MasterTable} WHERE JOB_STATUS IN(0,1) AND ASE_TARGET_QRCODE IS NOT NULL ) ";
            //sqlStr += " AND ST.ID LIKE CONCAT(@TARGETAREA, '%') ";
            sqlStr += " AND ST.GROUP_GUID = AT.GUID ";
            sqlStr += " AND AT.ID = @TARGETAREA ";
            sqlStr += " ORDER BY ID LIMIT 1 ;";
            return sqlStr;
        }
        public string ASEToRCSGetLocation()
        {
            string sqlStr = $@"SELECT ST.QRCODE FROM {StorageTable} ST ";
            sqlStr += " WHERE ST.ENABLE = 1 ";
            sqlStr += " AND ST.ID = @Location_ID ";
            return sqlStr;
        }
        public string GetDataList()
        {
            string sqlStr = string.Format(@"SELECT GUID AS 'Key', ID , NAME AS 'Value' FROM {0} D ", MasterTable);
            sqlStr += "WHERE D.ENABLE = 1 ";
            return sqlStr;
        }
        
        /// <summary>
        /// 回傳SQL指令--新增
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,TASK_GUID,PROGRESS,SUBTASK_TYPE,TASKTYP,POSITIONCODEPATH,PODCODE,PODDIR,MATERIALLOT,PRIORITY,AGVCODE,WEIGHTING,
                                        ASE_START_LOC,ASE_START_QRCODE,ASE_TARGET_LOC,ASE_TARGET_QRCODE,ASE_JOB_NAME,ASE_SEQ,ASE_CAR_NO,
                                        INSERT_USER,INSERT_TIME,ASE_A01_TIME ) ", MasterTable);
            sqlStr += @"VALUES (@GUID,@TASK_GUID,@PROGRESS,@SUBTASK_TYPE,@TASKTYP,@POSITIONCODEPATH,@PODCODE,@PODDIR,@MATERIALLOT,@PRIORITY,@AGVCODE,@WEIGHTING,
                                        @ASE_START_LOC,@ASE_START_QRCODE,@ASE_TARGET_LOC,@ASE_TARGET_QRCODE,@ASE_JOB_NAME,@ASE_SEQ,@ASE_CAR_NO,
                                        @INSERT_USER,now(),@ASE_A01_TIME );";
            sqlStr +="";

            return sqlStr;
        }
        public string TaskUpdateStatus(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"UPDATE {0} SET                     
                    TASKSTATUS=@TASKSTATUS,", MasterTable); ;
            if (!string.IsNullOrEmpty((string)conditions.AGVCODE))
            {
                sqlStr += "AGVCODE=@AGVCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.TASKTYP))
            {
                sqlStr += "TASKTYP=@TASKTYP,";
            }
            sqlStr +="UPDATE_USER='QUEUE', UPDATE_TIME=now() ";
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND TASKCODE = @TASKCODE ;";


            return sqlStr;
        }

        //
        public string GetTaskId(string jobName)
        {
            string sqlStr = $@"SELECT TASK_ID from {TaskTable} BT";
            sqlStr += " WHERE BT.ENABLE = 1 ";
            sqlStr += string.Format( " AND TASK_NAME = '{0}' ; ", jobName);

            return sqlStr;
        }
        //找起點、終點的區域
        public string GetStartArea(string startStorageId)
        {
            //string sqlStr = $@"SELECT TS.LOCATION_ID,TS.TROLLEY_ID,ST.ID,TT.NAME AS CAR_NO FROM {TTrolleyStatusTable} TS ,{StorageTable} ST ,{TrolleyTable} TT ";
            string sqlStr = $@" SELECT ID FROM {AreaTable} BA ";
            sqlStr += " WHERE ENABLE = 1 ";
            sqlStr += $@" AND GUID = (SELECT GROUP_GUID FROM {StorageTable} S WHERE ENABLE = 1 AND GUID = (SELECT GUID FROM {StorageTable} WHERE ID = '{startStorageId}'))  ;";
   
            

            return sqlStr;
        }
        public string GetTargetArea(string targetStorageId)
        {
            //string sqlStr = $@"SELECT TS.LOCATION_ID,TS.TROLLEY_ID,ST.ID,TT.NAME AS CAR_NO FROM {TTrolleyStatusTable} TS ,{StorageTable} ST ,{TrolleyTable} TT ";
            string sqlStr = $@" SELECT ID FROM {AreaTable} BA ";
            sqlStr += " WHERE ENABLE = 1 ";
            sqlStr += $@" AND GUID = (SELECT GROUP_GUID FROM {StorageTable} S WHERE ENABLE = 1 AND GUID = (SELECT GUID FROM {StorageTable} WHERE ID = '{targetStorageId}')) ; ";


            return sqlStr;
        }
        //
        public string GetTaskName(string taskName)
        {
            string sqlStr = string.Format(@"SELECT COUNT(*) FROM {0} D ", TaskTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += string.Format("AND TASK_NAME = '{0}' ; ", taskName);

            return sqlStr;
        }
        public string CreateAreaTable() 
        {
            string sqlStr = @"CREATE TABLE `base_area` (
  `GUID` varchar(38) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'uuid()' COMMENT '[基本欄] 唯一值',
  `ID` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `NAME` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `INSERT_USER` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `UPDATE_USER` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '[基本欄] 更新者',
  `UPDATE_TIME` datetime DEFAULT NULL COMMENT '[基本欄] 更新時間',
  `ENABLE` tinyint DEFAULT '1' COMMENT '[基本欄] 資料有效性',
  `REMARK` mediumtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY(`ID`),
  UNIQUE KEY `GUID_UNIQUE` (`GUID`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '基本資料_區域儲位'; ";
            return sqlStr;
        }
        public string CreateStorageTable()
        {
            string sqlStr = @"CREATE TABLE `base_storage` (
  `GUID` varchar(38) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'uuid()' COMMENT '[基本欄] 唯一值',
  `INSERT_USER` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `UPDATE_USER` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '[基本欄] 更新者',
  `UPDATE_TIME` datetime DEFAULT NULL COMMENT '[基本欄] 更新時間',
  `ENABLE` tinyint DEFAULT '1' COMMENT '[基本欄] 資料有效性',
  `REMARK` mediumtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `ID` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `NAME` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `QRCODE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `MAP_GUID` varchar(38) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `GROUP_GUID` varchar(38) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '儲位對應區域',
  `OverTime` int DEFAULT '0' COMMENT '物料超時時間(分)0代表無限制',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `GUID_UNIQUE` (`GUID`),
  KEY `base_storage_idx1` (`GROUP_GUID`,`ENABLE`,`QRCODE`,`GUID`,`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='基本資料_儲位';";
            return sqlStr;
        }
        public string CreateTrolleyTable() 
        {
            string sqlStr = @"CREATE TABLE `base_trolley` (
  `GUID` varchar(38) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'uuid()' COMMENT '[基本欄] 唯一值',
  `ID` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `NAME` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `TYPE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `INSERT_USER` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `UPDATE_USER` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '[基本欄] 更新者',
  `UPDATE_TIME` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '[基本欄] 更新時間',
  `ENABLE` tinyint DEFAULT '1' COMMENT '[基本欄] 資料有效性',
  `REMARK` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '描述',
  `QRCODE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `GROUP_GUID` varchar(38) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `SEQ` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `GUID_UNIQUE` (`GUID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='台車基本資料';";

            return sqlStr;
        }

    }
}