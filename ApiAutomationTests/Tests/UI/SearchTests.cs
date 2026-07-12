using ApiAutomationTests.Tests.UI;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ApiAutomationTests.Pages;

namespace ApiAutomationTests.Tests.UI;
[TestFixture]
public class SearchTests : BaseUiTest
{
    private PlaywrightHomePage _homePage;

    [SetUp]
    public void SetupPages()
    {
        // Инициализируем POM, передавая IPage из базового класса Playwright
        _homePage = new PlaywrightHomePage(Page);
    }

    [Test]
    public async Task Search_WhenQueryIsValid_ShouldDisplayRelevantResults()
    {
        // Arrange
        var searchQuery = "Playwright";

        // Act
        await Page.GotoAsync("/"); // Относительный путь благодаря настроенному ContextOptions
        await _homePage.SearchForAsync(searchQuery);

        // Assert (Используем Web-First Assertion для стабильности)
        await Expect(_homePage.SearchResultsHeader).ToContainTextAsync(searchQuery);
    }
}
