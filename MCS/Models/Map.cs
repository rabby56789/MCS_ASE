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
    public class Map : ISqlCreator
    {
        public string MasterTable { get { return "BASE_MAP"; } }
        public string HistoryTable { get { return "BASE_MAP_HISTORY"; } }
        public string SubTable { get { return "BASE_FLOOR"; } }
        public string DataTable { get { return "BASE_FACTORY"; } }
        public string NodeTable { get { return "BASE_NODE"; } }
        public string NodeHistoryTable { get { return "BASE_NODE_HISTORY"; } }
        public string EdgeTable { get { return "BASE_EDGE"; } }
        public string EdgeHistoryTable { get { return "BASE_EDGE_HISTORY"; } }
        public string StorageTable { get { return "BASE_STORAGE"; } }
        public string StorageHistoryTable { get { return "BASE_STORAGE_HISTORY"; } }
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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ,{1} F ,{2} A ", MasterTable, SubTable ,DataTable);
            }
            else
            {
                sqlStr = string.Format(@"SELECT D.*,F.NAME AS FLOOR_NAME,A.NAME AS FACTORY_NAME FROM {0} D ,{1} F ,{2} A ", MasterTable,SubTable, DataTable);
                //sqlStr = string.Format("SELECT A.GUID AS GUIDA , B.GUID AS GUIDB , A.NAME AS NAMEA,B.ID AS IDB,B.NAME AS NAMEB ,B.REMARK AS REMARK FROM {0} B ,{1} A ", SubTable, MasterTable);
                //sqlStr += "WHERE B.ENABLE = 1 AND A.GUID=B.FACTORY_GUID AND B.FACTORY_GUID = @GUIDA";
            }

            sqlStr += "WHERE D.ENABLE = 1 AND D.FLOOR_GUID=F.GUID AND F.FACTORY_GUID=A.GUID ";

            //廠GUID
            if (!string.IsNullOrEmpty((string)conditions.FACTORY_GUID))
            {
                sqlStr += "AND F.FACTORY_GUID LIKE CONCAT(@FACTORY_GUID, '%') ";
            }
            //樓層GUID
            if (!string.IsNullOrEmpty((string)conditions.FLOOR_GUID))
            {
                sqlStr += "AND D.FLOOR_GUID LIKE CONCAT(@FLOOR_GUID, '%') ";
            }
            //代碼
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.ID LIKE CONCAT(@ID, '%') ";
            }
            //名稱
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME , '%') ";
            }
            //建立時間-起
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_START))
            {
                sqlStr += "AND D.INSERT_TIME >= @INSERT_TIME_START ";
            }
            //建立時間-終
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_END))
            {
                sqlStr += "AND D.INSERT_TIME <= @INSERT_TIME_END ";
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
        public string Check()
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
        public string Insert(string uuid, JObject parm)
        {
            dynamic conditions = parm as dynamic;
            
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,FLOOR_GUID,ID,NAME,REMARK, ", MasterTable);
            string sqlValue = $@"VALUES ('{uuid}',@FLOOR_GUID,@ID,@NAME,@REMARK,";
            if (!string.IsNullOrEmpty((string)conditions.QRCODE))
            {
                sqlStr += "QRCODE,";
                sqlValue += "@QRCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.TYPE))
            {
                sqlStr += "TYPE,";
                sqlValue += "@TYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROW))
            {
                sqlStr += "`ROW`,";
                sqlValue += "@ROW,";
            }
            if (!string.IsNullOrEmpty((string)conditions.COL))
            {
                sqlStr += "COL,";
                sqlValue += "@COL,";
            }
            if (!string.IsNullOrEmpty((string)conditions.IMPORT_FILE))
            {
                sqlStr += "IMPORT_FILE,";
                sqlValue += "@IMPORT_FILE,";
            }
            sqlStr += "INSERT_USER,INSERT_TIME) ";
            sqlValue += "@INSERT_USER,now());";
            sqlStr += sqlValue;
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MAP_GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,QRCODE,TYPE,`ROW`,COL,IMPORT_FILE)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,QRCODE,TYPE,`ROW`,COL,IMPORT_FILE FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
        /// <summary>
        /// 擴充欄位
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,FLOOR_GUID,ID,NAME,REMARK,INSERT_USER,INSERT_TIME,QRCODE,TYPE,`ROW`,COL,IMPORT_FILE) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@FLOOR_GUID,@ID,@NAME,@REMARK,@INSERT_USER,now(),@QRCODE,@TYPE,@ROW,@COL,@IMPORT_FILE);";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MAP_GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,QRCODE,TYPE,`ROW`,COL,IMPORT_FILE)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,QRCODE,TYPE,`ROW`,COL,IMPORT_FILE FROM {0} ", MasterTable);
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
            sqlStr +="";

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
        public string GetRepeatGUID()
        {
            string sqlStr = string.Format(@"SELECT D.GUID,D.ID,D.NAME,D.REMARK FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND QRCODE = @QRCODE";

            return sqlStr;
        }
        public string DeleteRepeat(string uuid)
        {
            //map
            string sqlStr = string.Format("UPDATE {0} SET " +
                "ENABLE = 0," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                $"WHERE GUID = '{uuid}' AND ENABLE = 1 ;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MAP_GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";
            //node
            sqlStr += "SET SQL_SAFE_UPDATES=0;";
            sqlStr += string.Format("UPDATE {0} SET " +
                "ENABLE = 0," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                $"WHERE MAP_GUID = '{uuid}';", NodeTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP)", NodeHistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP FROM {0} ", NodeTable);
            sqlStr += $@"WHERE MAP_GUID='{uuid}';";
            //edge
            sqlStr += string.Format("UPDATE {0} SET " +
                "ENABLE = 0," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                $"WHERE MAP_GUID = '{uuid}';", EdgeTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,EDGE_GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT)", EdgeHistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT FROM {0} ", EdgeTable);
            sqlStr += $@"WHERE MAP_GUID='{uuid}';";
            //storaage
            sqlStr += string.Format("UPDATE {0} SET " +
                "ENABLE = 0," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                $"WHERE MAP_GUID = '{uuid}';", StorageTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,STORAGE_GUID,MAP_GUID,QRCODE,ID,NAME,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", StorageHistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,QRCODE,ID,NAME,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", StorageTable);
            sqlStr += $@"WHERE MAP_GUID='{uuid}';";
            sqlStr += "SET SQL_SAFE_UPDATES=1;";
            return sqlStr;
        }
        //匯入儲位
        public string InsertStorage()
        {
            string sqlStr = $"INSERT INTO {StorageTable} (GUID,ID,NAME,QRCODE,INSERT_USER,INSERT_TIME,MAP_GUID) ";
            sqlStr += $@"SELECT uuid(),N.QRCODE,N.QRCODE,N.QRCODE,INSERT_USER,now(),@MAP_GUID FROM {NodeTable} N WHERE ENABLE=1 AND (VALUE='1' OR VALUE='10');";
            

            return sqlStr;
        }
    }
}