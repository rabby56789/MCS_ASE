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
    public class AGVStatusController : ApiController
    {
        public JObject ReadAGVStatus()
        {            
            DAO dao = new DAO();
            JObject result = new JObject();
            DataTableExtensions extensions = new DataTableExtensions();
            string sqlAGVStatus = $@"
            select ROBOTCODE,BATTERY,ASE_START_LOC,ASE_TARGET_LOC,TASKCODE,ASE_JOB_NAME,tagv.STATUS AS AGV_STATUS,MAPCODE 
            from t_agv_status tagv
            left join t_subtask_status ts on ts.AGVCODE = tagv.ROBOTCODE and ts.JOB_STATUS in (0,1)";
            dao.AddExecuteItem(sqlAGVStatus,null);
            var data = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            //result = agvCallbackServiceController.AGVstatus;
            //result.Add("row",agvCallbackServiceController.AGVstatus);
            //result.Add("row",agvCallbackServiceController.AGVstatus);
            return data;
        }
        // GET: api/AGVStatus
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/AGVStatus/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/AGVStatus
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/AGVStatus/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/AGVStatus/5
        public void Delete(int id)
        {
        }
        [HttpPost]
        public JObject GetParamsByFunction()
        {
            DAO dao = new DAO();
            JObject result = new JObject();
            string sqlGetRefreshTime = $"select APIMethod,Hour,Minute,Second from base_timer where APIMethod = 'AgvStatus'";
            dao.AddExecuteItem(sqlGetRefreshTime, null);
            var data = dao.Query().Tables[0];
            if (data.Rows.Count > 0)
            {
                result.Add("Hour", data.Rows[0]["Hour"].ToString());
                result.Add("Minute", data.Rows[0]["Minute"].ToString());
                result.Add("Second", data.Rows[0]["Second"].ToString());
            }
            return result;
        }
    }
}
