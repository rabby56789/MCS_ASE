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
    public class ElvtAction : ISqlCreator
    {
        public string MasterTable { get { return "T_ELVT_ACTION"; } }
        public string HistoryTable { get { return "T_ELVT_ACTION_HISTORY"; } }
        public string ElvtTable { get { return "BASE_ELVT"; } }
        public string CmdTable { get { return "CMD_MQTT"; } }

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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} m left join {1} cmd on m.COMMAND_GUID = cmd.GUID ", MasterTable,CmdTable);
            }
            else
            {
                sqlStr = string.Format(@"SELECT m.GUID, m.ELVT_GUID, ELVT_ID, m.ID, m.NAME, m.COMMAND_GUID, cmd.COMMAND,
                                                cmd.NAME AS COMMAND_NAME, m.INDEX, m.NEXT_INDEX, m.TYPE, m.REMARK FROM {0} m left join {1} cmd on m.COMMAND_GUID = cmd.GUID ", MasterTable, CmdTable);
            }
            sqlStr += "WHERE m.ENABLE = 1 ";

            //電梯ID
            if (!string.IsNullOrEmpty((string)conditions.ELVT_ID))
            {
                sqlStr += "AND ELVT_ID LIKE CONCAT(@ELVT_ID , '%') ";
            }
            //電梯名稱
           /* if (!string.IsNullOrEmpty((string)conditions.ELVT_NAME))
            {
                sqlStr += "AND elvt.NAME LIKE CONCAT(@ELVT_NAME , '%') ";
            }*/
            //電梯命令ID
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND m.ID LIKE CONCAT(@ID , '%') ";
            }
            //電梯命令名稱
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND m.NAME LIKE CONCAT(@NAME , '%') ";
            }
            //命令名稱
            if (!string.IsNullOrEmpty((string)conditions.COMMAND_NAME))
            {
                sqlStr += "AND cmd.NAME LIKE CONCAT(@COMMAND_NAME , '%') ";
            }
            //命令
            if (!string.IsNullOrEmpty((string)conditions.COMMAND))
            {
                sqlStr += "AND cmd.COMMAND LIKE CONCAT(@COMMAND , '%') ";
            }
            //類型
            if (!string.IsNullOrEmpty((string)conditions.TYPE))
            {
                sqlStr += "AND m.TYPE LIKE CONCAT(@TYPE , '%') ";
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
                sqlStr += string.Format("ORDER BY m.{0} {1} ", (string)conditions.sort, (string)conditions.order);
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
            string sqlStr = string.Format(@"SELECT m.GUID, m.ELVT_GUID, m.ELVT_ID as ELVT_ID, m.ID, m.NAME, m.COMMAND_GUID, cmd.COMMAND,
                                                cmd.NAME AS COMMAND_NAME, m.INDEX, m.NEXT_INDEX, m.TYPE, m.REMARK FROM {0} m left join {1} cmd on m.COMMAND_GUID = cmd.GUID ", MasterTable, CmdTable);
            sqlStr += "WHERE m.ENABLE = 1 AND m.GUID = @GUID";

            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual"; 

            return sqlStr;
        }
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,ELVT_GUID,ELVT_ID,ID,NAME,COMMAND_GUID,`INDEX`,NEXT_INDEX,TYPE,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@ELVT_GUID,@ELVT_ID,@ID,@NAME,@COMMAND_GUID,@INDEX,@NEXT_INDEX,@TYPE,@REMARK,@INSERT_USER,now());";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,ELVT_ACTION_GUID,ELVT_GUID,ELVT_ID,ID,NAME,COMMAND_GUID,`INDEX`,NEXT_INDEX,TYPE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,ELVT_GUID,ELVT_ID,ID,NAME,COMMAND_GUID,`INDEX`,NEXT_INDEX,TYPE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
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
                ELVT_GUID = @ELVT_GUID,
                ID = @ID,
                `NAME` = @NAME,
                COMMAND_GUID = @COMMAND_GUID,
                `INDEX` = @INDEX,
                NEXT_INDEX = @NEXT_INDEX,
                `TYPE` = @TYPE,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,ELVT_ACTION_GUID,ELVT_GUID,ELVT_ID,ID,NAME,COMMAND_GUID,`INDEX`,NEXT_INDEX,TYPE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,ELVT_GUID,ELVT_ID,ID,NAME,COMMAND_GUID,`INDEX`,NEXT_INDEX,TYPE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,ELVT_ACTION_GUID,ELVT_GUID,ELVT_ID,ID,NAME,COMMAND_GUID,`INDEX`,NEXT_INDEX,TYPE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,ELVT_GUID,ELVT_ID,ID,NAME,COMMAND_GUID,`INDEX`,NEXT_INDEX,TYPE,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

        public string GetDataListElvt(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT GUID, ID, NAME FROM {0} D ", ElvtTable);
            sqlStr += "WHERE D.ENABLE = 1 ";

            //群組ID、NAME
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

        //找群組總數量
        public string GetElvtCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", ElvtTable);

            return sqlStr;
        }

        public string GetDataListCmd(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT GUID , NAME, COMMAND FROM {0} D ", CmdTable);
            sqlStr += "WHERE D.ENABLE = 1 ";

            //群組ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.COMMAND))
            {
                sqlStr += "AND D.COMMAND LIKE CONCAT(@COMMAND, '%') ";
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

        //找群組總數量
        public string GetCmdCount(JObject parm)
        {
            string sqlStr;
            dynamic conditions = parm as dynamic;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", CmdTable);
            //群組ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.COMMAND))
            {
                sqlStr += "AND D.COMMAND LIKE CONCAT(@COMMAND, '%') ";
            }
            return sqlStr;
        }
    }
}