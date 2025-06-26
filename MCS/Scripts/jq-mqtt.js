//前端控制MQTT
let client;
let username = '';
let password = "";
let topics = []; //訂閱主題清單

//查詢訂閱主題


function getSubscribedTopic() {
    return new Promise((resolve, reject) => {
        $.ajax({
            type: 'POST',
            url: "api/Global/GetParamsByFunction",
            data: { FUNCTION: "MQTT" },
            dataType: "json",
            success: (parms) => {
                //判斷是否開啟一個以上的MQTT主題訂閱
                $.each(parms, function (idx, row) {
                    if (row.VALUE == '1') {
                        topics.push(row.FILTER_KEY);
                    }
                });

                if (topics.length > 0) {
                    resolve();
                } else {
                    reject();
                }
            },
            error: () => {
                reject();
            }
        });
    })
}

//查詢MQTT連線資訊後,設定連線
function connectToBroke() {

    $.ajax({
        type: "POST",
        url: "api/Global/GetAppSettingsValueByKey",
        data: { keys: ["MQTT_URL", "MQTT_USER_NAME", "MQTT_PASSWORD"] },
        dataType: "json",
        success: function (settings) {
            const options = {
                clean: true, // retain session
                connectTimeout: 4000, // Timeout period
                // Authentication information
                username: settings.MQTT_USER_NAME,
                password: settings.MQTT_PASSWORD,
            }

            client = mqtt.connect(settings.MQTT_URL, options);
            client.on('connect', onConnect);
            client.on('reconnect', onReconnect);
            client.on('error', onError);
            client.on('message', onMessage);
        }
    });
}

//連線成功後
let onConnect = (error) => {
    //選擇訂閱內容
    $.each(topics, function (idx, val) {
        client.subscribe(val, function (error) {
            if (error) {
                console.log('subscribe error :' + error);
            } else {
                console.log('subscribe topic :' + val);
            }
        })
    });
}
//重新連線
let onReconnect = (error) => {
    console.log('reconnecting:', error);
}
//連線錯誤
let onError = (error) => {
    console.log('Connection failed:', error);
}
//接收到訊息時
let onMessage = (topic, message) => {
    switch (topic) {
        case "JQMS/Alarm":
            //console.log('receive message：', topic, message.toString());
            let alarmMsg = JSON.parse(message.toString());
            let obj = {
                "type": "alarm",
                "title": alarmMsg.FACTORY + "/" + alarmMsg.FLOOR + "/" + alarmMsg.MAP,
                "msg": alarmMsg
            }

            console.info(alarmMsg);

            //轉存至LOG_NOTICE表並刷新通知頁籤
            $.ajax({
                type: "POST",
                url: "api/Log/InsertNotice",
                data: obj,
                dataType: "json",
                success: function (response) {
                    //呼叫 jq-rightTab.js
                    getNotice();
                },
                complete: () => {
                    let msg = "";

                    for (var key in alarmMsg) {
                        msg += key + ":" + alarmMsg[key];
                        msg += "<br>";
                    }

                    //顯示警告
                    JQMessageBox(
                        globalDoc.getTextByKey('receiveAlarm'),
                        msg,
                        {
                            'OK': globalDoc.getTextByKey('ok')
                        }
                    );
                }
            });

            break;
        case "JQMS/AGVStatus":
            console.log('receive ' + topic + '：', message.toString());
            break;
        default:
            break;
    }
}

$(function () {
    //1.抓參數判斷是否連線MQTT Server並訂閱設定的topic
    getSubscribedTopic()
        .then(connectToBroke)
        .catch(() => { console.info("not any subscribe by topic") });
});