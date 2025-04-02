// @ts-check

class ToastManager{
    constructor(){
        /**
         * @type {Array.<Toast>};
         */
        this.toasts = new Array();

        /** @type {boolean} Происходит ли сейчас отображение */
        this.is_showing = false;
    }
    /**
     * 
     * @param {string} text 
     * @param {number} image_id 
     */
    addToast(text, image_id, is_centred){
        this.toasts.push(new Toast(text, image_id, is_centred));
        this.show();
    }
    
    show(){
        if(this.toasts.length <= 0 || this.is_showing) return;
        this.is_showing = true;
        const handle = ()=>{
            const t = this.toasts.shift();
            if(t === undefined) return;
            t.show(()=>{
                console.log(t);
                this.is_showing = false;
                handle();
                return;
            })
        }
        handle();
    }
}

class Toast{
    /**
     * 
     * @param {string} text Текст тоста
     * @param {number} image_id Номер иконки для тоста
     */
    constructor(text, image_id, is_centred){
        this.text = text;
        this.image_id = image_id;
        this.is_centred = is_centred;
    }
    /**
     * 
     * @param {function} onhideHandler обработчки, вызываестя, когда тост скрылся
     */
    show(onhideHandler){
        const element = this.#createElement();
        const toast = document.body.appendChild(element);
        if(this.is_centred) toast.classList.add('centred');
        toast.classList.add('toast');
        setTimeout(() => {
            toast.classList.add('state-1');
            // 4.5 секунды ожидания
            setTimeout(() => {
                toast.classList.remove('state-1')
                toast.classList.add('state-2');
                // Ждём пока уедет вниз
                setTimeout(() => {
                    console.log(this, onhideHandler.toString());
                    toast.remove();
                    onhideHandler();
                }, 500);
            }, 3500);
        }, 100);
    }

    #createElement(){
        const toast_div = document.createElement('div');
        toast_div.innerHTML = `
        <div class="content">
            <div class="image">
                <img src="./images/toasts/${this.image_id}.svg" alt="">
            </div>
            <div class="text">
                <span>${this.text}</span>
            </div>
        </div>`
        return toast_div;
    }
}