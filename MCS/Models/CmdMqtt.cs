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
    public class CmdMqtt : ISqlCreator
    {
        public string MasterTable { get { return "CMD_MQTT"; } }
        public string HistoryTable { get { return "CMD_MQTT_HISTORY"; } }
        public string IotTable { get { return "BASE_IOTDEVICE"; } }
        public string MapTable { get { return "BASE_MAP"; } }
        public string FloorTable { get { return "BASE_FLOOR"; } }
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
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} D ,{1} F ", MasterTable, IotTable);
            }
            else
            {
                sqlStr = string.Format(@"SELECT D.*,F.NAME AS IOTDEVICE_NAME, F.IP AS IOTDEVICE_IP, F.SN_KEY AS IOTDEVICE_SNKEY  FROM {0} D ,{1} F ", MasterTable,IotTable);
            }

            sqlStr += "WHERE D.ENABLE = 1 AND D.iotdevice_GUID=F.GUID ";
            
            //IotDevice Name
            if (!string.IsNullOrEmpty((string)conditions.IOTDEVICE_NAME))
            {
                sqlStr += "AND F.NAME LIKE CONCAT('%', @IOTDEVICE_NAME, '%') ";
            }
            //IotDevice IP
            if (!string.IsNullOrEmpty((string)conditions.IOTDEVICE_IP))
            {
                sqlStr += "AND F.IP LIKE CONCAT(@IOTDEVICE_IP, '%') ";
            }
            //名稱
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME , '%') ";
            }
            //TYPE
            if (!string.IsNullOrEmpty((string)conditions.TYPE))
            {
                sqlStr += "AND D.TYPE = @TYPE ";
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
            string sqlStr = string.Format(@"SELECT D.GUID,D.NAME,D.TYPE,D.COMMAND,D.COMMAND_JSON AS CONTENT,D.REMARK, F.GUID AS IOTDEVICE_GUID,F.NAME AS IOTDEVICE_NAME, F.IP AS IOTDEVICE_IP, F.SN_KEY AS IOTDEVICE_SNKEY FROM {0} D ,{1} F ", MasterTable, IotTable);
            sqlStr += "WHERE D.ENABLE = 1 AND D.IOTDEVICE_GUID=F.GUID AND D.GUID = @GUID";

            return sqlStr;
        }

        public string GetTopicInfoByGUID()
        {
            string sqlStr = string.Format(@"
        SELECT CMD.GUID,CMD.NAME,CMD.TYPE,CMD.COMMAND,CMD.COMMAND_JSON, CMD.INDEX,CMD.MQSERVER,CMD.REMARK, 
                        IOT.GUID AS IOTDEVICE_GUID,IOT.NAME AS IOTDEVICE_NAME, IOT.IP AS IOTDEVICE_IP, IOT.SN_KEY AS IOTDEVICE_SNKEY, IOT.DI_COUNT, IOT.DO_COUNT, 
                        FLOOR.ID AS FLOOR_ID,FLOOR.NAME AS FLOOR_NAME
          FROM {0} CMD 
   LEFT JOIN {1} IOT ON CMD.IOTDEVICE_GUID=IOT.GUID 
   LEFT JOIN {2} MAP ON IOT.MAP_GUID=MAP.GUID
   LEFT JOIN {3} FLOOR ON MAP.FLOOR_GUID=FLOOR.GUID
        WHERE CMD.ENABLE = 1              
             AND CMD.GUID = @GUID", MasterTable, IotTable, MapTable, FloorTable);

            return sqlStr;
        }

        public string GetTopicInfoByNAME()
        {
            string sqlStr = string.Format(@"
        SELECT CMD.GUID,CMD.NAME,CMD.TYPE,CMD.COMMAND,CMD.COMMAND_JSON, CMD.INDEX,CMD.MQSERVER,CMD.REMARK, 
                        IOT.GUID AS IOTDEVICE_GUID,IOT.NAME AS IOTDEVICE_NAME, IOT.IP AS IOTDEVICE_IP, IOT.SN_KEY AS IOTDEVICE_SNKEY, IOT.DI_COUNT, IOT.DO_COUNT, 
                        FLOOR.ID AS FLOOR_ID,FLOOR.NAME AS FLOOR_NAME
          FROM {0} CMD 
   LEFT JOIN {1} IOT ON CMD.IOTDEVICE_GUID=IOT.GUID 
   LEFT JOIN {2} MAP ON IOT.MAP_GUID=MAP.GUID
   LEFT JOIN {3} FLOOR ON MAP.FLOOR_GUID=FLOOR.GUID
        WHERE CMD.ENABLE = 1              
             AND CMD.NAME = @NAME", MasterTable, IotTable, MapTable, FloorTable);

            return sqlStr;
        }

        public string GetTopicInfoByID()
        {
            string sqlStr = string.Format(@"
        SELECT CMD.GUID,CMD.NAME,CMD.TYPE,CMD.COMMAND,CMD.COMMAND_JSON, CMD.INDEX,CMD.MQSERVER,CMD.REMARK, 
                        IOT.GUID AS IOTDEVICE_GUID,IOT.NAME AS IOTDEVICE_NAME, IOT.IP AS IOTDEVICE_IP, IOT.SN_KEY AS IOTDEVICE_SNKEY, IOT.DI_COUNT, IOT.DO_COUNT, 
                        FLOOR.ID AS FLOOR_ID,FLOOR.NAME AS FLOOR_NAME
          FROM {0} CMD 
   LEFT JOIN {1} IOT ON CMD.IOTDEVICE_GUID=IOT.GUID 
   LEFT JOIN {2} MAP ON IOT.MAP_GUID=MAP.GUID
   LEFT JOIN {3} FLOOR ON MAP.FLOOR_GUID=FLOOR.GUID
        WHERE CMD.ENABLE = 1              
             AND CMD.ID = @ID", MasterTable, IotTable, MapTable, FloorTable);

            return sqlStr;
        }

        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual;"; 

            return sqlStr;
        }
        public string Check()
        {
            string sqlStr = string.Format(@"SELECT D.GUID,D.NAME,D.COMMAND FROM {0} D ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND ( ID = @COMMAND or COMMAND = @COMMAND ) ";

            return sqlStr;
        }

        public string Insert(string uuid, JObject parm)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,IOTDEVICE_GUID,ID,`NAME`,`TYPE`,COMMAND,COMMAND_JSON, `INDEX`,MQSERVER,REMARK, ", MasterTable);
            string sqlValue = $@"VALUES ('{uuid}',@IOTDEVICE_GUID,@COMMAND,@NAME,@TYPE,@COMMAND,@COMMAND_JSON,@INDEX,@MQSERVER,@REMARK,";

            sqlStr += "INSERT_USER,INSERT_TIME) ";
            sqlValue += "@INSERT_USER,now());";
            sqlStr += sqlValue;

            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MQTT_GUID,IOTDEVICE_GUID,ID,`NAME`,`TYPE`,COMMAND,COMMAND_JSON,`INDEX`,MQSERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,IOTDEVICE_GUID,ID,`NAME`,`TYPE`,COMMAND,COMMAND_JSON,`INDEX`,MQSERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
        /// <summary>
        /// 擴充欄位
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} 
            (GUID,IOTDEVICE_GUID,ID,`NAME`,`TYPE`,COMMAND,COMMAND_JSON,MQSERVER,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@IOTDEVICE_GUID,@COMMAND,@NAME,@TYPE,@COMMAND,@COMMAND_JSON,'MQTT',@REMARK,@INSERT_USER,now()); ";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MQTT_GUID,IOTDEVICE_GUID,ID,`NAME`,`TYPE`,COMMAND,COMMAND_JSON,MQSERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,IOTDEVICE_GUID,ID,`NAME`,`TYPE`,COMMAND,COMMAND_JSON,MQSERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
        //public string Insert(string uuid)
        //{
        //    string sqlStr = string.Format(@"INSERT INTO {0} (GUID,FLOOR_GUID,ID,NAME,REMARK,INSERT_USER,INSERT_TIME) ", MasterTable);
        //    sqlStr += $@"VALUES ('{uuid}',@FLOOR_GUID,@ID,@NAME,@REMARK,@INSERT_USER,now());";
        //    sqlStr += string.Format(@"INSERT INTO {0}(GUID,MQTT_GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
        //    sqlStr += string.Format(@"SELECT UUID(),GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
        //    sqlStr += $@"WHERE GUID='{uuid}';";

        //    return sqlStr;
        //}
        /// <summary>
        /// 回傳SQL指令--新增
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,IOTDEVICE_GUID,`NAME`,`TYPE`,COMMAND, `INDEX`,MQSERVER,REMARK) ", MasterTable);
            sqlStr += $@"VALUES (UUID(),@IOTDEVICE_GUID,@NAME,@TYPE,@COMMAND,@INDEX,@MQSERVER,@REMARK,@INSERT_USER,now());";
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
                NAME = @NAME,
                COMMAND = @COMMAND,
                COMMAND_JSON = @COMMAND_JSON,
                TYPE= @TYPE,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MQTT_GUID,IOTDEVICE_GUID,ID,`NAME`,`TYPE`,COMMAND,MQSERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,IOTDEVICE_GUID,ID,`NAME`,`TYPE`,COMMAND,MQSERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,MQTT_GUID,IOTDEVICE_GUID,`NAME`,`TYPE`,COMMAND,`INDEX`,MQSERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(),GUID,IOTDEVICE_GUID,`NAME`,`TYPE`,COMMAND,`INDEX`,MQSERVER,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }
        //public string GetRepeatGUID()
        //{
        //    string sqlStr = string.Format(@"SELECT D.GUID,D.ID,D.NAME,D.REMARK FROM {0} D ", MasterTable);
        //    sqlStr += "WHERE ENABLE = 1 AND QRCODE = @QRCODE";

        //    return sqlStr;
        //}

        //public string DeleteRepeat(string uuid)
        //{
        //    //mqttcmd
        //    string sqlStr = string.Format("UPDATE {0} SET " +
        //        "ENABLE = 0," +
        //        "UPDATE_USER = @UPDATE_USER," +
        //        "UPDATE_TIME = now() " +
        //        $"WHERE GUID = '{uuid}' AND ENABLE = 1 ;", MasterTable);
        //    sqlStr += string.Format(@"INSERT INTO {0}(GUID,MQTT_GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", HistoryTable);
        //    sqlStr += string.Format(@"SELECT UUID(),GUID,FLOOR_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", MasterTable);
        //    sqlStr += $@"WHERE GUID='{uuid}';";
        //    ////node
        //    //sqlStr += string.Format("UPDATE {0} SET " +
        //    //    "ENABLE = 0," +
        //    //    "UPDATE_USER = @UPDATE_USER," +
        //    //    "UPDATE_TIME = now() " +
        //    //    $"WHERE MQTT_GUID = '{uuid}';", NodeTable);
        //    //sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MQTT_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMQTTCMD)", NodeHistoryTable);
        //    //sqlStr += string.Format(@"SELECT UUID(), GUID,MQTT_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMQTTCMD FROM {0} ", NodeTable);
        //    //sqlStr += $@"WHERE MQTT_GUID='{uuid}';";
        //    ////edge
        //    //sqlStr += string.Format("UPDATE {0} SET " +
        //    //    "ENABLE = 0," +
        //    //    "UPDATE_USER = @UPDATE_USER," +
        //    //    "UPDATE_TIME = now() " +
        //    //    $"WHERE MQTT_GUID = '{uuid}';", EdgeTable);
        //    //sqlStr += string.Format(@"INSERT INTO {0}(GUID,EDGE_GUID,MQTT_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT)", EdgeHistoryTable);
        //    //sqlStr += string.Format(@"SELECT UUID(), GUID,MQTT_GUID,NODE_ID,NEIGHB_ID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,DISTANCE,REVER,SPEED,ULTRASONIC,LEFTWIDTH,RIGHTWIDTH,ROADLEFTWIDTH,ROADRIGHTWIDTH,ROBOTTYPE,PODDIR,STARTDIR,STOPDIR,ALARMVOICE,PREFORKLIFT,ROTATE,LASERTYPE,PODLASERTYPE,SENSORSWITCH,CARRYTYPE,CTNRTYPE,CTRLPOINT FROM {0} ", EdgeTable);
        //    //sqlStr += $@"WHERE MQTT_GUID='{uuid}';";
        //    ////storaage
        //    //sqlStr += string.Format("UPDATE {0} SET " +
        //    //    "ENABLE = 0," +
        //    //    "UPDATE_USER = @UPDATE_USER," +
        //    //    "UPDATE_TIME = now() " +
        //    //    $"WHERE MQTT_GUID = '{uuid}';", StorageTable);
        //    //sqlStr += string.Format(@"INSERT INTO {0}(GUID,STORAGE_GUID,MQTT_GUID,QRCODE,ID,NAME,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK)", StorageHistoryTable);
        //    //sqlStr += string.Format(@"SELECT UUID(), GUID,MQTT_GUID,QRCODE,ID,NAME,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK FROM {0} ", StorageTable);
        //    //sqlStr += $@"WHERE MQTT_GUID='{uuid}';";
        //    return sqlStr;
        //}       
    }
}