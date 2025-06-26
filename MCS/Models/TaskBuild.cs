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
    public class TaskBuild : ISqlCreator
    {
        public string MasterTable { get { return "T_TASK_QUEUE"; } }
        public string HistoryTable { get { return "BASE_MAP_HISTORY"; } }
        public string SubTable { get { return "AGV_TASK_POSITION"; } }
        public string DataTable { get { return "BASE_FACTORY"; } }
        public string WeightingTable { get { return "BASE_WEIGHTING"; } }
        public string TrolleyTable { get { return "BASE_TROLLEY"; } }
        public string TTrolleyStatusTable { get { return "T_TROLLEY_STATUS"; } }
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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} TTQ ", MasterTable);
                sqlStr += "WHERE TTQ.SEQ IS NOT NULL  ";
            }
            else
            {
                sqlStr = string.Format(@"SELECT GUID,JOB_NAME,CAR_NO,START_LOC,START_AREA,SEC_START_AREA,TARGET_LOC,TARGET_AREA,SEC_TARGET_AREA,");
                sqlStr += string.Format(@"CASE IS_MIXED_AREA WHEN 0 THEN '否' ELSE '是' END AS IS_MIXED_AREA,");
                sqlStr += string.Format(@"CASE `STATUS` WHEN 0 THEN '未建立' WHEN 1 THEN '已建立' WHEN 2 THEN '不可執行' WHEN 3 THEN '取消' END AS `STATUS`,");
                sqlStr += string.Format(@"MESSAGE,TRY_COUNT,INSERT_TIME,REMARK FROM {0} TTQ ", MasterTable);
                sqlStr += "WHERE TTQ.SEQ IS NOT NULL  ";
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

        public string FindWorkingTaskWithPosition2()
        {
            string sqlStr = string.Format(@"SELECT D.GUID FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND (TASKSTATUS = '0' OR TASKSTATUS = '1' OR TASKSTATUS = '2') AND POSITIONCODE2  = @POSITIONCODE2";

            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual;";

            return sqlStr;
        }
        public string Chuck()
        {
            string sqlStr = string.Format(@"SELECT D.QRCODE FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND QRCODE = @QRCODE";

            return sqlStr;
        }
        public string GetDataList()
        {
            string sqlStr = string.Format(@"SELECT GUID AS 'Key', ID , NAME AS 'Value' FROM {0} D ", DataTable);
            sqlStr += "WHERE D.ENABLE = 1 ";
            return sqlStr;
        }
        /// <summary>
        /// 擴充欄位
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public string Insert(string uuid, JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,", MasterTable);
            string sqlValue = $@"VALUES ('{uuid}',";

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
            sqlStr += "INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME) ";
            sqlValue += "@INSERT_USER,now(),@INSERT_USER,now());";
            sqlStr += sqlValue;
            //sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP)", HistoryTable);
            //sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP FROM {0} ", MasterTable);
            //sqlStr += $@"WHERE GUID='{uuid}';";
            return sqlStr;
        }
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,FLOOR_GUID,ID,NAME,REMARK,INSERT_USER,INSERT_TIME,QRCODE,TYPE,ROW,COL,IMPORT_FILE) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@FLOOR_GUID,@ID,@NAME,@REMARK,@INSERT_USER,now(),@QRCODE,@TYPE,@ROW,@COL,@IMPORT_FILE);";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MAP_GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,QRCODE,TYPE,ROW,COL,IMPORT_FILE)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,@QRCODE,@TYPE,@ROW,@COL,@IMPORT_FILE FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
        //public string Insert(string uuid)
        //{
        //    string sqlStr = string.Format(@"INSERT INTO {0} (GUID,FLOOR_GUID,ID,NAME,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
        //    sqlStr += $@"VALUES ('{uuid}',@FLOOR_GUID,@ID,@NAME,@REMARK,@INSERT_USER,now());";
        //    sqlStr += string.Format(@"INSERT INTO {0}(GUID,MAP_GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
        //    sqlStr += string.Format(@"SELECT UUID(),GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
        //    sqlStr += $@"WHERE GUID='{uuid}';";

        //    return sqlStr;
        //}
        /// <summary>
        /// 回傳SQL指令--新增
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,ID,NAME,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += @"VALUES (UUID(),@ID,@NAME,@REMARK,@INSERT_USER,now());";
            sqlStr += "";

            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--更新
        /// </summary>
        /// <returns></returns>
        public string Update()
        {
            string sqlStr = string.Format(@"UPDATE {0} SET 
                ID = @ID,
                NAME = @NAME,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MAP_GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--刪除
        /// </summary>
        /// <returns></returns>
        public string Delete()
        {
            string sqlStr = string.Format("UPDATE {0} SET " +
                "ENABLE = 0," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                "WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MAP_GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }
        public string GetWeighting()
        {
            string sqlStr = $@"SELECT PRIORITY FROM {WeightingTable} ";
            sqlStr += "WHERE ID=@WEIGHTING_ID ;";
            return sqlStr;
        }
        public string GetLocation()
        {
            string sqlStr = $@"SELECT BT.`TYPE`, TS.LOCATION_ID FROM {TrolleyTable} BT, {TTrolleyStatusTable} TS ";
            sqlStr += " WHERE BT.ID = TS.TROLLEY_ID ";
            sqlStr += " AND BT.ENABLE = 1 ";
            sqlStr += " AND TS.ENABLE = 1 ";
            sqlStr += " AND TS.TROLLEY_ID = @PODCODE ";
            return sqlStr;
        }
        public string GetTrolley()
        {
            string sqlStr = $@"SELECT COUNT(*) `Count` FROM {TTrolleyStatusTable} TS ";
            sqlStr += " WHERE TS.ENABLE = 1 ";
            sqlStr += " AND TS.LOCATION_ID = @POSITIONCODE2 ";
            return sqlStr;
        }
        public string GetLocationing()
        {
            string sqlStr = $@"SELECT COUNT(*) `Count` FROM {MasterTable} D ";
            sqlStr += " WHERE D.ENABLE = 1 ";
            sqlStr += " AND D.POSITIONCODE2 = @POSITIONCODE2 ";
            sqlStr += " AND D.TASKSTATUS in (0,1,2) ";
            sqlStr += " ; ";
            return sqlStr;
        }
        public string SearchTrolley()
        {
            string sqlStr = $@"SELECT COUNT(*) `Count` FROM {TrolleyTable} BT ";
            sqlStr += " WHERE BT.ENABLE = 1  ";
            sqlStr += " AND BT.ID = @PODCODE ";
            return sqlStr;
        }

    }
}