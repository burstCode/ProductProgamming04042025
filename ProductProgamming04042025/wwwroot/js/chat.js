// @ts-check

window.addEventListener('load', () => {
    const chatElement = document.querySelector('#chat>.messages')
    const loadMore = document.querySelector('#chat .loadmore')
    const chat = new Chat(chatElement, loadMore)
})

class Chat {
    constructor(element, loadMore) {
        /** @type {HTMLElement} Элемент с сообщениями */
        this.element = element;
        this.loadMoreButton = loadMore;
        
        /**
         * @type {ToastManager}
        */
        // @ts-ignore
        this.toastManager = new ToastManager();

        /**
         * @type {HTMLFormElement}
         * 
         */
        // @ts-ignore
        this.form = document.getElementById('chat-form');
        if(this.form == null) return;
        this.form?.addEventListener('submit', (e)=>{
            e.preventDefault();
            fetch(this.form.action, {
                method: "post",
                body: new FormData(this.form)
            }).then((v)=>{
                v.json().then((json)=>{
                    if(json.error){
                        this.toastManager.addToast("Мы подготавливаем для вас план, подождите", 0, true);
                    }else{
                        this.showRecord(json, false);
                    }
                })
            })
            this.form.reset();
        })

        this.loadMoreButton.addEventListener('click', () => {
            this.load();
        })
        /**
         * @type {string} Время самой поздней записи
         */
        this.time = "";
        /**
         * @type {boolean} Если записей больше нет.
         */
        this.is_end = false;
        this.load();
        // @ts-ignore
        window.loadMore = () => {
            this.load();
        }
    }

    /**
     * @returns {object}
     */
    #getRecords(handler) {
        fetch("/Chat?handler=ChatRecord").then((value) => {
            value.json().then((json) => {
                handler(json);
            });
        });
    }
    /**
     * Возвращает записи до определённого времени
     * @param {string} time Время
     */
    #getRecordsTime(time, handler) {
        fetch("/Chat?handler=ChatRecordWidthTime&time=" + time).then((value) => {
            value.json().then((json) => {
                handler(json);
            });
        });
    }

    /**
     * 
     * @param {object} m 
     * @param {number} m.id
     * @param {string} m.text 
     * @param {boolean} m.isApplied 
     * @param {boolean} m.isAnswerReady 
     * @param {*} is_bot 
     */
    showMessage(m, is_bot, is_start = true) {
        const div = document.createElement('div');
        /** @type {HTMLElement} */
        let message;
        if(is_start){
            if (this.element.firstChild) {
                //@ts-ignore
                message = this.element.insertBefore(div, this.element.firstChild);
            } else {
                message = this.element.appendChild(div);
            }
        }else{
            message = this.element.appendChild(div);
        }

        message.classList.add('message')
        if (is_bot) {
            message.classList.add('bot-answer');
            if (!m.isAnswerReady) {

                div.innerHTML = `
                    <div class="content">
                        <div class="text">
                            Подбираем для вас план...
                        </div>
                        <button class="apply-plan" onclick="applyPlan(${m.id})">
                            <div class="content">
                                Применить план
                            </div>
                        </button>
                        
                    </div>
                    `;

                const content_message = div.firstElementChild;
                const text = content_message?.firstElementChild;
                if(text == null) return;
                if(content_message == null) return;
                // @ts-ignore
                const loader = new Loader(content_message, (stop) => {
                    const url = "https://192.168.31.188/Chat?handler=IsReady&id=" + m.id;
                    fetch(url).then((c) => {
                        c.json().then((json) => {
                            console.log(json)
                            if (json.isReady) {
                                stop();
                                text.innerHTML = json.response;
                            }
                        })
                    })
                });
                loader.start();
            } else {
                if (m.isApplied) {
                    div.innerHTML = `
                    <div class="content">
                        <div class="text">
                            ${m.text}
                        </div>
                        <button onclick="window.location='/'" class="apply-plan">
                            <div class="content">
                                План применён
                            </div>
                        </button>
                        
                    </div>
                    `;
                } else {
                    div.innerHTML = `
                    <div class="content">
                        <div class="text">
                            ${m.text}
                        </div>
                        <button class="apply-plan" onclick="applyPlan(${m.id})">
                            <div class="content">
                                Применить план
                            </div>
                        </button>
                        
                    </div>
                    `;
                }
            }
        } else {
            div.innerHTML = `
            <div class="content">
                <div class="text">
                    ${m.text}
                </div>
            </div>
            `;
        }

    }
    showRecord(r, is_start = true) {
        let bot = {
            text: r.modelResponseText,
            isApplied: r.isApplied,
            id: r.id,
            isAnswerReady: r.isAnswerReady,
        }

        let u = {
            text: r.userRequest,
            isApplied: r.isApplied,
            id: r.id,
            isAnswerReady: r.isAnswerReady,
        }

        if(is_start){
            this.showMessage(bot, true, is_start);
            this.showMessage(u, false, is_start);
        }else{
            this.showMessage(u, false, is_start);
            this.showMessage(bot, true, is_start);
            
        }
    }

    load() {
        console.log(this);
        const handler = (
            /**
             * @type {Array} 
             */data) => {
            if (data.length == 0) {
                this.is_end = true
                this.loadMoreButton.remove();
                return;
            };
            this.time = data[0].createdAt;
            console.log(data);
            for (const record of data.reverse()) {
                this.showRecord(record);
            }
        }
        if (this.time == "") {
            this.#getRecords(handler);
        } else {
            this.#getRecordsTime(this.time, handler);
        }
    }
}