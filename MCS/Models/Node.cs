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
    public class Node : ISqlCreator
    {
        public string MasterTable { get { return "BASE_NODE"; } }
        public string HistoryTable { get { return "BASE_NODE_HISTORY"; } }
        public string SubTable { get { return "BASE_FLOOR"; } }
        public string DataTable { get { return "BASE_FACTORY"; } }
        public string MapTable { get { return "BASE_MAP"; } }

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
                    case "XPOS":
                    case "YPOS":
                    case "WIDTH":
                    case "HEIGHT":
                        parm.Value = value;
                        parm.DbType = System.Data.DbType.Double;
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
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND D.ID LIKE CONCAT(@ID, '%') ";
            }
            //名稱
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND D.NAME LIKE CONCAT(@NAME , '%') ";
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
            //string sqlStr = string.Format(@"SELECT D.GUID,D.ID,D.NAME,D.REMARK,D.XPOS,D.YPOS FROM {0} D ", MasterTable);
            string sqlStr = string.Format(@"SELECT D.* , f.NAME AS MAP_NAME ,G.NAME AS FLOOR_NAME FROM {0} D ,{1} F ,{2} G ", MasterTable, MapTable, SubTable);
            sqlStr += "WHERE D.ENABLE = 1 AND D.GUID = @GUID AND F.GUID = @MAP_GUID";

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
        //public string Insert(string uuid)
        //{
        //    string sqlStr = string.Format(@"INSERT INTO {0} (GUID,MAP_GUID,ID,NAME,REMARK,INSERT_USER,INSERT_TIME,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP) ", MasterTable);
        //    sqlStr += $@"VALUES ('{uuid}',@MAP_GUID,@ID,@NAME,@REMARK,@INSERT_USER,now(),@XPOS,@YPOS,@WIDTH,@HEIGHT,@VALUE,@ROADPROPERTY,@ROT,@ALLDIRROT,@ALLDIR,@ELEDIR,@ELEPRE,@ROTRAD,@ROBOTTYPE,@ROTUNDERPOD,@ROTMECH,@EVIT,@ROTBYROBOTTYPE,@ROTFORPODTYPE,@SENSORSWITCHPOINT,@PALLET,@TRANZONETYPE,@ROTBARRIERAREA,@ISUPDATEMAP);";
        //    sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP)", HistoryTable);
        //    sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP FROM {0} ", MasterTable);
        //    sqlStr += $@"WHERE GUID='{uuid}';";

        //    return sqlStr;
        //}
        public string Insert(string uuid, JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,MAP_GUID,ID,NAME,REMARK, ", MasterTable);
            string sqlValue = $@"VALUES ('{uuid}',@MAP_GUID,@ID,@NAME,@REMARK,";
            
            if (!string.IsNullOrEmpty((string)conditions.XPOS))
            {
                sqlStr += "XPOS,";
                sqlValue += "@XPOS,";
            }
            if (!string.IsNullOrEmpty((string)conditions.YPOS))
            {
                sqlStr += "YPOS,";
                sqlValue += "@YPOS,";
            }
            if (!string.IsNullOrEmpty((string)conditions.QRCODE))
            {
                sqlStr += "QRCODE,";
                sqlValue += "@QRCODE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.WIDTH))
            {
                sqlStr += "WIDTH,";
                sqlValue += "@WIDTH,";
            }
            if (!string.IsNullOrEmpty((string)conditions.HEIGHT))
            {
                sqlStr += "HEIGHT,";
                sqlValue += "@HEIGHT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.VALUE))
            {
                sqlStr += "VALUE,";
                sqlValue += "@VALUE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROADPROPERTY))
            {
                sqlStr += "ROADPROPERTY,";
                sqlValue += "@ROADPROPERTY,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROT))
            {
                sqlStr += "ROT,";
                sqlValue += "@ROT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ALLDIRROT))
            {
                sqlStr += "ALLDIRROT,";
                sqlValue += "@ALLDIRROT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ALLDIR))
            {
                sqlStr += "ALLDIR,";
                sqlValue += "@ALLDIR,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ELEDIR))
            {
                sqlStr += "ELEDIR,";
                sqlValue += "@ELEDIR,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ELEPRE))
            {
                sqlStr += "ELEPRE,";
                sqlValue += "@ELEPRE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTRAD))
            {
                sqlStr += "ROTRAD,";
                sqlValue += "@ROTRAD,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROBOTTYPE))
            {
                sqlStr += "ROBOTTYPE,";
                sqlValue += "@ROBOTTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTUNDERPOD))
            {
                sqlStr += "ROTUNDERPOD,";
                sqlValue += "@ROTUNDERPOD,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTMECH))
            {
                sqlStr += "ROTMECH,";
                sqlValue += "@ROTMECH,";
            }
            if (!string.IsNullOrEmpty((string)conditions.EVIT))
            {
                sqlStr += "EVIT,";
                sqlValue += "@EVIT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTBYROBOTTYPE))
            {
                sqlStr += "ROTBYROBOTTYPE,";
                sqlValue += "@ROTBYROBOTTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTFORPODTYPE))
            {
                sqlStr += "ROTFORPODTYPE,";
                sqlValue += "@ROTFORPODTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.SENSORSWITCHPOINT))
            {
                sqlStr += "SENSORSWITCHPOINT,";
                sqlValue += "@SENSORSWITCHPOINT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.PALLET))
            {
                sqlStr += "PALLET,";
                sqlValue += "@PALLET,";
            }
            if (!string.IsNullOrEmpty((string)conditions.TRANZONETYPE))
            {
                sqlStr += "TRANZONETYPE,";
                sqlValue += "@TRANZONETYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTBARRIERAREA))
            {
                sqlStr += "ROTBARRIERAREA,";
                sqlValue += "@ROTBARRIERAREA,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ISUPDATEMAP))
            {
                sqlStr += "ISUPDATEMAP,";
                sqlValue += "@ISUPDATEMAP,";
            }
            sqlStr += "INSERT_USER,INSERT_TIME) ";
            sqlValue += "@INSERT_USER,now());";
            sqlStr += sqlValue;
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,QRCODE,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,QRCODE,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
        public string Insert(string uuid)
        {
            string sqlStr = string.Format(@"INSERT INTO {0} (GUID,MAP_GUID,ID,NAME,REMARK,INSERT_USER,INSERT_TIME,XPOS,YPOS,WIDTH,HEIGHT) ", MasterTable);
            sqlStr += $@"VALUES ('{uuid}',@MAP_GUID,@ID,@NAME,@REMARK,@INSERT_USER,now(),@XPOS,@YPOS,@WIDTH,@HEIGHT);";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID='{uuid}';";

            return sqlStr;
        }
        //public string Insert(string uuid)
        //{
        //    string sqlStr = string.Format(@"INSERT INTO {0} (GUID,ID,NAME,REMARK,INSERT_USER,INSERT_TIME,X_ASIX,Y_ASIX) ", MasterTable);
        //    sqlStr += $@"VALUES ('{uuid}',@ID,@NAME,@REMARK,@INSERT_USER,now(),@X_ASIX,@Y_ASIX);";
        //    sqlStr += string.Format(@"INSERT INTO {0}(GUID,POINT_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,X_ASIX,Y_ASIX)", HistoryTable);
        //    sqlStr += string.Format(@"SELECT UUID(), GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,X_ASIX,Y_ASIX FROM {0} ", MasterTable);
        //    sqlStr += $@"WHERE GUID='{uuid}';";

        //    return sqlStr;
        //}
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
                ID = @ID , NAME = @NAME , REMARK = @REMARK ,
                UPDATE_USER = @UPDATE_USER , UPDATE_TIME = now() , XPOS = @XPOS ,
                YPOS = @YPOS , WIDTH = @WIDTH , HEIGHT = @HEIGHT ,
                VALUE = @VALUE , ROADPROPERTY = @ROADPROPERTY , ROT = @ROT ,
                ALLDIRROT = @ALLDIRROT, ALLDIR = @ALLDIR, ELEDIR = @ELEDIR,
                ELEPRE = @ELEPRE, ROTRAD = @ROTRAD, ROBOTTYPE = @ROBOTTYPE,
                ROTUNDERPOD = @ROTUNDERPOD, ROTMECH = @ROTMECH, EVIT = @EVIT,
                ROTBYROBOTTYPE = @ROTBYROBOTTYPE, ROTFORPODTYPE = @ROTFORPODTYPE, SENSORSWITCHPOINT = @SENSORSWITCHPOINT,
                PALLET = @PALLET, TRANZONETYPE = @TRANZONETYPE, ROTBARRIERAREA = @ROTBARRIERAREA,
                ISUPDATEMAP = @ISUPDATEMAP, MAP_ENABLE = @MAP_ENABLE 
                WHERE GUID = @GUID;", MasterTable);
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP,MAP_ENABLE)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP,MAP_ENABLE FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";

            return sqlStr;
        }
        public string Update(JObject parm)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr = string.Format(@"UPDATE {0} SET ", MasterTable);
            if (!string.IsNullOrEmpty((string)conditions.MAP_ENABLE))
            {
                sqlStr += "MAP_ENABLE = @MAP_ENABLE ,";

            }
            if (!string.IsNullOrEmpty((string)conditions.XPOS))
            {
                sqlStr += "XPOS = @XPOS ,";
                
            }
            if (!string.IsNullOrEmpty((string)conditions.YPOS))
            {
                sqlStr += "YPOS = @YPOS ,";
                
            }
            
            if (!string.IsNullOrEmpty((string)conditions.WIDTH))
            {
                sqlStr += "WIDTH = @WIDTH ,";
                
            }
            if (!string.IsNullOrEmpty((string)conditions.HEIGHT))
            {
                sqlStr += "HEIGHT = @HEIGHT ,";
                
            }
            if (!string.IsNullOrEmpty((string)conditions.VALUE))
            {
                sqlStr += "VALUE = @VALUE ,";
                
            }
            if (!string.IsNullOrEmpty((string)conditions.ROADPROPERTY))
            {
                sqlStr += "ROADPROPERTY = @ROADPROPERTY ,";
                
            }
            if (!string.IsNullOrEmpty((string)conditions.ROT))
            {
                sqlStr += "ROT = @ROT ,";
                
            }
            if (!string.IsNullOrEmpty((string)conditions.ALLDIRROT))
            {
                sqlStr += "ALLDIRROT = @ALLDIRROT,";
                
            }
            if (!string.IsNullOrEmpty((string)conditions.ALLDIR))
            {
                sqlStr += "ALLDIR = @ALLDIR,";
                
            }
            if (!string.IsNullOrEmpty((string)conditions.ELEDIR))
            {
                sqlStr += "ELEDIR = @ELEDIR,";
                
            }
            if (!string.IsNullOrEmpty((string)conditions.ELEPRE))
            {
                sqlStr += "ELEPRE = @ELEPRE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTRAD))
            {
                sqlStr += "ROTRAD = @ROTRAD,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROBOTTYPE))
            {
                sqlStr += "ROBOTTYPE = @ROBOTTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTUNDERPOD))
            {
                sqlStr += "ROTUNDERPOD = @ROTUNDERPOD,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTMECH))
            {
                sqlStr += "ROTMECH = @ROTMECH,";
            }
            if (!string.IsNullOrEmpty((string)conditions.EVIT))
            {
                sqlStr += "EVIT = @EVIT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTBYROBOTTYPE))
            {
                sqlStr += "ROTBYROBOTTYPE = @ROTBYROBOTTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTFORPODTYPE))
            {
                sqlStr += "ROTFORPODTYPE = @ROTFORPODTYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.SENSORSWITCHPOINT))
            {
                sqlStr += "SENSORSWITCHPOINT = @SENSORSWITCHPOINT,";
            }
            if (!string.IsNullOrEmpty((string)conditions.PALLET))
            {
                sqlStr += "PALLET = @PALLET,";
            }
            if (!string.IsNullOrEmpty((string)conditions.TRANZONETYPE))
            {
                sqlStr += "TRANZONETYPE = @TRANZONETYPE,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ROTBARRIERAREA))
            {
                sqlStr += "ROTBARRIERAREA = @ROTBARRIERAREA,";
            }
            if (!string.IsNullOrEmpty((string)conditions.ISUPDATEMAP))
            {
                sqlStr += "ISUPDATEMAP = @ISUPDATEMAP,";
            }
            sqlStr += "MAP_ENABLE = @MAP_ENABLE WHERE GUID = @GUID;";
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP,MAP_ENABLE)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP,MAP_ENABLE FROM {0} ", MasterTable);
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
            sqlStr += string.Format(@"INSERT INTO {0}(GUID,NODE_GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP,MAP_ENABLE)", HistoryTable);
            sqlStr += string.Format(@"SELECT UUID(), GUID,MAP_GUID,ID,`NAME`,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,REMARK,XPOS,YPOS,WIDTH,HEIGHT,VALUE,ROADPROPERTY,ROT,ALLDIRROT,ALLDIR,ELEDIR,ELEPRE,ROTRAD,ROBOTTYPE,ROTUNDERPOD,ROTMECH,EVIT,ROTBYROBOTTYPE,ROTFORPODTYPE,SENSORSWITCHPOINT,PALLET,TRANZONETYPE,ROTBARRIERAREA,ISUPDATEMAP,MAP_ENABLE FROM {0} ", MasterTable);
            sqlStr += $@"WHERE GUID=@GUID;";
            return sqlStr;
        }

        /// <summary>
        /// 取得地碼
        /// </summary>
        /// <returns></returns>
        public string GetQrCode()
        {
            string sqlStr = $"SELECT * FROM {MasterTable} WHERE QRCODE = @QRCODE;";
            return sqlStr;
        }
    }
}