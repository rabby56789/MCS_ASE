
//判斷是否為空值
function IsNullorUndefined(param) {
	if (param == undefined || param.trim() == '') { return true; }
	else { return false; }
}

//判斷 是否為 小於100(允許小數一位) 的正浮點數
function valid_Under100_Float1(str) {
	var regExp = /^([1-9]?\d)$|^([1-9]?\d\.[1-9])$/;
	if (regExp.test(str))
		return true;
	else
		return false;
}

//判斷 是否包含特殊字元 (［］｛｝#%^*+=) 
function isIncludeSpecialChar() {
	let regExp = /[［］｛｝#%\^*+=]/;
	if (regExp.test(str))
		return true; //有特殊字元
	else
		return false;
}
