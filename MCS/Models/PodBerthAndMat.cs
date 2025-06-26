using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MCS.Models
{
    public class PodBerthAndMatData
    {
        public string areaCode;
        public string materialLot;
        public string podCode;
        public string mapDataCode;
        public string positionCode;
    }
    public class PodBerthAndMat
    {
        public string code;
        public string message;
        public string reqCode;
        public List<PodBerthAndMatData> data;
    }
}