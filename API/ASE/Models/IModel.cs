using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASE.Models
{
    /// <summary>
    /// 通用資料處理介面
    /// </summary>
    public interface IModel
    {
        JObject Run(dynamic parm);
    }
}
