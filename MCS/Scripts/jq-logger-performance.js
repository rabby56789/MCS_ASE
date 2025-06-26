let startRequestTime; //紀錄需求起始時間
let g_action;
let requestSql; //儲存request對應的sql

//綁定AJAX動作事件
function bindAjaxEvent() {
    $('.ajaxRequest').on('click', function (e) { //有呼叫API的按鈕強制disable
        e.currentTarget.disabled = true;
    });
}

/**
 * 取得Request URL對應的SQL,並回傳執行結果
 * @param {string} funcName 功能頁面名稱
 * @param {string} action 動作名稱
 * @param {string} params SQL參數
 */
async function getSql(funcName, action, params) {
    startRequestTime = new Date();
    g_action = action;
    let result;

    //取得Request SQL,並回傳取得結果
    await $.ajax({
        type: "POST",
        url: "../api/Log/GetSqlStringByAction",
        data: {
            function: funcName,
            action: action,
            params: params
        },
        dataType: "json",
        complete: function (jqXHR, textStatus) {
            if (textStatus == "success") {
                requestSql = jqXHR.responseJSON.sqlStr;
                result = true;
            } else {
                result = false;
            }
        }
    });

    return result;
}

//儲存SQL執行效能LOG
function saveRequestPerformanceLog() {
    let endRequestTime = new Date();
    let diff = parseInt((endRequestTime - startRequestTime)); //毫秒數

    $.ajax({
        type: "POST",
        url: "../api/Log/SaveSqlPerformanceLog",
        data: {
            USER_ID: sessionStorage.getItem("userId"),
            FUNCTION_ID: sessionStorage.getItem("CurrentFunction"), //抓目前頁的HTML Tag
            ACTION: g_action,
            SQL: requestSql,
            ELAPSED: diff
        },
        dataType: "json",
        complete: function (jqXHR, textStatus) {
            if (textStatus == "success") {

            } else {

            }
        }
    });
}

$(function () {
    bindAjaxEvent();
});