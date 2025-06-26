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
    public class AgvStorage : ISqlCreator
    {
        public string MasterTable { get { return "BASE_AGVSTORAGE"; } }
        public string HistoryTable { get { return "BASE_AGVSTORAGE_HISTORY"; } }
        public string SubTableEquipment { get { return "BASE_EQUIPMENT"; } }
        public string SubTableStorage { get { return "BASE_STORAGE"; } }
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
                    case "EQUIPMENT_GUID":
                    case "EQUIPMENT_NAME":
                    case "STORAGE_GUID":
                    case "STORAGE_NAME":
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
        //先找有無機台NAME在資料庫
        public string SearchEquipment(JObject parm)
        {
            string sqlStr = string.Format(@"SELECT GUID , ID , NAME FROM {1} D WHERE GUID not in (SELECT EQUIPMENT_GUID FROM {0} F WHERE ENABLE = 1 ) AND NAME = @EQUIPMENT_NAME; ", MasterTable, SubTableEquipment);

            return sqlStr;
        }
        //先找有無機台NAME在資料庫
        public string SearchStorage(JObject parm)
        {
            //string sqlStr = string.Format(@"SELECT GUID , ID , NAME FROM {1} D WHERE GUID not in (SELECT STORAGE_GUID FROM {0} F WHERE ENABLE = 1 ) AND NAME = @STORAGE_NAME; ", MasterTable, SubTableStorage);
            string sqlStr = string.Format(@"SELECT GUID , ID , NAME FROM {1} D WHERE ENABLE = 1 AND NAME = @STORAGE_NAME; ", MasterTable, SubTableStorage);

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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D , {1} F , {2} G ", MasterTable, SubTableEquipment, SubTableStorage);
            }
            else
            {

                sqlStr = string.Format(@"SELECT D.GUID,D.EQUIPMENT_GUID,D.STORAGE_GUID,D.REMARK,D.INSERT_TIME,F.NAME AS EQUIPMENT_NAME ,G.NAME AS STORAGE_NAME FROM {0} D , {1} F , {2} G ", MasterTable, SubTableEquipment, SubTableStorage);
                //sqlStr = string.Format(@"SELECT D.GUID,D.EQUIPMENT_GUID,D.EQUIPMENT_NAME,D.STORAGE_GUID,D.STORAGE_NAME,D.REMARK,D.INSERT_TIME FROM {0} D , {1} F , {2} G ", MasterTable, SubTableEquipment, SubTableStorage);

            }

            sqlStr += "WHERE D.ENABLE = 1 AND F.ENABLE = 1 AND G.ENABLE =1 AND D.EQUIPMENT_GUID=F.GUID AND D.STORAGE_GUID=G.GUID ";

            //機台GUID、NAME
            if (!string.IsNullOrEmpty((string)conditions.EQUIPMENT_GUID))
            {
                sqlStr += "AND F.GUID LIKE CONCAT(@EQUIPMENT_GUID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.EQUIPMENT_NAME))
            {
                sqlStr += "AND F.NAME LIKE CONCAT(@EQUIPMENT_NAME, '%') ";
            }

            //儲位GUID、NAME
            if (!string.IsNullOrEmpty((string)conditions.STORAGE_GUID))
            {
                sqlStr += "AND G.GUID LIKE CONCAT(@STORAGE_GUID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.STORAGE_NAME))
            {
                sqlStr += "AND G.NAME LIKE CONCAT(@STORAGE_NAME, '%') ";
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
        //找機台總數量
        public string SearchEquipmentAllCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", SubTableEquipment);
            return sqlStr;
        }
        //找機台未配對的數量
        public string SearchEquipmentLimitCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {1} WHERE GUID not in (SELECT EQUIPMENT_GUID FROM {0} WHERE ENABLE = 1) ", MasterTable, SubTableEquipment);
            return sqlStr;
        }
        //找儲位總數量
        public string SearchStorageAllCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", SubTableStorage);
            
            return sqlStr;
        }
        //找儲位未配對的數量
        public string SearchStorageLimitCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {1} WHERE GUID not in (SELECT STORAGE_GUID FROM {0} WHERE ENABLE = 1) ", MasterTable, SubTableStorage);
            return sqlStr;
        }
        /// <summary>
        /// 取得唯一值
        /// </summary>
        /// <returns></returns>
        public string GetOneByGUID()
        {
            string sqlStr = string.Format(@"SELECT D.GUID,D.EQUIPMENT_GUID,F.NAME AS EQUIPMENT_NAME,D.STORAGE_GUID,G.NAME AS STORAGE_NAME,D.REMARK FROM {0} D , {1} F , {2} G ", MasterTable, SubTableEquipment, SubTableStorage);
            //string sqlStr = string.Format(@"SELECT D.GUID,D.EQUIPMENT_GUID,D.STORAGE_GUID,D.REMARK FROM {0} D ", MasterTable);
            sqlStr += "WHERE D.ENABLE = 1 AND D.GUID = @GUID AND F.GUID = @EQUIPMENT_GUID AND G.GUID = @STORAGE_GUID ";
            sqlStr += "GROUP BY D.GUID ";
            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual";

            return sqlStr;
        }
        //查詢的equipment選項
        public string GetDataList(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT GUID , ID , NAME FROM {0} D ", SubTableEquipment);
            sqlStr += "WHERE D.ENABLE = 1 ";

            //機台ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.ID LIKE CONCAT(@ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME, '%') ";
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
        //沒有被選取的equipment
        public string GetDataListLimit(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT GUID , ID , NAME FROM {1} D WHERE ENABLE = 1 AND GUID not in (SELECT EQUIPMENT_GUID FROM {0} F WHERE ENABLE = 1) ", MasterTable, SubTableEquipment);

            //機台ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.ID LIKE CONCAT(@ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME, '%') ";
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
        //查詢的storage選項
        public string GetDataListStorage(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT GUID , ID , NAME FROM {0} D ", SubTableStorage);
            sqlStr += "WHERE D.ENABLE = 1 ";

            //儲位ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.ID LIKE CONCAT(@ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME, '%') ";
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
        //沒有被選取的storage
        public string GetDataListStorageLimit(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT GUID , ID , NAME FROM {1} D WHERE GUID not in (SELECT STORAGE_GUID FROM {0} F WHERE ENABLE = 1) AND ENABLE = 1 ", MasterTable, SubTableStorage);

            //儲位ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.ID LIKE CONCAT(@ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME, '%') ";
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

        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@" INSERT INTO {0} (GUID,EQUIPMENT_GUID,STORAGE_GUID,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += $@" VALUES ('{uuid}',@EQUIPMENT_GUID,@STORAGE_GUID,@REMARK,@INSERT_USER,now()); ";
            sqlStr += string.Format(@" INSERT INTO {0}(GUID,AGVSTORAGE_GUID,EQUIPMENT_GUID,STORAGE_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK) ", HistoryTable);
            sqlStr += string.Format(@" SELECT UUID(), GUID,EQUIPMENT_GUID,STORAGE_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@" WHERE GUID='{uuid}'; ";

            return sqlStr;
        }
        /// <summary>
        /// 回傳SQL指令--新增
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,EQUIPMENT_GUID,STORAGE_GUID,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += @"VALUES (UUID(),@EQUIPMENT_GUID,@STORAGE_GUID,@REMARK,@INSERT_USER,now());";
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
                STORAGE_GUID = @STORAGE_GUID,
                EQUIPMENT_GUID = @EQUIPMENT_GUID,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0} (GUID,AGVSTORAGE_GUID,EQUIPMENT_GUID,STORAGE_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK) ", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,EQUIPMENT_GUID,STORAGE_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,AGVSTORAGE_GUID,EQUIPMENT_GUID,STORAGE_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,EQUIPMENT_GUID,STORAGE_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

    }
}