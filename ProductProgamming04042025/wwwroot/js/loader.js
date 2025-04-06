// @ts-check

class Loader{
    /**
     * 
     * @param {HTMLElement} element 
     */
    constructor(element, handler){
        this.element = element;
        this.handler = handler;
        this.interval = null;
    }

    start(){
        this.element.classList.add('loader-cover');
        const loader = document.createElement('div');
        this.loaderElement = this.element.appendChild(loader);
        this.loaderElement.classList.add('loader');
        this.interval = setInterval(() => {
            this.handler(this.stop.bind(this));
        }, 1000);
    }

    stop(){
        if(this.interval) clearInterval(this.interval);
        this.loaderElement?.remove();
        this.element.classList.remove('loader-cover');
    }
}