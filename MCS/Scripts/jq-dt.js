//jq-dt.js 20210610 日期時間轉換及元件套用

let nomalFormat = 'YYYY/MM/DD hh:mm:ss';
var start = new Date();
start.setHours(0, 0, 0);
let language = 'zh';


//將後端回傳(格式為 yyyy/MM/dd/ HH:mm:ss)的日期字串 傳成 javascript Date 元件
String.prototype.DTformatCtoJ = function () {
	return new Date(moment(this, nomalFormat));
}

//javascript 的日期 轉成 C# 的格式字串 (傳回後端)
Date.prototype.DTformatJtoC = function () {
	return moment(this).format().substr(0, 19).replace('T', ' ');
	//moment(this).format()
	//"2021-06-01T00:00:00+08:00"
	//return moment(this).format(getMomentDateTimeFormat());
};

//日期時間格式轉換：C# → Moment
function getMomentDateTimeFormat() {
	let fmt = $('#dateTimeFormat').val();
	// 轉換(年)
	fmt = fmt.replace(/y/g, 'Y');

	// 轉換(日)
	fmt = fmt.replace(/d/g, 'D');

	// 轉換(12/24小時制)
	fmt = fmt.replace('tt', 'a');

	return fmt;
}

//日期格式轉換：C# → Moment
function getMomentDateFormat() {
	let fmt = $('#dateFormat').val();
	// 轉換(年)
	fmt = fmt.replace(/y/g, 'Y');

	// 轉換(日)
	fmt = fmt.replace(/d/g, 'D');

	// 轉換(12/24小時制)
	fmt = fmt.replace('tt', 'a');

	return fmt;
}

//時間格式轉換：C# → Moment
function getMomentTimeFormat() {
	let fmt = $('#timeFormat').val();
	// 轉換(年)
	fmt = fmt.replace(/y/g, 'Y');

	// 轉換(日)
	fmt = fmt.replace(/d/g, 'D');

	// 轉換(12/24小時制)
	fmt = fmt.replace('tt', 'a');

	return fmt;
}

//日期格式轉換：C# → air datepicker
function getAirDatepickerDateFormat() {
	let fmt = $('#dateFormat').val();
	if (fmt == '') { fmt = $('#dateFormat').attr('defualtvalue'); }
	fmt = fmt.replace('dddd', 'DD');//長星期 
	fmt = fmt.replace('ddd', 'D');//短星期
	fmt = fmt.replace('MMMM', 'MtM');//長月份
	fmt = fmt.replace('MM', 'mm');//短月份
	fmt = fmt.replace('MtM', 'MM');//長月份
	return fmt;
}

//時間格式轉換：C# → air datepicker
function getAirDatepickerTimeFormat() {
	let fmt = $('#timeFormat').val();
	if (fmt == '') { fmt = $('#timeFormat').attr('defualtvalue'); }
	fmt = fmt.replace('tt', 'aa');//12 小時制 'hh:ii' 24 小時制
	fmt = fmt.replace('HH', 'hh');
	fmt = fmt.replace('H', 'h');
	fmt = fmt.replace('mm', 'ii');
	fmt = fmt.replace(':ss', '');
	return fmt;
}

