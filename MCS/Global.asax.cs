using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using JQWEB;
using JQWEB.Controllers;
using JQWEB.Global;
using JQWEB.Models;
using Newtonsoft.Json.Linq;

namespace MCS
{
    public class Global : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            GlobalFilters.Filters.Add(new WebActionFilters());

            //引用log4net的config
            string log4netPath = Server.MapPath("~/log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netPath));

            LicenseManager license = new LicenseManager();
            license.Startup();
        }

        /// <summary>
        /// 啟用 Web API 可讀取Session
        /// </summary>
        protected void Application_PostAuthorizeRequest()
        {
            if (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api"))
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        /// <summary>
        /// Session到期時自動記錄為登出
        /// </summary>
        protected void Session_End()
        {
            var loginGuid = Session["LoginGuid"];
            var userGuid = Session["UserGuid"];

            if (!string.IsNullOrEmpty(Convert.ToString(loginGuid)))//清Session之前先紀錄
            {

                var dao = new DAO();
                var sqlCreator = new Login();
                var parmObj = new JObject();
                string clientIP = string.Empty;

                string getLoginInfo = sqlCreator.GetSqlStr("GetOneLoginInfoByGuid");
                string removeLogin = sqlCreator.GetSqlStr("ClearLoginInfo");
                string saveLogout = sqlCreator.GetSqlStr("SaveLogoutInfo");

                //1.取得登入時的IP位置
                parmObj.Add("GUID", loginGuid.ToString());
                dao.AddExecuteItem(getLoginInfo, sqlCreator.CreateParameterAry(parmObj));
                clientIP = dao.Query().Tables[0].Rows[0]["CLIENT_IP"].ToString();

                parmObj.RemoveAll();

                //2.刪除登入狀態
                parmObj.Add("LoginGuid", loginGuid.ToString());
                dao.AddExecuteItem(removeLogin, sqlCreator.CreateParameterAry(parmObj));

                parmObj.RemoveAll();

                //3.紀錄登出動作
                parmObj.Add("INSERT_USER", "SYSTEM");
                parmObj.Add("INSERT_TIME", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                parmObj.Add("USER_GUID", userGuid.ToString());
                parmObj.Add("CLIENT_IP", clientIP);
                parmObj.Add("ACTION", "Logout");
                dao.AddExecuteItem(saveLogout, sqlCreator.CreateParameterAry(parmObj));

                dao.Execute();
            }
            Session.Clear();
        }

        /// <summary>
        /// 應用程式關閉時執行的程式碼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_End(object sender, EventArgs e)
        {
            Session.Abandon();
            LicenseManager license = new LicenseManager();
            license.Release();
        }
    }
}
