//將HTML<form>標籤中所有<input>標籤的內容轉換為JSON物件(key為name屬性值,value為value屬性值)
$.fn.serializeObject = function () {
	let output = {};
	let objAry = this.serializeArray();

	//逐一填入object
	$.each(objAry, function () {
		//判斷物件中是否已有相同的key
		if (!(this.name in output)) {
			output[this.name] = this.value;
		}
	});
	return output;
};