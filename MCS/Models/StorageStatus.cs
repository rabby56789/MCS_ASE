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
    public class StorageStatus : ISqlCreator
    {
        public string MasterTable { get { return "T_STORAGE_STATUS"; } }
        public string HistoryTable { get { return "T_STORAGE_TRAVEL"; } }
        public string StorageTable { get { return "BASE_STORAGE"; } }
        public string AreaTable { get { return "BASE_AREA"; } }
        public string TrolleyTable { get { return "BASE_TROLLEY"; } }
        public string KeyCodeTable { get { return "SYS_KEYCODE"; } }
        /// <summary>
        /// 用JObject生成對應的SQL參數陣列
        /// </summary>
        /// <param name="input">前端輸入值</param>
        /// <returns></returns>
        public IDataParameter[] CreateParameterAry(JObject input)
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
               
                sqlStr = $@"SELECT Count(*) `Count` FROM {StorageTable} BS
                        LEFT JOIN {MasterTable} TS ON BS.GUID = TS.STORAGE_GUID
                        LEFT JOIN {AreaTable} BA ON BS.GROUP_GUID = BA.GUID 
                        WHERE BS.ENABLE = 1 ";
            }
            else
            {
                
                sqlStr = $@"SELECT TS.GUID AS GUID,BS.GUID AS STORAGE_GUID,BS.ID AS STORAGE_ID,BS.NAME AS STORAGE_NAME,BS.QRCODE AS QRCODE,BA.GUID AS AREA_GUID,BA.ID AS AREA_ID,BA.NAME AS AREA_NAME,
                        CASE TS.IS_LOCK
                        WHEN '1' THEN '已鎖定'
                        ELSE '未鎖定' 
                        END AS IS_LOCK
                        ,TS.JOB_NAME AS JOB_NAME FROM {StorageTable} BS
                        LEFT JOIN {MasterTable} TS ON BS.GUID = TS.STORAGE_GUID
                        LEFT JOIN {AreaTable} BA ON BS.GROUP_GUID = BA.GUID
                        WHERE BS.ENABLE = 1 ";
            }

            //儲位名稱
            if (!string.IsNullOrEmpty((string)conditions.STORAGE_NAME))
            {
                sqlStr += "AND BS.NAME LIKE CONCAT(@STORAGE_NAME , '%') ";
            }
            //區域名稱
            if (!string.IsNullOrEmpty((string)conditions.AREA_NAME))
            {
                sqlStr += "AND BA.NAME LIKE CONCAT(@AREA_NAME , '%') ";
            }
            //鎖定狀態
            if (!string.IsNullOrEmpty((string)conditions.IS_LOCK))
            {
                //sqlStr += "AND TS.IS_LOCK = 1";
                if ((string)conditions.IS_LOCK == "1")
                {
                    sqlStr += "AND BS.GUID IN (SELECT STORAGE_GUID FROM T_STORAGE_STATUS) ";
                }
                else
                {
                    sqlStr += "AND BS.GUID NOT IN (SELECT STORAGE_GUID FROM T_STORAGE_STATUS) ";
                }
            }
            ////建立時間-起
            //if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_START))
            //{
            //    sqlStr += "AND TSS.INSERT_TIME >= @INSERT_TIME_START ";
            //}
            ////建立時間-終
            //if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_END))
            //{
            //    sqlStr += "AND TSS.INSERT_TIME <= @INSERT_TIME_END ";
            //}

            //查詢數量不需換頁
            if (getCount)
            {
                return sqlStr;
            }

            if (parm.TryGetValue("sort", out _))
            {
                sqlStr += string.Format("ORDER BY TS.{0} ", (string)conditions.sort, (string)conditions.order);
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
            string sqlStr = "";
            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual;";
            return sqlStr;
        }
        /// <summary>
        /// 擴充欄位
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        /// <summary>
        /// 回傳SQL指令--新增
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            string sqlStr = "";

            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--更新
        /// </summary>
        /// <returns></returns>
        public string Update()
        {
            string sqlStr = "";
            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--刪除
        /// </summary>
        /// <returns></returns>
        public string Delete()
        {
            string sqlStr = "";
            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--刪除
        /// </summary>
        /// <returns></returns>
        public string executeLock()
        {
            string sqlStr = string.Format(@"UPDATE {0} SET 
                IS_LOCK = @isLock,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID; ", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(`GUID`,STORAGE_STATUS_GUID,`STORAGE_GUID`,`AREA_GUID`,
                                        `TROLLEY_GUID`,`PART_NO`,`JOB_NAME`,`DESTINATION`,`IS_LOCK`,`INSERT_USER`,`INSERT_TIME`,
                                        `UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,STORAGE_GUID,AREA_GUID,TROLLEY_GUID,PART_NO,JOB_NAME
                                        ,DESTINATION,`IS_LOCK`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,
                                        ENABLE,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

        /// <summary>
        /// 取得下拉選單選項
        /// </summary>
        /// <returns></returns>
        public string GetOption()
        {
            string sqlStr = string.Format(@"SELECT KEY_CODE, KEY_NAME FROM {0} D ", KeyCodeTable);
            sqlStr += "WHERE D.ENABLE = 1 And TABLE_NAME = 'storage_status' And COL_NAME = @name";
            return sqlStr;
        }
    }
}