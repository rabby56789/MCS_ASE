using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JQWEB.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace MCS.Models
{
    /// <summary>
    /// 資料存取物件範例
    /// </summary>
    public class Demo : ISqlCreator
    {
        /// <summary>
        /// 存取子:主表名稱
        /// </summary>
        public string MasterTable { get { return "填入DB的Table Schema Name"; } }

        /// <summary>
        /// 丟JSON物件,回傳SQL具名參數陣列
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public MySqlParameter[] CreateParameterAry(JObject input)
        {
            if (input is null)
            {
                return new MySqlParameter[] { };
            }

            List<MySqlParameter> parmList = new List<MySqlParameter>();

            //JSON項目逐一加入參數表中
            foreach (var x in input)
            {
                MySqlParameter parm = new MySqlParameter();
                string name = x.Key;
                JToken value = x.Value;

                parm.ParameterName = "@" + name;
                parm.Value = value;
                //指定參數資料型態
                if (name == "DATA_STATUS" || name == "AUTHORITY_STATUS")
                {
                    parm.DbType = System.Data.DbType.Int32;
                }
                else
                {
                    parm.DbType = System.Data.DbType.String;
                }

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        /// <summary>
        /// 取得SQL字串通用介面
        /// </summary>
        /// <param name="actionName">動作名稱</param>
        /// <param name="parm">傳入參數(選擇性)</param>
        /// <returns></returns>
        public string GetSqlStr(string actionName, JObject parm)
        {
            string sqlStr = string.Empty;

            switch (actionName)
            {
                case "Count":
                    sqlStr = Search(parm, true);
                    break;
                case "Query":
                    sqlStr = Search(parm, false);
                    break;
                case "GetOneByGUID":
                    sqlStr = GetOneByGUID();
                    break;
                case "Add":
                    sqlStr = Insert();
                    break;
                case "Edit":
                    sqlStr = Update();
                    break;
                case "Delete":
                    sqlStr = Delete();
                    break;
                default:
                    break;
            }

            return sqlStr;
        }

        /// <summary>
        /// 取得查詢SQL字串
        /// </summary>
        /// <param name="parm">Client端傳入參數</param>
        /// <param name="getCount">是否為查詢數量</param>
        /// <returns></returns>
        string Search(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;

            //是否為查詢資料筆數
            if (getCount)
            {
                sqlStr = string.Format(@"SELECT COUNT(*) 'Count' FROM {0} ", MasterTable);
            }
            else
            {
                sqlStr = string.Format(@"SELECT U.GUID,U.ID,E.ID AS EMPLOYEE_ID,U.INSERT_TIME,U.REMARK FROM {0} ", MasterTable);
            }

            //SQL加入查詢條件
            if (!string.IsNullOrEmpty((string)conditions.GROUP_ID))
            {
                sqlStr += "INNER JOIN SYS_USER_GROUP UG ON UG.USER_GUID = U.GUID ";
                sqlStr += "INNER JOIN SYS_GROUP G ON G.GUID = UG.GROUP_GUID ";
            }

            return sqlStr;
        }

        private string GetOneByGUID()
        {
            throw new NotImplementedException();
        }

        string Insert()
        {
            throw new NotImplementedException();
        }

        string Update()
        {
            throw new NotImplementedException();
        }

        string Delete()
        {
            throw new NotImplementedException();
        }
    }
}