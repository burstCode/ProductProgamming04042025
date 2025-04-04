
window.addEventListener('load', ()=>{
    const card_selected = document.querySelector('#plan>.cards>.card.selected')
    let offset = 0;
    if(window.innerWidth < 768){
        offset = document.getElementById('mobile-shell').offsetHeight+20;
    }
    if(card_selected){
        window.scrollTo({
            "top": card_selected.offsetTop - offset,
            "behavior": "smooth"
        })
    }
})