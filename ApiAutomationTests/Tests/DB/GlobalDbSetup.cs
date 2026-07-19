using NUnit.Framework;
using Testcontainers.PostgreSql;
using Npgsql;
using System.Threading.Tasks;

namespace ApiAutomationTests.Tests.DB;

[SetUpFixture]
public class GlobalDbSetup
{
    public static PostgreSqlContainer DbContainer { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task RunGlobalDockerContainer()
    {
        var initScript = @"
            CREATE TABLE IF NOT EXISTS posts (
                id SERIAL PRIMARY KEY,
                title VARCHAR(255),
                body TEXT,
                user_id INT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS comments (
                id SERIAL PRIMARY KEY,
                post_id INT NOT NULL,
                name VARCHAR(255) NOT NULL,
                email VARCHAR(255) NOT NULL,
                body TEXT,
                CONSTRAINT fk_post FOREIGN KEY (post_id) REFERENCES posts (id) ON DELETE CASCADE
            );
        ";

        DbContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await DbContainer.StartAsync();

        await using var conn = new NpgsqlConnection(DbContainer.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(initScript, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    [OneTimeTearDown]
    public async Task StopGlobalDockerContainer()
    {
        if (DbContainer != null)
        {
            await DbContainer.StopAsync();
            await DbContainer.DisposeAsync();
        }
    }
}
