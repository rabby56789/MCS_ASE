/*
 * 說明：右側頁籤功能
 * Layout頁的
 * <div id='head-tabs'>
 * <div id='right-tab'>
 */

let unreadCount = 0; //未讀訊息統計

//右上隱藏右側頁籤按鈕
function headTabsButtonOnClick() {
    $('.side-container').toggleClass('hide');
    if ($('.side-title').hasClass('active')) {
        $('.side-title.active').removeClass('active');
        $('.side-content.active').removeClass('active');
        $('.side-container.active').removeClass('active');
    }
}

//右側頁籤點選隱藏或顯示
function rightTabsSwitchShow() {
    let childindex = parseInt($(this).index()) + 1;
    if (!$('.side-title').hasClass('active')) {
        $('.side-container').addClass('active');
        $(this).addClass('active');
        $('.side-content:nth-child(' + childindex + ')').addClass('active');
    }
    else {
        if ($(this).hasClass('active')) {
            $('.side-container').removeClass('active');
            $(this).removeClass('active');
            $('.side-content:nth-child(' + childindex + ')').removeClass('active');
        }
        else {
            $('.side-title.active').removeClass('active');
            $('.side-content.active').removeClass('active');
            $('.side-content:nth-child(' + childindex + ')').addClass('active');
            $(this).addClass('active');
        }
    }
}

//當內容標籤隱藏時要做的動作
function onTabContentHide(e) {
    let rightTabs = document.querySelectorAll('.side-title');

    [].forEach.call(rightTabs, function (elm) {
        let classList = elm.classList;

        if (!classList.contains('active')) { //收起
            switch (elm.id) {
                case 'rightTab_lightbulb':
                    $('#tableUserActionLog').empty();
                    break;
                case 'rightTab_bell':
                    break;
                case 'rightTab_user':
                    break;
                case 'rightTab_info':
                    break;
            }
        }
    });
}

//顯示使用者操作紀錄
function getActionLog(e) {
    let classList = e.currentTarget.classList;

    //判斷頁籤是否為展開狀態
    if (classList.contains('active')) { //展開
        $('#tableUserActionLog').empty();

        $.ajax({
            type: 'POST',
            url: 'api/Log/GetTodayActionLogByUser',
            data: { 'USER_ID': sessionStorage.getItem('userId') },
            dataType: 'json',
            success: function (response) {
                let logData = response.rows;
                let logTable = document.getElementById('tableUserActionLog');

                logData.forEach((item, idx) => {

                    let row = document.createElement('tr');
                    let col_funcName = document.createElement('td');
                    let col_actionDescribe = document.createElement('td');
                    let col_actionTime = document.createElement('td');

                    col_funcName.innerText = layoutDoc.getTextByKey(item.DOC_KEY);
                    col_actionDescribe.innerText = item.ACTION + ' ' + item.TARGET_ELEMENT;
                    col_actionTime.innerText = item.INSERT_TIME.replace('T', ' ');

                    row.appendChild(col_actionDescribe);
                    row.appendChild(col_funcName);
                    row.appendChild(col_actionTime);

                    logTable.appendChild(row);
                });
            }
        });
    }
}

//取得警告事件
function getNotice() {
    $.ajax({
        type: 'POST',
        url: 'api/Log/GetNoticeByUser',
        data: { 'USER_GUID': sessionStorage.getItem('userGuid') },
        dataType: 'json',
        success: function (notices) {
            $('#noticeContainer').children().remove();
            unreadCount = 0;

            for (let idx in notices) {
                let item = notices[idx]; //資料
                let oneNotice = document.createElement('div');
                let hyperlinkContainer = document.createElement('a');
                let iconDiv = document.createElement('div');
                let icon = document.createElement('i');
                let textDiv = document.createElement('div');
                let titleDiv = document.createElement('div');
                let contentDiv = document.createElement('div');
                let dateDiv = document.createElement('div');
                let content = JSON.parse(item.CONTENT);

                oneNotice.classList.add('sec');
                oneNotice.id = item.GUID;

                //超連結
                hyperlinkContainer.classList.add('flex-row');
                hyperlinkContainer.dataset.GUID = item.GUID;
                //點選超連結後更新讀取狀態
                hyperlinkContainer.addEventListener('click', updateNoticeReadStatus);

                //鈴鐺圖案
                icon.classList.add('far', 'fa-envelope');
                iconDiv.classList.add('profCont');
                iconDiv.appendChild(icon);

                titleDiv.innerText = item.TITLE;
                contentDiv.innerHTML += content.MESSAGE;
                contentDiv.innerHTML += "<br>";

                dateDiv.innerText = item.INSERT_TIME.replace('T', ' ');

                //內文
                textDiv.classList.add('flex-column');
                titleDiv.classList.add('txt');
                contentDiv.classList.add('txt');
                dateDiv.classList.add('txt', 'sub')

                textDiv.appendChild(titleDiv);
                textDiv.appendChild(contentDiv);
                textDiv.appendChild(dateDiv);

                //組合
                hyperlinkContainer.appendChild(iconDiv);
                hyperlinkContainer.appendChild(textDiv);
                oneNotice.appendChild(hyperlinkContainer);

                //未讀訊息標註+紀錄筆數
                if (item.READED == 'N') {
                    oneNotice.classList.add('new');
                    unreadCount += 1;
                }

                $('#noticeContainer').append(oneNotice);

            } //Add Notice Loop

            //顯示未讀訊息數量
            if (unreadCount > 0) {
                let countDiv = document.createElement('div');
                countDiv.id = "unreadCount";

                countDiv.classList.add('noti-number');

                if (unreadCount > 9) {
                    countDiv.innerText = "9+";
                } else {
                    countDiv.innerText = unreadCount;
                }

                $('#rightTab_bell').prepend(countDiv);
            }
        }
    });
}

