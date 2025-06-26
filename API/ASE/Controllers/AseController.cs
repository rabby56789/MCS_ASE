using ASE.Models;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ASE.Controllers
{
    public class AseController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [System.Web.Http.HttpPost]
        public IHttpActionResult CallFunc()
        {
            //建立兩種回傳資料型態
            var responseJson = new JObject();
            var responseFromData = new HttpResponseMessage(HttpStatusCode.OK);
            
            string contentType = HttpContext.Current.Request.ContentType.Split(';')[0];
            string functionId = "";
            HttpPostedFile inputFile;
            dynamic input;

            //依照不同ContentType讀取資料
            if (contentType == "multipart/form-data")
            {
                string jsonUrlDecode = HttpContext.Current.Request.Form.Get("JSON");
                string jsonStr = Uri.UnescapeDataString(jsonUrlDecode);
                input = JObject.Parse(jsonStr);
                inputFile = HttpContext.Current.Request.Files["File_1"];
                functionId = (string)input.function;
            }
            else //application/json
            {
                var stream = HttpContext.Current.Request.InputStream;
                var readStream = new StreamReader(stream, Encoding.UTF8);
                string json = readStream.ReadToEnd();
                input = JObject.Parse(json) as dynamic;
                inputFile = null;
                functionId = input.function;
            }

            responseFromData.Headers.Add("function", (string)input.function);
            responseFromData.Headers.Add("seq", (string)input.seq);
            responseJson.Add(new JProperty("function", (string)input.function));
            responseJson.Add(new JProperty("seq", (string)input.seq));
            responseJson.Add("return_code", "");
            responseJson.Add("return_msg", "");

            string return_msg = string.Empty;
            IModel model;
            log.Info("開始functionId : " + functionId + " , responseJson : " + responseJson);

            try
            {
                //依照Function代號進行對應操作
                switch (functionId)
                {
                    case "A01":
                        model = new A01();
                        //model.Run(input);
                        JObject Success_msg = new JObject();
                        Success_msg.Add(model.Run(input).Children());
                        return_msg = Success_msg.GetValue("msg").ToString();
                        break;
                    case "A02": //取得儲位狀態
                        model = new A02();
                        JToken tmp;
                        responseJson.Add(model.Run(input).Children());
                        return_msg = responseJson.TryGetValue("loc_status", out tmp) ? "" : "此儲位不存在";
                        break;
                    case "A03": //更新儲位狀態
                        model = new A03();
                        model.Run(input);
                        break;
                    case "A04": //新增/變更設定主檔
                        A04 a04 = new A04();
                        a04.file = inputFile;
                        a04.Run(input);
                        break;
                    case "A05": //取消任務
                        model = new A05();
                        //model.Run(input);
                        JObject Cancel_msg = new JObject();
                        Cancel_msg.Add(model.Run(input).Children());
                        return_msg = Cancel_msg.GetValue("msg").ToString();
                        break;
                    case "B01":
                        model = new B01();
                        responseJson.Add(model.Run(input));
                        break;
                    case "B02":
                        model = new B02();
                        return_msg = JsonConvert.SerializeObject(model.Run(input));
                        //responseJson.Add(model.Run(input));
                        break;
                    case "B03": //Raw Data回傳
                        B03 b03 = new B03();
                        b03.Run(input);
                        responseFromData.Content = b03.file;
                        responseFromData.Content.Headers.ContentLength = b03.contentLength;
                        break;
                    case "B04":
                        model = new B04();
                        model.Run(input);
                        break;
                    case "B05":
                        model = new B05();
                        model.Run(input);
                        break;
                    case "W01":
                        model = new W01();
                        responseJson.Add(model.Run(input));
                        break;
                    case "P01":
                        model = new P01();
                        responseJson.Add(model.Run(input));
                        break;
                    case "P02":
                        model = new P02();
                        responseJson.Add(model.Run(input));
                        break;
                    case "X01":
                        model = new X01();
                        model.Run(input);
                        break;
                    case "C01":
                        model = new C01();
                        model.Run(input);
                        break;
                    case "C02":
                        model = new C02();
                        model.Run(input);
                        break;
                    case "C03":
                        model = new C03();
                        model.Run(input);
                        break;
                    case "ping": 
                        model = new ping();
                        return_msg = JsonConvert.SerializeObject(model.Run(input));
                        break;
                    default:
                        throw new Exception("Can not found function id.");
                }
                
                responseJson["return_code"] = "S";
                responseJson["return_msg"] = return_msg;

                responseFromData.Headers.Add("return_code", "S");
                responseFromData.Headers.Add("return_msg", return_msg);
                log.Info("結束functionId : " + functionId + " , responseFromData : " + responseFromData);
                
                
            }
            catch (Exception ex)
            {   //依照日月光IT人員要求,目前return_code一律回傳S
                responseJson["return_code"] = "F";
                responseJson["return_msg"] = ex.Message;

                responseFromData.Headers.Add("return_code", "F");
                responseFromData.Headers.Add("return_msg", ex.Message);

                //log.Error("錯誤functionId : " + functionId +" , " + ex.ToString());
                log.Error("錯誤functionId : " + functionId);
                log.Error(ex);
            }
            finally
            {
                //儲存API Log
                APILog aPILog = new APILog();
                dynamic api_code = responseFromData.Headers.GetValues("return_code");//成功:S,失敗:F
                dynamic api_msg = responseFromData.Headers.GetValues("return_msg");//失敗msg
                //input:上位輸入參數,return_code,return_msg
                //responseFromData.Headers.GetValues("XXXX")帶回來的型態為Array
                aPILog.InsertAPILog(input,api_code[0],api_msg[0]);

                //string HostUrl = $"{Request.RequestUri}{Request.Method}";
                //Global.SaveApiLog(HostUrl, functionId, input.ToString(), responseJson.ToString());
            }

            //須回傳資料時回傳FormData
            if(functionId == "B03")
            {
                responseFromData.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("render");
                responseFromData.Content.Headers.ContentDisposition.FileName = input.file_data;
                responseFromData.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

                return ResponseMessage(responseFromData);
            }
            else
            {
                return Json<dynamic>(responseJson);
            }
        }
    }
}
