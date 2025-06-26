//AJAX回傳錯誤統一處理區
$(function () {
    $.ajaxSetup({
        global: true //啟用通用AJAX處理
    });
});

//通用AJAX回傳處理
$(document).ajaxComplete(function (event, xhr, settings) {
    let errorMsg = "";

    if (xhr.status != 200) {
        errorMsg = xhr.responseText;
    }

    switch (xhr.status) {
        case 500:
            $.get("api/ApiError/Status500", {},
                function (data, textStatus, jqXHR) {
                    //填入Server端回傳錯誤訊息
                    document.write(data.replace("#errorContent", errorMsg));
                },
                "text"
            );
            break;
        default:
    }
});