//更新訊息讀取狀態
function updateNoticeReadStatus(e) {
    //console.log(e.currentTarget.dataset.GUID);
    let noticeGUID = e.currentTarget.dataset.GUID;

    $.ajax({
        type: "POST",
        url: "api/Log/UpdateReadStatus",
        data: {
            "NOTICE_GUID": noticeGUID,
            "USER_GUID": sessionStorage.getItem("userGuid")
        },
        dataType: "json",
        success: function (response) {
            let oneNotice = document.getElementById(noticeGUID);
            let unreadCountDiv = document.getElementById("unreadCount");

            unreadCount = unreadCount - 1;

            if (unreadCount < 0) { unreadCount = 0; }

            if (unreadCount > 9) {
                unreadCountDiv.innerText = "9+";
            } else {
                unreadCountDiv.innerText = unreadCount;
            }

            //無未讀訊息時移除數字
            if (unreadCount == 0) {
                unreadCountDiv.remove();
            }

            //移除未讀訊息標示
            oneNotice.classList.remove('new');

        }
    });
}

let getSystemVersionNum = new Promise((resolve) => {
    let infoElm = document.getElementById('infoContent');
    let content = document.createElement('div');
    let title = document.createElement('h4');
    let text = document.createElement('div');

    console.info("getSystemVersionNum");
    $('#infoContent').empty();

    content.classList.add('info-content');
    title.classList.add('info-title');
    text.classList.add('info-txt');

    title.innerText = "檔案版本";

    $.ajax({
        type: "POST",
        url: "api/Global/GetSystemVersionNum",
        data: {},
        dataType: "json",
        success: function (versionList) {

            for (var fileName in versionList) {
                let versionInfo = fileName + " : " + versionList[fileName] + "<br>";
                text.innerHTML += versionInfo;
            }

            content.appendChild(title);
            content.appendChild(text);

            infoElm.appendChild(content);

            resolve();
        }
    });
});

//取得系統資訊
function getSystemInfo() {
    console.info("getSystemInfo");

    $.ajax({
        type: 'POST',
        url: "api/Global/GetParamsByFunction",
        data: { FUNCTION: "SYS" },
        dataType: "json",
        success: function (response) {
            // console.info(response);
            let infoElm = document.getElementById('infoContent');

            for (var idx in response) {
                let content = document.createElement('div');
                let title = document.createElement('h4');
                let text = document.createElement('div');

                console.info(response[idx]);

                content.classList.add('info-content');
                title.classList.add('info-title');
                text.classList.add('info-txt');


                switch (response[idx].FILTER_KEY) {
                    case "DBName":
                        title.innerText = response[idx].TEXT;
                        text.innerText = response[idx].VALUE;
                        break;
                    case "BusinessUnit":
                        title.innerText = response[idx].TEXT;
                        text.innerText = response[idx].VALUE;
                        break;
                    case "ManagementUnit":
                        title.innerText = response[idx].TEXT;
                        text.innerText = response[idx].VALUE;
                        break;
                    case "BestResolution":
                        title.innerText = response[idx].TEXT;
                        text.innerText = response[idx].VALUE;
                        break;
                    default:
                        break;
                }
                content.appendChild(title);
                content.appendChild(text);

                infoElm.appendChild(content);

            } // next systen info
        }
    });
}

$(function () {
    //滑鼠點選事件註冊
    document.querySelector('#head-tabs').addEventListener('click', headTabsButtonOnClick, true);
    let rightTabs = document.querySelectorAll('.side-title');

    [].forEach.call(rightTabs, function (elm) {
        elm.addEventListener('click', rightTabsSwitchShow, true);
        elm.addEventListener('click', onTabContentHide, true);
    });

    //即時資料取得事件註冊
    document.querySelector('#rightTab_lightbulb').addEventListener('click', getActionLog, true);

    //頁面讀取完畢後動作
    getNotice();
    getSystemVersionNum.then((val) => {
        getSystemInfo();
    })
});