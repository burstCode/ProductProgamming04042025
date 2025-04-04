// @ts-check

class Loader{
    /**
     * 
     * @param {HTMLElement} element 
     */
    constructor(element){
        this.element = element;
    }

    start(){
        this.element.classList.add('loader-cover');
        const loader = document.createElement('div');
        this.loaderElement = this.element.appendChild(loader);
        this.loaderElement.classList.add('loader');
    }

    stop(){
        this.loaderElement?.remove();
        this.element.classList.remove('loader-cover');
    }
}