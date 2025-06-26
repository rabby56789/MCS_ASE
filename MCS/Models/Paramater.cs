using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Data;

namespace MCS.Models
{
    /// <summary>
    /// 使用者設定功能頁面相關資料操作:繼承資料存取通用介面
    /// </summary>
    public class Paramater : ISqlCreator
    {
        public string MasterTable { get { return "SYS_PARAM"; } }
        public string HistoryTable { get { return "SYS_PARAM_HISTORY"; } }

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
                //custom code
                case "SearchFunctionID":
                    sqlStr = SearchFunctionID(parm);
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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ", MasterTable);
            }
            else
            {
                sqlStr = string.Format(@"SELECT D.GUID,D.FUNCTION,D.FILTER_KEY,D.TEXT,D.VALUE,D.TYPE,D.REMARK FROM {0} D ", MasterTable);
            }

            sqlStr += "WHERE D.ENABLE = 1 ";

            //功能代碼
            if (!string.IsNullOrEmpty((string)conditions.FUNCTION))
            {
                sqlStr += "AND D.FUNCTION LIKE CONCAT(@FUNCTION , '%') ";
            }
            //篩選條件
            if (!string.IsNullOrEmpty((string)conditions.FILTER_KEY))
            {
                sqlStr += "AND D.FILTER_KEY LIKE CONCAT(@FILTER_KEY , '%') ";
            }


            //查詢數量不需換頁
            if (getCount)
            {
                return sqlStr;
            }

            if (parm.TryGetValue("sort", out _))
            {
                sqlStr += string.Format("ORDER BY {0} {1} ", TransferReservedWord((string)conditions.sort), (string)conditions.order);
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
        /// 轉換保留字 增加 `[string]`
        /// </summary>
        /// <param name="v">欄位值</param>
        /// <returns></returns>
        private string TransferReservedWord(string v)
        {
            string result = v;
            switch(v.Trim().ToUpper())
            {
                case "FUNCTION":
                    result = "`" + v + "`";
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// 取得唯一值
        /// </summary>
        /// <returns></returns>
        public string GetOneByGUID()
        {
            string sqlStr = string.Format(@"SELECT D.GUID,D.FUNCTION,D.FILTER_KEY,D.TEXT,D.VALUE,D.TYPE,D.REMARK FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND GUID = @GUID";

            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual";

            return sqlStr;
        }
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,`FUNCTION`,FILTER_KEY,TEXT,VALUE,TYPE,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@FUNCTION,@FILTER_KEY,@TEXT,@VALUE,@TYPE,@REMARK,@INSERT_USER,now());";
            
            sqlStr += string.Format(@"INSERT INTO {0}(`GUID`,`PARAM_GUID`,`FUNCTION`,`FILTER_KEY`,`TEXT`,`VALUE`,`TYPE`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), `GUID`,`FUNCTION`,`FILTER_KEY`,`TEXT`,`VALUE`,`TYPE`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
        /// <summary>
        /// 回傳SQL指令--新增
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,FUNCTION,FILTER_KEY,TEXT,VALUE,TYPE,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += $@"VALUES (UUID(),@FUNCTION,@FILTER_KEY,@TEXT,@VALUE,@TYPE,@REMARK,@INSERT_USER,now());";
            sqlStr += "";

            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--更新
        /// </summary>
        /// <returns></returns>
        public string Update()
        {
            string sqlStr = string.Format(@"UPDATE {0} SET " +
                "`FUNCTION` = @FUNCTION," +
                "FILTER_KEY = @FILTER_KEY, " +
                "TEXT = @TEXT, " +
                "VALUE = @VALUE, " +
                "TYPE = @TYPE, " +
                "REMARK = @REMARK, " +
                "UPDATE_USER = @UPDATE_USER, " +
                "UPDATE_TIME = now()" +
                "WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@" INSERT INTO {0} (`GUID`,`PARAM_GUID`,`FUNCTION`,`FILTER_KEY`,`TEXT`,`VALUE`,`TYPE`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`) ", HistoryTable);
            sqlStr += string.Format(@" SELECT UUID(), `GUID`,`FUNCTION`,`FILTER_KEY`,`TEXT`,`VALUE`,`TYPE`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@" INSERT INTO {0} (`GUID`,`PARAM_GUID`,`FUNCTION`,`FILTER_KEY`,`TEXT`,`VALUE`,`TYPE`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`) ", HistoryTable);
            sqlStr += string.Format(@" SELECT UUID(), `GUID`,`FUNCTION`,`FILTER_KEY`,`TEXT`,`VALUE`,`TYPE`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

        public string SearchFunctionID(JObject parm)
        {
            dynamic conditions = parm as dynamic;

            StringBuilder sb = new StringBuilder();
            sb.Append(" SELECT function_id FROM( ");
            sb.Append(" 	SELECT 'GLOBAL' AS 'function_id'  ");
            sb.Append(" 	union  ");
            sb.Append(" 	(  ");
            sb.Append(" 		SELECT   ");
            sb.Append(" 			function_id  ");
            sb.Append(" 		FROM   ");
            sb.Append(" 			sys_function D ");
            sb.Append(" 		WHERE function_id <> '#'  ");
            sb.Append("         AND D.ENABLE = 1 ");
            sb.Append(" 	) ");
            sb.Append(" ) D ");
            sb.Append(" WHERE 1=1 ");
            //功能代碼
            if (!string.IsNullOrEmpty((string)conditions.FUNCTION_ID))
            {
                sb.Append(" AND D.function_id LIKE CONCAT(@FUNCTION_ID , '%') ");
            }

            sb.Append(" ; ");

            return sb.ToString();
        }

    }
}