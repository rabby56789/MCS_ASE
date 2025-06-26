using JQWEB.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace ASE.Models
{
    public static class Global
    {
        /// <summary>
        /// 儲存API紀錄
        /// </summary>
        /// <param name="API_HOST_URL">API URL路徑</param>
        /// <param name="API_ACTION_NAME">API Function名稱</param>
        /// <param name="SEND_DATA">由Client端接收到的資料</param>
        public static void SaveApiLog(string API_HOST_URL,string API_ACTION_NAME,string SEND_DATA,string RESPONSE_DATA)
        {
            IDAO dao = new DAO();
            LOG log = new LOG();
            JObject obj = new JObject();

            obj.Add("API_HOST_URL", API_HOST_URL);
            obj.Add("API_ACTION_NAME", API_ACTION_NAME);
            obj.Add("SEND_DATA", SEND_DATA);
            obj.Add("RESPONSE_DATA", RESPONSE_DATA);

            var sqlStr = log.GetSqlStr("InsertToApiLog");
            var parm = log.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, parm);
            dao.Execute();
        }
    }
}
