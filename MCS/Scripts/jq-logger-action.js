//綁定按鈕動作事件
function bindActionEvent() {
    //需要紀錄Log的動作
    let elms = document.getElementsByClassName('log');
    [].forEach.call(elms, function (elm) {
        elm.addEventListener('click', saveActionLog, false);
    });
}

//儲存事件紀錄
function saveActionLog(e) {
    let functionName = sessionStorage.getItem("CurrentFunction"); //地
    let targetElement = e.currentTarget; //物
    let targetName = "";
    let action = e.type; //事

    switch (action) {
        case "click":
            action = '按下';
            break;
    }

    if (targetElement.innerText != "") {
        targetName = targetElement.innerText;
    } else {
        targetName = targetElement.dataset.targetName;
    }

    $.ajax({
        type: "POST",
        url: "api/Log/SaveEventLog",
        data: {
            "USER_ID": sessionStorage.getItem("userId"),
            "USER_NAME": sessionStorage.getItem("userName"),
            "DEPARTMENT_ID": sessionStorage.getItem("depart"),
            "FUNCTION_ID": functionName,
            "TARGET_ELEMENT": targetName,
            "ACTION": action
        },
        dataType: "json",
        complete: function (jqXHR, textStatus) {
            if (textStatus == "success") { } else {
                sessionStorage.setItem('errorMsg', jqXHR.responseText);
                $('.container').load("api/Error/Status500");
            }
        }
    });
}

$(function () {
    bindActionEvent();
});