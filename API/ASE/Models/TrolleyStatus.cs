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

namespace ASE.Models
{
    /// <summary>
    /// 使用者設定功能頁面相關資料操作:繼承資料存取通用介面
    /// </summary>
    public class TrolleyStatus 
    {
        public string MasterTable { get { return "T_TROLLEY_STATUS"; } }
        public string SubTable { get { return "T_TROLLEY_TRAVEL"; } }
        public string TaskTable { get { return "T_TASK_STATUS"; } }
        public string SubTaskTable { get { return "T_SUBTASK_STATUS"; } }
        public string TrolleyTable { get { return "BASE_TROLLEY"; } }
        public string AGVStorageTable { get { return "BASE_AGVSTORAGE"; } }
        public string EQPTable { get { return "BASE_EQUIPMENT"; } }
        public string StorageTable { get { return "BASE_STORAGE"; } }
        public string WeightingTable { get { return "BASE_WEIGHTING"; } }

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
        public string GetWeighting()
        {
            string sqlStr = $@"SELECT PRIORITY FROM {WeightingTable} ";
            sqlStr += "WHERE ID=@WEIGHTING_ID ;";
            return sqlStr;
        }
        public string GetTrolleyIDAndType()
        {
            string sqlStr = $@"SELECT TT.TROLLEY_ID , BT.TYPE FROM {MasterTable} TT, {TrolleyTable} BT ";
            sqlStr += "WHERE TT.TROLLEY_ID=BT.ID ";
            sqlStr += "AND TT.ENABLE=1 ";
            sqlStr += "AND BT.ENABLE=1 ";
            sqlStr += "AND TT.LOCATION_ID LIKE @LOCATION_ID ; ";
            return sqlStr;
        }
        public string Insert(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,", TaskTable);
            string sqlValue = "VALUES (uuid(),";

            if (!string.IsNullOrEmpty((string)conditions.REQCODE))
            {
                sqlStr += "REQCODE,";
                sqlValue += "@REQCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.REQTIME))
            {
                sqlStr += "REQTIME,";
                sqlValue += "@REQTIME,";
            }
            if (!string.IsNullOrEmpty((string)conditions.CLIENTCODE))
            {
                sqlStr += "CLIENTCODE,";
                sqlValue += "@CLIENTCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.TOKENCODE))
            {
                sqlStr += "TOKENCODE,";
                sqlValue += "@TOKENCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.TASKTYP))
            {
                sqlStr += "TASKTYP,";
                sqlValue += "@TASKTYP,";
            }
            if (!string.IsNullOrEmpty((string)conditions.CTNRTYP))
            {
                sqlStr += "CTNRTYP,";
                sqlValue += "@CTNRTYP,";
            }
            if (!string.IsNullOrEmpty((string)conditions.CTNRCODE))
            {
                sqlStr += "CTNRCODE,";
                sqlValue += "@CTNRCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.WBCODE))
            {
                sqlStr += "WBCODE,";
                sqlValue += "@WBCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.POSITIONCODE1))
            {
                sqlStr += "POSITIONCODE1,";
                sqlValue += "@POSITIONCODE1,";
            }
            if (!string.IsNullOrEmpty((string)conditions.POSITIONCODE2))
            {
                sqlStr += "POSITIONCODE2,";
                sqlValue += "@POSITIONCODE2,";
            }
            if (!string.IsNullOrEmpty((string)conditions.PODCODE))
            {
                sqlStr += "PODCODE,";
                sqlValue += "@PODCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.PODDIR))
            {
                sqlStr += "PODDIR,";
                sqlValue += "@PODDIR,";
            }
            if (!string.IsNullOrEmpty((string)conditions.MATERIALLOT))
            {
                sqlStr += "MATERIALLOT,";
                sqlValue += "@MATERIALLOT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.PRIORITY))
            {
                sqlStr += "PRIORITY,";
                sqlValue += "@PRIORITY,";
            }
            if (!string.IsNullOrEmpty((string)conditions.TASKCODE))
            {
                sqlStr += "TASKCODE,";
                sqlValue += "@TASKCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.AGVCODE))
            {
                sqlStr += "AGVCODE,";
                sqlValue += "@AGVCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.DATA))
            {
                sqlStr += "DATA,";
                sqlValue += "@DATA,";
            }
            if (!string.IsNullOrEmpty((string)conditions.TASKSTATUS))
            {
                sqlStr += "TASKSTATUS,";
                sqlValue += "@TASKSTATUS,";
            }
            if (!string.IsNullOrEmpty((string)conditions.WEIGHTING))
            {
                sqlStr += "WEIGHTING,";
                sqlValue += "@WEIGHTING,";
            }
            sqlStr += "INSERT_USER,INSERT_TIME) ";
            sqlValue += "@INSERT_USER,now());";
            sqlStr += sqlValue;
            //sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP)", HistoryTable);
            //sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP FROM {0} ", MasterTable);
            //sqlStr += $@"WHERE GUID='{uuid}';";
            return sqlStr;
        }
        public string GetOneEmptyTrolly()
        {
            string sqlStr = $"SELECT T1.TROLLEY_ID AS TROLLEY_ID,T1.LOCATION_ID AS LOCATION_ID FROM {MasterTable} T1 ";
            sqlStr += $"WHERE T1.ENABLE = 1 AND T1.TROLLEY_STATUS = '1' ";
            sqlStr += "AND T1.LOCATION_ID NOT IN ('','064371DD029325','064371DD028425','064271DD027525') ";
            //sqlStr += "AND T2.TASKSTATUS NOT IN ('0','1','2','5') ";
            sqlStr += $"AND (SELECT COUNT(1) AS NUM FROM {TaskTable} T2 WHERE T1.TROLLEY_ID = T2.PODCODE AND T2.TASKSTATUS IN ('0','1','2')) = 0 ";
            sqlStr += "AND T1.MATERIALLOT IN (NULL,'') LIMIT 1;";

            return sqlStr;
        }
        public string GetTrollyStatus()
        {
            string sqlStr = $"SELECT T.TROLLEY_ID ,T.LOCATION_ID ,T.MATERIALLOT ,E.NAME AS EQUIPMENT_NAME,S.NAME AS STORAGE_NAME FROM {MasterTable} T ,{AGVStorageTable} A ,{EQPTable} E ,{StorageTable} S ";
            sqlStr += $"WHERE T.ENABLE = 1 AND T.TROLLEY_STATUS = '1' AND T.LOCATION_ID = S.NAME AND A.EQUIPMENT_GUID = E.GUID AND A.STORAGE_GUID = S.GUID ;";
            

            return sqlStr;
        }
        public string GetTrollyID()
        {
            string sqlStr = $"SELECT T.TROLLEY_ID  FROM {MasterTable} T  ";
            sqlStr += $"WHERE T.ENABLE = 1 AND T.LOCATION_ID = @SUBTASK_FUNCTION ;";

            return sqlStr;
        }
        public string CheckStorageTask()
        {
            string sqlStr = string.Format(@"SELECT D.POSITIONCODE1,D.POSITIONCODE2 FROM {0} D ", TaskTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND (TASKSTATUS = 0 OR TASKSTATUS = 1 OR TASKSTATUS = 2) ";
            sqlStr += "AND ((POSITIONCODE1=@LOCATION_ID OR POSITIONCODE2=@LOCATION_ID) ";
            sqlStr += "OR ((POSITIONCODE1=@LOCATION_ID OR POSITIONCODE2=@LOCATION_ID) AND UPDATE_TIME>SUBDATE(now(),interval 2 minute))) ;";
            return sqlStr;
        }
        public string CheckElevatorTask()
        {
            string sqlStr = string.Format(@"SELECT D.POSITIONCODEPATH FROM {0} D ", SubTaskTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND (TASKSTATUS = 0 OR TASKSTATUS = 1 OR TASKSTATUS = 2) ";
            sqlStr += "AND ((POSITIONCODEPATH LIKE CONCAT('%',@LOCATION_ID,'%')) ";
            sqlStr += "OR ((POSITIONCODEPATH LIKE CONCAT('%',@LOCATION_ID,'%') AND UPDATE_TIME>SUBDATE(now(),interval 2 minute)))) ;";
            return sqlStr;
        }
        public string CheckStorageTrolley()
        {
            string sqlStr = string.Format(@"SELECT D.LOCATION_ID FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND LOCATION_ID = @LOCATION_ID ;";
            return sqlStr;
        }
        public string SelectTrolleyID()
        {
            string sqlStr = string.Format(@"SELECT ID FROM {0} ", TrolleyTable); 
            return sqlStr;
        }
        public string SelectGUID()
        {
            string sqlStr = string.Format(@"SELECT D.GUID FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND TROLLEY_ID = @TROLLEY_ID";

            return sqlStr;
        }
        public string InsertTrolleyStatus(string uuid)
        {
            ////20220804 KJ
            /*
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,INSERT_USER,INSERT_TIME,TROLLEY_ID,MATERIALLOT,LOCATION_ID)", MasterTable);
            sqlStr += $"SELECT '{uuid}','QUEUE',now(),@TROLLEY_ID,@MATERIALLOT,@LOCATION_ID FROM DUAL WHERE NOT EXISTS ";
            sqlStr += "(SELECT T.TROLLEY_ID FROM T_TROLLEY_STATUS T WHERE TROLLEY_ID=@TROLLEY_ID LIMIT 1);";
            */
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,INSERT_USER,INSERT_TIME,TROLLEY_ID,MATERIALLOT,LOCATION_ID)", MasterTable);
            sqlStr += $"SELECT '{uuid}','QUEUE',now(),@TROLLEY_ID,@MATERIALLOT,@LOCATION_ID FROM DUAL";
            sqlStr += "WHERE(SELECT COUNT(1) AS tot FROM T_TROLLEY_STATUS T WHERE (T.TROLLEY_ID =@TROLLEY_ID) = 0);";
            ////
            sqlStr += string.Format(@"INSERT INTO {0} (GUID,INSERT_USER,INSERT_TIME,GUID_STATUS,TROLLEY_ID,MATERIALLOT,LOCATION_ID)", SubTable);
            sqlStr += string.Format(@"SELECT uuid(),INSERT_USER,INSERT_TIME,GUID,TROLLEY_ID,MATERIALLOT,LOCATION_ID FROM {0} ", MasterTable);
            sqlStr += $"WHERE GUID='{uuid}' ;";
            return sqlStr;
        }
        public string InsertTrolleyTravel(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,INSERT_USER,INSERT_TIME,GUID_STATUS,TROLLEY_ID,MATERIALLOT,LOCATION_ID)", SubTable);
            sqlStr += string.Format(@"SELECT uuid(),INSERT_USER,INSERT_TIME,GUID,TROLLEY_ID,MATERIALLOT,LOCATION_ID FROM {0} ", MasterTable);
            sqlStr += $"WHERE GUID='{uuid}' ;";
            return sqlStr;
        }
        public string UpdateTrolleyStatus()
        {
            //string sqlStr = "SET SQL_SAFE_UPDATES=0 ; \r\n";
            string sqlStr = string.Format(@"UPDATE {0} SET 
                    MATERIALLOT=@MATERIALLOT, 
                    LOCATION_ID=@LOCATION_ID, 
                    UPDATE_USER=@UPDATE_USER, UPDATE_TIME=now() ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 ";
            sqlStr += "AND TROLLEY_ID = @TROLLEY_ID ;";
            //sqlStr += "SET SQL_SAFE_UPDATES=1 ; ";
            sqlStr += string.Format(@"INSERT INTO {0} (GUID,UPDATE_USER,UPDATE_TIME,GUID_STATUS,TROLLEY_ID,MATERIALLOT,LOCATION_ID)", SubTable);
            sqlStr += string.Format(@"SELECT uuid(),UPDATE_USER,UPDATE_TIME,GUID,TROLLEY_ID,MATERIALLOT,LOCATION_ID FROM {0} ", MasterTable);
            sqlStr += $"WHERE TROLLEY_ID=@TROLLEY_ID;";
            return sqlStr;
        }
        public string InsertTrolleyTravel()
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,UPDATE_USER,UPDATE_TIME,GUID_STATUS,TROLLEY_ID,MATERIALLOT,LOCATION_ID)", SubTable);
            sqlStr += string.Format(@"SELECT uuid(),UPDATE_USER,UPDATE_TIME,GUID,TROLLEY_ID,MATERIALLOT,LOCATION_ID FROM {0} ", MasterTable);
            sqlStr += $"WHERE TROLLEY_ID=@TROLLEY_ID;";
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

        public string GetSqlStr(string actionName, JObject parm)
        {
            throw new NotImplementedException();
        }
    }
}