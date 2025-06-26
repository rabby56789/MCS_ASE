using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCS.Models
{
    public class Task
    {
        public string REQCODE { get; set; }
        public string REQTIME { get; set; }
        public string CLIENTCODE { get; set; }
        public string TASKTYP { get; set; }
        public Task(string Line)
        {
            var sp = Line.Split(',');
            REQCODE = sp[0];
            REQTIME = sp[1];
            CLIENTCODE = sp[2];
            TASKTYP = sp[3];
        }
        
    }
}