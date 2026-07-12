using ApiAutomationTests.Tests.UI;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ApiAutomationTests.Tests.UI
{
    [TestFixture]
    // 💡 LIVE COMMENT: Зачем здесь [Parallelizable(ParallelScope.Self)]?
    // По умолчанию NUnit гонит всё последовательно. Этот атрибут говорит раннеру: 
    // "Этот класс тестов можно запускать параллельно с другими классами!"
    // Почему ParallelScope.Self, а не Children? Потому что внутри этого класса тесты 
    // могут зависеть от разделяемого контекста (если бы мы его шарили). Для UI-тестов 
    // идеальная формула: один класс = один поток/браузер. Так мы избегаем каши в консоли и падений.
    [Parallelizable(ParallelScope.Self)]
    public class SmokeUiContextTests : BaseUiTest
    {
        [Test]
        [Description("Проверка корректности инициализации UiTestContext и базового взаимодействия через App")]
        public async Task UiTestContext_ShouldSuccessfullyInitializeAndNavigate()
        {
            // 💡 LIVE COMMENT: Наш BaseUiTest под капотом переопределяет ContextOptions() 
            // и намертво вшивает туда BaseURL из appsettings.json.
            // Поэтому "/" — это не просто косая черта, это команда Playwright: 
            // "Возьми конфиг, например 'https://staging.my-app.com', и иди на корень".
            await Page.GotoAsync("/");

            // 💡 LIVE COMMENT: Проверяем, что наша фабрика UiTestContext вообще родилась.
            // Если тут упадет NullReferenceException — значит, в [SetUp] базового класса 
            // мы забыли написать `App = new UiTestContext(Page);`
            App.Should().NotBeNull("UiTestContext (App) должен автоматически инициализироваться в BaseUiTest");
            App.HomePage.Should().NotBeNull("Lazy-инициализация PlaywrightHomePage должна отработать успешно");

            // 💡 LIVE COMMENT: Декларативный стиль в действии. Тест не знает, КАК устроен поиск.
            // Он просто говорит странице: "Найди мне QA". Вся грязь с локаторами скрыта внутри POM.
            await App.HomePage.SearchForAsync("QA");

            // 💡 LIVE COMMENT: А вот и Web-First Assertion, ради которого мы чинили ошибку CS1061.
            // Почему Expect(...), а не Assert.IsTrue(...)? 
            // Потому что Expect обладает встроенным "умным ожиданием" (по умолчанию 5 секунд).
            // Он будет опрашивать DOM каждые 100мс. Если инпут очистился/перерисовался не мгновенно, 
            // обычный Assert упадет, а Expect дождется его появления. Флакучесть (flakiness) = 0.
            await Expect(App.HomePage.SearchInput).ToBeVisibleAsync(new() { Timeout = 5000 });
        }
    }
}