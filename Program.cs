using ApiGateway.FakeResponses;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient(); // Для отправки HTTP запросов
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<MakeResponseUtility>();
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Настроим маршрут для приема заявок от клиента
app.MapPost("/submit-application", async (Application application, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient();
    // Логируем полученные данные в консоль
    Console.WriteLine($"Received application: {application.LoanPurpose}, {application.LoanAmount}, {application.LoanTermMonths}");
    Console.WriteLine($"User: {application.User.FirstName}, {application.User.LastName}, {application.User.Email}");

    // Отправляем заявку в сервис обработки заявок
    var response = await client.PostAsJsonAsync("https://localhost:7172/api/application", application);

    if (response.IsSuccessStatusCode)
    {
        return Results.Ok("Application sent successfully.");
    }
    else
    {
        return Results.StatusCode((int)response.StatusCode);
    }
});

// Метод для проверки клиента
app.MapPost("/api/validate/application", (Application application) =>
{
    // Типа ответ от Anti-Fraud system
    Console.WriteLine($"Application for client {application.User.FirstName} {application.User.LastName} is valid.");

    // Возвращаем успешный ответ
    return Results.Ok("Application validate processed successfully.");
});

// Метод для сбора данных 1
app.MapPost("/api/find/endpoint1", async ([FromBody] ApplicationSubmittedMessage application, [FromServices] MakeResponseUtility responseUtility) =>
{
    Console.WriteLine("Сбор данных 1 завершен.");
    return await responseUtility.MakeResponse("nbki", application); // Вызываем метод через экземпляр класса
});

// Метод для сбора данных 2
app.MapPost("/api/find/endpoint2", async ([FromBody] ApplicationSubmittedMessage application, [FromServices] MakeResponseUtility responseUtility) =>
{
    Console.WriteLine("Сбор данных 2 завершен.");
    return await responseUtility.MakeResponse("fns1", application); // Вызываем метод через экземпляр класса
});

// Метод для сбора данных 3
app.MapPost("/api/find/endpoint3", async ([FromBody] ApplicationSubmittedMessage application, [FromServices] MakeResponseUtility responseUtility) =>
{
    Console.WriteLine("Сбор данных 3 завершен.");
    return await responseUtility.MakeResponse("fssp", application); // Вызываем метод через экземпляр класса
});

app.Run();

public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Passport { get; set; } // Номер паспорта
    public string INN { get; set; } // ИНН
    public int Age { get; set; } // Возраст
    public string Gender { get; set; } // Пол
    public string MaritalStatus { get; set; } // Семейное положение
    public string Education { get; set; } // Образование
    public string EmploymentType { get; set; }
}

public class Application
{
    public string LoanPurpose { get; set; } // Цель кредита
    public double LoanAmount { get; set; } // Сумма кредита
    public int LoanTermMonths { get; set; } // Срок кредита в месяцах
    public string LoanType { get; set; } // Тип кредита (например, ипотека, автокредит)
    public double InterestRate { get; set; } // Процентная ставка
    public string PaymentType { get; set; } // Тип платежей (аннуитетные, дифференцированные)
    public User User { get; set; }
}

public record ApplicationSubmittedMessage(
    string FirstName, string LastName, int Age, string Passport, string INN, 
    string Gender, string MaritalStatus, string Education, string EmploymentType, 
    string LoanPurpose, double LoanAmount, int LoanTermMonths, string LoanType,
    double InterestRate, string PaymentType, string timestamp);