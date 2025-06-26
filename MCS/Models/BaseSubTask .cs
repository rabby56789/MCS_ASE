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
    public class BaseSubTask : ISqlCreator
    {
        public string MasterTable { get { return "BASE_SUBTASK"; } }
        public string HistoryTable { get { return "BASE_SUBTASK_HISTORY"; } }
        public string TaskTable { get { return "BASE_TASK"; } }
        public string KeyCodeTable { get { return "SYS_KEYCODE"; } }
        public string SubTableCmd { get { return "CMD_MQTT"; } }
        public string SubTableIOT { get { return "BASE_IOTDEVICE"; } }
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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ", MasterTable);
            }
            else
            {
                sqlStr = string.Format(@"SELECT 
                          GUID
                        , SUBTASK_ID
                        , SUBTASK_NAME
                        , TASK_TYPE
                        , SUBTASK_FUNCTION
                        , SUBTASK_TYPE
                        , SERVER_FUNCTION    
                        , REMARK
                        FROM {0} ", MasterTable);
            }
            sqlStr += "WHERE ENABLE = 1 ";

            //代碼
            //if (!string.IsNullOrEmpty((string)conditions.TASK_GUID))
            //{
            //    sqlStr += "AND E.GUID = @TASK_GUID ";
            //}

            //子任務類型
            if (!string.IsNullOrEmpty((string)conditions.SUBTASK_TYPE))
            {
                sqlStr += "AND SUBTASK_TYPE = @SUBTASK_TYPE ";
            }

            ////代碼
            if (!string.IsNullOrEmpty((string)conditions.SUBTASK_ID))
            {
                sqlStr += "AND SUBTASK_ID LIKE CONCAT(@SUBTASK_ID, '%') ";
            }
            //名稱
            if (!string.IsNullOrEmpty((string)conditions.SUBTASK_NAME))
            {
                sqlStr += "AND SUBTASK_NAME LIKE CONCAT(@SUBTASK_NAME , '%') ";
            }
            //建立時間-起
            //if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_START))
            //{
            //    sqlStr += "AND INSERT_TIME >= @INSERT_TIME_START ";
            //}
            //建立時間-終
            //if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_END))
            //{
            //    sqlStr += "AND INSERT_TIME <= @INSERT_TIME_END ";
            //}

            //查詢數量不需換頁
            if (getCount)
            {
                return sqlStr;
            }

            if (parm.TryGetValue("sort", out _))
            {
                if (conditions.sort == "INDEX")
                {
                    conditions.sort = "`INDEX`";
                }
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
            //string sqlStr = string.Format(@"SELECT 
            //D.GUID
            //, D.SUBTASK_ID
            //, D.SUBTASK_NAME
            //, D.TASK_TYPE
            //, CMD.NAME AS SUBTASK_FUNCTION
            //, SUBTASK_FUNCTION
            //, D.SUBTASK_TYPE
            //, D.SERVER_FUNCTION
            //, D.BASE_SERVER
            //, D.REMARK
            //FROM {0} D left join {1} CMD on D.SUBTASK_FUNCTION = CMD.GUID ", MasterTable, SubTableCmd);
            //sqlStr += "WHERE D.`ENABLE` = 1 ";
            //sqlStr += "AND D.GUID = @GUID ";

            string sqlStr = $@"SELECT GUID,SUBTASK_ID,SUBTASK_NAME,TASK_TYPE,SERVER_FUNCTION,SUBTASK_FUNCTION,SUBTASK_TYPE,REMARK
                            FROM BASE_SUBTASK WHERE ENABLE = 1 AND GUID = @GUID ";

            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual"; 

            return sqlStr;
        }
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,SUBTASK_ID,SUBTASK_NAME,TASK_TYPE,SUBTASK_FUNCTION,SUBTASK_TYPE,SERVER_FUNCTION,BASE_SERVER,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@SUBTASK_ID,@SUBTASK_NAME,@TASK_TYPE,@SUBTASK_FUNCTION,@SUBTASK_TYPE,@SERVER_FUNCTION,@BASE_SERVER,@REMARK,@INSERT_USER,now()); ";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,SUBTASK_GUID,SUBTASK_ID,SUBTASK_NAME,TASK_TYPE,SUBTASK_FUNCTION,SUBTASK_TYPE,SERVER_FUNCTION,BASE_SERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK) ", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,SUBTASK_ID,SUBTASK_NAME,TASK_TYPE,SUBTASK_FUNCTION,SUBTASK_TYPE,SERVER_FUNCTION,BASE_SERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}'; ";

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
                SUBTASK_ID = @SUBTASK_ID,
                SUBTASK_NAME = @SUBTASK_NAME,
                TASK_TYPE=@TASK_TYPE,
                SUBTASK_FUNCTION=@SUBTASK_FUNCTION,
                SUBTASK_TYPE=@SUBTASK_TYPE,
                SERVER_FUNCTION=@SERVER_FUNCTION,
                BASE_SERVER=@BASE_SERVER,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,SUBTASK_GUID,SUBTASK_ID,SUBTASK_NAME,TASK_TYPE,SUBTASK_FUNCTION,SUBTASK_TYPE,SERVER_FUNCTION,BASE_SERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,SUBTASK_ID,SUBTASK_NAME,TASK_TYPE,SUBTASK_FUNCTION,SUBTASK_TYPE,SERVER_FUNCTION,BASE_SERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,SUBTASK_GUID,SUBTASK_ID,SUBTASK_NAME,TASK_TYPE,SUBTASK_FUNCTION,SUBTASK_TYPE,SERVER_FUNCTION,BASE_SERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,SUBTASK_ID,SUBTASK_NAME,TASK_TYPE,SUBTASK_FUNCTION,SUBTASK_TYPE,SERVER_FUNCTION,BASE_SERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }
        public string GetDataList(JObject obj)
        {
            dynamic conditions = obj as dynamic;
            string sqlStr = $@"SELECT GUID,ID,NAME,COMMAND,COMMAND_JSON FROM {SubTableCmd} WHERE ENABLE = 1 ";
            //string sqlStr = string.Format(@"SELECT D.GUID, D.TYPE AS CMDTYPE, D.NAME AS CMDNAME, D.COMMAND, D.INDEX, D.REMARK, D.MQSERVER, F.NAME AS IOTDEVICE_NAME, F.IP AS IOTDEVICE_IP, F.SN_KEY AS IOTDEVICE_SNKEY  FROM {0} D ,{1} F ", SubTableCmd, SubTableIOT);
            //sqlStr += "WHERE D.ENABLE = 1 AND D.iotdevice_GUID = F.GUID ";

            ////命令搜尋條件
            if (!string.IsNullOrEmpty((string)conditions.IOT_GUID))
            {
                sqlStr += "AND IOTDEVICE_GUID = @IOT_GUID ";
            }
            //if (!string.IsNullOrEmpty((string)conditions.IOTDEVICE_IP))
            //{
            //    sqlStr += "AND F.IP LIKE CONCAT(@IOTDEVICE_IP, '%') ";
            //}
            //if (!string.IsNullOrEmpty((string)conditions.CMDNAME))
            //{
            //    sqlStr += "AND D.NAME LIKE CONCAT(@CMDNAME, '%') ";
            //}
            //if (!string.IsNullOrEmpty((string)conditions.CMD_TYPE))
            //{
            //    sqlStr += "AND D.TYPE LIKE CONCAT(@CMD_TYPE, '%') ";
            //}

            //if (obj.TryGetValue("sort", out _))
            //{
            //    sqlStr += string.Format("ORDER BY {0} {1} ", (string)conditions.sort, (string)conditions.order);
            //}

            return sqlStr;
        }
        public string GetIOTList(JObject obj)
        {
            dynamic conditions = obj as dynamic;
            string sqlStr = $@"SELECT GUID,SN_KEY,NAME FROM {SubTableIOT} WHERE ENABLE = 1 ";

            if (!string.IsNullOrEmpty((string)conditions.IOTDEVICE_SN_KEY))
            {
                sqlStr += "AND SN_KEY LIKE CONCAT(@IOTDEVICE_SN_KEY, '%') ";
            }
            
            if (!string.IsNullOrEmpty((string)conditions.IOTDEVICE_NAME))
            {
                sqlStr += "AND NAME LIKE CONCAT(@IOTDEVICE_NAME, '%') ";
            }

            if (obj.TryGetValue("sort", out _))
            {
                sqlStr += string.Format("ORDER BY {0} {1} ", (string)conditions.sort, (string)conditions.order);
            }

            return sqlStr;
        }

        public string GetOption(JObject obj)
        {
            dynamic Objdata = obj as dynamic;
            string sqlStr = "";
            if (Objdata.name == "task_type" || Objdata.name == "server_function")
            {
                sqlStr = string.Format(@"SELECT {0} AS `Key`, {0} AS `Value` FROM {1} where ENABLE = 1 GROUP BY {0}", Objdata.name, MasterTable);
            }
            else 
            {
                sqlStr = string.Format(@"SELECT KEY_CODE AS `Key`, KEY_NAME AS `Value` FROM {0} D ", KeyCodeTable);
                sqlStr += "WHERE D.ENABLE = 1 And TABLE_NAME = 'base_subtask' And COL_NAME = @name";
            }
            //string sqlStr = string.Format(@"SELECT KEY_CODE AS `Key`, KEY_NAME AS `Value` FROM {0} D ", KeyCodeTable);
            //sqlStr += "WHERE D.ENABLE = 1 And TABLE_NAME = 'base_subtask' And COL_NAME = @name";
            return sqlStr;
        }
        public string GetServerFunctionOption(JObject obj)
        {
            string sqlStr = "";
            switch (obj["Subtask_type"].ToString()) {
                case "0":
                    sqlStr = $@"select KEY_CODE AS `Key`,KEY_NAME AS `Value` from sys_keycode where TABLE_NAME='base_subtask' and COL_NAME = 'server_function' 
                                and enable = 1 AND KEY_NAME not in ('elvtGenSchedulingTask','elvtNotify','bindPodAndBerth');";
                    break;
                case "1":
                    sqlStr = $@"select KEY_NAME AS `Key`,KEY_NAME AS `Value` from sys_keycode where TABLE_NAME='base_subtask' and COL_NAME = 'subtask_type' 
                                and enable = 1 AND KEY_CODE =1";
                    break;
                default:
                    sqlStr = $@"SELECT '' AS `Key` , '' AS `Value` FROM {MasterTable}
                                GROUP BY `Key` ";
                    break;
            }
            return sqlStr;
        }

        //找群組總數量
        public string SearchCmdAllCount(JObject parm)
        {
            dynamic conditions = parm as dynamic;

            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", SubTableCmd);

            //參數IOT GUID
            if (!string.IsNullOrEmpty((string)conditions.IOT_GUID))
            {
                sqlStr += "AND IOTDEVICE_GUID = @IOT_GUID ";
            }

            return sqlStr;
        }
        public string SearchIOTAllCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", SubTableIOT);

            return sqlStr;
        }
    }
}