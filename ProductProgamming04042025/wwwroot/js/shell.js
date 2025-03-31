// @ts-check

window.addEventListener('load', ()=>{
    const shell = document.getElementById('shell')
    if(!shell) throw new Error("Shell not found");
    /**
     * @param {HTMLElement} element 
     */
    const changeHide =   (element)=>{
        if(element.classList.contains('hide')){
            element.classList.remove('hide')
        }else{
            element.classList.add('hide');
        }
    }

    const hideMenuHandler = ()=>{
        const menu = document.getElementById('menu');
        const buttons = shell.getElementsByClassName('hide-shell');
        console.log(buttons)
        if(!menu) throw new Error("Menu not found");
        for (const button of buttons) {
            button.addEventListener('click', (e)=>{
                changeHide(menu);
                e.preventDefault();
            })
        }
    }


    hideMenuHandler();
})