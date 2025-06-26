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
    public class TaskSubTask : ISqlCreator
    {
        public string MasterTable { get { return "BASE_TASK_SUBTASK"; } }
        public string HistoryTable { get { return "BASE_TASK_SUBTASK_HISTORY"; } }
        public string TaskTable { get { return "BASE_TASK"; } }
        public string SubTaskTable { get { return "BASE_SUBTASK"; } }
        public string AgvGroupTable { get { return "BASE_AGVGROUP"; } }
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
                    sqlStr = Update(parm);
                    break;
                case "Delete":
                    sqlStr = Delete();
                    break;
                default:
                    break;
            }

            return sqlStr;
        }
        
        public string SearchTask(JObject parm)
        {
            string sqlStr = string.Format(@"SELECT * FROM {0} D WHERE ENABLE=1 AND TASK_NAME = @TASK_NAME; ", TaskTable);

            return sqlStr;
        }
        
        public string SearchSubTask(JObject parm)
        {
            string sqlStr = string.Format(@"SELECT * FROM {0} D WHERE ENABLE = 1 AND SUBTASK_NAME = @SUBTASK_NAME; ",SubTaskTable);

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
                sqlStr += string.Format(@"LEFT JOIN {0} T ON D.TASK_GUID = T.GUID ", TaskTable);
                sqlStr += string.Format(@"LEFT JOIN {0} S ON D.SUBTASK_GUID = S.GUID ",SubTaskTable);
            }
            else
            {

                sqlStr = string.Format(@"SELECT D.GUID, T.GUID AS TASK_GUID, T.TASK_ID, T.TASK_NAME, S.GUID AS SUBTASK_GUID, S.SUBTASK_ID, S.SUBTASK_NAME, D.PROGRESS, G.GUID AS AGVGROUP_GUID,
                                            G.AGVGROUP_NAME, D.POSITIONCODEPATH, D.REMARK, D.START_FLOOR, D.TARGET_FLOOR, 
                                            case D.BOOKING when '0' then ""Unbooking"" when '1' then ""Booking"" else '' end AS BOOKING,
                                            case D.DIRECTION
                                            when 0 then ""上行""
                                            when 1 then ""下行""
                                            END AS DIRECTION,
                                            D.API FROM {0} D 
                                            left join {1} T on D.TASK_GUID = T.GUID left join {2} S on D.SUBTASK_GUID = S.GUID 
                                            left join {3} G on D.AGVGROUP_GUID = G.GUID ", MasterTable, TaskTable, SubTaskTable, AgvGroupTable);

            }
            //任務查詢，直接帶TASK_GUID(前端有先做查詢檢查)
            sqlStr += "WHERE D.ENABLE = 1 AND D.TASK_GUID = @GUID ";

            
            //if (!string.IsNullOrEmpty((string)conditions.TASK_NAME))//帶TASK_NAME若有複製任務有風險顯示錯誤
            //{
            //    sqlStr += "AND T.ENABLE = 1 AND  T.TASK_NAME = @TASK_NAME ";
            //}

            //子任務查詢
            if (!string.IsNullOrEmpty((string)conditions.SUBTASK_NAME))
            {
                sqlStr += "AND S.ENABLE = 1 AND S.SUBTASK_NAME = @SUBTASK_NAME ";
            }
            

            //建立時間-起
            //if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_START))
            //{
            //    sqlStr += "AND D.INSERT_TIME >= @INSERT_TIME_START ";
            //}
            //建立時間-終
            //if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_END))
            //{
            //    sqlStr += "AND D.INSERT_TIME <= @INSERT_TIME_END ";
            //}

            //查詢數量不需換頁
            if (getCount)
            {
                return sqlStr;
            }

            if (parm.TryGetValue("sort", out _))
            {
                sqlStr += string.Format("ORDER BY {0} {1} ,PROGRESS asc ", (string)conditions.sort, (string)conditions.order);
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
        
        public string SearchTaskCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", TaskTable);
            return sqlStr;
        }
        
        public string SearchSubTaskCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", SubTaskTable);
            
            return sqlStr;
        }

        public string SearchAgvGroupCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", AgvGroupTable);

            return sqlStr;
        }

        // 取得唯一值
        public string GetOneByGUID()
        {
            string sqlStr = string.Format(@"SELECT D.GUID, T.GUID AS TASK_GUID, T.TASK_NAME, S.GUID AS SUBTASK_GUID, S.SUBTASK_NAME, AGV.GUID AS AGVGROUP_GUID, AGV.AGVGROUP_NAME, 
                                                D.PROGRESS, D.START_FLOOR, D.TARGET_FLOOR, D.DIRECTION, D.BOOKING, D.API, D.POSITIONCODEPATH, D.REMARK FROM {0} D inner join {1} T 
                                                on D.TASK_GUID = T.GUID left join {2} S on D.SUBTASK_GUID = S.GUID left join {3} AGV on D.AGVGROUP_GUID = AGV.GUID ", MasterTable,TaskTable,SubTaskTable,AgvGroupTable);
            sqlStr += "WHERE D.ENABLE = 1 AND D.GUID = @GUID";
            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual";

            return sqlStr;
        }
        //查詢BaseTask
        public string GetTaskList(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT * FROM {0} D ", TaskTable);
            sqlStr += "WHERE D.ENABLE = 1 ";

            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.TASK_ID LIKE CONCAT(@ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.TASK_NAME LIKE CONCAT(@NAME, '%') ";
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
        
        //查詢subtask選項
        public string GetSubTaskList(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT * FROM {0} D ", SubTaskTable);
            sqlStr += "WHERE D.ENABLE = 1 ";

            //儲位ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.SUBTASK_ID LIKE CONCAT(@ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.SUBTASK_NAME LIKE CONCAT(@NAME, '%') ";
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

        //查詢subtask選項
        public string GetAgvGroupList(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT * FROM {0} D ", AgvGroupTable);
            sqlStr += "WHERE D.ENABLE = 1 ";

            //儲位ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.AGVGROUP_ID LIKE CONCAT(@ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.AGVGROUP_NAME LIKE CONCAT(@NAME, '%') ";
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

        public string Insert(string uuid,JObject obj)
        {
            dynamic ObjData = obj as dynamic;
            string sqlStr = string.Format(@" INSERT INTO {0} (GUID,TASK_GUID,SUBTASK_GUID,AGVGROUP_GUID,`PROGRESS`,START_FLOOR,TARGET_FLOOR,API,POSITIONCODEPATH,REMARK,INSERT_USER,INSERT_TIME,DIRECTION,BOOKING)", MasterTable);            
            sqlStr += $@" VALUES ('{uuid}',@TASK_GUID,@SUBTASK_GUID,@AGVGROUP_GUID,@PROGRESS,@START_FLOOR,@TARGET_FLOOR,@API,@POSITIONCODEPATH,@REMARK,@INSERT_USER,now()";
            //DIRECTION為非必填，但不可用空字串新增，所以加入判斷
            if (ObjData.DIRECTION == "")
            {
                sqlStr += @",NULL";
            }
            else
            {
                sqlStr += @",@DIRECTION";
            }
            if (ObjData.BOOKING == "")
            {
                sqlStr += @",NULL";
            }
            else
            {
                sqlStr += @",@BOOKING";
            }
            //sqlStr += ",@BOOKING ";
            sqlStr += ");";
            sqlStr += string.Format(@" INSERT INTO {0}(GUID,TASK_SUBTASK_GUID,TASK_GUID,SUBTASK_GUID,AGVGROUP_GUID,`PROGRESS`,START_FLOOR,TARGET_FLOOR,DIRECTION,API,POSITIONCODEPATH,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,BOOKING) ", HistoryTable);
            sqlStr += string.Format(@" SELECT UUID(), GUID,TASK_GUID,SUBTASK_GUID,AGVGROUP_GUID,`PROGRESS`,START_FLOOR,TARGET_FLOOR,DIRECTION,API,POSITIONCODEPATH,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,BOOKING FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";
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
        public string Update(JObject obj)
        {
            dynamic ObjData = obj as dynamic;
            string sqlStr = string.Format(@"UPDATE {0} SET 
                TASK_GUID = @TASK_GUID,
                SUBTASK_GUID = @SUBTASK_GUID,
                AGVGROUP_GUID = @AGVGROUP_GUID,
                START_FLOOR = @START_FLOOR,
                TARGET_FLOOR = @TARGET_FLOOR,                
                API = @API,
                `PROGRESS` = @PROGRESS,
                POSITIONCODEPATH = @POSITIONCODEPATH,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now(),
                BOOKING = @BOOKING ", MasterTable);
            //DIRECTION為非必填，但不可用空字串更新，更新為NULL
            if (ObjData.DIRECTION == "")
            {
                sqlStr += @",DIRECTION = NULL WHERE GUID = @GUID;";
            }
            else 
            {
                sqlStr += @",DIRECTION = @DIRECTION WHERE GUID = @GUID;";
            }
            sqlStr += string.Format(@" INSERT INTO {0}(GUID,TASK_SUBTASK_GUID,TASK_GUID,SUBTASK_GUID,AGVGROUP_GUID,`PROGRESS`,START_FLOOR,TARGET_FLOOR,BOOKING,DIRECTION,API,POSITIONCODEPATH,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK) ", HistoryTable);
            sqlStr += string.Format(@" SELECT UUID(), GUID,TASK_GUID,SUBTASK_GUID,AGVGROUP_GUID,`PROGRESS`,START_FLOOR,TARGET_FLOOR,BOOKING,DIRECTION,API,POSITIONCODEPATH,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@" INSERT INTO {0}(GUID,TASK_SUBTASK_GUID,TASK_GUID,SUBTASK_GUID,AGVGROUP_GUID,`PROGRESS`,START_FLOOR,TARGET_FLOOR,DIRECTION,API,POSITIONCODEPATH,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK) ", HistoryTable);
            sqlStr += string.Format(@" SELECT UUID(), GUID,TASK_GUID,SUBTASK_GUID,AGVGROUP_GUID,`PROGRESS`,START_FLOOR,TARGET_FLOOR,DIRECTION,API,POSITIONCODEPATH,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

        public string GetOption()
        {
            //string sqlStr = string.Format(@"SELECT KEY_CODE, KEY_NAME FROM {0} D ", KeyCodeTable);
            //sqlStr += "WHERE D.ENABLE = 1 And TABLE_NAME = 'base_tasksubtask' And COL_NAME = @name";
            string sqlStr = string.Format(@"SELECT DIRECTION AS 'Key', 
                                            CASE DIRECTION 
                                            WHEN 0 THEN ""上行""
                                            WHEN 1 THEN ""下行""
                                            END AS 'Value'
                                            FROM {0} WHERE DIRECTION IN (0,1) GROUP BY DIRECTION", MasterTable);            
            return sqlStr;
        }

        public string SubTaskCount(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;

            //是否為查詢資料筆數
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ", SubTaskTable);

            sqlStr += "WHERE D.ENABLE = 1 ";

            //代號
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.SUBTASK_ID LIKE CONCAT(@ID, '%') ";
            }

            //名稱
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.SUBTASK_NAME LIKE CONCAT(@NAME, '%') ";
            }

            return sqlStr;
        }
        public string TaskCount(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;

            //是否為查詢資料筆數
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ", TaskTable);

            sqlStr += "WHERE D.ENABLE = 1 ";

            //代號
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.TASK_ID LIKE CONCAT(@ID, '%') ";
            }

            //名稱
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.TASK_NAME LIKE CONCAT(@NAME, '%') ";
            }

            return sqlStr;
        }
        public string AgvGroupListCount(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;

            //是否為查詢資料筆數
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ", AgvGroupTable);

            sqlStr += "WHERE D.ENABLE = 1 ";

            //代號
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.AGVGROUP_ID LIKE CONCAT(@ID, '%') ";
            }

            //名稱
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.AGVGROUP_NAME LIKE CONCAT(@NAME, '%') ";
            }

            return sqlStr;
        }
    }
}