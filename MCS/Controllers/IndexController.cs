using JQWEB.Models;
using MCS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.Web.Http;

namespace MCS.Controllers
{
    public class IndexController : ApiController
    {
        /// <summary>
        /// 取得目前與RCS2000連線的AGV數量
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetOnLineAgvCount(JObject obj)
        {
            RCS2000 sqlCreator = new RCS2000();
            IDAO dao = new DAO();
            var returnVal = new JObject();

            var sqlStr = sqlCreator.GetOnLineAgvCount();

            dao.AddExecuteItem(sqlStr, null);

            var result = dao.Query().Tables[0];

            if (result != null)
            {
                returnVal.Add("count", result.Rows[0][0].ToString());
            }

            return returnVal;
        }

        /// <summary>
        /// 取得RCS系統已定義的AGV數量
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetDefinedAgvCount(JObject obj)
        {
            //Carrier sqlCreator = new Carrier();
            IDAO dao = new DAO();
            var returnVal = new JObject();

            //var sqlStr = sqlCreator.Search(obj, true);
            var sqlStr = $"select count(*) from t_agv_status ";
            //var sqlParms = sqlCreator.CreateParameterAry(obj);


            dao.AddExecuteItem(sqlStr, null);

            var result = dao.Query().Tables[0];

            if (result != null)
            {
                returnVal.Add("count", result.Rows[0][0].ToString());
            }

            return returnVal;
        }

        /// <summary>
        /// 今日任務總數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetTodayTaskCount(JObject obj)
        {
            var returnVal = new JObject();
            //TaskBuild task = new TaskBuild();  //主任務
            //TaskList subTask = new TaskList();  //子任務 20221227 Ruby 現在任務都用subtask計算
            TaskOnline subTask = new TaskOnline();  //子任務 20221227 Ruby 現在任務都用t_subtask_status計算
            IDAO MySQL = new DAO();

            //MySQL.AddExecuteItem(task.Search(obj, true), task.CreateParameterAry(obj));
            MySQL.AddExecuteItem(subTask.Search(obj, true), subTask.CreateParameterAry(obj));

            DataSet ds = MySQL.Query();
            //int total = int.Parse(ds.Tables[0].Rows[0][0].ToString()) + int.Parse(ds.Tables[1].Rows[0][0].ToString());
            int total = int.Parse(ds.Tables[0].Rows[0][0].ToString());

            returnVal.Add("count", total);

            return returnVal;
        }

        /// <summary>
        /// 統計當日已完成任務
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject GetTodayCompleteTaskCount(JObject obj)
        {
            var returnVal = new JObject();
            //TaskBuild task = new TaskBuild();  //主任務
            //TaskList subTask = new TaskList();  
            TaskOnline subTask = new TaskOnline();  //子任務 20221227 Ruby 現在任務都用t_subtask_status計算
            IDAO MySQL = new DAO();

            //MySQL.AddExecuteItem(task.Search(obj, true), task.CreateParameterAry(obj));
            MySQL.AddExecuteItem(subTask.Search(obj, true), subTask.CreateParameterAry(obj));

            DataSet ds = MySQL.Query();
            //int total = int.Parse(ds.Tables[0].Rows[0][0].ToString()) + int.Parse(ds.Tables[1].Rows[0][0].ToString());
            int total = int.Parse(ds.Tables[0].Rows[0][0].ToString());

            returnVal.Add("count", total);

            return returnVal;
        }

        /// <summary>
        /// 統計當日發生Alarm總數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetTodayAlarmCount(JObject obj)
        {
            var returnVal = new JObject();

            Alarm alarm = new Alarm();
            IDAO MySQL = new DAO();

            MySQL.AddExecuteItem(alarm.Search(obj, true), alarm.CreateParameterAry(obj));

            int total = int.Parse(MySQL.Query().Tables[0].Rows[0][0].ToString());

            returnVal.Add("count", total);

            return returnVal;
        }

        /// <summary>
        /// 取得統計圖資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject GetChartData(JObject obj)
        {
            var returnVal = new JObject();

            IndexChart sqlCreator = new IndexChart();
            DataTableExtensions extensions = new DataTableExtensions();
            IDAO MySQL = new DAO();

            var now = DateTime.Now;

            //前七天 ~ 前一天
            var end_day = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            end_day = end_day.AddDays(-1);
            var start_day=end_day.AddDays(-6);
            start_day = new DateTime(start_day.Year, start_day.Month, start_day.Day, 0, 0, 0);

            obj.Add("start_day", start_day.ToString("yyyy-MM-dd HH:mm:ss"));
            obj.Add("end_day", end_day.ToString("yyyy-MM-dd HH:mm:ss"));

            MySQL.AddExecuteItem(sqlCreator.Search(obj, false), sqlCreator.CreateParameterAry(obj));

            returnVal.Add("data", extensions.ConvertDataTableToJObject(MySQL.Query().Tables[0]).GetValue("rows"));

            return returnVal;
        }
    }
}