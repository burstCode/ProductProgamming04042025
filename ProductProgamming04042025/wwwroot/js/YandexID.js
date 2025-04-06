window.onload = function () {
    window.YaSendSuggestToken("https://192.168.31.188", {
        "kek": true
    }).then((v) => {
        window.close();
    })
};