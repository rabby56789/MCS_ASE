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
    public class Agv : ISqlCreator
    {
        public string MasterTable { get { return "BASE_AGV"; } }
        public string HistoryTable { get { return "BASE_AGV_HISTORY"; } }
        public string SubTableGroup { get { return "BASE_AGVGROUP"; } }

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
                if (String.IsNullOrEmpty(value.ToString()))
                {
                    value = null;
                }
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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D left join {1} S on D.AGVGROUP_GUID = S.GUID ", MasterTable, SubTableGroup);
            }
            else
            {
                sqlStr = string.Format(@"SELECT D.GUID,D.AGV_ID,D.AGV_NAME,D.AGV_TYPE,
                                    D.RATED_LOAD,D.LIFTING_HEIGHT,D.WEIGHT,D.ROTATION_DIAMETER,
                                    D.NAVIGATION,D.DIMENSION,D.RATED_SPEED,D.ROTATION_SPEED,S.GUID as AGVGROUP_GUID,S.AGVGROUP_ID,
                                    D.REMARK,D.INSERT_TIME FROM {0} D left join {1} S on D.AGVGROUP_GUID = S.GUID ", MasterTable,SubTableGroup);
            }

            sqlStr += "WHERE D.ENABLE = 1 ";

            //AGV代碼
            if (!string.IsNullOrEmpty((string)conditions.AGV_ID))
            {
                sqlStr += "AND D.AGV_ID LIKE CONCAT(@AGV_ID , '%') ";
            }
            //AGV名稱
            if (!string.IsNullOrEmpty((string)conditions.AGV_NAME))
            {
                sqlStr += "AND D.AGV_NAME LIKE CONCAT(@AGV_NAME , '%') ";
            }
            //AGV類型
            if (!string.IsNullOrEmpty((string)conditions.AGV_TYPE))
            {
                sqlStr += "AND D.AGV_TYPE LIKE CONCAT(@AGV_TYPE , '%') ";
            }
            //額定負載
            if (!string.IsNullOrEmpty((string)conditions.RATED_LOAD))
            {
                sqlStr += "AND D.RATED_LOAD = @RATED_LOAD ";
            }
            //舉升高度
            if (!string.IsNullOrEmpty((string)conditions.LIFTING_HEIGHT))
            {
                sqlStr += "AND D.LIFTING_HEIGHT = LIFTING_HEIGHT ";
            }
            //本身重量
            if (!string.IsNullOrEmpty((string)conditions.WEIGHT))
            {
                sqlStr += "AND D.WEIGHT = @WEIGHT ";
            }
            //旋轉直徑
            if (!string.IsNullOrEmpty((string)conditions.ROTATION_DIAMETER))
            {
                sqlStr += "AND D.ROTATION_DIAMETER = @ROTATION_DIAMETER ";
            }
            //導航方式
            if (!string.IsNullOrEmpty((string)conditions.NAVIAGATION))
            {
                sqlStr += "AND D.NAVIAGATION LIKE CONCAT(@NAVIAGATION , '%') ";
            }
            //外型尺寸
            if (!string.IsNullOrEmpty((string)conditions.DIMENSION))
            {
                sqlStr += "AND D.DIMENSION LIKE CONCAT(@DIMENSION , '%') ";
            }
            //額定速度
            if (!string.IsNullOrEmpty((string)conditions.RATED_SPEED))
            {
                sqlStr += "AND D.RATED_SPEED = @RATED_SPEED ";
            }
            //旋轉速度
            if (!string.IsNullOrEmpty((string)conditions.ROTATION_SPEED))
            {
                sqlStr += "AND D.ROTATION_SPEED = @ROTATION_SPEED ";
            }
            //群組代碼
            if (!string.IsNullOrEmpty((string)conditions.GROUP_ID))
            {
                sqlStr += "AND S.AGVGROUP_ID LIKE CONCAT(@GROUP_ID , '%') ";
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
            string sqlStr = string.Format(@"SELECT D.GUID,D.AGV_ID,D.AGV_NAME,D.AGV_TYPE,
                                    D.RATED_LOAD,D.LIFTING_HEIGHT,D.WEIGHT,D.ROTATION_DIAMETER,
                                    D.NAVIGATION,D.DIMENSION,D.RATED_SPEED,D.ROTATION_SPEED,D.AGVGROUP_GUID, S.AGVGROUP_ID,
                                    D.REMARK,D.INSERT_TIME FROM {0} D left join {1} S on D.AGVGROUP_GUID = S.GUID ", MasterTable, SubTableGroup);
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
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,AGV_ID,AGV_NAME,AGV_TYPE,RATED_LOAD,LIFTING_HEIGHT,WEIGHT,ROTATION_DIAMETER,NAVIGATION,
                                    DIMENSION,RATED_SPEED,ROTATION_SPEED,AGVGROUP_GUID,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@AGV_ID,@AGV_NAME,@AGV_TYPE,@RATED_LOAD,@LIFTING_HEIGHT,@WEIGHT,@ROTATION_DIAMETER,@NAVIGATION,@DIMENSION,
                                    @RATED_SPEED,@ROTATION_SPEED,@AGVGROUP_GUID,@REMARK,@INSERT_USER,now());";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,AGV_GUID,AGV_ID,AGV_NAME,AGV_TYPE,RATED_LOAD,LIFTING_HEIGHT,WEIGHT,ROTATION_DIAMETER,NAVIGATION,
                                    DIMENSION,RATED_SPEED,ROTATION_SPEED,AGVGROUP_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,AGV_ID,AGV_NAME,AGV_TYPE,RATED_LOAD,LIFTING_HEIGHT,WEIGHT,ROTATION_DIAMETER,NAVIGATION,
                                    DIMENSION,RATED_SPEED,ROTATION_SPEED,AGVGROUP_GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,ENABLE,REMARK FROM {0} ", MasterTable);
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
                AGV_ID = @AGV_ID,
                AGV_NAME = @AGV_NAME,
                AGV_TYPE = @AGV_TYPE,
                RATED_LOAD = @RATED_LOAD,
                LIFTING_HEIGHT = @LIFTING_HEIGHT,
                WEIGHT = @WEIGHT,
                ROTATION_DIAMETER = @ROTATION_DIAMETER,
                NAVIGATION = @NAVIGATION,
                DIMENSION = @DIMENSION,
                RATED_SPEED = @RATED_SPEED,
                ROTATION_SPEED = @ROTATION_SPEED,
                AGVGROUP_GUID = @AGVGROUP_GUID,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,AGV_GUID,AGV_ID,AGV_NAME,AGV_TYPE,RATED_LOAD,LIFTING_HEIGHT,
                                WEIGHT,ROTATION_DIAMETER,NAVIGATION,DIMENSION,RATED_SPEED,ROTATION_SPEED,AGVGROUP_GUID,INSERT_USER,
                                INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,AGV_ID,AGV_NAME,AGV_TYPE,RATED_LOAD,LIFTING_HEIGHT,
                                WEIGHT,ROTATION_DIAMETER,NAVIGATION,DIMENSION,RATED_SPEED,ROTATION_SPEED,AGVGROUP_GUID,INSERT_USER,
                                INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,AGV_GUID,AGV_ID,AGV_NAME,AGV_TYPE,RATED_LOAD,LIFTING_HEIGHT,
                                WEIGHT,ROTATION_DIAMETER,NAVIGATION,DIMENSION,RATED_SPEED,ROTATION_SPEED,AGVGROUP_GUID,INSERT_USER,
                                INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,AGV_ID,AGV_NAME,AGV_TYPE,RATED_LOAD,LIFTING_HEIGHT,
                                WEIGHT,ROTATION_DIAMETER,NAVIGATION,DIMENSION,RATED_SPEED,ROTATION_SPEED,AGVGROUP_GUID,INSERT_USER,
                                INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

        public string GetDataList(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"SELECT GUID , AGVGROUP_ID , AGVGROUP_NAME FROM {0} D ", SubTableGroup);
            sqlStr += "WHERE D.ENABLE = 1 ";

            //群組ID、NAME
            if (!string.IsNullOrEmpty((string)conditions.GROUP_ID))
            {
                sqlStr += "AND D.AGVGROUP_ID LIKE CONCAT(@GROUP_ID, '%') ";
            }
            if (!string.IsNullOrEmpty((string)conditions.GROUP_NAME))
            {
                sqlStr += "AND D.AGVGROUP_NAME LIKE CONCAT(@GROUP_NAME, '%') ";
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
        public string SearchGroupAllCount(JObject parm)
        {
            string sqlStr;
            sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D WHERE D.ENABLE = 1 ", SubTableGroup);

            return sqlStr;
        }
    }
}