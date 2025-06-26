using JQWEB.Models;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Web;

namespace ASE.Models
{
    /// <summary>
    /// AGV系統的Raw Data回傳給User用來分析使用
    /// </summary>
    public class B03 : IModel
    {
        /// <summary>
        /// 回傳文字檔
        /// </summary>
        public StreamContent file { get; set; }
        public long contentLength { get; set; }

        /// <summary>
        /// 執行方法
        /// </summary>
        /// <param name="parm">帶入參數</param>
        /// <returns></returns>
        public JObject Run(dynamic parm)
        {
            JObject input = parm;
            JObject result = new JObject();
            //DAO dao = new DAO();
            //string sqlStr = string.Empty;
            //DataTable dt;
            //MemoryStream memoryStream = new MemoryStream();
            //TextWriter tw = new StreamWriter(memoryStream);

            //switch (input.Value<int>("data_type"))
            //{
            //    case 1: //任務紀錄(t_subtask_travel)
            //        sqlStr = "SELECT TASKCODE,TASKTYP,POSITIONCODE1,POSITIONCODE2,AGVCODE,PODCODE,PRIORITY,TASKSTATUS,";
            //        sqlStr += "'00:00:00.000' AS 'DEPARTURE_TIME',";
            //        sqlStr += "'00:00:00.000' AS ARRIVAL_TIME,";
            //        sqlStr += "'00:00:00.000' AS COMPLETE_TIME,";
            //        sqlStr += "'0' AS CONSUMING_TIME,";
            //        sqlStr += "'0' AS WAITING_TIME ";
            //        sqlStr += "FROM t_task_travel;";

            //        dao.AddExecuteItem(sqlStr, null);

            //        dt = dao.Query().Tables[0];

            //        foreach (DataRow row in dt.Rows)
            //        {
            //            tw.WriteLine(
            //                row[0].ToString() + "," + row[1].ToString() + "," + row[2].ToString() + "," + row[3].ToString() + "," +
            //                row[4].ToString() + "," + row[5].ToString() + "," + row[6].ToString() + "," + row[7].ToString() + "," +
            //                row[8].ToString() + "," + row[9].ToString() + "," + row[10].ToString() + "," + row[11].ToString() + "," + row[12].ToString()
            //                );
            //        }

            //        break;
            //    case 2: //AGV任務次數記錄(t_task_travel)
            //        sqlStr = "SELECT AGVCODE,REQTIME,POSITIONCODE2,COUNT(TASKCODE) ";
            //        sqlStr += "FROM t_task_travel ";
            //        sqlStr += "GROUP BY AGVCODE,REQTIME ";
            //        sqlStr += "ORDER BY AGVCODE,REQTIME;";

            //        dao.AddExecuteItem(sqlStr, null);

            //        dt = dao.Query().Tables[0];

            //        foreach (DataRow row in dt.Rows)
            //        {
            //            tw.WriteLine(row[0].ToString() + "," + row[1].ToString() + "," + row[2].ToString() + "," + row[3].ToString());
            //        }

            //        break;
            //    default:
            //        break;
            //}

            //memoryStream.Position = 0;
            //file = new StreamContent(memoryStream);
            //contentLength = memoryStream.Length;

            //tw.Flush();
            //tw.Close();

            return result;
        }
    }
}
