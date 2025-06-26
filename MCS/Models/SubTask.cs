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
    public class SubTask
    {
        public string MasterTable { get { return "T_SUBTASK_STATUS"; } }
        public string SubTable { get { return "T_SUBTASK_TRAVEL"; } }
        public string SubTaskTable { get { return "BASE_SUBTASK"; } }
        public string TaskTable { get { return "BASE_TASK"; } }
        public string Task_SubTable { get { return "BASE_TASK_SUBTASK"; } }
        public string ServerTable { get { return "BASE_SERVER"; } }
        public string WeightingTable { get { return "BASE_WEIGHTING"; } }
        public string RCSAPITable { get { return "LOG_RCS_AGVCALLBACK"; } }
        public string StorageTable { get { return "BASE_STORAGE"; } }
        /// <summary>
        /// 用JObject生成對應的SQL參數陣列
        /// </summary>
        /// <param name="input">前端輸入值</param>
        /// <returns></returns>
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
        //public string GetSubTaskType()
        //{
        //    string sqlStr = $@"SELECT T1.SUBTASK_TYPE 
        //                       FROM {SubTaskTable} T1 ";
        //    sqlStr += "WHERE T1.ENABLE = 1 ";
        //    sqlStr += "AND T1.PROGRESS=@PROGRESS ";
        //    sqlStr += "AND T1.SUBTASK_ID=@SUBTASK_ID ;";

        //    return sqlStr;
        //}
        public string GetSubTaskType()
        {
            string sqlStr = $@"SELECT T2.SUBTASK_TYPE 
                               FROM {Task_SubTable} T1,{SubTaskTable} T2 ";
            sqlStr += "WHERE T1.ENABLE = 1 ";
            sqlStr += "AND T2.ENABLE = 1 ";
            sqlStr += "AND T1.SUBTASK_GUID = T2.GUID ";
            sqlStr += "AND T1.PROGRESS=@PROGRESS ";
            sqlStr += "AND T1.TASK_GUID=@TASK_GUID ;";

            return sqlStr;
        }
        public string GetB01Task()
        {
            string sqlStr = string.Format(@"SELECT * FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "GUID=@GUID ;";
            

            return sqlStr;
        }
        public string GetTask()
        {
            string sqlStr = $@"SELECT T1.GUID,T1.TASK_ID,T1.SUB_INDEX,T1.`INDEX`,T1.POSITIONCODEPATH,
                               T1.AGVCODE,T1.PODCODE,T1.PRIORITY,T1.TASKTYP,T1.MATERIALLOT,T1.TASKCODE,
                               T3.SUBTASK_TYPE,T3.SERVER_ID,
                               T4.SERVER_IP,T4.SERVER_PORT,T4.URL 
                               FROM {MasterTable} T1,{Task_SubTable} T2,{SubTaskTable} T3,{ServerTable} T4 ";
            sqlStr += "WHERE T1.ENABLE = 1 ";
            sqlStr += "AND T1.TASK_ID=T2.TASK_ID ";
            sqlStr += "AND T1.SUB_INDEX=T2.`INDEX` ";
            sqlStr += "AND T1.`INDEX`=T3.`INDEX` ";
            sqlStr += "AND T2.SUBTASK_ID=T3.SUBTASK_ID ";
            sqlStr += "AND T3.SERVER_FUNCTION=T4.SERVER_FUNCTION ";
            sqlStr += "AND T1.TASKSTATUS IN(0,1,2) ";
            sqlStr += "ORDER BY PRIORITY DESC ;";

            return sqlStr;
        }
        /// <summary>
        /// 取得任務資料，使用TASKCODE
        /// </summary>
        /// <returns></returns>
        public string GetTaskFunction()
        {
            string sqlStr = $@"SELECT T1.GUID,T1.TASK_GUID,T1.PROGRESS,T1.POSITIONCODEPATH,T1.ASE_JOB_NAME,
                               T1.AGVCODE,T1.PODCODE,T1.PRIORITY,T1.TASKTYP,T1.MATERIALLOT,T1.TASKCODE,                               
                               T3.SUBTASK_ID,T3.SUBTASK_TYPE,T3.SERVER_FUNCTION,T3.SUBTASK_FUNCTION  
                               FROM {MasterTable} T1,{Task_SubTable} T2,{SubTaskTable} T3 ";
            sqlStr += "WHERE T1.ENABLE = 1 ";
            sqlStr += "AND T1.TASK_GUID=T2.TASK_GUID ";
            //sqlStr += "AND T1.SUB_INDEX=T2.`INDEX` ";
            sqlStr += "AND T1.PROGRESS=T2.PROGRESS ";
            sqlStr += "AND T2.SUBTASK_GUID=T3.GUID ";
            sqlStr += "AND T1.TASKCODE= @TASKCODE ";
            sqlStr += "AND T3.SERVER_FUNCTION= @SERVER_FUNCTION ";
            sqlStr += "AND T3.SUBTASK_TYPE= @SUBTASK_TYPE ;";

            return sqlStr;
        }
        /// <summary>
        /// 取得任務資料，使用GUID
        /// </summary>
        /// <returns></returns>
        public string GetTaskFunction_GUID()
        {
            string sqlStr = $@"SELECT T1.GUID,T1.TASK_GUID,T1.PROGRESS,T1.POSITIONCODEPATH,
                               T1.AGVCODE,T1.PODCODE,T1.PRIORITY,T1.TASKTYP,T1.MATERIALLOT,T1.TASKCODE,                               
                               T3.SUBTASK_ID,T3.SUBTASK_TYPE,T3.SERVER_FUNCTION 
                               FROM {MasterTable} T1,{Task_SubTable} T2,{SubTaskTable} T3 ";
            sqlStr += "WHERE T1.ENABLE = 1 ";
            sqlStr += "AND T1.TASK_GUID=T2.TASK_GUID ";
            //sqlStr += "AND T1.SUB_INDEX=T2.`INDEX` ";
            sqlStr += "AND T1.PROGRESS=T2.PROGRESS ";
            sqlStr += "AND T2.SUBTASK_GUID=T3.GUID ";
            sqlStr += "AND T1.GUID=@GUID ";
            sqlStr += "AND T3.SERVER_FUNCTION=@SERVER_FUNCTION ";
            sqlStr += "AND T3.SUBTASK_TYPE=@SUBTASK_TYPE ;";

            return sqlStr;
        }
        /// <summary>
        /// 更新電梯通知
        /// </summary>
        /// <returns></returns>
        public string UPDATE_SUBTASK_STATUS_ELVT_FLOOR(string elvtStatus)
        {
            string sqlStr = $@"UPDATE {MasterTable} SET ";
            if (elvtStatus== "ELVT_START_FLOOR")
            {
                sqlStr += "ELVT_START_FLOOR =@ELVT_START_FLOOR, ";
            }
            else
            {
                sqlStr += "ELVT_TARGET_FLOOR =@ELVT_TARGET_FLOOR, ";
            }

            sqlStr += "ELVT_TASK_GUID=@ELVT_TASK_GUID, ";
            sqlStr += "ELVT_ID=@ELVT_ID, ";
            sqlStr += "ELVT_SPACE_ID=@ELVT_SPACE_ID, ";
            sqlStr += "ELVT_SPACE_INDEX=@ELVT_SPACE_INDEX, ";
            sqlStr += "ELVT_SPACE_QRCODE=@ELVT_SPACE_QRCODE, ";
            sqlStr += "ELVT_SPACE_STATUS_TROLLEY_ID=@ELVT_SPACE_STATUS_TROLLEY_ID, ";
            sqlStr += "ELVT_SPACE_STATUS_GUID=@ELVT_SPACE_STATUS_GUID, ";

            sqlStr += "UPDATE_USER ='MCS_agvCallback', UPDATE_TIME=now() ";
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND GUID = @GUID ;";

            return sqlStr;
        }
        //public string GetTaskFunction()
        //{
        //    string sqlStr = $@"SELECT T1.GUID,T1.TASK_ID,T1.SUB_INDEX,T1.`INDEX`,T1.POSITIONCODEPATH,
        //                       T1.AGVCODE,T1.PODCODE,T1.PRIORITY,T1.TASKTYP,T1.MATERIALLOT,T1.TASKCODE,                               
        //                       T3.SUBTASK_TYPE,T3.SERVER_ID,T3.SUBTASK_FUNCTION,
        //                       T4.SERVER_IP,T4.SERVER_PORT,T4.URL 
        //                       FROM {MasterTable} T1,{Task_SubTable} T2,{SubTaskTable} T3,{ServerTable} T4 ";
        //    sqlStr += "WHERE T1.ENABLE = 1 ";
        //    sqlStr += "AND T1.TASK_ID=T2.TASK_ID ";
        //    sqlStr += "AND T1.SUB_INDEX=T2.`INDEX` ";
        //    sqlStr += "AND T1.`INDEX`=T3.`INDEX` ";
        //    sqlStr += "AND T2.SUBTASK_ID=T3.SUBTASK_ID ";
        //    sqlStr += "AND T3.SERVER_FUNCTION=T4.SERVER_FUNCTION ";
        //    sqlStr += "AND T1.TASKCODE=@TASKCODE ";
        //    sqlStr += "AND T3.SUBTASK_FUNCTION=@SUBTASK_FUNCTION ;";

        //    return sqlStr;
        //}
        public string GetWeighting()
        {
            string sqlStr = $@"SELECT PRIORITY FROM {WeightingTable} ";
            sqlStr += "WHERE ID=@WEIGHTING_ID ;";
            return sqlStr;
        }
        public string GetSubTaskMaxIndex()
        {
            string sqlStr = $@"SELECT max(PROGRESS) AS MAX_PROGRESS FROM {Task_SubTable} ";
            sqlStr += "WHERE TASK_GUID=@TASK_GUID ;";
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
        public string GetTempBin()
        {
            string sqlStr = $@"SELECT T1.ID,T1.QRCODE FROM {StorageTable} T1 ";
            sqlStr += "WHERE T1.ENABLE=1 ";            
            sqlStr += "AND T1.QRCODE=@QRCODE ;";
            //sqlStr += "AND T1.`INDEX`=@SUB_INDEX LIMIT 1;";
            //sqlStr += "ORDER BY T1.`INDEX` LIMIT 2;";
            return sqlStr;
        }
        public string UpdateTaskMCSStatus()
        {
            string sqlStr = string.Format(@"UPDATE {0} SET                     
                    JOB_STATUS=2,
                    UPDATE_USER='QUEUE', UPDATE_TIME=now() ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND GUID = @GUID ";
            return sqlStr;
        }
        public string UpdateB01TempBin()
        {
            string sqlStr = string.Format(@"UPDATE {0} SET                     
                    ASE_TEMP_BIN=@ASE_TEMP_BIN,
                    UPDATE_USER='QUEUE', UPDATE_TIME=now() ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND TASKCODE = @TASKCODE ";
            return sqlStr;
        }
        /// <summary>
        /// 更新任務PROGRESS
        /// </summary>
        /// <returns></returns>
        public string UpdateTaskIndex()
        {
            string sqlStr = $@"UPDATE {MasterTable} SET ";
            sqlStr += "PROGRESS =@PROGRESS, ";
            sqlStr += "SUBTASK_TYPE =@SUBTASK_TYPE, ";
            sqlStr += "UPDATE_USER ='MCS_agvCallback', UPDATE_TIME=now() ";
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND GUID = @GUID ;";


            return sqlStr;
        }
        /// <summary>
        /// 更新任務PROGRESS與電梯資訊
        /// </summary>
        /// <returns></returns>
        public string UpdateTaskELVTInfo()
        {
            string sqlStr = $@"UPDATE {MasterTable} SET ";
            sqlStr += "PROGRESS =@PROGRESS, ";
            sqlStr += "SUBTASK_TYPE =@SUBTASK_TYPE, ";
            sqlStr += "ELVT_TASK_GUID=@ELVT_TASK_GUID, ";
            sqlStr += "ELVT_ID=@ELVT_ID, ";
            sqlStr += "ELVT_SPACE_ID=@ELVT_SPACE_ID, ";
            sqlStr += "ELVT_SPACE_INDEX=@ELVT_SPACE_INDEX, ";
            sqlStr += "ELVT_SPACE_QRCODE=@ELVT_SPACE_QRCODE, ";
            sqlStr += "ELVT_SPACE_STATUS_TROLLEY_ID=@ELVT_SPACE_STATUS_TROLLEY_ID, ";
            sqlStr += "ELVT_SPACE_STATUS_GUID=@ELVT_SPACE_STATUS_GUID, ";
            sqlStr += "UPDATE_USER ='MCS_elvtCallback', UPDATE_TIME=now() ";
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND GUID = @GUID ;";


            return sqlStr;
        }
        /// <summary>
        /// 回傳SQL指令--新增
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,TASK_GUID,PROGRESS,SUBTASK_TYPE,TASKTYP,POSITIONCODEPATH,PODCODE,PODDIR,MATERIALLOT,PRIORITY,AGVCODE,WEIGHTING,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += @"VALUES (@GUID,@TASK_GUID,@PROGRESS,@SUBTASK_TYPE,@TASKTYP,@POSITIONCODEPATH,@PODCODE,@PODDIR,@MATERIALLOT,@PRIORITY,@AGVCODE,@WEIGHTING,@INSERT_USER,now());";
            sqlStr += "";

            return sqlStr;
        }
        public string InsertTask()
        {
            //插入新任務，目前未指派AGV
            string sqlStr = $"INSERT INTO {MasterTable} (GUID,INSERT_USER,INSERT_TIME,TASK_ID,SUB_INDEX,`INDEX`,SUBTASK_TYPE,POSITIONCODEPATH,TASKTYP,PODCODE," +
                            $"ASE_START_LOC,ASE_START_QRCODE,ASE_TARGET_LOC,ASE_TARGET_QRCODE,ASE_JOB_NAME,ASE_SEQ)";
            sqlStr += "VALUES (@GUID,'MCS_agvCallback',now(),@TASK_ID,@SUB_INDEX,@INDEX,@SUBTASK_TYPE,@POSITIONCODEPATH,@TASKTYP,@PODCODE," +
                            "@ASE_START_LOC,@ASE_START_QRCODE,@ASE_TARGET_LOC,@ASE_TARGET_QRCODE,@ASE_JOB_NAME,@ASE_SEQ);";
            return sqlStr;
        }
        public string InsertRCSAPI()
        {
            //插入新任務，目前未指派AGV
            string sqlStr = $"INSERT INTO {RCSAPITable} (GUID,INSERT_USER,INSERT_TIME,TASK_ID,API_ACTION_NAME,SEND_DATA,`TYPE`,RESPONSE_DATA)";
            sqlStr += "VALUES (uuid(),'MCS_agvCallback',now(),@TASKCODE,@SUBTASK_FUNCTION,@SEND_DATA,@TYPE,@RESPONSE_DATA);";
            return sqlStr;
        }
        public string InsertTaskTravel()
        {
            string sqlStr = string.Format(@"INSERT INTO {0}(GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,
                            TASK_GUID,PROGRESS,SUBTASK_TYPE,POSITIONCODEPATH,WEIGHTING,
                            ASE_START_LOC,ASE_START_QRCODE,ASE_TARGET_LOC,ASE_TARGET_QRCODE,ASE_JOB_NAME,ASE_SEQ,
                            ELVT_TASK_GUID,ELVT_ID,ELVT_SPACE_ID,ELVT_SPACE_INDEX,ELVT_SPACE_QRCODE,ELVT_SPACE_STATUS_TROLLEY_ID,ELVT_SPACE_STATUS_GUID,
                            SUBTASK_STATUS_GUID,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,CTNRTYP,CTNRCODE,WBCODE,
                            PODCODE,PODDIR,MATERIALLOT,PRIORITY,TASKCODE,AGVCODE,DATA,TASKSTATUS,CODE,MESSAGE)", SubTable);
            sqlStr += string.Format(@"SELECT UUID(),INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK,
                            TASK_GUID,PROGRESS,SUBTASK_TYPE,POSITIONCODEPATH,WEIGHTING,
                            ASE_START_LOC,ASE_START_QRCODE,ASE_TARGET_LOC,ASE_TARGET_QRCODE,ASE_JOB_NAME,ASE_SEQ,
                            ELVT_TASK_GUID,ELVT_ID,ELVT_SPACE_ID,ELVT_SPACE_INDEX,ELVT_SPACE_QRCODE,ELVT_SPACE_STATUS_TROLLEY_ID,ELVT_SPACE_STATUS_GUID,
                            GUID,REQCODE,REQTIME,CLIENTCODE,TOKENCODE,TASKTYP,CTNRTYP,CTNRCODE,WBCODE,
                            PODCODE,PODDIR,MATERIALLOT,PRIORITY,TASKCODE,AGVCODE,DATA,TASKSTATUS,CODE,MESSAGE FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";

            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() AS UUID from dual;";

            return sqlStr;
        }
    }
}