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
    public class Area : ISqlCreator
    {
        /// <summary>
        /// 存取子:主表名稱
        /// </summary>
        public string MasterTable { get { return "BASE_AREA"; } }
        public string HistoryTable { get { return "BASE_AREA_HISTORY"; } }
        
        public string StorageTable { get { return "BASE_STORAGE"; } }
        public string StorageHistoryTable { get { return "BASE_STORAGE_HISTORY"; } }

        /// <summary>
        /// 丟JSON物件,回傳SQL具名參數陣列
        /// </summary>
        /// <param name="input"></param>
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

        /// <summary>
        /// 取得SQL字串通用介面
        /// </summary>
        /// <param name="actionName">動作名稱</param>
        /// <param name="parm">傳入參數(選擇性)</param>
        /// <returns></returns>
        public string GetSqlStr(string actionName, JObject parm)
        {
            string sqlStr = string.Empty;

            switch (actionName)
            {
                case "Count":
                    sqlStr = Search(parm, true);
                    break;
                case "CountR":
                    sqlStr = SearchR(parm, true);
                    break;
                case "CountB":
                    sqlStr = SearchB(parm, true);
                    break;
                case "Query":
                    sqlStr = Search(parm, false);
                    break;
                case "QueryR":
                    sqlStr = SearchR(parm, false);
                    break;
                case "QueryB":
                    sqlStr = SearchB(parm, false);
                    break;
                case "GetOneByGUID":
                    sqlStr = GetOneByGUID(parm);
                    break;
                case "EditA":
                    sqlStr = Update(parm, true);
                    break;
                case "Delete":
                    sqlStr = Delete(parm, true);
                    break;
                case "DeleteBind":
                    sqlStr = Delete(parm, false);
                    break;
                case "InsertBind":
                    sqlStr = InsertBind(parm);
                    break;
                default:
                    break;


            }

            return sqlStr;
        }

        /// <summary>
        /// 取得查詢SQL字串
        /// </summary>
        /// <param name="parm">Client端傳入參數</param>
        /// <param name="getCount">是否為查詢數量</param>
        /// <returns></returns>
        public string Search(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;


            //是否為查詢資料筆數
            if (getCount)
            {
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ", MasterTable);
                
            }
            else
            {
                //sqlStr = $"SELECT GUID, GROUP_NAME, ID, REMARK FROM {MasterTable} where enable = '1'";
                sqlStr = string.Format(@"SELECT D.GUID,D.ID,D.NAME,D.REMARK FROM {0} D ", MasterTable);
            }

            sqlStr += "WHERE D.ENABLE = 1 ";

            //SQL加入查詢條件
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.ID LIKE CONCAT(@ID, '%') ";
            }
            //名稱
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT('%', @NAME , '%') ";
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
        //找區域
        public string SearchR(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;
            if (getCount)
            {
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ", StorageTable);
                sqlStr += "WHERE GROUP_GUID = @GROUP_GUID ";
                sqlStr += "AND enable = '1' ";
            }
            else
            {
                sqlStr = string.Format(@"SELECT GUID, ID, NAME, GROUP_GUID FROM {0} ", StorageTable);
                sqlStr += "WHERE GROUP_GUID = @GROUP_GUID ";
                sqlStr += "AND enable = '1' ";
                
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
        //找未加入區域的儲位
        public string SearchB(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;
            if (getCount)
            {
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ", StorageTable);
                sqlStr += "WHERE (GROUP_GUID is null OR GROUP_GUID = '') AND enable = '1' ";
                return sqlStr;
            }
            else
            {
                sqlStr = string.Format(@"SELECT GUID, ID, NAME, GROUP_GUID FROM {0} D ", StorageTable);
                sqlStr += "WHERE (GROUP_GUID is null OR GROUP_GUID = '') ";
                sqlStr += "AND enable = '1' ";
            }

            //儲位ID、NAME字串條件搜尋
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.ID LIKE CONCAT(@ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME, '%') ";
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
        
        //判斷更動的資料是否都一樣(updateA用)
        public string SearchArea(JObject parm)
        {
            string sqlStr = string.Format(@"SELECT ID, NAME FROM {0} D WHERE ENABLE = 1 AND ID = @ID AND NAME = @NAME; ", MasterTable);
            return sqlStr;

        }
        //找輸入的區域ID有沒有在資料庫
        public string SearchAreaID(JObject parm)
        {
            string sqlStr = string.Format(@"SELECT ID, NAME FROM {0} D WHERE ENABLE = 1 AND ID = @ID ; ", MasterTable);
            return sqlStr; 
        }
        //找輸入的區域Name有沒有在資料庫
        public string SearchAreaNAME(JObject parm)
        {
            string sqlStr = string.Format(@"SELECT ID, NAME FROM {0} D WHERE ENABLE = 1 AND NAME = @NAME; ", MasterTable);
            return sqlStr;
        }

        //GetGUID
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual";
            return sqlStr;
        }

        //
        //public string Insert(JObject parm, bool getCount)
        //{
        //    dynamic conditions = parm as dynamic;
        //    string sqlStr;
        //    //是否為
        //    if (getCount)
        //    {

        //    }
        //    sqlStr = ;
        //    return sqlStr;
        //}

        //insert area & area_history table
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@" INSERT INTO {0} (GUID , ID , NAME , REMARK , INSERT_USER , INSERT_TIME ) ", MasterTable);
            sqlStr += $@" VALUES ('{uuid}',@ID,@NAME,@REMARK,@INSERT_USER,now()); ";
            sqlStr += string.Format(@" INSERT INTO {0}(GUID , AREA_GUID , ID , NAME , INSERT_USER , INSERT_TIME , UPDATE_USER , UPDATE_TIME , `ENABLE` , REMARK) ", HistoryTable);
            sqlStr += string.Format(@" SELECT UUID(), GUID , ID , NAME , INSERT_USER , INSERT_TIME , UPDATE_USER , UPDATE_TIME , `ENABLE` , REMARK FROM {0} ", MasterTable);
            sqlStr += $@" WHERE GUID='{uuid}'; ";

            return sqlStr;
        }
        public string InsertBind(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"UPDATE {0} SET 
                GROUP_GUID = @GuidA,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @BindGuid;", StorageTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,STORAGE_GUID,ID,`NAME`,QRCODE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,GROUP_GUID) ", StorageHistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,ID,`NAME`,QRCODE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,GROUP_GUID FROM {0} ", StorageTable);
            sqlStr += $@"WHERE GUID=@BindGuid;";
            return sqlStr;

        }
        //用 GUID 找'區域'的資料
        public string GetOneByGUID(JObject parm)
        {
            string sqlStr = string.Format(@"SELECT D.GUID,D.ID,D.NAME,D.REMARK FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND GUID = @GUID";
            return sqlStr;
        }
        //編輯 true->區域 false->儲位
        public string Update(JObject parm, bool area)
        {
            string sqlStr;
            if (area)
            {
                sqlStr = string.Format(@"UPDATE {0} SET 
                NAME = @NAME,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID;", MasterTable);
                sqlStr += string.Format(@"INSERT INTO {0}(GUID,AREA_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
                sqlStr += string.Format(@"SELECT UUID(), GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
                sqlStr += $@"WHERE GUID=@GUID;";
                return sqlStr;
            }
            // area = false -> 編輯儲位
            //sqlStr = string.Format(@"UPDATE {0} SET 
            //    ID = @ID,
            //    NAME = @NAME,
            //    REMARK = @REMARK,
            //    UPDATE_USER = @UPDATE_USER,
            //    UPDATE_TIME = now()
            //    WHERE GUID = @GUID;", MasterTable);
            //sqlStr += string.Format(@"INSERT INTO {0}(GUID,AREA_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            //sqlStr += string.Format(@"SELECT UUID(), GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            //sqlStr += $@"WHERE GUID=@GUID;";

            sqlStr = string.Format(@"");
            return sqlStr;
        }
        //刪除 true->區域 false->儲位
        public string Delete(JObject parm, bool area)
        {
            string sqlStr = string.Empty;
            if (area)
            {
                //刪除區域儲位
                sqlStr = string.Format("UPDATE {0} SET " +
                "ENABLE = 0," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                "WHERE GUID = @GUID;", MasterTable);
                sqlStr += string.Format(@"INSERT INTO {0}(GUID,AREA_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
                sqlStr += string.Format(@"SELECT UUID(), GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
                sqlStr += $@"WHERE GUID=@GUID;";
                //區域刪除連動更新儲位GROUP_GUID=NULL
                sqlStr += string.Format("UPDATE {0} SET " +
                "GROUP_GUID = NULL," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                "WHERE GROUP_GUID = @GUID;", StorageTable);
                sqlStr += string.Format(@"INSERT INTO {0}(GUID,STORAGE_GUID,ID,`NAME`,QRCODE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", StorageHistoryTable);
                sqlStr += string.Format(@"SELECT UUID(), GUID,ID,`NAME`,QRCODE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", StorageTable);
                sqlStr += $@"WHERE GUID=@GUID;";
                return sqlStr;
            }
            else
            {
                //更新儲位GROUP_GUID=NULL
                sqlStr += string.Format("UPDATE {0} SET " +
                "GROUP_GUID = NULL," +
                "UPDATE_USER = @UPDATE_USER," +
                "UPDATE_TIME = now() " +
                "WHERE GUID = @GUID;", StorageTable);
                sqlStr += string.Format(@"INSERT INTO {0}(GUID,STORAGE_GUID,ID,`NAME`,QRCODE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", StorageHistoryTable);
                sqlStr += string.Format(@"SELECT UUID(), GUID,ID,`NAME`,QRCODE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", StorageTable);
                sqlStr += $@"WHERE GUID=@GUID;";
                return sqlStr;
            }

        }
        

    }
}