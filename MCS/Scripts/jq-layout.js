let token = $('#hdnToken').val();
let idleCounter = 0;  //閒置計時
let idleLimit; //閒置登出時間設定(抓web.config設定)
let globalDoc; //共用元件語系工具
let layoutDoc; //主板頁面專用語系工具
let currentFuncId;

//每30秒呼叫一次Session防過期
let hnd = setInterval(function (token) {
    $.ajax({
        url: '../Home/Heartbeat',
        type: 'POST',
        success: function (result) {
            if (result !== 'OK') {
                clearInterval(hnd);
                logout();
            }
        }
    });
}, 30 * 1000);

//每秒抓Session有效時間,超過自動登出
let hndIdleDetect = setInterval(function () {
    idleCounter++;
    if (idleCounter > idleLimit) {
        clearInterval(hnd);
        clearInterval(hndIdleDetect);
        JQMessageBox(
            globalDoc.getTextByKey('prompt'),
            globalDoc.getTextByKey('idleTimeout'),
            { 'Y': globalDoc.getTextByKey('ok') },
            logout
        );
    }
}, 1000);

//檢查登入資訊
let checkLoginStatus = new Promise((resolve) => {
    //判斷是否儲存登入者GUID (同Session分頁瀏覽用)
    let loginGuid = sessionStorage.getItem('userGuid');

    if (loginGuid == null) {
        $.ajax({
            type: "POST",
            url: "../api/ApiLogin/ReloadLoginUserInfo",
            dataType: "json",
            success: function (userInfo) {
                sessionStorage.setItem("userGuid", userInfo.userGuid);
                sessionStorage.setItem("userId", userInfo.userId);
                sessionStorage.setItem("userName", userInfo.userName);
                sessionStorage.setItem("depart", userInfo.depart);
                sessionStorage.setItem("title", userInfo.title);
                sessionStorage.setItem("userLng", userInfo.userLng);
                resolve(loginGuid);
            }
        });
    } else {
        resolve(loginGuid);
    }
});

//登出
function logout(btnVal) {
    $.ajax({
        url: '../Home/Logout',
        data: { UserGuid: sessionStorage.getItem('userGuid') },
        type: 'POST',
        success: function (result) {
            window.location.href = result.redirectToUrl;
        }
    });
}

//載入系統參數,新增至主版頁面body底部Hidden標籤
async function getGlobalParam() {
    return await $.ajax({
        type: 'POST',
        async: false,
        url: 'api/Global/GetGlobalParam',
        dataType: 'json',
        success: function (data) {
            $.each(data, function (key, value) {
                let elm = document.createElement('input');

                switch (key) {
                    case 'DateFormat': //日期格式
                        elm.setAttribute('id', 'dateFormat');
                        break;
                    case 'TimeFormat': //時間格式
                        elm.setAttribute('id', 'timeFormat');
                        break;
                    case 'DateTimeFormat': //日期+時間格式
                        elm.setAttribute('id', 'dateTimeFormat');
                        break;
                    case 'WordLength': //Data Grid 中單格可顯示字數上限
                        elm.setAttribute('id', 'wordLength');
                        break;
                    default:
                }
                elm.setAttribute('value', value);
                elm.hidden = true;

                document.body.appendChild(elm);
            });
            //Session到期時間
            idleLimit = data.SessionTimeout;
        }
    });
}

//取得左側導覽列項目
async function getLeftNavbarItems() {
    return await $.ajax({
        type: 'POST',
        url: '../api/Layout/GetSideBarItems',
        dataType: 'json',
        success: function (json) {
            layoutLeftNavbar(json);
        }
    });
}

