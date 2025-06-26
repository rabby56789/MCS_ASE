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
    public class MixedArea : ISqlCreator
    {
        public string MasterTable { get { return "BASE_AREA"; } }
        public string MixedTable { get { return "BASE_MIXED_AREA"; } }

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
                    case "QRCODE":
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
                    //sqlStr = Insert();
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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} BA ", MasterTable);
            }
            else
            {
                sqlStr = string.Format(@"SELECT BA.GUID,BA.ID,BA.NAME,BA.REMARK,BXA.FOR_START_AREA,BXA.FOR_TARGET_AREA FROM {0} BA ", MasterTable);
            }

            sqlStr += $"LEFT JOIN {MixedTable} BXA ON BXA.ORIGINAL_AREA = BA.ID AND BXA.ENABLE = 1 ";
            sqlStr += "WHERE BA.ENABLE = 1 ";

            //代碼
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND BA.ID LIKE CONCAT(@ID, '%') ";
            }
            //名稱
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND BA.NAME LIKE CONCAT(@NAME , '%') ";
            }

            //查詢數量不需換頁
            if (getCount)
            {
                return sqlStr;
            }

            if (parm.TryGetValue("sort", out _))
            {
                sqlStr += string.Format("ORDER BY BA.{0} {1} ", (string)conditions.sort, (string)conditions.order);
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
            string sqlStr = $@"SELECT BA.ID,BA.NAME,BXA.FOR_START_AREA,BXA.FOR_TARGET_AREA 
            FROM BASE_AREA BA
            LEFT JOIN BASE_MIXED_AREA BXA ON BA.ID = BXA.ORIGINAL_AREA AND BXA.ENABLE = 1
            WHERE BA.ENABLE = 1 AND BA.GUID = @GUID";

            return sqlStr;
        }
        //public string GetUUID()
        //{
        //    string sqlStr = "SELECT UUID() from dual";

        //    return sqlStr;
        //}
        //public string Insert(string uuid)
        //{
        //    string sqlStr = string.Format(@"INSERT INTO {0} (GUID,ID,NAME,QRCODE,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
        //    sqlStr += $@"VALUES ('{uuid}',@ID,@NAME,@QRCODE,@REMARK,@INSERT_USER,now());";
        //    sqlStr += string.Format(@"INSERT INTO {0}(GUID,STORAGE_GUID,ID,`NAME`,QRCODE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
        //    sqlStr += string.Format(@"SELECT UUID(), GUID,ID,`NAME`,QRCODE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
        //    sqlStr += $@"WHERE GUID='{uuid}';";

        //    return sqlStr;
        //}
        ///// <summary>
        ///// 回傳SQL指令--新增
        ///// </summary>
        ///// <returns></returns>
        public string Insert()
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (ORIGINAL_AREA,FOR_START_AREA,FOR_TARGET_AREA,GUID,ENABLE,INSERT_USER,INSERT_TIME) ", MixedTable);
            sqlStr += @"VALUES (@ID,@FOR_START_AREA,@FOR_TARGET_AREA,uuid(),'1','MCS_WEB',NOW());";
            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--更新
        /// </summary>
        /// <returns></returns>
        public string Update()
        {
            string sqlStr = string.Format(@"UPDATE {0} SET 
                FOR_START_AREA = @FOR_START_AREA,
                FOR_TARGET_AREA = @FOR_TARGET_AREA,
                UPDATE_TIME = now()
                WHERE ORIGINAL_AREA = @ID;", MixedTable);           
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
                "WHERE ORIGINAL_AREA = @ID;", MixedTable);
            return sqlStr;
        }

    }
}