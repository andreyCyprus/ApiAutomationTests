using RestSharp;
using RestSharp.Authenticators; // Обязательно добавь этот using
using ApiAutomationTests.Services;
using ApiAutomationTests.Infrastructure;

namespace ApiAutomationTests.Tests.API;

public class BaseTest
{
    protected RestClient Client;
    protected PostService PostService;
    protected CommentService CommentService;

    [OneTimeSetUp]
    public void Setup()
    {
        var options = new RestClientOptions(ConfigProvider.BaseUrl)
        {
            Timeout = TimeSpan.FromMilliseconds(int.Parse(ConfigProvider.Config["Timeout"] ?? "3000"))
        };

        // 1. Получаем токен через наш новый сервис
        // Создаем временный клиент для авторизации (без аутентификатора)
        var authService = new AuthService(new RestClient(options));
        var token = authService.GetToken();

        // 2. Настраиваем автоматическую подстановку токена
        // RestSharp сам добавит заголовок "Authorization: Bearer <token>"
        //options.Authenticator = new JwtAuthenticator(token);

        // 3. Инициализируем основной клиент с уже настроенной авторизацией
        Client = new RestClient(options);

        PostService = new PostService(Client);
        CommentService = new CommentService(Client);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Client?.Dispose();
    }
}