//輸出左導覽列
function layoutLeftNavbar(jsonItems) {
    let navMenu = document.getElementById('navMenu'); //左側導覽列

    jsonItems.forEach((value) => {
        let module = document.createElement('li');
        let moduleHyperLink = document.createElement('a');
        let moduleIcon = document.createElement('i');
        let moduleText = document.createElement('span');
        let moduleDownIcon = document.createElement('i');
        let mpLevel = document.createElement('div');
        let funcMainList = document.createElement('ul');

        //模組大項
        module.classList.add('menu-lists');

        moduleHyperLink.href = value.Function;

        let iconCss = value.IconKey.split(' ');

        iconCss.forEach((val) => {
            moduleIcon.classList.add(val);
        });

        moduleText.dataset.lngkey = value.DocKey;
        moduleDownIcon.classList.add('fas', 'fa-angle-down');

        moduleHyperLink.appendChild(moduleIcon); //模組左側圖示
        moduleHyperLink.appendChild(moduleText); //模組標題文字
        moduleHyperLink.appendChild(moduleDownIcon); //模組右側下拉選單圖示
        module.appendChild(moduleHyperLink);

        //功能第一層
        mpLevel.classList.add('mp-level');

        value.Sub.forEach((funcClass) => {
            let levelOneItem = document.createElement('li');
            let levelOneHyperLink = document.createElement('a');
            let levelOneIcon = document.createElement('i');
            let levelOneText = document.createElement('span');

            levelOneItem.classList.add('menu-lists');

            levelOneHyperLink.href = funcClass.Function;
            levelOneText.dataset.lngkey = funcClass.DocKey;

            //無子項目
            if (funcClass.Sub == null) {
                //levelOneIcon.classList.add('fas', 'fa-circle');
                levelOneIcon.classList.add('far', 'fa-dot-circle');
                levelOneHyperLink.appendChild(levelOneIcon);
                levelOneHyperLink.appendChild(levelOneText);
                levelOneItem.appendChild(levelOneHyperLink);
            }
            else { //有子項目
                let levelTwoDropDownIcon = document.createElement('i');
                let levelTwoMpLevel = document.createElement('div');
                let levelTwoList = document.createElement('ul');

                //levelOneIcon.classList.add('fas', 'fa-list-ul');
                levelOneIcon.classList.add('fas', 'fa-sitemap');
                levelTwoDropDownIcon.classList.add('fas', 'fa-angle-down');
                levelTwoMpLevel.classList.add('mp-level');

                //子項目逐筆加入清單
                funcClass.Sub.forEach((subFunc) => {
                    let levelTwoItem = document.createElement('li');
                    let levelTwoHyperLink = document.createElement('a');
                    let levelTwoIcon = document.createElement('i');
                    let levelTwoText = document.createElement('span');

                    levelTwoItem.classList.add('menu-lists');

                    //levelTwoIcon.classList.add('fas', 'fa-circle');
                    levelTwoIcon.classList.add('far', 'fa-dot-circle');
                    levelTwoText.dataset.lngkey = subFunc.DocKey;
                    levelTwoHyperLink.href = subFunc.Function;

                    levelTwoHyperLink.appendChild(levelTwoIcon);
                    levelTwoHyperLink.appendChild(levelTwoText);
                    levelTwoItem.appendChild(levelTwoHyperLink);
                    levelTwoList.appendChild(levelTwoItem);
                    levelTwoMpLevel.appendChild(levelTwoList);
                });

                levelOneHyperLink.appendChild(levelOneIcon);
                levelOneHyperLink.appendChild(levelOneText);
                levelOneHyperLink.appendChild(levelTwoDropDownIcon);
                levelOneItem.appendChild(levelOneHyperLink);
                levelOneItem.appendChild(levelTwoMpLevel);
            }

            funcMainList.appendChild(levelOneItem);
        }); // sub forHach End

        mpLevel.appendChild(funcMainList);
        module.appendChild(mpLevel);

        //將模組功能加至導覽列底下
        navMenu.appendChild(module);

    }); // module forHach End
}

//畫面事件註冊
function eventBind() {
    //左側導覽列選擇狀態切換
    document.querySelector('#left-menu').addEventListener('click', function () {
        document.querySelector('#left-menu').classList.toggle('active');
        document.querySelector('#logo').classList.toggle('logotoggle');
        document.querySelector('#left-nav').classList.toggle('navtoggle');
        document.querySelector('#main').classList.toggle('pushmain');
    });

    //左側導覽列縮放
    $('li.menu-lists>a').click(function (e) {
        let target = $(e.target).closest('.menu-lists');
        target.toggleClass('listshow');
        if (!$(target).hasClass('listshow')) {
            target.find('.listshow').removeClass('listshow');
        }
    });

    //頂部頁面按鈕點選後呈現凹陷效果
    document.querySelector('#func-group').addEventListener('click', function () {
        document.querySelector('#func-group').classList.toggle('active');
    });

    //框架右上頁面下拉選單按鈕展開與收合
    $('body').on('click', function (event) {
        if (!$(event.target.parentNode).is('#func-group') && !$(event.target.parentNode).is('button.dropbtn')) {
            $('#func-group').removeClass('active');
        }
    });

    //按下滑鼠或鍵盤鍵時閒置計時歸零
    $('body').on('mousedown keydown', function () {
        idleCounter = 0;
    });

    //下載使用說明書
    document.getElementById('btnHelpFileDownload').addEventListener('click', downloadHelpFile, true);

    //登出
    $('#btnLogout').on('click', logout);
}

//儲存登入資訊
function saveLoginInfo() {
    for (var i = 0; i <= sessionStorage.length; i++) {
        let key = sessionStorage.key(i);
        let val = sessionStorage.getItem(key);
        let elm = document.createElement('input');

        elm.setAttribute('id', key);
        elm.setAttribute('value', val);
        elm.hidden = true;

        document.body.appendChild(elm);
    }
}

//顯示登入者資訊
function showUserInfo() {
    $('#txtName').html(sessionStorage.getItem('userName'));
    $('#txtDepart').html(sessionStorage.getItem('depart'));
    $('#txtTitle').html(sessionStorage.getItem('title'));
}

