using ApiAutomationTests.Tests.UI;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ApiAutomationTests.Tests.UI;  

[Parallelizable(ParallelScope.Self)]
public class IFrameTests : BaseUiTest
{
    [Test]
    public async Task UserShouldBeAbleToSubmitPaymentInsideIFrame()
    {
        // Напрямую подсовываем браузеру HTML-страницу с iFrame внутри!
        await Page.SetContentAsync(@"
<html>
    <body>
        <h1>Advanced Elements</h1>
        <div id='status-container'></div>
        
        <iframe id='payment-iframe' srcdoc='
            <html>
                <body>
                    <input placeholder=""Номер карты"" id=""card""/>
                    <button id=""pay-btn"">Оплатить</button>

                    <script>
                        document.getElementById(""pay-btn"").addEventListener(""click"", () => {
                            // Передаем сообщение из iFrame на родительскую страницу
                            window.parent.postMessage(""payment_success"", ""*"");
                        });
                    </script>
                </body>
            </html>
        '></iframe>

        <script>
            // Слушаем сообщение от iFrame и выводим нужный текст на главной странице
            window.addEventListener(""message"", (event) => {
                if (event.data === ""payment_success"") {
                    const statusDiv = document.getElementById(""status-container"");
                    statusDiv.innerHTML = ""<h2>Оплата успешно принята!</h2>"";
                }
            });
        </script>
    </body>
</html>
");



        // 2. В одну строку выполняем бизнес-логику внутри фрейма
        await App.AdvancedPage.PayWithCardAsync("4444555566667777");

        // 3. Проверяем результат на основной странице (например, появление алерта)
        var successMessage = Page.GetByText("Оплата успешно принята!");
        await Assertions.Expect(successMessage).ToBeVisibleAsync();
    }
}
