using ApiAutomationTests.Pages;
using NUnit.Framework;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace ApiAutomationTests.Tests.UI;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class FirstUiTests : BaseUiTest
{
    [Test]
    public async Task Navigation_ToGetStarted_ShouldOpenIntroPage_ViaPom()
    {
        // Открываем страницу
        await Page.GotoAsync("/");

        // Инициализируем наш Page Object
        var homePage = new PlaywrightHomePage(Page);

        // ИСПРАВЛЕНО: Используем Web-First Assertion вместо старого кастомного метода
        await Expect(homePage.GetStartedButton).ToBeVisibleAsync();

        // Кликаем по кнопке
        await homePage.ClickGetStartedAsync();

        // Проверка итогового URL
        await Expect(Page).ToHaveURLAsync(new Regex(".*intro"));
    }
}
