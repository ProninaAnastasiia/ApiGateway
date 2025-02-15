using System.Text.Json;

namespace ApiGateway.FakeResponses;

public class MakeResponseUtility
{
    private readonly IWebHostEnvironment _env;

    public MakeResponseUtility(IWebHostEnvironment env)
    {
        _env = env;
    }
    public async Task<IResult> MakeResponse(string jsonName, ApplicationSubmittedMessage application)
    {
        string jsonPath = Path.Combine(_env.ContentRootPath, "FakeResponses", $"{jsonName}.json");
        // 1. Прочитать содержимое nbki.json
        string jsonContent;
        try
        {
            jsonContent = await File.ReadAllTextAsync(jsonPath);
        }
        catch (FileNotFoundException)
        {
            return Results.Problem($"Файл {jsonName}.json не найден по пути: {jsonPath}", statusCode: 500);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Ошибка при чтении {jsonName}.json по пути: {jsonPath}: {ex.Message}", statusCode: 500);
        }

        // 2. Десериализовать JSON в динамический объект (JObject из System.Text.Json)
        JsonDocument data;
        try
        {
            data = JsonDocument.Parse(jsonContent);
        }
        catch (JsonException ex)
        {
            return Results.Problem($"Ошибка при разборе JSON из {jsonName}.json: {ex.Message}", statusCode: 400); // Вернуть ошибку, если JSON невалиден
        }

        // 3. Создать новый объект для ответа.  Использовать Dictionary<string, object> для добавления свойств.
        var response = new Dictionary<string, object>();

        // 4. Добавить дополнительные поля
        response.Add("request_date", DateTimeOffset.UtcNow);
        response.Add("passport", application.Passport); 
        response.Add("first_name", application.FirstName);
        response.Add("last_name", application.LastName);
        response.Add("age", application.Age);
        response.Add("inn", application.INN);
        
        // 5. Добавить содержимое nbki.json
        response.Add("history", data.RootElement.GetProperty("history"));

        // 6. Сериализовать в JSON и вернуть.
        return Results.Json(response);
    }
}