using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MCS.Models
{
    /// <summary>
    /// 資料存取物件範例
    /// </summary>
    public class Iotdevice : ISqlCreator
    {
        /// <summary>
        /// 存取子:主表名稱
        /// </summary>
        public string ATable { get { return "base_iotdevice"; } }
        public string ATable_HISTORY { get { return "base_iotdevice_history"; } }
        public string BTable { get { return "base_iotdevice_detail"; } }
        public string BTable_HISTORY { get { return "base_iotdevice_detail_history"; } }

        /// <summary>
        /// 丟JSON物件,回傳SQL具名參數陣列
        /// </summary>
        /// <param name="input"></param>
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
                parm.Value = value;
                //指定參數資料型態
                if (name == "DATA_STATUS" || name == "AUTHORITY_STATUS")
                {
                    parm.DbType = System.Data.DbType.Int32;
                }
                else
                {
                    parm.DbType = System.Data.DbType.String;
                }

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        /// <summary>
        /// 取得SQL字串通用介面
        /// </summary>
        /// <param name="actionName">動作名稱</param>
        /// <param name="parm">傳入參數(選擇性)</param>
        /// <returns></returns>
        public string GetSqlStr(string actionName, JObject parm)
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
                case "GetOneByGUID":
                    sqlStr = GetBOneByGUID();
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

                case "CountA":
                    sqlStr = SearchA(parm, true);
                    break;
                case "QueryA":
                    sqlStr = SearchA(parm, false);
                    break;
                case "GetAOneByGUID":
                    sqlStr = GetAOneByGUID();
                    break;
                case "GetBOneByGUID":
                    sqlStr = GetBOneByGUID();
                    break;
                case "AddA":
                    sqlStr = InsertA();
                    break;
                case "EditA":
                    sqlStr = UpdateA();
                    break;
                case "DeleteA":
                    sqlStr = DeleteA();
                    break;
                default:
                    break;

                case "CountBind":
                    sqlStr = QueryBind(parm, true);
                    break;
                case "QueryBind":
                    sqlStr = QueryBind(parm, false);
                    break;
                case "InsertBind":
                    sqlStr = InsertBind();
                    break;
                case "UpdateBind":
                    sqlStr = UpdateBind();
                    break;
                case "DeleteBind":
                    sqlStr = DeleteBind();
                    break;

            }

            return sqlStr;
        }


        /// <summary>
        /// 取得查詢SQL字串
        /// </summary>
        /// <param name="parm">Client端傳入參數</param>
        /// <param name="getCount">是否為查詢數量</param>
        /// <returns></returns>
        string Search(JObject parm, bool getCount)
        {
            return "";
        }

        /// <summary>
        /// 取得查詢SQL字串
        /// </summary>
        /// <param name="parm">Client端傳入參數</param>
        /// <param name="getCount">是否為查詢數量</param>
        /// <returns></returns>
        public string SearchA(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            StringBuilder sb = new StringBuilder();
            //是否為查詢資料筆數
            if (getCount)
            {
                //sqlStr = string.Format(@"SELECT COUNT(*) 'Count' FROM {0} DEVICE ", ATable);
                sb.AppendFormat(" SELECT COUNT(*) 'Count' FROM {0} DEVICE ",ATable);
                //sb.Append(" LEFT JOIN base_map M ON M.GUID = DEVICE.MAP_GUID ");
            }
            else
            {
                sb.Append(" SELECT  ");                
                sb.Append("     DEVICE.`GUID` ");
                sb.Append("     ,DEVICE.`MAP_GUID` ");
                sb.Append("     ,DEVICE.`NAME` ");

                sb.Append("     ,DEVICE.`IP` ");
                sb.Append("     ,DEVICE.`SN_KEY` ");

                sb.Append("     ,DEVICE.`DI_COUNT` ");
                sb.Append("     ,DEVICE.`DO_COUNT` ");
                sb.Append("     ,DEVICE.`INSERT_USER` ");
                sb.Append("     ,DEVICE.`INSERT_TIME` ");
                sb.Append("     ,DEVICE.`UPDATE_USER` ");
                sb.Append("     ,DEVICE.`UPDATE_TIME` ");
                sb.Append("     ,DEVICE.`ENABLE` ");
                sb.Append("     ,DEVICE.`REMARK` ");
                //sb.Append("     ,M.`NAME` as 'MAP_NAME'  ");
                sb.AppendFormat(" FROM {0} DEVICE ", ATable);
                //sb.Append(" LEFT JOIN base_map M ON M.GUID = DEVICE.MAP_GUID ");
            }
            sb.Append( " WHERE DEVICE.ENABLE = 1 ");

            //SQL加入查詢條件
            //if (!string.IsNullOrEmpty((string)conditions.MAPNAME))
            //{
            //    sb.Append("AND M.`NAME` LIKE CONCAT(@MAPNAME , '%') ");
            //}
            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sb.Append("AND locate(@NAME,DEVICE.`NAME`) > 0 ");
            }

            if (!string.IsNullOrEmpty((string)conditions.SN_KEY))
            {
                sb.Append("AND locate(@SN_KEY,DEVICE.`SN_KEY`) > 0 ");
            }

            if (!string.IsNullOrEmpty((string)conditions.IP))
            {
                sb.Append("AND locate(@IP,DEVICE.`IP`) > 0 ");
            }

            //查詢數量不需換頁
            if (getCount)
            {
                return sb.ToString();
            }

            if (parm.TryGetValue("sort", out _))
            {
                string sortString = TransferReservedWord((string)conditions.sort).Trim();
                
                if (sortString.Equals("MAP_NAME"))
                {
                    sb.AppendFormat(" ORDER BY M.NAME {0} ", (string)conditions.order);
                }
                else
                {
                    sb.AppendFormat(" ORDER BY DEVICE.{0} {1} ", sortString, (string)conditions.order);
                }

            }
            //含排序 or 換頁
            if (parm.TryGetValue("page", out _))
            {
                int offset = (int)conditions.rows * ((int)conditions.page - 1);
                sb.AppendFormat("LIMIT {0} ", conditions.rows);
                sb.AppendFormat("OFFSET {0}", offset);
            }
            sb.Append(";");


            return sb.ToString();
        }

        public string SearchIOT_STATUS(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;

            //是否為查詢資料筆數
            if (getCount)
            {
                sqlStr = "select count(*) from base_iot_status ";
            }
            else
            {
                sqlStr = "select snkey,area,areasn,machine_title,ip from base_iot_status ";
            }

            sqlStr += "where snkey not in (select sn_key from base_iotdevice where enable = 1 ) ";

            //snkey
            if (!string.IsNullOrEmpty((string)conditions.SN_KEY))
            {
                sqlStr += "and snkey like concat(@SN_KEY, '%') ";
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

        private string Insert()
        {
            throw new NotImplementedException();
        }
        private string InsertA()
        {
            throw new NotImplementedException();
        }

        public string Insert(string uuid)
        {
            throw new NotImplementedException();
        }

        public string InsertA(string uuid)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" INSERT INTO {0} ( ", ATable);
            sb.Append("     GUID ");
            //sb.Append("     ,MAP_GUID ");
            sb.Append("     ,NAME ");
            sb.Append("     ,SN_KEY ");
            sb.Append("     ,IP ");
            sb.Append("     ,ERROR_STATE ");
            //sb.Append("     ,DI_COUNT ");
            //sb.Append("     ,DO_COUNT ");
            sb.Append("     ,REMARK ");
            sb.Append("     ,INSERT_USER ");
            sb.Append("     ,INSERT_TIME ");
            sb.Append(" ) VALUES ( ");
            sb.AppendFormat(" '{0}' ", uuid);
            //sb.Append("     ,@MAP_GUID ");
            sb.Append("     ,@NAME ");
            sb.Append("     ,@SN_KEY ");
            sb.Append("     ,@IP ");
            sb.Append("     ,'' ");
            //sb.Append("     ,@DI_COUNT ");
            //sb.Append("     ,@DO_COUNT ");
            sb.Append("     ,@REMARK ");
            sb.Append("     ,@INSERT_USER ");
            sb.Append("     ,now() ");
            sb.Append(" ); ");

            //history
            sb.AppendFormat(" INSERT INTO {0}( ", ATable_HISTORY);
            sb.Append("     `GUID`,`MAP_GUID`,`NAME`,SN_KEY,IP,ERROR_STATE,`DI_COUNT`,`DO_COUNT`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` ");
            sb.Append(" ) ");
            sb.Append(" SELECT  ");
            sb.Append("     `GUID`,`MAP_GUID`,`NAME`,SN_KEY,IP,ERROR_STATE,`DI_COUNT`,`DO_COUNT`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`  ");
            sb.AppendFormat(" FROM {0} ", ATable);
            sb.AppendFormat(" WHERE GUID='{0}'; ", uuid);


            return sb.ToString();
        }

        public string Update()
        {
            return null;
        }
        public string UpdateA()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" UPDATE {0} SET ", ATable);
            //sb.Append("     MAP_GUID = @MAP_GUID, ");
            sb.Append("     NAME = @NAME, ");
            sb.Append("     SN_KEY = @SN_KEY, ");
            sb.Append("     IP = @IP, ");
            //sb.Append("     ERROR_STATE = @ERROR_STATE, ");
            //sb.Append("     DI_COUNT = @DI_COUNT, ");
            //sb.Append("     DO_COUNT = @DO_COUNT, ");
            sb.Append("     REMARK = @REMARK, ");
            //sb.Append("     REMARK = @REMARK, ");
            sb.Append("     UPDATE_USER = @UPDATE_USER, ");
            sb.Append("     UPDATE_TIME = now()");
            sb.Append(" WHERE GUID = @GUID;");
                        

            ///history
            //sb.AppendFormat(" INSERT INTO {0}( ", ATable_HISTORY);
            //sb.Append("     `GUID`,`MAP_GUID`,`NAME`,SN_KEY,IP,ERROR_STATE,`DI_COUNT`,`DO_COUNT`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` ");
            //sb.Append(" ) ");
            //sb.Append(" SELECT  ");
            //sb.Append("     `GUID`,`MAP_GUID`,`NAME`,SN_KEY,IP,ERROR_STATE,`DI_COUNT`,`DO_COUNT`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`  ");
            //sb.AppendFormat(" FROM {0} ", ATable);
            //sb.AppendFormat(" WHERE GUID=@GUID; ");

            return sb.ToString();
        }

        public string GetOneByGUID()
        {
            string sqlStr = string.Format(@"SELECT DEVICE.`GUID`,DEVICE.`MAP_GUID`,DEVICE.`NAME`,DEVICE.`SN_KEY`,DEVICE.`IP`,DEVICE.`DI_COUNT`,DEVICE.`DO_COUNT`,DEVICE.`REMARK`,M.`NAME` as 'MAP_NAME' FROM {0} DEVICE", ATable);

            sqlStr += " INNER JOIN base_map M ON M.GUID = DEVICE.MAP_GUID ";
            sqlStr += "WHERE DEVICE.ENABLE = 1 AND DEVICE.GUID = @GUID";

            return sqlStr;
        }
        public string GetAOneByGUID()
        {
            //StringBuilder sb = new StringBuilder();
            //sb.Append(" SELECT ");
            //sb.Append("     DEVICE.`GUID`");
            //sb.Append("     ,DEVICE.`MAP_GUID`");
            //sb.Append("     ,DEVICE.`NAME`");
            //sb.Append("     ,DEVICE.`DI_COUNT`");
            //sb.Append("     ,DEVICE.`DO_COUNT`");
            //sb.Append("     ,DEVICE.`REMARK`");
            //sb.Append("     ,M.`NAME` as 'MAP_NAME' ");
            //sb.AppendFormat(" FROM {0} DEVICE", ATable);
            //sb.Append(" INNER JOIN base_map M ON M.GUID = DEVICE.MAP_GUID ");
            //sb.Append(" WHERE DEVICE.ENABLE = 1 AND DEVICE.GUID = @GUID ");

            String sqlStr = $"select * from {ATable} where enable = 1 and guid = @GUID";

            return sqlStr;
        }

        public string GetBOneByGUID()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" SELECT ");
            sb.Append("     DDetail.`GUID`,");
            sb.Append("     DDetail.SN_KEY,");
            sb.Append("     DDetail.`TYPE`,");
            sb.Append("     DDetail.`INDEX`,");
            sb.Append("     DDetail.`SIGNAL_0`,");
            sb.Append("     DDetail.`SIGNAL_1`,");
            sb.Append("     DDetail.`INSERT_USER`,");
            sb.Append("     DDetail.`INSERT_TIME`,");
            sb.Append("     DDetail.`UPDATE_USER`,");
            sb.Append("     DDetail.`UPDATE_TIME`,");
            sb.Append("     DDetail.`ENABLE`,");
            sb.Append("     DDetail.`REMARK`");
            sb.AppendFormat(" FROM {0} DDetail", BTable);
            sb.Append(" WHERE DDetail.ENABLE = 1 ");
            sb.Append(" AND DDetail.GUID = @GUID ");

            return sb.ToString();
        }


        public string GetUUID()
        {
            string sqlStr = "SELECT UUID() from dual;";

            return sqlStr;
        }

        public string Delete()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" UPDATE {0} SET ", ATable);
            sb.Append(" ENABLE = 0,");
            sb.Append(" UPDATE_USER = @UPDATE_USER,");
            sb.Append(" UPDATE_TIME = now() ");
            sb.Append(" WHERE GUID = @GUID;");

            return sb.ToString();
        }
        public string DeleteA()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" UPDATE {0} SET ", ATable);
            sb.Append("     ENABLE = 0, ");
            sb.Append("     UPDATE_USER = @UPDATE_USER, ");
            sb.Append("     UPDATE_TIME = now()  ");
            sb.Append(" WHERE GUID = @GUID; ");

            //Bind 連動刪除
            //sb.AppendFormat(" UPDATE {0} SET ", BTable);
            //sb.Append("     ENABLE = 0, ");
            //sb.Append("     UPDATE_USER = @UPDATE_USER, ");
            //sb.Append("     UPDATE_TIME = now()  ");
            //sb.Append(" WHERE IOTDEVICE_GUID = @GUID; ");

            ///history
            //sb.AppendFormat(" INSERT INTO {0}( ", ATable_HISTORY);
            //sb.Append("     `GUID`,`MAP_GUID`,SN_KEY,`NAME`,IP,`DI_COUNT`,`DO_COUNT`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` ");
            //sb.Append(" ) ");
            //sb.Append(" SELECT  ");
            //sb.Append("     `GUID`,`MAP_GUID`,SN_KEY,`NAME`,IP,`DI_COUNT`,`DO_COUNT`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`  ");
            //sb.AppendFormat(" FROM {0} ", ATable);
            //sb.AppendFormat(" WHERE GUID=@GUID; ");

            ////history 2
            //sb.AppendFormat(" INSERT INTO {0}( ", BTable_HISTORY);
            //sb.Append("     `GUID`,SN_KEY,`TYPE`,`INDEX`,`SIGNAL_0`,`SIGNAL_1`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` ");
            //sb.Append(" ) ");
            //sb.Append(" SELECT  ");
            //sb.Append("     `GUID`,`SN_KEY`,`TYPE`,`INDEX`,`SIGNAL_0`,`SIGNAL_1`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`  ");
            //sb.AppendFormat(" FROM {0} ", BTable);
            //sb.Append(" WHERE IOTDEVICE_GUID = @GUID; ");

            return sb.ToString();
        }

        public string GetListOfMap()
        {
            string sqlStr = string.Format(@"SELECT GUID AS 'Key', ID , NAME AS 'Value' FROM {0} D ", "base_map");
            sqlStr += "WHERE D.ENABLE = 1 ";
            return sqlStr;
        }

        public string QueryBind(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            StringBuilder sb = new StringBuilder();

            //是否為查詢資料筆數
            if (getCount)
            {
                sb.Append(" SELECT COUNT(*) 'Count' ");
                sb.AppendFormat(" FROM {0} Device", ATable);
                sb.AppendFormat(" LEFT JOIN {0} DDetail", BTable);
                sb.Append("     ON Device.SN_KEY = DDetail.SN_KEY");
            }
            else
            {
                sb.Append(" SELECT ");
                sb.Append("     DDetail.`GUID`,");
                sb.Append("     DDetail.`SN_KEY`,");
                sb.Append("     DDetail.`TYPE`,");
                sb.Append("     DDetail.`INDEX`,");
                sb.Append("     DDetail.`SIGNAL_0`,");
                sb.Append("     DDetail.`SIGNAL_1`,");
                sb.Append("     DDetail.`INSERT_USER`,");
                sb.Append("     DDetail.`INSERT_TIME`,");
                sb.Append("     DDetail.`UPDATE_USER`,");
                sb.Append("     DDetail.`UPDATE_TIME`,");
                sb.Append("     DDetail.`ENABLE`,");
                sb.Append("     DDetail.`REMARK`");
                sb.AppendFormat(" FROM {0} Device", ATable);
                sb.AppendFormat(" LEFT JOIN {0} DDetail", BTable);
                sb.Append("     ON Device.SN_KEY = DDetail.SN_KEY");
            }
            sb.Append(" WHERE DDetail.ENABLE = 1 ");
            sb.Append("AND DDetail.SN_KEY = @SN_KEY ");

            ////SQL加入查詢條件
            if (parm.TryGetValue("sort", out _))
            {
                string sortString = TransferReservedWord((string)conditions.sort).Trim() ;
                sb.AppendFormat("ORDER BY DDetail.{0} {1} ", sortString, (string)conditions.order);               
               
            }


            return sb.ToString();
        }

        public string InsertBind()
        {
            return null;
        }
        public string InsertBind(string uuid)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" INSERT INTO {0}", BTable);
            sb.Append(" ( ");
            sb.Append("     `GUID`,");
            sb.Append("     `SN_KEY`,");
            sb.Append("     `TYPE`,");
            sb.Append("     `INDEX`,");
            sb.Append("     `SIGNAL_0`,");
            sb.Append("     `SIGNAL_1`,");
            sb.Append("     `INSERT_USER`,");
            sb.Append("     `INSERT_TIME`,");
            sb.Append("     `REMARK`");
            sb.Append(" ) ");
            sb.Append(" VALUES ");
            sb.Append(" ( ");
            sb.AppendFormat(" '{0}', ", uuid);
            sb.Append("     @SN_KEY, ");
            sb.Append("     @TYPE, ");
            sb.Append("     @INDEX, ");
            sb.Append("     @SIGNAL_0, ");
            sb.Append("     @SIGNAL_1, ");
            sb.Append("     @INSERT_USER, ");
            sb.Append("     now(), ");
            sb.Append("     @REMARK ");
            sb.Append(" ); ");


            sb.AppendFormat(" INSERT INTO {0}( ", BTable_HISTORY);
            sb.Append("     `GUID`,`SN_KEY`,`TYPE`,`INDEX`,`SIGNAL_0`,`SIGNAL_1`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` ");
            sb.Append(" ) ");
            sb.Append(" SELECT  ");
            sb.Append("     `GUID`,`SN_KEY`,`TYPE`,`INDEX`,`SIGNAL_0`,`SIGNAL_1`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`  ");
            sb.AppendFormat(" FROM {0} ", BTable);
            sb.AppendFormat(" WHERE GUID='{0}'; ", uuid);

            return sb.ToString();
        }

        
        public string UpdateBind()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" UPDATE {0} SET ", BTable);
            sb.Append(" `TYPE` = @TYPE, ");
            sb.Append(" `INDEX` = @INDEX, ");
            sb.Append(" `SIGNAL_0` = @SIGNAL_0, ");
            sb.Append(" `SIGNAL_1` = @SIGNAL_1, ");
            sb.Append(" `REMARK` = @REMARK, ");
            sb.Append(" `UPDATE_USER` = @UPDATE_USER, ");
            sb.Append(" `UPDATE_TIME` = now()  ");
            sb.Append(" WHERE GUID = @GUID; ");


            sb.AppendFormat(" INSERT INTO {0}( ", BTable_HISTORY);
            sb.Append("     `GUID`,`SN_KEY`,`TYPE`,`INDEX`,`SIGNAL_0`,`SIGNAL_1`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` ");
            sb.Append(" ) ");
            sb.Append(" SELECT  ");
            sb.Append("     UUID(),`SN_KEY`,`TYPE`,`INDEX`,`SIGNAL_0`,`SIGNAL_1`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`  ");
            sb.AppendFormat(" FROM {0} ", BTable);
            sb.Append(" WHERE GUID = @GUID; ");

            return sb.ToString();
        }

        public string DeleteBind()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" UPDATE {0} SET  ", BTable);
            sb.Append(" ENABLE = 0, ");
            sb.Append(" UPDATE_USER = @UPDATE_USER, ");
            sb.Append(" UPDATE_TIME = now()  ");
            sb.Append(" WHERE GUID = @GUID; ");

            sb.AppendFormat(" INSERT INTO {0}( ", BTable_HISTORY);
            sb.Append("     `GUID`,`SN_KEY`,`TYPE`,`INDEX`,`SIGNAL_0`,`SIGNAL_1`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK` ");
            sb.Append(" ) ");
            sb.Append(" SELECT  ");
            sb.Append("     UUID(),`SN_KEY`,`TYPE`,`INDEX`,`SIGNAL_0`,`SIGNAL_1`,`INSERT_USER`,`INSERT_TIME`,`UPDATE_USER`,`UPDATE_TIME`,`ENABLE`,`REMARK`  ");
            sb.AppendFormat(" FROM {0} ", BTable);
            sb.Append(" WHERE GUID = @GUID; ");

            return sb.ToString();
        }

        /// <summary>
        /// 轉換保留字 增加 `[string]`
        /// </summary>
        /// <param name="v">欄位值</param>
        /// <returns></returns>
        private string TransferReservedWord(string v)
        {
            string result = v;
            switch (v.Trim().ToUpper())
            {
                case "FUNCTION":
                    result = "`" + v + "`";
                    break;
                default:
                    break;
            }
            return result;
        }

    }
}