//jq-easyui.js 2021/06/09 easyui 延伸應用
//整合bootstrap的Collapse插件,使用前須先引用bootstrap.js

/**
 * 綁定Bootstrap的Collapse插件事件,隱藏與展開時自動調整DataGrid尺寸
 * @param {any} datagrid EasyUI的DataGrid物件
 */
function bindResizeEvent(datagrid) {
	$('.datagrid-body .collapse').on('shown.bs.collapse', function () {
		setDatagridBodyHeight(datagrid);
	});
	$('.datagrid-body .collapse').on('hidden.bs.collapse', function () {
		setDatagridBodyHeight(datagrid);
	});
}

/**
 * Easy UI DataGrid 自訂Column顯示方式,直接套用至Easy UI DataGrid設定columns的formatter)
 * @param {any} value 欄位值
 * @param {any} row 
 * @param {any} index 列數序號
 */
function textareaFormatter(value, row, index) {
	let str = '';
	let wordLength = ($('#wordLength').val() == '') ? $('#wordLength').attr('defualtvalue') : $('#wordLength').val();
	str = '<div data-toggle="collapse" data-target="#collapse_item' + index + '" aria-expanded="true"  aria-controls="collapse_item' + index + '">'
	str += (value.length > wordLength) ? value.substring(0, wordLength - 3) + '...' : value;
	str += '</div>'
	str += '<div id="collapse_item' + index + '" class="collapse" style="padding:0; border:0; border-top:1px solid; word-break:break-all; word-wrap:break-word;white-space:pre-wrap;text-align: left;">' + value + '</div>'
	return str;
}

/**
 * 重設ezsyui的 datagrid 高度-依照內容高度調整，如果內容高度小於 預設總行高(單行高*每頁筆數)，則以預設高度為主(用於onLoadSuccess事件中)
 * @param {any} datagrid EasyUI的DataGrid物件
 */
function setDatagridBodyHeight(datagrid) {
	let panelHeight = $(datagrid).datagrid('getPanel').parents('.tableParent').css("height").replace('px', '');
	let clientHeight = $(datagrid).datagrid('getPanel').find('.datagrid-body')[1].clientHeight;
	let bodyHeight = $(datagrid).datagrid('getPanel').find('.datagrid-btable').height();//$(datagrid).datagrid('getPanel').find('.datagrid-body')[1].scrollHeight;

	if ($(datagrid).datagrid('getPanel').find('.datagrid-row').length < $(datagrid).datagrid('options').pageSize) {
		let tatolrowHeight = $(datagrid).datagrid('getPanel').find('.datagrid-row').height() * $(datagrid).datagrid('options').pageSize;
		bodyHeight = (bodyHeight < tatolrowHeight) ? tatolrowHeight : bodyHeight;
	}

	$(datagrid).datagrid('getPanel').parents('.tableParent').css("height", panelHeight - clientHeight + bodyHeight);
	$(datagrid).datagrid("resize");
}