window.onload = function () {
    window.YaSendSuggestToken("https://localhost", {
        "kek": true
    }).then((v) => {
        window.close();
    })
};