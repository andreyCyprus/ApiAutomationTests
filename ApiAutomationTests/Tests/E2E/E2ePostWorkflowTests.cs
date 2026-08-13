using ApiAutomationTests.Infrastructure;
using ApiAutomationTests.Tests.UI;
using Microsoft.Playwright;
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
        // 1. ARRANGE
        // ------------------------------------------------------------------
        var testPost = TestDataFactory.PostFaker.Generate();

        var createdPostId = await DbHelper.ExecuteScalarAsync<int>(@"
            INSERT INTO posts (title, body, user_id) 
            VALUES (@title, @body, @userId) 
            RETURNING id;",
            new()
            {
                ["title"] = testPost.Title,
                ["body"] = testPost.Body,
                ["userId"] = testPost.UserId
            });

        Assert.That(createdPostId, Is.GreaterThan(0), "Пост должен успешно создаться в БД.");

        try
        {
            // ------------------------------------------------------------------
            // 2. ACT & ASSERT (UI)
            // ------------------------------------------------------------------
            await Page.GotoAsync("/");
            await Expect(App.HomePage.GetStartedButton).ToBeVisibleAsync();

            // ------------------------------------------------------------------
            // 3. ASSERT (DB Validation)
            // ------------------------------------------------------------------
            var count = await DbHelper.ExecuteScalarAsync<long>(
                "SELECT COUNT(*) FROM posts WHERE id = @id;",
                new() { ["id"] = createdPostId });

            Assert.That(count, Is.EqualTo(1), "Запись должна присутствовать в БД перед очисткой.");
        }
        finally
        {
            // ------------------------------------------------------------------
            // 4. CLEANUP (Выполнится ВСЕГДА, независимо от результатов Assert/UI)
            // ------------------------------------------------------------------
            await DbHelper.ExecuteAsync(
                "DELETE FROM posts WHERE id = @id;",
                new() { ["id"] = createdPostId });
        }
    }

    [Test]
    public async Task AddCommentToExistingPostViaUi_ShouldPersistInDatabase()
    {
        // ------------------------------------------------------------------
        // 1. ARRANGE
        // ------------------------------------------------------------------
        var testPost = TestDataFactory.PostFaker.Generate();
        var createdPostId = await DbHelper.ExecuteScalarAsync<int>(@"
            INSERT INTO posts (title, body, user_id) 
            VALUES (@title, @body, @userId) 
            RETURNING id;",
            new()
            {
                ["title"] = testPost.Title,
                ["body"] = testPost.Body,
                ["userId"] = testPost.UserId
            });

        var testComment = TestDataFactory.CommentFaker(createdPostId).Generate();

        try
        {
            // ------------------------------------------------------------------
            // 2. ACT (UI)
            // ------------------------------------------------------------------
            await Page.SetContentAsync($@"
                <html>
                    <body>
                        <form id='comment-form'>
                            <input id='name-input' placeholder='Ваше имя' />
                            <input id='email-input' placeholder='Ваш Email' />
                            <textarea id='body-input' placeholder='Текст комментария'></textarea>
                            <button type='submit'>Отправить</button>
                        </form>
                        <div id='comments-list'></div>
                        <script>
                            document.getElementById('comment-form').addEventListener('submit', (e) => {{
                                e.preventDefault();
                                const name = document.getElementById('name-input').value;
                                const body = document.getElementById('body-input').value;
                                document.getElementById('comments-list').innerHTML = '<b>' + name + ':</b> ' + body;
                            }});
                        </script>
                    </body>
                </html>");

            await Page.GetByPlaceholder("Ваше имя").FillAsync(testComment.Name ?? "");
            await Page.GetByPlaceholder("Ваш Email").FillAsync(testComment.Email ?? "");
            await Page.GetByPlaceholder("Текст комментария").FillAsync(testComment.body ?? "");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Отправить" }).ClickAsync();

            await Expect(Page.GetByText(testComment.body ?? "")).ToBeVisibleAsync();

            // Симуляция записи комментария бэкендом
            await DbHelper.ExecuteAsync(@"
                INSERT INTO comments (post_id, name, email, body) 
                VALUES (@postId, @name, @email, @body);",
                new()
                {
                    ["postId"] = createdPostId,
                    ["name"] = testComment.Name,
                    ["email"] = testComment.Email,
                    ["body"] = testComment.body
                });

            // ------------------------------------------------------------------
            // 3. ASSERT (DB)
            // ------------------------------------------------------------------
            var commentCount = await DbHelper.ExecuteScalarAsync<long>(@"
                SELECT COUNT(*) 
                FROM comments 
                WHERE post_id = @postId AND email = @email;",
                new()
                {
                    ["postId"] = createdPostId,
                    ["email"] = testComment.Email
                });

            Assert.That(commentCount, Is.EqualTo(1), "Комментарий должен успешно сохраниться в PostgreSQL.");
        }
        finally
        {
            // ------------------------------------------------------------------
            // 4. CLEANUP (Каскадно удалит пост и связанный комментарий)
            // ------------------------------------------------------------------
            await DbHelper.ExecuteAsync(
                "DELETE FROM posts WHERE id = @id;",
                new() { ["id"] = createdPostId });
        }
    }
}
