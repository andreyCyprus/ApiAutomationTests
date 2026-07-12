using Microsoft.Playwright;
using System.Text.Json;

namespace ApiAutomationTests.Pages;

public class AdvancedElementsPage : BasePage
{
    // 1. Локаторы для работы с iFrame
    private readonly IFrameLocator _paymentFrame;
    private readonly ILocator _cardNumberInput;
    private readonly ILocator _submitPaymentButton;

    // 2. Локаторы для кастомного Dropdown (на базе div/li, а не стандартного HTML select)
    private readonly ILocator _dropdownTrigger;
    private readonly ILocator _dropdownMenu;

    public AdvancedElementsPage(IPage page) : base(page)
    {
        // Инициализируем локатор самого фрейма по ID
        _paymentFrame = page.FrameLocator("#payment-iframe");

        // Локаторы элементов внутри фрейма — ищем строго через _paymentFrame!
        _cardNumberInput = _paymentFrame.GetByPlaceholder("Номер карты");
        _submitPaymentButton = _paymentFrame.GetByRole(AriaRole.Button, new() { Name = "Оплатить" });

        // Кастомный дропдаун на главной странице
        _dropdownTrigger = page.Locator(".custom-select-trigger");
        _dropdownMenu = page.Locator(".custom-select-options");
    }

    /// <summary>
    /// Декларативное выполнение платежного флоу внутри изолированного iFrame
    /// </summary>
    public async Task PayWithCardAsync(string cardNumber)
    {
        // Playwright сам асинхронно подождет появления фрейма, загрузки его DOM и доступности инпута
        await _cardNumberInput.FillAsync(cardNumber);
        await _submitPaymentButton.ClickAsync();
    }

    /// <summary>
    /// Взаимодействие со сложным кастомным дропдауном с ручным управлением ожиданиями состояний
    /// </summary>
    public async Task SelectCustomOptionAsync(string optionText)
    {
        await _dropdownTrigger.ClickAsync();

        // Кастомное ожидание: убедимся, что меню дропдауна стало видимым перед кликом
        await _dropdownMenu.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        // Выбираем элемент по тексту внутри раскрытого меню
        await _dropdownMenu.GetByText(optionText, new() { Exact = true }).ClickAsync();

        // Кастомное ожидание: убедимся, что меню успешно скрылось из DOM/интерфейса
        await _dropdownMenu.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    /// <summary>
    /// Перехват новой вкладки (Page Context) при клике на внешние ссылки
    /// </summary>
    public async Task<IPage> ClickSocialLinkAndGetNewTabAsync(string linkRoleName)
    {
        // Перехватываем событие рождения новой страницы внутри контекста браузера
        var newTab = await Page.Context.RunAndWaitForPageAsync(async () =>
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = linkRoleName }).ClickAsync();
        });

        return newTab;
    }

    /// <summary>
    /// Включает мок для API кортов с кастомным объектом данных
    /// </summary>
    public async Task MockCourtsResponseAsync(object responseData, int statusCode = 200)
    {
        // Перехватываем все GET запросы на указанный эндпоинт
        await Page.RouteAsync("**/api/v1/courts", async route =>
        {
            if (route.Request.Method == "GET")
            {
                var jsonPayload = JsonSerializer.Serialize(responseData);

                await route.FulfillAsync(new()
                {
                    Status = statusCode,
                    ContentType = "application/json",
                    Body = jsonPayload
                });
            }
            else
            {
                // Если метод другой (POST/PUT), пускаем запрос дальше в сеть
                await route.ContinueAsync();
            }
        });
    }

    /// <summary>
    /// Имитирует падение бэкенда (например, 500 Internal Server Error)
    /// </summary>
    public async Task MockCourtsServerErrorAsync()
    {
        await Page.RouteAsync("**/api/v1/courts", async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 500,
                Body = "Internal Server Error"
            });
        });
    }

}
