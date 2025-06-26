using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCS.Models
{
    public class WarnCallbackModel
    {

        public string reqCode { get; set; }
        public string reqTime { get; set; }
        public string clientCode { get; set; }
        public string tokenCode { get; set; }
        public Datum[] data { get; set; }

        public class Datum
        {
            public string agvCode { get; set; }
            public string beginTime { get; set; }
            public string warnContent { get; set; }
            public string taskCode { get; set; }
        }

    }
}