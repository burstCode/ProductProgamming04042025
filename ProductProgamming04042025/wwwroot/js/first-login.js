window.addEventListener('load', ()=>{
    const age_element = document.getElementById('age');
    const height_element = document.getElementById('height');
    const weight_element = document.getElementById('weight');

    const text_element = document.getElementById('text');

    const form = document.getElementById('form');

    const tm = new ToastManager();

    form.onsubmit = (e)=>{
        const age = age_element.value
        const height = height_element.value
        const weight = weight_element.value
        const text = text_element.value
        if(age <= 0 || weight <= 0 || height <= 0 || text == ""){
            tm.addToast('Вы ввели отрицательное значение.', 3, true)
            e.preventDefault();
        }
        if(age > 130){
            tm.addToast('Возраст не может быть больше 130', 3, true)
            e.preventDefault();
        }
        if(weight > 300){
            tm.addToast('Вес не может быть больше 300 кг', 3, true)
            e.preventDefault();
        }
        if(height > 300 || height < 50){
            tm.addToast('Вес не может быть больше 300 см и меньше 50 см', 3, true)
            e.preventDefault();
        }
    }
})