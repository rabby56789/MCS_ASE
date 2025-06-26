using JQWEB.Controllers;
using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;

namespace MCS.Controllers
{
    public class ApiWarnCallbackController : ApiController, IJqOneTable 
    { 
        [System.Web.Http.HttpPost]
        public JObject WarnCallback(WarnCallbackModel valuen)
        {

            //InsertWarn status = new InsertWarn();
            JObject result = new JObject();

            try
            {
              //  status = JsonConvert.DeserializeObject<InsertWarn>((string)valuen);
                //status = JsonConvert.DeserializeObject<InsertWarn>(jsonResult);

            }
            catch
            {
                result.Add("code", "1");
                result.Add("message", "参数相关的错误");
                result.Add("reqCode", valuen.reqCode);
                return result;
            }

            try
            {
                if (valuen.reqCode == null || valuen.data == null)
                {
                    result.Add("code", "1");
                    result.Add("message", "参数相关的错误");
                    result.Add("reqCode", valuen.reqCode);
                    return result;
                }

                WarnCallback sqlCreator = new WarnCallback();
                DAO dao = new DAO();
                for (int i = 0; i < valuen.data.Length; i++)
                {
                    //目前只能輸入第一筆 ????
                    JObject parm = new JObject();
                    parm.Add("INSERT_USER", valuen.clientCode);
                    parm.Add("REQCODE", valuen.reqCode);
                    parm.Add("AGVCODE", valuen.data[i].agvCode);
                    parm.Add("BEGINTIME", valuen.data[i].beginTime);
                    parm.Add("WARNCONTENT", valuen.data[i].warnContent);
                    parm.Add("TASKCODE", valuen.data[i].taskCode);
                    var sqlParms = sqlCreator.CreateParameterAry(parm);
                    var sqlStruuid = sqlCreator.GetUUID();
                    dao.AddExecuteItem(sqlStruuid, sqlParms);
                    var uuid = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();
                    var sqlStr = sqlCreator.Insert(uuid);
                    dao.AddExecuteItem(sqlStr, sqlParms);
                   

                }
                dao.Execute();
            }
            catch (Exception ex)
            {
                result.Add("code", "1");
                result.Add("message", "参数相关的错误");
                result.Add("reqCode", valuen.reqCode);
                return result;
            }

            result.Add("code", "0");
            result.Add("message", "成功");
            result.Add("reqCode", valuen.reqCode);
            return result;

        }

        public JObject Count(JObject obj)
        {
            return obj;
        }

        
        public JObject Query(JObject obj)
        {
            return obj;
        }

        public JObject Insert(JObject obj)
        {
            return obj;
        }
        public JObject Update(JObject obj)
        {
            return obj;
        }
        public JObject Delete(JObject obj)
        {
            return obj;
        }

        public JObject Import(JObject obj)
        {
            return obj;
        }

        public JObject Export(JObject obj)
        {
            return obj;
        }

        public JObject GetOneByGUID(JObject obj)
        {
            return obj;
        }
    }
}