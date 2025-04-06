window.onload = function () {
    window.YaAuthSuggest.init(
        {
            client_id: "03948de1ac304db7a7dfedb458d11fd3",
            response_type: "token",
            redirect_uri: "https://192.168.31.188/YandexID"
        },
        "http://192.168.31.188",
        {
            view: "button",
            parentId: "buttonContainerId",
            buttonSize: 'l',
            buttonView: 'main',
            buttonTheme: 'light',
            buttonBorderRadius: "0",
            buttonIcon: 'ya',
        }
    )
        .then(({ handler }) => handler())
        .then(data => {
            // console.log(data);
            window.location = "/YandexID?handler=Token&t=" + data.access_token;
        })
        .catch(error => console.log('Обработка ошибки', error))
};