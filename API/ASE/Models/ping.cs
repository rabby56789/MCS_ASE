using JQWEB.Models;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ASE.Models
{
    public class ping : IModel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public JObject Run(dynamic obj)
        {

            //string job_seq = obj["job_seq"].ToString();

            


            Task sqlCreator = new Task();
            DAO dao = new DAO();
            var sqlStruuid = sqlCreator.GetUUID();
            dao.AddExecuteItem(sqlStruuid, null);
            var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString().Replace("-", "");
            //var sqlStr = sqlCreator.GetTASKCODE();

            //while (true)
            //{
            //    try { Thread.Sleep(30000); break; } catch (Exception exception) { log.Error("Sleep exception", exception); }
            //}


            //Subtask
            string SubtaskTable = "T_SUBTASK_STATUS";
            var sqlSub = $"SELECT GUID, JOB_STATUS FROM {SubtaskTable} ";
            dao.AddExecuteItem(sqlSub, null);
            var dataSub = dao.Query().Tables[0];
            var count = dataSub.Rows.Count;

            JObject returnMsg = new JObject();

            returnMsg.Add("Count",count);
            returnMsg.Add("IsSuccess", true);

            return returnMsg;
        }
        
    }
    //class TaskException : Exception, ISerializable
    //{
    //    public TaskException()
    //        : base("show message") { }
    //    public TaskException(string message)
    //        : base(message) { }
    //    public TaskException(string message, Exception inner)
    //        : base(message, inner) { }
    //    protected TaskException(SerializationInfo info, StreamingContext context)
    //        : base(info, context) { }
    //}
}
