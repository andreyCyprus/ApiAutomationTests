using NUnit.Framework;

namespace ApiAutomationTests.Tests.DB;

public abstract class BaseDbTest
{
    // Ссылаемся на строку подключения глобального контейнера
    protected string ConnectionString => GlobalDbSetup.DbContainer.GetConnectionString();
}
