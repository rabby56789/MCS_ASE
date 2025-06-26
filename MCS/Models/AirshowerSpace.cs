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
    public class AirshowerSpace : ISqlCreator
    {
        public string MasterTable { get { return "BASE_AIRSHOWER_SPACE"; } }
        public string HistoryTable { get { return "BASE_AIRSHOWER_SPACE_HISTORY"; } }
        public string AirshowerTable { get { return "BASE_AIRSHOWER"; } }

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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D left join {1} AIRSHOWER on D.AIRSHOWER_GUID = AIRSHOWER.GUID ", MasterTable, AirshowerTable);
            }
            else
            {
                sqlStr = string.Format(@"SELECT D.GUID,AIRSHOWER.GUID,AIRSHOWER.AIRSHOWER_ID,AIRSHOWER.AIRSHOWER_NAME,D.SPACE_NAME,D.PLACE_ORDER,SPACE_GROUP,D.REMARK,D.INSERT_TIME FROM {0} D 
                                            left join {1} AIRSHOWER on D.AIRSHOWER_GUID = AIRSHOWER.GUID ", MasterTable, AirshowerTable);
            }

            sqlStr += "WHERE D.ENABLE = 1 ";

            //代碼
            if (!string.IsNullOrEmpty((string)conditions.AIRSHOWER_GUID))
            {
                sqlStr += "AND AIRSHOWER.GUID LIKE CONCAT(@AIRSHOWER_GUID , '%') ";
            }
            //名稱
            if (!string.IsNullOrEmpty((string)conditions.SPACE_NAME))
            {
                sqlStr += "AND D.SPACE_NAME LIKE CONCAT(@SPACE_NAME , '%') ";
            }
            //廠
            if (!string.IsNullOrEmpty((string)conditions.PLACE_ORDER))
            {
                sqlStr += "AND D.PLACE_ORDER LIKE CONCAT(@PLACE_ORDER , '%') ";
            }
            //棟
            if (!string.IsNullOrEmpty((string)conditions.SPACE_GROUP))
            {
                sqlStr += "AND D.SPACE_GROUP LIKE CONCAT(@SPACE_GROUP , '%') ";
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
            string sqlStr = string.Format(@"SELECT D.GUID,D.AIRSHOWER_GUID,AIRSHOWER.AIRSHOWER_NAME,D.SPACE_NAME,PLACE_ORDER,SPACE_GROUP,D.REMARK FROM {0} D 
                                                left join {1} AIRSHOWER on D.AIRSHOWER_GUID = AIRSHOWER.GUID ", MasterTable, AirshowerTable);
            sqlStr += "WHERE D.ENABLE = 1 AND D.GUID = @GUID";

            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual"; 

            return sqlStr;
        }
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,AIRSHOWER_GUID,SPACE_NAME,PLACE_ORDER,SPACE_GROUP,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@AIRSHOWER_GUID,@SPACE_NAME,@PLACE_ORDER,@SPACE_GROUP,@REMARK,@INSERT_USER,now());";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,AIRSHOWER_SPACE_GUID,AIRSHOWER_GUID,SPACE_NAME,PLACE_ORDER,SPACE_GROUP,REMARK,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,AIRSHOWER_GUID,SPACE_NAME,PLACE_ORDER,SPACE_GROUP,REMARK,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE` FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
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
                AIRSHOWER_GUID = @AIRSHOWER_GUID,
                SPACE_NAME = @SPACE_NAME,
                PLACE_ORDER = @PLACE_ORDER,
                SPACE_GROUP = @SPACE_GROUP,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,AIRSHOWER_SPACE_GUID,AIRSHOWER_GUID,SPACE_NAME,PLACE_ORDER,SPACE_GROUP,REMARK,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,AIRSHOWER_GUID,SPACE_NAME,PLACE_ORDER,SPACE_GROUP,REMARK,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE` FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,AIRSHOWER_SPACE_GUID,AIRSHOWER_GUID,SPACE_NAME,PLACE_ORDER,SPACE_GROUP,REMARK,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,AIRSHOWER_GUID,SPACE_NAME,PLACE_ORDER,SPACE_GROUP,REMARK,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE` FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

        //查詢風淋門
        public string GetAirshowerList(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT * FROM {0} D ", AirshowerTable);
            sqlStr += "WHERE D.ENABLE = 1 ";

            //ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.AIRSHOWER_ID LIKE CONCAT(@ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.AIRSHOWER_NAME LIKE CONCAT(@NAME, '%') ";
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

        public string SearchAirshowerCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", AirshowerTable);
            return sqlStr;
        }

        public string SearchAirshower(JObject parm)
        {
            string sqlStr = string.Format(@"SELECT * FROM {0} D WHERE ENABLE = 1 AND AIRSHOWER_NAME = @AIRSHOWER_NAME; ", AirshowerTable);

            return sqlStr;
        }
    }
}