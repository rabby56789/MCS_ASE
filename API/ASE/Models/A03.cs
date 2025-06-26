using JQWEB.Models;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ASE.Models
{
    /// <summary>
    /// 更新儲位狀態
    /// </summary>
    public class A03 : IModel
    {
        /// <summary>
        /// 任務狀態
        /// </summary>
        string task { get { return "t_subtask_status"; } }
        /// <summary>
        /// 儲位及時狀態
        /// </summary>
        string storage { get { return "t_storage"; } }
        /// <summary>
        /// 儲位及時狀態
        /// </summary>
        string TstorageStatus { get { return "t_storage_status"; } }
        string baseStorage { get { return "base_storage"; } }
        string baseTrolley { get { return "base_trolley"; } }
        /// 
        string trolley { get { return "base_trolley"; } }
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public JObject Run(dynamic parm)
        {
            if (A04.A04isRun == true)
            {
                log.Debug($"A03 Function parameter: {parm}  A04isRun : {A04.A04isRun} ");
                throw new Exception($"A04執行中，請稍後");
            }
            log.Debug($"A03 Function parameter: {parm}");

            JObject returnVal = new JObject();
            JObject input = parm;
            DAO dao = new DAO();
            bool result = false;
            string selectSql;
            string trolleyGUID = "";           

            //檢查 t_storage_status 中是否有該 location
            string location = (string)parm.location;
            string job_name = (string)parm.job_name;
            string _sqlString = $@"select GUID,GROUP_GUID from base_storage where ID = '{location}' || NAME = '{location}' || QRCODE = '{location}'";
            dao.AddExecuteItem(_sqlString, null);
            var data = dao.Query().Tables[0];
            if (data.Rows.Count == 0)
            {
                throw new Exception($@"查無儲位 {location}");
            }

            string _storageGuid = data.Rows[0]["GUID"].ToString();
            string _groupGuid = data.Rows[0]["GROUP_GUID"].ToString();

            //1.檢查任務清單是否包含此儲位 JOB_STATUS 0 = 建立任務, 1 = 執行中
            //string queryLocationStatus = $"SELECT COUNT(*) FROM {task} WHERE positioncodepath LIKE @location ";
            //string queryLocationStatus = $"SELECT COUNT(*) FROM {task} ";
            string queryLocationStatus = $"SELECT COUNT(*) FROM {task} ";
            queryLocationStatus += $"WHERE ASE_TARGET_LOC = @location ";
            queryLocationStatus += $"AND JOB_STATUS IN(0,1); ";
            dao.AddExecuteItem(queryLocationStatus, new MySqlParameter[] {
                new MySqlParameter("@location",(string)parm.location)
            });

            DataTable dt = dao.Query().Tables[0];
            bool isInTask = int.Parse(dt.Rows[0][0].ToString()) > 0 ? true : false;

            //該儲位在建立任務或執行中回傳
            if (isInTask)
            {
                throw new Exception("This location is on Task. T_TASK_SUBTASK執行中");
            }

            _sqlString = $@"select GUID from t_storage_status where STORAGE_GUID = '{_storageGuid}'";
            dao.AddExecuteItem(_sqlString, null);
            data = dao.Query().Tables[0];
            if (data.Rows.Count == 0)
            {
                if ((string)parm.lock_status == "1")
                {
                    //檢查是否為前段任務，用seq查詢
                    string FEseq = parm["seq"].ToString();
                    string Pre_Job_Name = string.Empty;
                    int FECount = 0;//計算查詢次數，若查詢次數小於則表示不是前段最後一段任務
                    while (true)
                    {
                        string sqlseq = $"select Pre_seq,Pre_Job_Name from t_holding_info where Next_seq = '{FEseq}'";
                        dao.AddExecuteItem(sqlseq, null);
                        var FEdata = dao.Query().Tables[0];
                        if (FEdata.Rows.Count > 0)
                        {
                            FEseq = FEdata.Rows[0][0].ToString();
                            Pre_Job_Name = FEdata.Rows[0][1].ToString();
                            FECount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (FECount == 4)//前段退庫用
                    {
                        job_name = Pre_Job_Name;
                    }


                    _sqlString = $@"INSERT INTO t_storage_status (GUID, STORAGE_GUID, AREA_GUID, TROLLEY_GUID, JOB_NAME, IS_LOCK, INSERT_USER, UPDATE_USER, ENABLE) VALUES ((SELECT uuid()), '{_storageGuid}', '{_groupGuid}', '','{job_name}', '1', 'A03', 'A03', '1');";
                    dao.AddExecuteItem(_sqlString, null);
                    dao.Execute();

                    JObject _jObject = new JObject() { new JProperty("result", "ok") };
                    log.Debug($"A03 result: {_jObject}");
                    return _jObject;
                }
                else
                {
                    throw new Exception("儲位並無鎖定，無需解鎖。");
                }
                    
            }
            else
            {
                if ((string)parm.lock_status == "1")
                {
                    throw new Exception("儲位本已鎖定，無需再鎖定。");
                }
                else
                {
                    string _guid = data.Rows[0]["GUID"].ToString();
                    _sqlString = $@"delete from t_storage_status where GUID = '{_guid}';";
                    dao.AddExecuteItem(_sqlString, null);
                    dao.Execute();

                    JObject _jObject = new JObject() { new JProperty("result", "ok") };
                    log.Debug($"A03 result: {_jObject}");
                    return _jObject;
                }
            }

            
            ////
            //#region 先查T_STORAGE_STATUS有無此ID的貨架狀態，有則更改，無則回傳錯誤訊息
            //if (parm.location == "")
            //{
            //    //找此貨架GUID
            //    selectSql = $"SELECT GUID FROM {baseTrolley} ";
            //    selectSql += $"where ID = @trolley_id ";
            //    dao.AddExecuteItem(selectSql, new MySqlParameter[] {
            //        new MySqlParameter("@trolley_id",(string)parm.car_no)
            //    });
            //    DataTable dtBaseTrolley = dao.Query().Tables[0];
            //    trolleyGUID = dtBaseTrolley.Rows[0][0].ToString();
            //}
            //else
            //{
            //    //找此儲位GUID
            //    //selectSql = $"SELECT GUID FROM {baseStorage} ";
            //    //selectSql += $"where NAME = @location || ID = @location || QRCODE = @location";
            //    //dao.AddExecuteItem(selectSql, new MySqlParameter[] {
            //    //    new MySqlParameter("@location",(string)parm.location)
            //    //});
            //    //DataTable dtBaseStorage = dao.Query().Tables[0];
            //    //storageGUID = dtBaseStorage.Rows[0][0].ToString();
            //}
            
            
            ////找此儲位有無在T_storage_Status中
            //selectSql = $"SELECT COUNT(*) FROM {TstorageStatus} ";
            //selectSql += $"WHERE STORAGE_GUID = @storageGUID ";
            //dao.AddExecuteItem(selectSql, new MySqlParameter[] {
            //    new MySqlParameter("@storageGUID",_storageGuid)
            //});
            //DataTable dtStorage = dao.Query().Tables[0];
            //int stroageCount = int.Parse(dtStorage.Rows[0][0].ToString());
            ////找此貨架有無在T_storage_Status中
            //selectSql = $"SELECT COUNT(*) FROM {TstorageStatus} ";
            //selectSql += $"WHERE TROLLEY_GUID = @trolleyGUID ";
            //dao.AddExecuteItem(selectSql, new MySqlParameter[] {
            //    new MySqlParameter("@trolleyGUID",trolleyGUID)
            //});
            //DataTable dtTrolley = dao.Query().Tables[0];
            //int TrolleyCount = int.Parse(dtTrolley.Rows[0][0].ToString());
            ////有則update，無則回傳錯誤訊息
            //if (stroageCount == 0 )
            //{
            //    throw new Exception("T_STORAGE_STATUS查無此儲位");
            //}
            //else if(stroageCount == 0 && TrolleyCount == 0)
            //{
            //    throw new Exception("T_STORAGE_STATUS查無此儲位及貨架");
            //}
            //#endregion
            ////判斷是否lock
            //if ((string)parm.lock_status == "1")
            //{
            //    string update = $"UPDATE {TstorageStatus} SET ";
            //    update += $"IS_LOCK = @lock, ";
            //    update += $"UPDATE_USER = 'SYSTEM_A03', ";
            //    update += $"UPDATE_TIME = now() ";
            //    update += $"WHERE STORAGE_GUID = @STORAGE_GUID ";
            //    update += $"; ";
            //    dao.AddExecuteItem(update, new MySqlParameter[] {
            //        new MySqlParameter("@STORAGE_GUID",_storageGuid),
            //        new MySqlParameter("@lock",(string)parm.lock_status)
            //    });
                
            //}
            //else
            //{
            //    string update = $"UPDATE {TstorageStatus} SET ";
            //    update += $"IS_LOCK = 0, ";
            //    update += $"UPDATE_USER = 'SYSTEM_A03', ";
            //    update += $"UPDATE_TIME = now() ";
            //    update += $"WHERE STORAGE_GUID = @STORAGE_GUID ";
            //    update += $"; ";
            //    dao.AddExecuteItem(update, new MySqlParameter[] {
            //        new MySqlParameter("@STORAGE_GUID",_storageGuid)
            //    });
            //}
            //result = dao.Execute();
            
            
            
            //log.Debug("A03 / 更新命令result : " + result);
            return returnVal;
        }

        //狀態轉換
        public string ConvertStatus(string input)
        {
            string returnVal = "";

            switch (input)
            {
                case "物料已送達":
                    returnVal = "A";
                    break;
                case "運送中":
                    returnVal = "B";
                    break;
                case "待AGV作業":
                    returnVal = "C";
                    break;
                case "空儲位":
                    returnVal = "D";
                    break;
                case "閒置":
                    returnVal = "E";
                    break;
                case "裝箱中":
                    returnVal = "W1";
                    break;
                case "卸貨中":
                    returnVal = "W2";
                    break;
                case "作業中":
                    returnVal = "W3";
                    break;
                default:
                    break;
            }

            return returnVal;
        }
    }
}
