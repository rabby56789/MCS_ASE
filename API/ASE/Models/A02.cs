using JQWEB.Models;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ASE.Models
{
    /// <summary>
    /// 取得儲位狀態
    /// </summary>
    public class A02 : IModel
    {
        string Storage { get { return "t_storage"; } }
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public JObject Run(dynamic parm)
        {
            if (A04.A04isRun == true)
            {
                log.Debug($"A02 Function parameter: {parm}  A04isRun : {A04.A04isRun} ");
                throw new Exception($"A04執行中，請稍後");
            }
            log.Debug($"A02 Function parameter: {parm}");

            JObject returnVal = new JObject();
            IDAO dao = new DAO();

            //正式流程
            string sqlStr = $"SELECT status AS loc_status,carrier_id AS car_no ";
            sqlStr += $"FROM {Storage} ";
            sqlStr += "WHERE enable = 1 AND ID = @location;";

            dao.AddExecuteItem(sqlStr, new MySqlParameter[] {
                new MySqlParameter("@location",(string)parm.location)
            });
            log.Debug("A02 / parm : " + parm);
            var result = dao.Query().Tables[0];

            if (result.Rows.Count > 0)
            {
                returnVal.Add("loc_status", ConvertStatus((string)result.Rows[0]["loc_status"]));
                returnVal.Add("car_no", (string)result.Rows[0]["car_no"]);
            }
            log.Debug("A02 / parm : " + parm);
            log.Debug("A02 / returnVal : " + returnVal);
            return returnVal;
        }

        //狀態轉換
        public string ConvertStatus(string input)
        {
            string returnVal = "";

            switch (input)
            {
                case "A":
                    returnVal = "物料已送達";
                    break;
                case "B":
                    returnVal = "運送中";
                    break;
                case "C":
                    returnVal = "待AGV作業";
                    break;
                case "D":
                    returnVal = "空儲位";
                    break;
                case "E":
                    returnVal = "閒置";
                    break;
                case "W1":
                    returnVal = "裝箱中";
                    break;
                case "W2":
                    returnVal = "卸貨中";
                    break;
                case "W3":
                    returnVal = "作業中";
                    break;
                default:
                    break;
            }

            return returnVal;
        }
    }
}
