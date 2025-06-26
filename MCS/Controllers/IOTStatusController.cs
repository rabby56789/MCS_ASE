using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JQWEB.Models;

namespace MCS.Controllers
{
    public class IOTStatusController : ApiController
    {
        public JObject ReadIOTStatus()
        {            
            DAO dao = new DAO();
            JObject result = new JObject();
            DataTableExtensions extensions = new DataTableExtensions();
            string sqlIOTStatus = $@"
            select MACHINE_TITLE,SNKEY,AREA,IP,INTERNET,MQ_DI,MQ_DATETIME from base_iot_status";
            dao.AddExecuteItem(sqlIOTStatus,null);
            var data = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            //result = agvCallbackServiceController.AGVstatus;
            //result.Add("row",agvCallbackServiceController.AGVstatus);
            //result.Add("row",agvCallbackServiceController.AGVstatus);
            return data;
        }
        [HttpPost]
        public JObject GetParamsByFunction()
        {
            DAO dao = new DAO();
            JObject result = new JObject();
            string sqlGetRefreshTime = $"select APIMethod,Hour,Minute,Second from base_timer where APIMethod = 'EqptStatus'";
            dao.AddExecuteItem(sqlGetRefreshTime, null);
            var data = dao.Query().Tables[0];
            if (data.Rows.Count > 0)
            {
                result.Add("Hour",data.Rows[0]["Hour"].ToString());
                result.Add("Minute", data.Rows[0]["Minute"].ToString());
                result.Add("Second", data.Rows[0]["Second"].ToString());
            }
            return result;
        }
        // GET: api/IOTStatus
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/IOTStatus/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/IOTStatus
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/IOTStatus/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/IOTStatus/5
        public void Delete(int id)
        {
        }
    }
}