//取得前台按鈕下拉選單項目
function getNavigationItems() {
    $.ajax({
        type: 'POST',
        url: '../api/Layout/GetFrontOfficeDropDownItems',
        dataType: 'json',
        success: function (response) {
            let dropdownMenu = document.getElementById('dropdownFrontOffice');
            dropdownMenu.innerHTML = '';

            response.rows.forEach((item, idx) => {
                let dropdownItem = document.createElement('a');

                dropdownItem.href = item.URL;
                dropdownItem.target = '_blank';
                dropdownItem.dataset.lngkey = item.DOC_KEY;

                dropdownMenu.appendChild(dropdownItem);
            });
            layoutDoc.converStaticElm('dropdownFrontOffice');
        }
    });
}

//取得目前所在功能代碼
let getCurrentFunc = function () {
    let urlAry = window.location.href.split('/');
    let funcId = urlAry[urlAry.length - 1];
    funcId = funcId.replace('#', '');
    sessionStorage.setItem('CurrentFunction', funcId);

    return new Promise((resolve) => { resolve(); });
}

//標註目前所在項目
let highlightCurrentNavItem = function () {
    //1.找出目前所頁面func name
    funcId = sessionStorage.getItem('CurrentFunction');

    if (funcId == '') { return; }
    else { currentFuncId = funcId; }

    //2.用function name找導覽項目,高亮度標示
    funcId = funcId.split('?')[0];
    let hyperLinkElm = document.querySelector('[href=' + funcId + ']');
    hyperLinkElm.classList.add('nowpage');

    //3.查hiperlink上層li class='menu-lists ,加上listshow class 展開
    let parentElm = hyperLinkElm.parentElement;

    while (parentElm != document.getElementById('navMenu')) {
        //console.info(parentElm.firstChild);

        if (parentElm.nodeName = 'LI' && parentElm.firstChild.nodeName == 'A') {
            parentElm.classList.add('listshow');
        }
        parentElm = parentElm.parentElement;
    }

    return new Promise((resolve) => { resolve(); });
}

//麵包屑導航
let getBreadcrumb = function () {
    //1.找出目前所頁面func name
    let funcId = sessionStorage.getItem('CurrentFunction');

    if (funcId == '') {
        return;
    }

    $.ajax({
        type: 'POST',
        url: '../api/Global/GetBreadcrumb',
        data: { 'FUNCTION_ID': currentFuncId },
        dataType: 'json',
        success: function (response) {
            let breadcrumb = document.createElement('div');
            let items = Object.entries(response.rows[0]);
            breadcrumb.classList.add('bread-crumb');
            breadcrumb.id = 'breadcrumb';

            for (var i = 0; i < items.length; i++) {
                let docKey = items[i][1];

                if (docKey != null) {
                    let text = document.createElement('span');

                    text.dataset.lngkey = docKey;
                    breadcrumb.appendChild(text);

                    if (i != items.length - 1) {
                        let arrow = document.createElement('span');
                        arrow.innerText = ' > ';
                        breadcrumb.appendChild(arrow);
                    }
                }
            }

            document.getElementById('content').insertBefore(breadcrumb, document.getElementById('content').childNodes[0]);
            layoutDoc.converStaticElm('breadcrumb');
        }
    });
}

//下載使用說明書
function downloadHelpFile() {
    //1.抓目前所在功能ID
    let currentFunc = sessionStorage.getItem('CurrentFunction');

    if (currentFunc == '') { return; }

    //2.後端找檔案
    $.ajax({
        type: "POST",
        url: "api/Global/GetHelpFile",
        data: { "FUNCTION_ID": currentFunc },
        dataType: "json",
        success: function (response) {

            console.info(response);

            if (response.filePath == null) {
                JQMessageBox(
                    globalDoc.getTextByKey('prompt'),
                    layoutDoc.getTextByKey('noHelpFile'),
                    { 'ok': globalDoc.getTextByKey('ok') },
                    (val) => { }
                );
                return;
            }

            let hiperLink = document.createElement('a');

            hiperLink.setAttribute('href', response.filePath);
            hiperLink.setAttribute('download', "");
            hiperLink.click();
        }
    });
}

$(document).ready(async function () {
    await getGlobalParam();
    await getLeftNavbarItems();

    //檢查登入人員資訊後顯示
    await checkLoginStatus.then(() => {
        let lng = sessionStorage.getItem('userLng');
        layoutDoc = new Doc('layout', lng);
        saveLoginInfo();
        showUserInfo();
        //切換顯示語言
        return layoutDoc.searchFile();
    }).then((val) => {
        layoutDoc.converStaticElm('top-bar');
        layoutDoc.converStaticElm('left-nav');
        layoutDoc.converStaticElm('right-tab');
        getNavigationItems();
        return getCurrentFunc();
    }).then((val) => {
        return highlightCurrentNavItem();
    }).then((val) => {
        return getBreadcrumb();
    }).then((val) => {
        let lng = sessionStorage.getItem('userLng');
        globalDoc = new Doc('global', lng);
        globalDoc.searchFile();
        eventBind();
    });
});