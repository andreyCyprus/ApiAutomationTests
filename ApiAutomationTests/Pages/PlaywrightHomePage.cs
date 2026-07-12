using Microsoft.Playwright;

namespace ApiAutomationTests.Pages;

public class PlaywrightHomePage : BasePage
{
    // 💡 LIVE COMMENT: Наше публичное свойство для Web-First Assertions в тестах.
    // Оно смотрит на роль Searchbox — это как раз то, что нужно для поля ввода на playwright.dev.
    public ILocator SearchInput => Page.GetByRole(AriaRole.Searchbox);

    private readonly ILocator _getStartedButton;

    // Переменные для поиска на playwright.dev
    private readonly ILocator _searchNavbarButton;
    private readonly ILocator _searchInputField; // 💡 LIVE COMMENT: Оставляем, раз оно завязано на GetByPlaceholder в конструкторе
    private readonly ILocator _searchResultsHeader;
    private readonly ILocator _searchResultItems;

    public PlaywrightHomePage(IPage page) : base(page)
    {
        _getStartedButton = page.GetByRole(AriaRole.Link, new() { Name = "Get started" });

        // 1. Кнопка навигационной панели (открывает модалку)
        _searchNavbarButton = page.GetByRole(AriaRole.Button, new() { Name = "Search" });

        // 2. Реальное поле ввода внутри открывшейся модалки DocSearch
        _searchInputField = page.GetByPlaceholder("Search docs");

        // 3. Заголовок первого уровня на странице результатов
        _searchResultsHeader = page.GetByRole(AriaRole.Heading, new() { Level = 1 });

        // Список результатов внутри DocSearch
        _searchResultItems = page.Locator(".DocSearch-Hit a");
    }

    // 💡 LIVE COMMENT: Выносим кнопку навбара наружу через public-свойство, 
    // если вдруг в будущем захотим проверить её состояние в тестах.
    public ILocator SearchNavbarButton => _searchNavbarButton;

    public async Task ClickGetStartedAsync() => await _getStartedButton.ClickAsync();
    public ILocator GetStartedButton => _getStartedButton;

    /// <summary>
    /// Выполняет двухэтапный поиск по сайту playwright.dev
    /// </summary>
    /// <summary>
    /// Выполняет ввод поискового запроса в модалку DocSearch
    /// </summary>
    public async Task SearchForAsync(string query)
    {
        // 💡 LIVE COMMENT: Шаг 1 — Кликаем по кнопке в навбаре, чтобы открыть модалку поиска.
        await _searchNavbarButton.ClickAsync();

        // 💡 LIVE COMMENT: Шаг 2 — Вводим текст. 
        await SearchInput.FillAsync(query);

        // 💡 LIVE COMMENT: Шаг 3 — СТИРАЕМ НАЖАТИЕ ENTER! 
        // Нам нельзя нажимать Enter, иначе модалка с результатами закроется. 
        // Мы просто ввели текст и ждем, когда Algolia DocSearch отрендерит подсказки в DOM.
    }

    public async Task<string?> GetResultHeaderRenderingTextAsync() => await _searchResultsHeader.TextContentAsync();
    public ILocator SearchResultsHeader => _searchResultsHeader;

    // Новый метод для SDET-логики (получение коллекции строк)
    public async Task<List<string>> GetProductTitlesAsync()
    {
        await _searchResultItems.First.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var elements = await _searchResultItems.AllAsync();
        var titles = new List<string>();

        foreach (var element in elements)
        {
            var text = await element.TextContentAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var trimmedText = text.Trim();
                titles.Add(trimmedText);
                Console.WriteLine($"[FOUND ITEM]: {trimmedText}");
            }
        }

        return titles;
    }

    // Метод для целевого клика по тексту из коллекции
    public async Task ClickOnSearchResultByTextAsync(string exactItemText)
    {
        var targetElement = _searchResultItems.Filter(new() { HasText = exactItemText });
        await targetElement.ClickAsync();
    }
}
