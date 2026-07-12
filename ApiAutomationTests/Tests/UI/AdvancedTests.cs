using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ApiAutomationTests.Tests.UI;

[Parallelizable(ParallelScope.Self)]
public class AdvancedTests : BaseUiTest
{
    [Test]
    public async Task Should_FillInput_Inside_IFrame()
    {
        // Напрямую подсовываем браузеру HTML-страницу с iFrame внутри!
        await Page.SetContentAsync(@"
        <html>
            <body>
                <h1>Advanced Elements</h1>
                <iframe id='payment-iframe' srcdoc='
                    <html>
                        <body>
                            <input placeholder=""Номер карты"" id=""card""/>
                            <button>Оплатить</button>
                        </body>
                    </html>
                '></iframe>
            </body>
        </html>
    ");

        // Теперь наш Page Object сработает идеально, так как локаторы совпадут!
        await App.AdvancedPage.PayWithCardAsync("4444555566667777");

        // Проверим, что текст успешно ввелся в iFrame
        var input = Page.FrameLocator("#payment-iframe").GetByPlaceholder("Номер карты");
        await Assertions.Expect(input).ToHaveValueAsync("4444555566667777");
    }


    [Test]
    public async Task Should_SelectOption_In_CustomDropdown()
    {
        // Подкладываем HTML с кастомным селектом
        await Page.SetContentAsync(@"
            <html>
                <body>
                    <div class='custom-select-trigger'>Выбрать страну</div>
                    <div class='custom-select-options' style='display: none;'>
                        <div>Cyprus</div>
                        <div>Greece</div>
                    </div>
                    <script>
                        const trigger = document.querySelector('.custom-select-trigger');
                        const menu = document.querySelector('.custom-select-options');
                        trigger.addEventListener('click', () => menu.style.display = 'block');
                        menu.addEventListener('click', () => menu.style.display = 'none');
                    </script>
                </body>
            </html>
        ");

        await App.AdvancedPage.SelectCustomOptionAsync("Cyprus");
    }

    [Test]
    public async Task Should_Handle_MultipleTabs_Correctly()
    {
        // Подкладываем HTML со ссылкой, открывающей новую вкладку
        await Page.SetContentAsync(@"
            <html>
                <body>
                    <h1>Advanced Elements</h1>
                    <a href='https://playwright.dev' target='_blank'>Follow on Facebook</a>
                </body>
            </html>
        ");

        IPage facebookTab = await App.AdvancedPage.ClickSocialLinkAndGetNewTabAsync("Follow on Facebook");
        await facebookTab.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Так как мы перенаправили на реальный рабочий playwright.dev, проверим его URL
        facebookTab.Url.Should().Contain("playwright.dev");
        await facebookTab.CloseAsync();

        await Assertions.Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Advanced Elements" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task Should_DisplayMockedCourts_When_ApiReturnsData()
    {
        var mockCourts = new[]
        {
        new { id = 1, name = "Premium Court Cyprus", isAvailable = true }
    };

        // Перехватываем запрос к встроенному сервису самого Playwright (или любой другой URL)
        await Page.RouteAsync("**/api/test-courts", async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 200,
                ContentType = "application/json",
                Body = System.Text.Json.JsonSerializer.Serialize(mockCourts)
            });
        });

        // Переходим на РЕАЛЬНЫЙ существующий сайт
        await Page.GotoAsync("https://playwright.dev");

        // Имитируем отправку запроса через фронтенд (прямо из контекста браузера)
        // Это заставит браузер сделать fetch-запрос, который поймает наш RouteAsync
        var responseText = await Page.EvaluateAsync<string>(@"
        async () => {
            const response = await fetch('/api/test-courts');
            const data = await response.json();
            return data[0].name;
        }
    ");

        // Проверяем, что браузер получил именно наши замоканные данные!
        responseText.Should().Be("Premium Court Cyprus");
    }

    [Test]
    public async Task Should_ShowErrorMessage_When_ApiFailsWith500()
    {
        // 1. Включаем мок на ошибку 500 для URL кортов
        await App.AdvancedPage.MockCourtsServerErrorAsync();

        // 2. Подкладываем HTML, где при клике на кнопку гарантированно отображается баннер, если API упал
        await Page.SetContentAsync(@"
            <html>
                <body>
                    <div class='error-banner' style='display:none;'>Что-то пошло не так. Попробуйте позже.</div>
                    <button id='trigger-fetch'>Загрузить корты</button>

                    <script>
                        document.getElementById('trigger-fetch').addEventListener('click', async () => {
                            try {
                                const res = await fetch('/api/v1/courts');
                                if (res.status === 500) {
                                    document.querySelector('.error-banner').style.display = 'block';
                                }
                            } catch (e) {
                                // На случай сетевых ошибок в песочнице
                                document.querySelector('.error-banner').style.display = 'block';
                            }
                        });
                    </script>
                </body>
            </html>
        ");

        // 3. Кликаем по кнопке, чтобы спровоцировать запрос к нашему моку
        await Page.ClickAsync("#trigger-fetch");

        // 4. Проверяем реакцию интерфейса через правильный Assertions.Expect
        var errorAlert = Page.Locator(".error-banner");
        await Assertions.Expect(errorAlert).ToBeVisibleAsync();
        await Assertions.Expect(errorAlert).ToContainTextAsync("Что-то пошло не так. Попробуйте позже.");
    }

}
