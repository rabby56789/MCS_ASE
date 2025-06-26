/*
 * 作用:
 * (1) toastr元件客製化設定(需先引用toastr.js)
 * (2) 互動式MessageBox (Bootstrap Modal 客製化,需先引用bootstrap.min.js)
 * 
 * 相依元件:
 * jquery.min.js
 * bootstrap.min.js
 * toastr-2.1.4.min.js
 * 
 * toastr元件用法:
 * toastr.success("Success");
 * toastr.warning("Warning");
 * toastr.error("Error");
 */

//toastr 訊息框初始設定
function toastrInit() {
    toastr.options = {
        // 參數設定[註1]
        "closeButton": false, // 顯示關閉按鈕
        "debug": false, // 除錯
        "newestOnTop": false,  // 最新一筆顯示在最上面
        "progressBar": true, // 顯示隱藏時間進度條
        "positionClass": "toast-top-center", // 位置的類別
        "preventDuplicates": false, // 隱藏重覆訊息
        "onclick": null, // 當點選提示訊息時，則執行此函式
        "showDuration": "300", // 顯示時間(單位: 毫秒)
        "hideDuration": "3000", // 隱藏時間(單位: 毫秒)
        "timeOut": "5000", // 當超過此設定時間時，則隱藏提示訊息(單位: 毫秒)
        "extendedTimeOut": "2000", // 當使用者觸碰到提示訊息時，離開後超過此設定時間則隱藏提示訊息(單位: 毫秒)
        "showEasing": "swing", // 顯示動畫時間曲線
        "hideEasing": "linear", // 隱藏動畫時間曲線
        "showMethod": "fadeIn", // 顯示動畫效果
        "hideMethod": "fadeOut" // 隱藏動畫效果
    }
}

//建立Bootstrap的Modal物件
function createModal() {
    let main = document.createElement("div");
    let dialog = document.createElement("div");
    let content = document.createElement("div");
    let header = document.createElement("div");
    let body = document.createElement("div");
    let footer = document.createElement("div");
    let title = document.createElement("h4");
    let closeBtn = document.createElement("button");
    let msg = document.createElement("span");

    //主框
    main.id = 'modal_confirm';
    $(main).addClass('modal');
    $(main).addClass('fade');
    $(main).attr('role', 'dialog');

    $(dialog).addClass('modal-dialog');
    $(content).addClass('modal-content');
    $(header).addClass('modal-header');
    $(body).addClass('modal-body');
    $(footer).addClass('modal-footer');

    //標題文字與關閉按鈕
    title.id = 'title';
    $(title).addClass('modal-title');

    $(closeBtn).addClass('close');
    $(closeBtn).val('N');
    $(closeBtn).attr('data-dismiss', 'modal');
    $(closeBtn).html('&times;');

    //內文
    msg.id = 'msg';

    //組合
    header.append(title);
    header.append(closeBtn);
    body.append(msg);
    content.append(header);
    content.append(body);
    content.append(footer);
    dialog.append(content);
    main.append(dialog);

    return main;
}

/**
 * 顯示BootStrap彈出視窗
 * @param {string} title 彈出視窗標題
 * @param {string} content 彈出視窗內文
 * @param {object} buttons 顯示按鈕JSON物件清單,第一個按鈕顏色高亮度顯示(key:實際值/value:顯示文字)
 * @param {any} callback 按下按鈕後對應的動作(function name),帶入參數為button value屬性
 */
function JQMessageBox(title, content, buttons, callback) {
    let obj = $('#modal_confirm')[0];

    //彈出視窗物件不存在時自動產生
    if (!obj) {
        obj = createModal();
        $(obj).modal({
            show: false, //預設不顯示
            backdrop: 'static', //彈出視窗點背景無法關閉
            keyboard: false, //按Esc鍵不會關閉視窗
        });

    }

    //移除全部按鈕
    $(obj).find('.modal-footer button').remove();

    //生成底部自訂按鈕
    let btnContainer = $(obj).find('.modal-footer');
    let btnAry = Object.keys(buttons);

    for (let idx = 0; idx < btnAry.length; idx++) {
        let btn = document.createElement('button');
        btn.value = btnAry[idx];
        btn.innerText = buttons[btnAry[idx]];

        $(btn).addClass('btn');

        if (idx == 0) {
            $(btn).addClass('btn-success');
        } else {
            $(btn).addClass('btn-secondary');
        }

        $(btn).attr('data-dismiss', 'modal');
        btnContainer.append(btn);
    }

    //視窗按鈕事件綁定
    $(obj).find('button').each(function (index, item) {
        item.addEventListener('click', function () { callback($(item).attr('value')); }, false);
    });

    $(obj).find('#title').html(title); //標題
    $(obj).find('#msg').html(content); //內文
    $(obj).modal('show');
}

$(function () {
    toastrInit();
});