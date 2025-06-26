//多語系切換元件
class Doc {
    /**
     * 建構式
     * @param {string} func 功能代碼
     * @param {string} firstLang 優先顯示語系(依照locale設定標準)
     */
    constructor(func, firstLang) {
        this.func = func; //對應功能頁面代碼
        this.langList = new Array(); //屬性:語系顯示順位,尋找字典檔用
        this.docFile = new Object(); //屬性:字典檔

        //建立字典檔抓取順位清單,若有自訂優先顯示語系則排第一順位
        if (firstLang != null || firstLang != "") {
            this.langList.push(firstLang.toLowerCase());
        }

        //將剩下的語言順位設定加入字典檔搜尋清單順位
        navigator.languages.forEach((val) => {
            if (val.toLowerCase() != this.langList[0]) {
                this.langList.push(val);
            }
        });
    }

    //依照語系顯示順序尋找,沒找到繼續往下個順位搜尋,找到時停止搜尋
    async searchFile() {
        this.getSucceed = false;
        for (var i = 0; i < this.langList.length; i++) {
            await this.getFile(this.langList[i]).then((val) => {
                this.docFile = val;
                this.getSucceed = true;
            }).catch((val) => {

            });

            if (this.getSucceed) {
                return this.docFile;
                break;
            }
        }
    }

    /**
     * 從Server端取得字典檔
     * @param {string} name 字典檔名稱
     */
    async getFile(name) {
        let url = './i18n/' + this.func + '/' + name + '.json';

        return await new Promise((resolve, reject) => {
            $.ajax({
                type: "GET",
                url: url,
                dataType: "json",
                success: function (response) {
                    resolve(response);
                }, error: function (response) {
                    reject(response);
                }.bind(this)
            });
        });
    }

    /**
     * 轉換靜態HTML元素顯示語系
     * @param {string} target 元素id屬性值
     */
    converStaticElm(target) {
        let area = document.getElementById(target);
        let elms = area.querySelectorAll('[data-lngKey]');

        elms.forEach((elm) => {
            let textElm = document.createElement("span");
            let key = elm.dataset.lngkey;
            let text = this.docFile[key];

            if (text == undefined) {
                text = key;
            }

            if (elm.tagName == "SPAN" | elm.tagName == "LABEL") {
                elm.innerText = text;
            } else {
                textElm.innerText = text;
                elm.appendChild(textElm);
            }

            //elm.innerText += text;
        });
    }

    /**
     * 依鍵名在字典檔中尋找對應顯示文字
     * @param {string} lngKey
     */
    getTextByKey(lngKey) {
        let text = this.docFile[lngKey];

        if (text == undefined) {
            text = lngKey;
        }

        return text;
    }
}