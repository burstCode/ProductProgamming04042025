//using Azure;
//using Azure.AI.Inference;
//using Microsoft.Extensions.AI;

////using Models;
//using Newtonsoft.Json;

//namespace ProductProgamming04042025.AI
//{
//    public class Bot
//    {
//        // Переменные для доступа к моделям GitHub
//        readonly private AzureKeyCredential _credential;
//        readonly private string _modelName;

//        readonly private Uri _modelEndpoint;
//        readonly private IChatClient _chatClient;

//        // Конструктор
//        public Bot(string token, string modelName)
//        {
//            _credential = new(token);
//            _modelName = modelName;

//            // Этот адрес не изменяется
//            _modelEndpoint = new Uri("https://models.inference.ai.azure.com");

//            _chatClient = new ChatCompletionsClient(_modelEndpoint, _credential)
//                .AsChatClient(_modelName);
//        }

//        // Отправка запроса к нейросети
//        public async string SendRequest(string request)
//        {
//            string prompt =
//                $"Система: "+
//                $"Пользователь: {request}";

//            string jsonAnswer = "";

//            // Получение ответа от нейросети
//            IAsyncEnumerable<StreamingChatCompletionUpdate> response = _chatClient.CompleteStreamingAsync(prompt);

//            if (response == null)
//            {
//                throw new NullReferenceException("Переменная response приняла значение null, ответ от нейросети получить не удалось");
//            }

//            string collectedResponse = await CollectResponse(response);

//            // Десериализация в модель напоминания
//            try
//            {
//                notification = Newtonsoft.Json.JsonConvert.DeserializeObject<Notification>(collectedResponse);
//            }
//            catch (JsonSerializationException e)
//            {
//                Console.WriteLine(e.Message);
//            }

//            if (notification != null)
//            {
//                return notification;
//            }

//            return new Notification();
//        }

//        // Собирает с ответом нейросети из вида
//        // "Microsoft.Extensions.AI.AzureAIInferenceChatClient+<CompleteStreamingAsync>d__12"
//        // в вид { json-чик }
//        private async Task<string> CollectResponse(IAsyncEnumerable<StreamingChatCompletionUpdate> response)
//        {
//            string collectedResponse = "";

//            await foreach (var item in response)
//            {
//                collectedResponse += item;
//            }

//            return collectedResponse;
//        }
//    }
//}
