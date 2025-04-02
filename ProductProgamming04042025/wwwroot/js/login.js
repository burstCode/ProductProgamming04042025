// @ts-check

window.addEventListener('load', ()=>{
    const tm = new ToastManager();

    /** @type {HTMLFormElement} */
    // @ts-ignore
    const form = document.getElementById('form');

    form?.addEventListener('submit', /** @type {SubmitEvent} */(e)=>{
        const formData = new FormData(form);
        const s = formData.get('login')?.toString();
        if(!s) return;
        if(s.toLowerCase() != "qliane"){
            form.querySelector('input[name=login]')?.classList.add('error');
            tm.addToast("Ты кто такой?! Пшел отседова!", 0, true);
        }else{
            form.querySelector('input[name=login]')?.classList.remove('error');
            tm.addToast(s + ", здравствуйте!", 1, true);
            window.location.href = "/";
        }
        e.preventDefault();
    })
})