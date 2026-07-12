using System.Net;
using ApiAutomationTests.Infrastructure;
using FluentAssertions;
using RestSharp;

namespace ApiAutomationTests.Tests.API;

public class SecurityAndMetadataTests : BaseTest
{
    [Test]
    public async Task GetPost_ShouldReturnApplicationJson_AndSuccessCode()
    {
        // 1. Send Request
        var response = await Client.ExecuteAsync(new RestRequest("posts/1"));

        // 2. Validate Status Code (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Validate Content-Type Header
        // Ищем заголовок Content-Type и проверяем, что он содержит json
        var contentType = response.ContentHeaders?
            .FirstOrDefault(h => h.Name == "Content-Type")?.Value?.ToString();

        contentType.Should().Contain("application/json");
    }

    [Test]
    public async Task GetNonExistingPost_ShouldReturnNotFound()
    {
        // Специально запрашиваем ID, которого нет (например, 9999)
        var response = await Client.ExecuteAsync(new RestRequest("posts/9999"));

        // Проверяем, что API честно отдает 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
        public async Task GetPost_ShouldReturnUnauthorized_IfNoToken() 
        {
            // Создаем временный клиент без токена
            var clientWithoutToken = new RestClient(new RestClientOptions(ConfigProvider.BaseUrl)
            {
                Timeout = TimeSpan.FromMilliseconds(int.Parse(ConfigProvider.Config["Timeout"] ?? "3000"))
            });

            var response = await clientWithoutToken.ExecuteAsync(new RestRequest("posts/1"));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
}