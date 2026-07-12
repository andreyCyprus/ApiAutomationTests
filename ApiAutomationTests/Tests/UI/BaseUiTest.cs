using ApiAutomationTests.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.IO;
using System.Threading.Tasks;

[assembly: Parallelizable(ParallelScope.Fixtures)] // Запускать тестовые классы параллельно
[assembly: LevelOfParallelism(4)]                  // Количество одновременно работающих браузеров/потоков

namespace ApiAutomationTests.Tests.UI;

public class BaseUiTest : PageTest
{
    protected string BaseUiUrl => ConfigProvider.BaseUiUrl;

    // LIVE-КОММЕНТАРИЙ: Шаг 1. Объявляем свойство App на уровне базового класса.
    // Благодаря protected, оно автоматически станет видимым во всех наших UI-тестах.
    protected UiTestContext App { get; private set; } = null!;

    [SetUp]
    public async Task BaseUiSetup()
    {
        // Устанавливаем таймаут для всех операций со страницами из нашего конфига
        Page.SetDefaultTimeout(ConfigProvider.TimeoutSeconds * 1000);

        // LIVE-КОММЕНТАРИЙ: Шаг 2. Инициализируем контекст страниц перед каждым тестом.
        // Передаем текущий объект Page во фреймворк.
        App = new UiTestContext(Page);

        // ==========================================
        // НАСТРОЙКА ТРЕЙСИНГА (Включается перед каждым тестом)
        // ==========================================
        await Context.Tracing.StartAsync(new()
        {
            Screenshots = true, // Записывать скриншоты каждого шага
            Snapshots = true,   // Записывать DOM-состояние страницы
            Sources = true      // Привязать исходный код теста к логам
        });

        await Task.CompletedTask;
    }

    /// <summary>
    /// Переопределяет стандартные настройки контекста Playwright для каждого теста.
    /// Устанавливает базовый URL из конфигурации, позволяя использовать относительные пути при навигации (например, Page.GotoAsync("/")).
    /// </summary>
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = ConfigProvider.BaseUiUrl
        };
    }

    [TearDown]
    public async Task BaseUiTearDown()
    {
        var testStatus = TestContext.CurrentContext.Result.Outcome.Status;

        // Автоматический скриншот и сохранение Трейса при падении теста
        if (testStatus == TestStatus.Failed)
        {
            // 1. Создаем скриншот падения
            var screenshotDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots");
            Directory.CreateDirectory(screenshotDir);
            var screenshotPath = Path.Combine(screenshotDir, $"{TestContext.CurrentContext.Test.Name}.png");

            await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
            TestContext.AddTestAttachment(screenshotPath, "Скриншот падения теста");

            // 2. СОХРАНЯЕМ ТРЕЙС
            var traceDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestResults");
            Directory.CreateDirectory(traceDir);
            var tracePath = Path.Combine(traceDir, $"{TestContext.CurrentContext.Test.Name}.zip");

            await Context.Tracing.StopAsync(new()
            {
                Path = tracePath
            });
            TestContext.AddTestAttachment(tracePath, "Архив Trace Viewer");

            // ИСПРАВЛЕНИЕ NUnit1033: Используем TestContext.Out.WriteLine вместо прямого WriteLine
            TestContext.Out.WriteLine($"[TRACE SAVED]: {tracePath}");
        }
        else
        {
            // Если тест прошел успешно — просто выключаем трейсинг без сохранения на диск
            await Context.Tracing.StopAsync();
        }
    }
}
