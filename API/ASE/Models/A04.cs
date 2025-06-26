using JQWEB.Models;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ASE.Models
{
    /// <summary>
    /// 將User設定的主檔資料傳給AGV系統,AGV系統接到資料後會再維護對應實際座標等資料
    /// </summary>
    public class A04 : IModel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public HttpPostedFile file { get; set; }

        public static bool A04isRun = false;//A04是否在執行，執行中其他API無法使用

        public JObject Run(dynamic parm)
        {
            A04isRun = true;//執行A04狀態為true
            bool isUpdateSuccess = true;//更新是否成功
            log.Debug($"A04 Function parameter: {parm} A04 Start A04isRun : {A04isRun}");            
            JObject input = parm;
            JObject returnVal = new JObject();           
            StreamReader reader = new StreamReader(file.InputStream);
            SubTask sqlCreator = new SubTask();
            DAO dao = new DAO();

            //1.檢查有無進行中的任務(Queue和t_subtask_status)
            string sqlHaveTask = @"SELECT * FROM t_subtask_status where JOB_STATUS in(0,1);";
            string sqlHaveQueue = @"SELECT * FROM t_task_queue where STATUS in (0);";
            dao.AddExecuteItem(sqlHaveTask,null);
            int TaskCount = dao.Query().Tables[0].Rows.Count;
            dao.AddExecuteItem(sqlHaveQueue,null);
            int QueueCount = dao.Query().Tables[0].Rows.Count;
            log.Debug($"A04 TaskCount : {TaskCount}  A04 QueueCount : {QueueCount}");
            if (TaskCount + QueueCount > 0)
            {
                A04isRun = false;//不更新
                log.Debug($"Have other task can't run A04  A04isRun : {A04isRun}");
                throw new Exception($"目前還有任務在執行中，請先確認Queue和在線任務管理內無任務");

            }
            //2.備份資料表資料
            string backuppath = "C:/ProgramData/MySQL/MySQL Server 8.0/Uploads";//存放備份路徑

            string base_area_filename = "Area_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");//Area檔名            
            string base_storage_filename = "Storage_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");//Storage檔名
            string base_trolley_filename = "Trolley_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");//Trolley檔名

            string sqlBackUpArea = $"SELECT * FROM base_area into outfile '{backuppath}/{base_area_filename}.txt';";/*匯出資料*/
            string sqlBackUpStorage = $"SELECT * FROM base_storage into outfile '{backuppath}/{base_storage_filename}.txt';";/*匯出資料*/            
            string sqlBackUpTrolley = $"SELECT * FROM base_trolley into outfile '{backuppath}/{base_trolley_filename}.txt';";/*匯出資料*/
                        
            dao.AddExecuteItem(sqlBackUpArea, null);
            dao.AddExecuteItem(sqlBackUpStorage, null);
            dao.AddExecuteItem(sqlBackUpTrolley, null);
            dao.Execute();
            //3.依照主檔代號更新
            switch (input.Value<int>("data_type"))//判斷主檔代號
            {
                               
                //區域儲格主檔 date_type = 1
                case (int)DataType.Storage:                    

                    string sqlStr_insertArea = "INSERT INTO base_area (GUID,ID,NAME,INSERT_USER) VALUE(uuid(),@ID,@NAME,'A04') ;";
                    //sqlStr_insertArea += "ON DUPLICATE KEY UPDATE NAME = @NAME;";

                    string sqlStr_insertStorage = "INSERT INTO base_storage (GUID,ID,NAME,QRCODE,GROUP_GUID,OverTime,INSERT_USER,INSERT_TIME) VALUE(uuid(),@ID,@NAME,@QRCODE,(SELECT GUID FROM base_area WHERE ID = @AREA_ID LIMIT 1),@OverTime,'A04',now()) ;";
                    //sqlStr_insertStorage += "ON DUPLICATE KEY UPDATE NAME = @NAME ,QRCODE = @QRCODE,GROUP_GUID = (SELECT GUID FROM base_area WHERE ID = @AREA_ID LIMIT 1);";

                    List<Storage> storageData = new List<Storage>();

                    //存入暫存List
                    do
                    {
                        string textLine = reader.ReadLine();
                        string[] ary = textLine.Split(',');

                        //OverTime有可能為空值...20221209 Ruby
                        if (string.IsNullOrEmpty(ary[(int)AseStorage.OverTime]))
                        {
                            ary[(int)AseStorage.OverTime] = "0";
                        }

                        //MB2F,FE,MB2F-FE,01,MB2F-FE-01,0001,30 ([Floor],[],[Area],)
                        storageData.Add(new Storage()
                        {
                            ID = ary[(int)AseStorage.Id],
                            Index = int.Parse(ary[(int)AseStorage.Index]),
                            Floor = ary[(int)AseStorage.Floor],
                            Area = ary[(int)AseStorage.Area],
                            QrCode = ary[(int)AseStorage.QrCode],
                            OverTime = int.Parse(ary[(int)AseStorage.OverTime])

                        });

                    } while (reader.Peek() != -1);

                    DropCreateForAreaStorageTable();//Drop and Create Area&Storage Table                    

                    //撈所有不重複區域
                    var AreaIDs = storageData.Select(x => x.Area).Distinct();                    
                    
                    //新增區域主檔
                    foreach (var area in AreaIDs)
                    {
                        var parms = new List<MySqlParameter>();                        
                        parms.Add(new MySqlParameter("@ID", area));
                        parms.Add(new MySqlParameter("@NAME", area));                        
                        dao.AddExecuteItem(sqlStr_insertArea, parms.ToArray());                        
                    }

                    //新增儲位
                    foreach (var storage in storageData)
                    {
                        var parms = new List<MySqlParameter>();
                        parms.Add(new MySqlParameter("@ID", storage.ID));
                        parms.Add(new MySqlParameter("@NAME", storage.ID));
                        parms.Add(new MySqlParameter("@QRCODE", storage.QrCode));
                        parms.Add(new MySqlParameter("@AREA_ID", storage.Area));
                        parms.Add(new MySqlParameter("@OverTime", storage.OverTime));
                        dao.AddExecuteItem(sqlStr_insertStorage, parms.ToArray());
                    }
                    dao.Execute();

                    //更新檢查(20221209 Ruby 改為檢查數量)
                    string sqlStorageCheck = "select count(*) as Count from base_storage";
                    dao.AddExecuteItem(sqlStorageCheck, null);

                    //資料庫更新筆數
                    int checkdatastorage_count = Convert.ToInt32(dao.Query().Tables[0].Rows[0][0].ToString());
                    //主檔資料筆數
                    int storagedatacount = storageData.Count();

                    //確認筆數相同
                    if (checkdatastorage_count != storagedatacount)
                    {
                        isUpdateSuccess = false;
                        log.Debug($"資料更新失敗，checkdata Count : {checkdatastorage_count} txtdata Count : {storagedatacount}");
                        break;
                    }


                    //var checkdata = dao.Query().Tables[0];
                    //for (int i = 0; i < checkdata.Rows.Count; i++) 
                    //{
                    //    if (checkdata.Rows[i]["ID"].ToString() != storageData[i].ID)
                    //    {
                    //        isUpdateSuccess = false;
                    //        log.Debug($"資料更新失敗，第{i}行，checkdata : {checkdata.Rows[i]["ID"]} ，txtdata : {storageData[i].ID}");
                    //        break; 
                    //    }
                    //}

                    break;

                //料架車號主檔 date_type = 2
                case (int)DataType.Trolley:
                    
                    DropCreateTrolleyTable();

                    string sqlStr_insertTrolley = "INSERT INTO base_trolley (GUID,ID,NAME,TYPE,SEQ,QRCODE,INSERT_USER,INSERT_TIME) VALUE(UUID(),@ID,@NAME,@TYPE,@SEQ,@QRCODE,'A04',now()) ";
                    //string sqlStr_insertTrolley = "INSERT INTO base_trolley (GUID,ID,NAME,TYPE,SEQ,QRCODE,GROUP_GUID,INSERT_USER,INSERT_TIME) VALUE(UUID(),@ID,@NAME,@TYPE,@SEQ,@QRCODE,(SELECT GUID FROM base_area WHERE ID = @AREA_ID LIMIT 1),'A04',now()) ";
                    //sqlStr_insertTrolley += "ON DUPLICATE KEY UPDATE NAME = @NAME,TYPE = @TYPE,SEQ = @SEQ,QRCODE = @QRCODE,GROUP_GUID = (SELECT GUID FROM base_area WHERE ID = @AREA_ID LIMIT 1);";

                    List<BaseTrolley> trolleyData = new List<BaseTrolley>();
                    do
                    {
                        string textLine = reader.ReadLine();
                        string[] data = textLine.Split(',');

                        dao.AddExecuteItem(sqlStr_insertTrolley, new MySqlParameter[] {
                            new MySqlParameter("@ID",data[(int)Trolley.QrCode]),
                            new MySqlParameter("@NAME",data[(int)Trolley.TrolleyId]),
                            new MySqlParameter("@TYPE",data[(int)Trolley.QrCode]),
                            new MySqlParameter("@SEQ",data[(int)Trolley.Seq]),
                            new MySqlParameter("@QRCODE",""),
                            //new MySqlParameter("@AREA_ID",data[(int)Trolley.AreaId]),
                        });

                        trolleyData.Add(new BaseTrolley() 
                        {
                            ID = data[(int)Trolley.QrCode],
                            NAME = data[(int)Trolley.TrolleyId]                            
                        });

                    } while (reader.Peek() != -1);

                    dao.Execute();

                    //更新檢查(20221209 Ruby 改為檢查數量)
                    string sqlTrolleyCheck = "select count(*) as Count from base_trolley";
                    dao.AddExecuteItem(sqlTrolleyCheck, null);
                    
                    //資料庫更新筆數
                    int checkdatatrolley_count = Convert.ToInt32( dao.Query().Tables[0].Rows[0][0].ToString());
                    //主檔資料筆數
                    int trollydatacount = trolleyData.Count();

                    //確認筆數相同
                    if (checkdatatrolley_count != trollydatacount)
                    {
                        isUpdateSuccess = false;
                        log.Debug($"資料更新失敗，checkdata Count : {checkdatatrolley_count} txtdata Count : {trollydatacount}");
                        break;
                    }

                    //每一筆檢查
                    //for (int i = 0; i < checkdatatrolley.Rows.Count; i++)
                    //{
                    //    if (checkdatatrolley.Rows[i]["ID"].ToString() != trolleyData[i].ID)
                    //    {
                    //        //isUpdateSuccess = false;
                    //        isUpdateSuccess = true;
                    //        log.Debug($"資料更新失敗，第{i}行，checkdata : {checkdatatrolley.Rows[i]["ID"]} ，txtdata : {trolleyData[i].ID}");
                    //        break;
                    //    }
                    //}

                    break;
                    
                default:
                    log.Info($"A04 data_type : {input.Value<int>("data_type")}");
                    break;
                    throw new Exception($"data_type : '{input.Value<int>("data_type")}'不在定義範圍內");
                    
            }

            //4.更新失敗，Import資料回去
            if (isUpdateSuccess == false)
            {
                //先dropcreate在匯入
                DropCreateForAreaStorageTable();//重建AreaTable and StorageTable
                DropCreateTrolleyTable();//重建TrolleyTable
                string sqlImportArea = $"load data infile '{backuppath}/{base_area_filename}.txt'into table base_area;";
                dao.AddExecuteItem(sqlImportArea, null);
                string sqlImportStorage = $"load data infile '{backuppath}/{base_storage_filename}.txt'into table base_storage;";
                dao.AddExecuteItem(sqlImportStorage, null);
                string sqlImportTrolley = $"load data infile '{backuppath}/{base_trolley_filename}.txt'into table base_trolley;";
                dao.AddExecuteItem(sqlImportTrolley, null);
                dao.Execute();
                throw new Exception($"資料更新失敗，已還原");
            }
            else//將暫存區資料加回去 MB2F-AA、M01F-AA、M01F-BB
            {
                if (input.Value<int>("data_type") == 1)
                {
                    InsertTemAreaAndStorage();
                }
            }
            A04isRun = false;
            log.Info($"A04 returnVal: {returnVal}");
            return returnVal;
        }

        public bool DropCreateForAreaStorageTable()
        {
            DAO dao = new DAO();
            SubTask sqlCreator = new SubTask();
            //drop table base_area and base_storage 
            string sqlStr_dropArea = "DROP TABLE IF EXISTS base_area";
            string sqlStr_dropStorage = "DROP TABLE IF EXISTS base_storage";
            dao.AddExecuteItem(sqlStr_dropArea, null);
            dao.AddExecuteItem(sqlStr_dropStorage, null);
            if (dao.Execute() == false)
            {
                log.Error($"A04 Drop AreaTable fail");
                return false;
            }
            //create table base_area and base_storage
            string sqStr_createArea = sqlCreator.CreateAreaTable();
            string sqlStr_createStorage = sqlCreator.CreateStorageTable();
            dao.AddExecuteItem(sqlStr_createStorage, null);
            dao.AddExecuteItem(sqStr_createArea, null);
            if (dao.Execute() == false)
            {
                log.Error($"A04 Create StorageTable fail");
                return false;
            }
            return true;
        }
        public bool DropCreateTrolleyTable() 
        {
            DAO dao = new DAO();
            SubTask sqlCreator = new SubTask();
            //drop table base_area and base_storage 
            string sqlStr_dropTrolley = "DROP TABLE IF EXISTS base_trolley";
            dao.AddExecuteItem(sqlStr_dropTrolley, null);
            if (dao.Execute() == false)
            {
                log.Error($"A04 Drop TrolleyTable fail");
                return false;
            }
            //create table base_area and base_storage
            string sqStr_createTrolley = sqlCreator.CreateTrolleyTable();
            dao.AddExecuteItem(sqStr_createTrolley, null);
            if (dao.Execute() == false)
            {
                log.Error($"A04 Create TrolleyTable fail");
                return false;
            }
            return true;
        }
        public bool InsertTemAreaAndStorage()
        {
            DAO dao = new DAO();
            string sqlTemArea = $@"Insert into base_area (GUID,ID,NAME,INSERT_USER,INSERT_TIME,ENABLE,REMARK)
            VALUE('guid_mb2f-aa-for-back','MB2F-AA','MB2F-AA','A04',now(),1,'B2風淋門前 For 退庫'),
            ('guid_mb2f-aau-for-transport','MB2F-AAU','MB2F-AAU','A04',now(),1,'B2風淋門前 For 運輸'),
            ('guid-m01f-aa-for-back','M01F-AA','M01F-AA','A04',now(),1,'1樓FE貨架暫存區 For 退庫'),
            ('guid-m01f-aau-for-transport','M01F-AAU','M01F-AAU','A04',now(),1,'1樓FE貨架暫存區 For 運輸'),
            ('guid-m01f-bb-for-transport-back','M01F-BB','M01F-BB','A04',now(),1,'一樓五米風淋門內') ";
            dao.AddExecuteItem(sqlTemArea, null);
            string sqlTemStorage = $@"Insert into base_storage (GUID,ID,NAME,QRCODE,GROUP_GUID,INSERT_USER,INSERT_TIME,ENABLE,REMARK)
            VALUE(uuid(),'MB2F-AA-01','MB2F-AA-01','100000MB096490','guid_mb2f-aa-for-back','A04',now(),1,'B2風淋門前 For 退庫'),
            (uuid(),'MB2F-AAU-01','MB2F-AAU-01','098430MB098690','guid_mb2f-aau-for-transport','A04',now(),1,'B2風淋門前 For 運輸'),
            (uuid(),'M01F-AA-01','M01F-AA-01','036334MC073080','guid-m01f-aa-for-back','A04',now(),1,'1樓FE貨架暫存區 For 退庫'),
            (uuid(),'M01F-AA-02','M01F-AA-02','039894MC070150','guid-m01f-aa-for-back','A04',now(),1,'1樓FE貨架暫存區 For 退庫'),
            (uuid(),'M01F-AAU-01','M01F-AAU-01','039894MC067100','guid-m01f-aau-for-transport','A04',now(),1,'1樓FE貨架暫存區 For 運輸'),
            (uuid(),'M01F-AAU-02','M01F-AAU-02','039894MC068650','guid-m01f-aau-for-transport','A04',now(),1,'1樓FE貨架暫存區 For 運輸'),
            (uuid(),'M01F-BB-01','M01F-BB-01','103000MF100950','guid-m01f-bb-for-transport-back','A04',now(),1,'一樓五米風淋門內'),
            (uuid(),'M01F-BB-02','M01F-BB-02','104573MF100950','guid-m01f-bb-for-transport-back','A04',now(),1,'一樓五米風淋門內'),
            (uuid(),'M01F-BB-03','M01F-BB-03','106115MF100950','guid-m01f-bb-for-transport-back','A04',now(),1,'一樓五米風淋門內')";
            dao.AddExecuteItem(sqlTemStorage, null);
            return dao.Execute();
        }
        /// <summary>
        /// 列舉:主檔代號
        /// </summary>
        enum DataType
        {
            /// <summary>
            ///  區域儲格主檔
            /// </summary>
            Storage = 1,
            /// <summary>
            /// 料架車號主檔
            /// </summary>
            Trolley = 2
        }

        public class Storage
        {
            public string ID { get; set; }
            public int Index { get; set; }
            public string Floor { get; set; }
            public string Area { get; set; }
            public string QrCode { get; set; }
            public int OverTime { get; set; }
        }

        public class BaseTrolley
        {
            public string ID { get; set; }
            public string NAME { get; set; }
        }

        enum AseStorage
        {
            Floor = 0,
            Area = 2,
            Index = 3,
            Id = 4,
            QrCode = 5,
            OverTime = 6
        }

        enum Trolley
        {
            Seq = 1,
            TrolleyId = 2,
            QrCode = 3,
            AreaId = 4
        }
    }
}
