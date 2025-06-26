using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MCS.Controllers
{
    public class LogTaskController : Controller
    {
        // GET: LogTask
        public ActionResult Index()
        {
            return View("~/Views/Task/LogTask.cshtml");
        }
    }
}