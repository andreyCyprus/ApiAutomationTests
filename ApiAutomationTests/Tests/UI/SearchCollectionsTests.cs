using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;

namespace ApiAutomationTests.Tests.UI;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
// 💡 LIVE COMMENT: Наследуемся от BaseUiTest. Теперь свойство 'App' (наш UiTestContext) 
// и объект 'Page' автоматически доступны в каждом тесте.
public class SearchCollectionsTests : BaseUiTest
{
    [Test]
    [Description("Проверка, что все собранные результаты поиска содержат поисковый запрос")]
    public async Task Search_WhenMultipleResultsExist_AllItemsShouldContainQuery()
    {
        // ARRANGE
        var searchQuery = "Locator";

        // ACT
        // 💡 LIVE COMMENT: ConfigProvider под капотом подставит базовый URL, поэтому идем на корень сайта
        await Page.GotoAsync("/");

        // Запускаем наш двухэтапный поиск (клик по навбару -> ввод -> Enter)
        await App.HomePage.SearchForAsync(searchQuery);

        // Вытягиваем тексты всех найденных элементов в виде списка строк
        List<string> resultTitles = await App.HomePage.GetProductTitlesAsync();

        // ASSERT
        // Проверяем, что коллекция не пустая
        resultTitles.Should().NotBeEmpty("поисковая выдача не должна быть пустой");

        // Умная проверка: каждый элемент списка обязан содержать слово "Locator" (без учета регистра)
        resultTitles.Should().OnlyContain(
            title => title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase),
            $"все результаты поиска должны содержать слово '{searchQuery}'"
        );
    }

    [Test]
    [Description("Проверка фильтрации коллекции: клик по конкретному тексту и переход на нужную страницу")]
    public async Task Search_WhenSpecificItemClicked_ShouldNavigateToCorrectPage()
    {
        // ARRANGE
        var searchQuery = "Locator";
        var targetItem = "FrameLocator"; // Текст элемента внутри списка, по которому хотим кликнуть

        // ACT
        await Page.GotoAsync("/");
        await App.HomePage.SearchForAsync(searchQuery);

        // 💡 LIVE COMMENT: Вызываем метод, который внутри использует мощный .Filter(new() { HasText = ... })
        // Playwright сам найдет нужную ссылку в DOM без циклов foreach на стороне C#
        await App.HomePage.ClickOnSearchResultByTextAsync(targetItem);

        // ASSERT
        // Web-First Assertion: проверяем, что браузер перешел на страницу, в URL которой есть "framelocator"
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*framelocator"));
    }
}
