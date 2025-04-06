using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using ProductProgamming04042025.Pages.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Collections;
using Microsoft.Build.Framework;
using Org.BouncyCastle.Asn1.Cmp;
using System.Runtime.InteropServices;

namespace ProductProgamming04042025.AI
{
    public class Bot
    {
        private readonly AzureKeyCredential _credential;
        private readonly string _modelName;
        private readonly Uri _modelEndpoint;
        private readonly IChatClient _chatClient;

        public Bot(string token, string modelName)
        {
            _credential = new(token);
            _modelName = modelName;
            _modelEndpoint = new Uri("https://models.inference.ai.azure.com");
            _chatClient = new ChatCompletionsClient(_modelEndpoint, _credential)
                .AsChatClient(_modelName);
        }


        public async Task<object[]> SendRequest(string request, UserProfile userProfile)
        {
            string textResponse = string.Empty;

            try
            {
                string prompt = BuildPrompt(request, userProfile);
                var response = _chatClient.CompleteStreamingAsync(prompt);

                if (response == null)
                {
                    Console.WriteLine("Не получилось получить ответ от нейросети - он равен null");
                    throw new NullReferenceException("AI response is null");
                }

                string collectedResponse = await CollectResponse(response);
                //string jsonAnswer = SanitizeJson(collectedResponse);
                // Разделяем ответ на текстовую часть и JSON
                var parts = collectedResponse.Split(new[] { "===JSON===" }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    textResponse = parts[0].Trim();
                }

                string jsonAnswer = parts.Length > 1 ? SanitizeJson(parts[1]) : "{}";

                Console.WriteLine($"Ответ нейросети: {jsonAnswer}");

                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                };

                // Временно парсим со string-ключом
                var rawPlan = JsonConvert.DeserializeObject<Dictionary<string, DayPlan>>(jsonAnswer);
                
                if (rawPlan == null)
                {
                    Console.WriteLine("Не удбалось десериализовать фитнес-план");
                    throw new JsonSerializationException("Failed to deserialize fitness plan");
                }

                // Конвертируем в FitnessPlan
                var fitnessPlan = new FitnessPlan
                {
                    WeekPlan = rawPlan.ToDictionary(
                        kvp => int.Parse(kvp.Key), // Конвертируем ключ из string в int
                        kvp => kvp.Value
                    )
                };

            return new object[] { textResponse, fitnessPlan };
        }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Ошибка в SendRequest");
                throw;
            }
        }

        private string BuildPrompt(string request, UserProfile userProfile)
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
                $"Оформи ответ в следующем формате: сначала сообщение, понятное для пользователя, в html без использования тега html. Для форматирования текста можешь использовать теги: " +
                $"h2, h3 - для заголовков разного размера, p - для параграфов, ul li ol - для списков, hr для разделения данных, например дней недели. " +
                $"После сообщения для пользователя установи разделитель с текстом \"===JSON===\", после чего оформи Json в соответствии со следующим примером:\n" +
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
                $"                    \"count\": \"Калорийность. Белки/жиры/углеводы (указать без единиц измерения)\"\n" +
                $"                }},\n" +
                $"                {{\n" +
                $"                    \"name\": \"Блюдо 2 (обед)\"," +
                $"                    \"count\": \"Калорийность. Белки/жиры/углеводы (указать без единиц измерения)\"\n" +
                $"                }}\n," +
                $"                {{\n" +
                $"                    \"name\": \"Блюдо 3 (ужин)\"," +
                $"                    \"count\": \"Калорийность. Белки/жиры/углеводы (указать без единиц измерения)\"\n" +
                $"                }}\n," +
                $"            ],\n" +
                $"        \"extra\": \"exercises или diet могут быть пустыми, если это так, то запиши ответ в это поле\"\n" +
                $"}}\n" +
                $"Пользователь: {request}";

            return prompt;
        }

        private async Task<string> CollectResponse(IAsyncEnumerable<StreamingChatCompletionUpdate> response)
        {
            System.Text.StringBuilder collectedResponse = new();

            await foreach (var item in response)
            {
                collectedResponse.Append(item);
            }

            return collectedResponse.ToString();
        }

        private string SanitizeJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return "{}";

            // Удаляем Markdown обертки если есть
            json = json.Replace("```json", "").Replace("```", "").Trim();

            // Ищем начало и конец JSON
            int start = json.IndexOf('{');
            int end = json.LastIndexOf('}') + 1;

            if (start >= 0 && end > start)
            {
                return json[start..end];
            }

            Console.WriteLine($"Некорретный вид Json: {json}");
            return "{}";
        }
    }
}