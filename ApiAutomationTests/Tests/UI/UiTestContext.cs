using ApiAutomationTests.Pages;
using Microsoft.Playwright;

namespace ApiAutomationTests.Tests.UI;

// LIVE-КОММЕНТАРИЙ: Это класс-фабрика, который лениво (Lazy) инициализирует страницы
// по мере того, как они становятся нужны в тестах.
public class UiTestContext
{
    private readonly IPage _page;
    private readonly Lazy<PlaywrightHomePage> _homePage;
    private readonly Lazy<AdvancedElementsPage> _advancedPage;

    public UiTestContext(IPage page)
    {
        _page = page;
        _homePage = new Lazy<PlaywrightHomePage>(() => new PlaywrightHomePage(_page));
        _advancedPage = new Lazy<AdvancedElementsPage>(() => new AdvancedElementsPage(_page));
    }

    public PlaywrightHomePage HomePage => _homePage.Value;
    public AdvancedElementsPage AdvancedPage => _advancedPage.Value;
}
