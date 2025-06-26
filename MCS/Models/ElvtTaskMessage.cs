using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MCS.Models
{
    // for calling elvtAPI(MCS) use: elvtNotifyService (Controller)
    public class ElvtCallMessage
    {
        /// <summary>
        ///  ** API回傳結果: {enum CODE}
        /// </summary>
        public string code { get; set; }

        /// <summary>
        ///  ** 任務狀態GUID
        ///  記錄 '任務' 與 '電梯任務' 之間，API訊息的傳遞對象 (come from | reply to 哪一筆任務 {task_status_guid})
        /// </summary>
        public string taskStatusGuid { get; set; }

        /// <summary>
        ///  ** 電梯任務狀態GUID
        ///  記錄 '任務' 與 '電梯任務' 之間，API訊息的傳遞對象 (come from | reply to 哪一筆任務 {task_status_guid})
        /// </summary>
        public string elvtTaskStatusGuid { get; set; }

        /// <summary>
        ///  當前樓層
        /// </summary>
        public string taskFloor { get; set; }

        /// <summary>
        ///  ** 電梯儲格狀態: {enum SPACESTATUS}
        ///       表達: 任務階段。
        ///       例如: "4F到2F任務"："Ready":     電梯到4F了
        ///                                                "SpaceIn":  4F的貨架移動任務完成(貨架移入)
        ///                                                "Empty":     4F的貨架移動任務完成(貨架移出)
        /// </summary>
        public string taskSpaceStatus { get; set; }

        /// <summary>
        ///  * 通知: 任務搬運數與貨架 所 '實際移入(by SPACESTATUS: SpaceIn)' 的電梯儲位" ( 1個貨架, 1個JObject)
        ///  * 通知: 任務搬運數與貨架 所 '實際移出(by SPACESTATUS: Empty)'    的電梯儲位" ( 1個貨架, 1個JObject)
        /// </summary>
        public List<JObject> elvtSpaceInfoList { get; set; }
        // * 電梯編號
        // {JObject}.Add("ELVT_ID", elvtId);
        // * 電梯儲格編號
        // {JObject}.Add("ELVT_SPACE_ID", elvtSpaceId);
        //電梯儲格位置順序
        // {JObject}.Add("ELVT_SPACE_INDEX", elvtSpaceIndex);
        //電梯儲格地碼
        // {JObject}.Add("ELVT_SPACE_QRCODE", elvtSpaceQRCode);
        // * 電梯儲格狀態: 貨架編號
        // {JObject}.Add("ELVT_SPACE_STATUS_TROLLEY_ID", trolleyId);
        //電梯儲格狀態GUID
        // {JObject}.Add("ELVT_SPACE_STATUS_GUID", elvtSpaceStatusGuid);
    }

    public enum SPACESTATUS
    {
        Ready,
        SpaceIn,
        Empty
    }

    public enum CODE
    {
        Success,
        Fail
    }

}
