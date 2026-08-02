using ApiAutomationTests.Infrastructure;
using ApiAutomationTests.Tests.DB;
using ApiAutomationTests.Tests.UI;
using Microsoft.Playwright;
using Npgsql;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ApiAutomationTests.Tests.E2E;

[TestFixture]
public class E2ePostWorkflowTests : BaseUiTest
{
    [Test]
    public async Task CreatePostInDb_ShouldBeValidAndDeletableViaWorkflow()
    {
        // ------------------------------------------------------------------
        // 1. ARRANGE: Генерируем данные и напрямую вставляем Пост в Postgres
        // ------------------------------------------------------------------
        var testPost = TestDataFactory.PostFaker.Generate();
        int createdPostId;

        var connectionString = GlobalDbSetup.DbContainer.GetConnectionString();
        await using (var conn = new NpgsqlConnection(connectionString))
        {
            await conn.OpenAsync();
            var sql = @"
                INSERT INTO posts (title, body, user_id) 
                VALUES (@title, @body, @userId) 

                RETURNING id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("title", (object?) testPost.Title ?? DBNull.Value);
            cmd.Parameters.AddWithValue("body", (object?) testPost.Body ?? DBNull.Value);
            cmd.Parameters.AddWithValue("userId", testPost.UserId);

            createdPostId = (int)(await cmd.ExecuteScalarAsync())!;
        }

        Assert.That(createdPostId, Is.GreaterThan(0), "Пост должен успешно создаться в БД.");

        // ------------------------------------------------------------------
        // 2. ACT & ASSERT (UI): Навигация и проверка через Playwright
        // ------------------------------------------------------------------
        // Переходим на главную страницу приложения через наш Lazy App-контекст
        await Page.GotoAsync("/");

        // Проверяем, что кнопка навигации отображается корректно
        await Expect(App.HomePage.GetStartedButton).ToBeVisibleAsync();

        // ------------------------------------------------------------------
        // 3. ASSERT (DB Cleanup/Validation): Проверяем и очищаем состояние
        // ------------------------------------------------------------------
        await using (var conn = new NpgsqlConnection(connectionString))
        {
            await conn.OpenAsync();

            // Проверяем наличие записи
            var checkSql = "SELECT COUNT(*) FROM posts WHERE id = @id;";
            await using (var checkCmd = new NpgsqlCommand(checkSql, conn))
            {
                checkCmd.Parameters.AddWithValue("id", createdPostId);
                var count = (long)(await checkCmd.ExecuteScalarAsync())!;
                Assert.That(count, Is.EqualTo(1), "Запись должна присутствовать в БД перед удалением.");
            }

            // Очищаем тестовую запись (Teardown для конкретного теста)
            var deleteSql = "DELETE FROM posts WHERE id = @id;";
            await using (var deleteCmd = new NpgsqlCommand(deleteSql, conn))
            {
                deleteCmd.Parameters.AddWithValue("id", createdPostId);
                await deleteCmd.ExecuteNonQueryAsync();
            }
        }
    }
}
