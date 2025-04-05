using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;

using ProductProgamming04042025.Pages.Models;
using Newtonsoft.Json;

namespace ProductProgamming04042025.AI
{
    public class Bot
    {
        // Переменные для доступа к моделям GitHub
        readonly private AzureKeyCredential _credential;
        readonly private string _modelName;

        readonly private Uri _modelEndpoint;
        readonly private IChatClient _chatClient;

        // Конструктор
        public Bot(string token, string modelName)
        {
            _credential = new(token);
            _modelName = modelName;

            // Этот адрес не изменяется
            _modelEndpoint = new Uri("https://models.inference.ai.azure.com");

            _chatClient = new ChatCompletionsClient(_modelEndpoint, _credential)
                .AsChatClient(_modelName);
        }

        // Отправка запроса к нейросетиa
        public async Task<FitnessPlan > SendRequest(string request, UserProfile userProfile)
        {
            string prompt =
                $"Система: Вы - спортивный тренер и диетолог, разрабатывающий план тренировок в совокупности с питанием на неделю. " +
                $"Вы подбираете наиболее оптимальные набор упражнений и питания. Создайте персонализированный недельный план. " +
                $"Учти параметры пользователя, которые будет переданы далее, а из запроса пользователя извлеки цель, которой он хочет достичь " +
                $"(например набор мышечной массы, похудение, и так далее)." +
                $"Параметры пользователя:\n" +
                $"- Возраст: {userProfile.Age};\n" +
                $"- Пол: {(userProfile.Sex ? "мужской" : "женский")};\n" +
                $"- Рост: {userProfile.Height} см;\n" +
                $"- Вес: {userProfile.Weight} кг.\n" +
                $"Оформи ответ в формате Json в соответствии со следующим примером:\n" +
                $"\"Номер дня недели (начинай с 0, имеется ввиду, что  0 - это понедельник)\": {{\n" +
                $"\"exercises\":[\n" +
                $"                {{\n" +
                $"                    \"name\": \"Упражнение 1\",\n" +
                $"                    \"count\": \"Количество повторений x Количество подходов\"\n" +
                $"                }},\n" +
                $"                {{\n" +
                $"                    \"name\": \"Упражнение 2\",\n" +
                $"                    \"count\": \"Количество повторения x Количество подходов\"\n" +
                $"                }},\n" +
                $"                ..." +
                $"              ]" +
                $"\"diet\":[\n" +
                $"                {{\n" +
                $"                    \"name\": \"Блюдо 1 (завтрак)\",\n" +
                $"                    \"count\": \"Калорийность. Белки/жиры/углеводы\"\n" +
                $"                }},\n" +
                $"                {{\n" +
                $"                    \"name\": \"Блюдо 2 (обед)\"," +
                $"                    \"count\": \"Калорийность. Белки/жиры/углеводы\"\n" +
                $"                }}\n," +
                $"                {{\n" +
                $"                    \"name\": \"Блюдо 3 (ужин)\"," +
                $"                    \"count\": \"Калорийность. Белки/жиры/углеводы\"\n" +
                $"                }}\n," +
                $"            ],\n" +
                $"        \"extra\": \"exercises или diet могут быть пустыми, если это так, то запиши ответ в это поле\"\n" +
                $"}}\n" +
                $"Важно: нужно вернуть без какого-либо форматирования для последующего корректного парсинга.\n" +
                $"Пользователь: {request}";

            string jsonAnswer;

            // Получение ответа от нейросети
            IAsyncEnumerable<StreamingChatCompletionUpdate> response = _chatClient.CompleteStreamingAsync(prompt);

            if (response == null)
            {
                throw new NullReferenceException("Переменная response приняла значение null, ответ от нейросети получить не удалось");
            }

            string collectedResponse = await CollectResponse(response);

            // Десериализация в модель 
            try
            {
                jsonAnswer = SanitizeJson(collectedResponse);

                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                };

                var plan = JsonConvert.DeserializeObject<FitnessPlan>(jsonAnswer, settings);
                return plan ?? throw new JsonSerializationException("Не удалось десериализовать план");
            }
            catch (JsonSerializationException e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        // Собирает с ответом нейросети из вида
        // "Microsoft.Extensions.AI.AzureAIInferenceChatClient+<CompleteStreamingAsync>d__12"
        // в вид { json-чик }
        private async Task<string> CollectResponse(IAsyncEnumerable<StreamingChatCompletionUpdate> response)
        {
            string collectedResponse = "";

            await foreach (var item in response)
            {
                collectedResponse += item;
            }

            return collectedResponse;
        }

        private string SanitizeJson(string json)
        {
            // Удаляем возможные Markdown-обертки, ЕСЛИ ПРОМПТА НЕ ХВАТИЛО
            json = json.Replace("```json", "").Replace("```", "").Trim();

            // Ищем начало и конец Json'а
            int start = json.IndexOf('{');
            int end = json.LastIndexOf('}') + 1;

            if (start >= 0 && end > start)
            {
                return json[start..end];
            }

            return json;
        }
    }
}
