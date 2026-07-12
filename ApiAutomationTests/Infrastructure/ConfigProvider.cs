using Microsoft.Extensions.Configuration;

namespace ApiAutomationTests.Infrastructure;

public static class ConfigProvider
{
    public static IConfiguration Config { get; }


    static ConfigProvider()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            // Принудительно приводим к нижнему регистру, чтобы имя файла совпадало с env.ToLower()
            .AddJsonFile($"appsettings.{env.ToLower()}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    // Мои текущие настройки для API
    public static string BaseUrl => Config["BaseUrl"] ?? throw new Exception("BaseUrl not found in config");

    // Новые настройки для UI (Playwright)
    public static string BaseUiUrl => Config["UiSettings:BaseUrl"] ?? throw new Exception("UiSettings:BaseUrl not found in config");
    public static int TimeoutSeconds => int.TryParse(Config["UiSettings:TimeoutSeconds"], out var timeout) ? timeout : 30; // 30 по умолчанию
    public static bool Headless => !bool.TryParse(Config["UiSettings:Headless"], out var headless) || headless;

    // Настройки для подключения к базе данных PostgreSQL
    public static string PostgresConnectionString => Config.GetConnectionString("PostgresConnection")
?? throw new Exception("Connection string 'PostgresConnection' not found in config");


}