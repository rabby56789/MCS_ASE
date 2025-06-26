using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MCS.Models
{
    /// <summary>
    /// 存取RCS 2000 DB
    /// </summary>
    public class RCS2000 : ISqlCreator
    {
        public string Status { get { return "t_agv_status"; } }
        public string Path { get { return "t_agv_path"; } }

        public IDataParameter[] CreateParameterAry(JObject input)
        {
            List<MySqlParameter> parmList = new List<MySqlParameter>();
            string[] numType = { "inserttime" };

            if (input is null)
            {
                return new IDataParameter[] { };
            }

            //JSON項目逐一加入參數表中
            foreach (var x in input)
            {
                MySqlParameter parm = new MySqlParameter();
                string name = x.Key;
                JToken value = x.Value;

                parm.ParameterName = "@" + name;
                parm.Value = value;

                if (numType.Contains(name))
                {
                    parm.MySqlDbType = MySqlDbType.Int32;
                    parm.DbType = DbType.Int32;
                }
                else
                {
                    parm.MySqlDbType = MySqlDbType.VarChar;
                    parm.DbType = DbType.String;
                }

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        public string GetSqlStr(string actionName, JObject parm)
        {
            switch (actionName)
            {
                case "UpdateAgvStatus":
                    return UpdateAgvStatus();
                case "GetOnLineAgvCount":
                    return GetOnLineAgvCount();
                default:
                    return "";
            }
        }

        /// <summary>
        /// 更新AGV及時狀態
        /// </summary>
        /// <returns></returns>
        public string UpdateAgvStatus()
        {
            string sqlStr = string.Empty;
            sqlStr = $"INSERT {Status} (GUID,UPDATE_TIME,battery,exclType,mapCode,source,target,podCode,podDir,posX,posY,robotCode,robotDir,robotIp,speed,status,stop,timestamp)";
            sqlStr += "VALUE (UUID(),now(),@battery,@exclType,@mapCode,@source,@target,@podCode,@podDir,@posX,@posY,@robotCode,@robotDir,@robotIp,@speed,@status,@stop,@timestamp)";
            sqlStr += "ON DUPLICATE KEY UPDATE UPDATE_TIME = now(),battery = @battery,exclType = @exclType,mapCode = @mapCode,source = @source,target = @target,podCode = @podCode,podDir = @podDir,posX = @posX,posY = @posY,robotCode = @robotCode,robotDir = @robotDir,robotIp = @robotIp,speed = @speed,status = @status,stop = @stop,timestamp = @timestamp ";

            return sqlStr;
        }

        /// <summary>
        /// 取得及時連線RCS的AGV數量
        /// </summary>
        /// <returns></returns>
        public string GetOnLineAgvCount()
        {
            string sqlStr = string.Empty;
            sqlStr = $"SELECT COUNT(DISTINCT robotCode) AS count FROM {Status} WHERE ";
            sqlStr += "UPDATE_TIME > DATE_SUB(now(), INTERVAL 2 MINUTE) AND ";
            sqlStr += "UPDATE_TIME < DATE_ADD(now(), INTERVAL 2 MINUTE);";

            return sqlStr;
        }
    }
}