$(function () {
	let dateFormat = getAirDatepickerDateFormat();
	let timeFormat = getAirDatepickerTimeFormat();

	//#region 日期時間元件綁定

	//#region 日期
	$('.date').datepicker({
		dateFormat: dateFormat,
		startDate: start,
		todayButton: start,
		clearButton: true,
		autoClose: true,
		language: language,
		onShow: function () {
			$('#queryDate-Start').val();
		}
	});

	//手動輸入日期 input keyin 時回寫至日期元件 
	$('.date').change(
		function (e) {
			let obj = e.target;
			if (obj.value == undefined || obj.value == '') return;
			let fmt = getMomentDateFormat();
			let dt = moment(obj.value, fmt);
			if (dt.format() == "Invalid date") {
				alert('輸入錯誤，請參考正確格式' + fmt);
				$(obj).val(moment($(obj).datepicker().data('datepicker').selectedDates[0]).format(fmt));
				$(obj).click();
			}
			else {
				$(obj).datepicker().data('datepicker').selectDate(new Date(dt));
			}
		});
	//#endregion

	//#region 時間
	$('.time').datepicker({
		timepicker: true,
		onlyTimepicker: true,
		language: language,
		startDate: start,
		timeFormat: timeFormat,
		clearButton: true,
		autoClose: true,
	});

	$('.time').change(
		function (e) {
			let obj = e.target;
			if (obj.value == undefined || obj.value == '') return;
			let fmt = getMomentTimeFormat().replace(":ss", '');
			let dt = moment(obj.value, fmt);
			if (dt.format() == "Invalid date") {
				alert('輸入錯誤，請參考正確格式' + fmt);
				$(obj).val(moment($(obj).datepicker().data('datepicker').selectedDates[0]).format(fmt));
				$(obj).click();
			}
			else {
				$(obj).datepicker().data('datepicker').selectDate(new Date(dt));
			}
		});
	//#endregion

	//#region 日期時間
	$('.date-time').datepicker({
		timepicker: true,
		language: language,
		startDate: start,
		dateFormat: dateFormat,
		timeFormat: timeFormat,
		todayButton: start,
		clearButton: true,
		autoClose: true,
		toggleSelected: false,
	});
	//手動輸入日期 input keyin 時回寫至日期元件 
	$('.date-time').change(
		function (e) {
			let obj = e.target;
			if (obj.value == undefined || obj.value == '') return;
			let fmt = getMomentDateTimeFormat().replace(":ss", '');
			let dt = moment(obj.value, fmt);
			if (dt.format() == "Invalid date") {
				alert('輸入錯誤，請參考正確格式' + fmt);
				$(obj).val(moment($(obj).datepicker().data('datepicker').selectedDates[0]).format(fmt));
				$(obj).click();
			}
			else {
				$(obj).datepicker().data('datepicker').selectDate(new Date(dt));
			}
		});
	//#endregion

	//#region 日期區間
	$('.date-range').datepicker({
		language: language,
		startDate: start,
		dateFormat: dateFormat,
		timeFormat: timeFormat,
		todayButton: start,
		clearButton: true,
		autoClose: true,
		range: true,
		multipleDatesSeparator: ' ~ ',
		toggleSelected: true,
	});
	$('.date-range').change(
		function (e) {
			let obj = e.target;
			if (obj.value == undefined || obj.value == '') return;
			let fmt = getMomentDateFormat();
			let ls_str = obj.value.split($(obj).datepicker().data('datepicker').opts.multipleDatesSeparator.replace(' ', ''));
			let ls_date = [];
			$.each(ls_str, function (idx, value) {
				let dt = moment(value, fmt);
				if (dt.format() == "Invalid date") {
					alert('輸入錯誤，請參考正確格式' + fmt);
					let ls = $(obj).datepicker().data('datepicker').selectedDates;
					map1 = ls.map(value => moment(value).format(fmt));
					$(obj).val(map1.join($(obj).datepicker().data('datepicker').opts.multipleDatesSeparator));
					ls_date = $(obj).datepicker().data('datepicker').selectedDates;
					$(obj).click();
					return false;
				}
				else {
					ls_date.push(new Date(moment(value, fmt)));
				}
			});
			$(obj).datepicker().data('datepicker').selectDate(ls_date);
		});
	//#endregion

	//#region 日期時間區間
	$('.date-time-range').datepicker({
		timepicker: true,
		language: language,
		startDate: start,
		dateFormat: dateFormat,
		timeFormat: timeFormat,
		todayButton: start,
		clearButton: true,
		range: true,
		multipleDatesSeparator: ' ~ ',
		toggleSelected: true,
	});

	//#endregion

	//#endregion
});
