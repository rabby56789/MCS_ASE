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
    public class Edge : ISqlCreator
    {
        public string MasterTable { get { return "BASE_EDGE"; } }
        public string HistoryTable { get { return "BASE_EDGE_HISTORY"; } }
        public string SubTable { get { return "BASE_FLOOR"; } }
        public string DataTable { get { return "BASE_FACTORY"; } }
        public string MapTable { get { return "BASE_MAP"; } }
        public string NodeTable { get { return "BASE_NODE"; } }

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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ,{1} F ,{2} A ,{3} M ", MasterTable, SubTable, DataTable, MapTable);
            }
            else
            {
                //sqlStr = string.Format(@"SELECT D.GUID,D.ID,D.NAME,D.REMARK,D.INSERT_TIME,D.XPOS,D.YPOS FROM {0} D ", MasterTable);
                sqlStr = string.Format(@"SELECT D.*,CASE D.MAP_ENABLE WHEN '0' THEN 'OFF' WHEN '1' THEN 'ON' END AS MAPENABLE,F.NAME AS FLOOR_NAME,A.NAME AS FACTORY_NAME,M.NAME AS MAP_NAME ,M.QRCODE AS MAP_QRCODE FROM {0} D ,{1} F ,{2} A ,{3} M ", MasterTable, SubTable, DataTable, MapTable);
            }

            sqlStr += "WHERE D.ENABLE = 1 AND D.MAP_GUID=M.GUID AND M.FLOOR_GUID=F.GUID AND F.FACTORY_GUID=A.GUID ";
            //廠GUID
            if (!string.IsNullOrEmpty((string)conditions.FACTORY_GUID))
            {
                sqlStr += "AND F.FACTORY_GUID LIKE CONCAT(@FACTORY_GUID, '%') ";
            }
            //樓層GUID
            if (!string.IsNullOrEmpty((string)conditions.FLOOR_GUID))
            {
                sqlStr += "AND M.FLOOR_GUID LIKE CONCAT(@FLOOR_GUID, '%') ";
            }
            //地圖GUID
            if (!string.IsNullOrEmpty((string)conditions.MAP_GUID))
            {
                sqlStr += "AND D.MAP_GUID LIKE CONCAT(@MAP_GUID, '%') ";
            }
            //代碼
            if (!string.IsNullOrEmpty((string)conditions.NODE_ID))
            {
                sqlStr += "AND D.NODE_ID LIKE CONCAT(@NODE_ID, '%') ";
            }
            //名稱
            if (!string.IsNullOrEmpty((string)conditions.NEIGHB_ID))
            {
                sqlStr += "AND D.NEIGHB_ID LIKE CONCAT(@NEIGHB_ID , '%') ";
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
            //string sqlStr = string.Format(@"SELECT D.GUID,D.NODE_ID,D.NEIGHB_ID,D.REMARK FROM {0} D ", MasterTable);
            string sqlStr = string.Format(@"SELECT D.* FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND GUID = @GUID";

            return sqlStr;
        }
        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual;"; 

            return sqlStr;
        }
        /// <summary>
        /// 擴充
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        //public string Insert(string uuid)
        //{
        //    string sqlStr = string.Format(@"INSERT INTO {0} (GUID,MAP_GUID,NODE_ID,NEIGHB_ID,REMARK,INSERT_USER,INSERT_TIME,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT) ", MasterTable);
        //    sqlStr += $@"VALUES ('{uuid}',@MAP_GUID,@NODE_ID,@NEIGHB_ID,@REMARK,@INSERT_USER,now(),@DISTANCE,@REVER,@SPEED,@ULTRASONIC,@LEFTWIDTH,@RIGHTWIDTH,@ROADLEFTWIDTH,@ROADRIGHTWIDTH,@ROBOTTYPE,@PODDIR,@STARTDIR,@STOPDIR,@ALARMVOICE,@PREFORKLIFT,@ROTATE,@LASERTYPE,@PODLASERTYPE,@SENSORSWITCH,@CARRYTYPE,@CTNRTYPE,@CTRLPOINT);";
        //    sqlStr += string.Format(@"INSERT INTO {0}(GUID,EDGE_GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT)", HistoryTable);
        //    sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT FROM {0} ", MasterTable);
        //    sqlStr += $@"WHERE GUID='{uuid}';";

        //    return sqlStr;
        //}
        public string Insert(string uuid, JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,MAP_GUID,NODE_ID,NEIGHB_ID,REMARK,", MasterTable);
            string sqlValue = $@"VALUES ('{uuid}',@MAP_GUID,@NODE_ID,@NEIGHB_ID,@REMARK,";
            if (!string.IsNullOrEmpty((string)conditions.DISTANCE))
            {
                sqlStr += "DISTANCE,";
                sqlValue += "@DISTANCE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.REVER))
            {
                sqlStr += "REVER,";
                sqlValue += "@REVER,";
            }
            if (!string.IsNullOrEmpty((string)conditions.SPEED))
            {
                sqlStr += "SPEED,";
                sqlValue += "@SPEED,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ULTRASONIC))
            {
                sqlStr += "ULTRASONIC,";
                sqlValue += "@ULTRASONIC,";
            }
            if (!string.IsNullOrEmpty((string)conditions.LEFTWIDTH))
            {
                sqlStr += "LEFTWIDTH,";
                sqlValue += "@LEFTWIDTH,";
            }
            if (!string.IsNullOrEmpty((string)conditions.RIGHTWIDTH))
            {
                sqlStr += "RIGHTWIDTH,";
                sqlValue += "@RIGHTWIDTH,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROADLEFTWIDTH))
            {
                sqlStr += "ROADLEFTWIDTH,";
                sqlValue += "@ROADLEFTWIDTH,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROADRIGHTWIDTH))
            {
                sqlStr += "ROADRIGHTWIDTH,";
                sqlValue += "@ROADRIGHTWIDTH,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROBOTTYPE))
            {
                sqlStr += "ROBOTTYPE,";
                sqlValue += "@ROBOTTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.PODDIR))
            {
                sqlStr += "PODDIR,";
                sqlValue += "@PODDIR,";
            }
            if (!string.IsNullOrEmpty((string)conditions.STARTDIR))
            {
                sqlStr += "STARTDIR,";
                sqlValue += "@STARTDIR,";
            }
            if (!string.IsNullOrEmpty((string)conditions.STOPDIR))
            {
                sqlStr += "STOPDIR,";
                sqlValue += "@STOPDIR,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ALARMVOICE))
            {
                sqlStr += "ALARMVOICE,";
                sqlValue += "@ALARMVOICE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.PREFORKLIFT))
            {
                sqlStr += "PREFORKLIFT,";
                sqlValue += "@PREFORKLIFT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTATE))
            {
                sqlStr += "ROTATE,";
                sqlValue += "@ROTATE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.LASERTYPE))
            {
                sqlStr += "LASERTYPE,";
                sqlValue += "@LASERTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.PODLASERTYPE))
            {
                sqlStr += "PODLASERTYPE,";
                sqlValue += "@PODLASERTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.SENSORSWITCH))
            {
                sqlStr += "SENSORSWITCH,";
                sqlValue += "@SENSORSWITCH,";
            }
            if (!string.IsNullOrEmpty((string)conditions.CARRYTYPE))
            {
                sqlStr += "CARRYTYPE,";
                sqlValue += "@CARRYTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.CTNRTYPE))
            {
                sqlStr += "CTNRTYPE,";
                sqlValue += "@CTNRTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.CTRLPOINT))
            {
                sqlStr += "CTRLPOINT,";
                sqlValue += "@CTRLPOINT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.MAP_ENABLE))
            {
                sqlStr += "MAP_ENABLE,";
                sqlValue += "@MAP_ENABLE,";
            }
            sqlStr += "INSERT_USER,INSERT_TIME) ";
            sqlValue += "@INSERT_USER,now());";
            sqlStr += sqlValue;
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,EDGE_GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,MAP_GUID,NODE_ID,NEIGHB_ID,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@MAP_GUID,@NODE_ID,@NEIGHB_ID,@REMARK,@INSERT_USER,now());";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,EDGE_GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
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
                NODE_ID = @NODE_ID , NEIGHB_ID = @NEIGHB_ID , REMARK = @REMARK ,
                UPDATE_USER = @UPDATE_USER , UPDATE_TIME = now() ,
                DISTANCE = @DISTANCE , REVER = @REVER , SPEED = @SPEED ,
                ULTRASONIC = @ULTRASONIC , LEFTWIDTH = @LEFTWIDTH , RIGHTWIDTH = @RIGHTWIDTH ,
                ROADLEFTWIDTH = @ROADLEFTWIDTH , ROADRIGHTWIDTH = @ROADRIGHTWIDTH , ROBOTTYPE = @ROBOTTYPE ,
                PODDIR = @PODDIR , STARTDIR = @STARTDIR , STOPDIR = @STOPDIR ,
                ALARMVOICE = @ALARMVOICE , PREFORKLIFT = @PREFORKLIFT , ROTATE = @ROTATE ,
                LASERTYPE = @LASERTYPE , PODLASERTYPE = @PODLASERTYPE , SENSORSWITCH = @SENSORSWITCH ,
                CARRYTYPE = @CARRYTYPE , CTNRTYPE = @CTNRTYPE , CTRLPOINT = @CTRLPOINT , MAP_ENABLE = @MAP_ENABLE 
                WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,EDGE_GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,MAP_ENABLE,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,MAP_ENABLE,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,EDGE_GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

    }
}