using ApiAutomationTests.Infrastructure;
using ApiAutomationTests.Models;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using Npgsql;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAutomationTests.Tests.DB;

[TestFixture]
public class PostgresRelationsTests : BaseDbTest
{
    [Test]
    public async Task InsertPostWithComments_ShouldRetrieveCorrectly()
    {
        // Используем ConnectionString из базового класса BaseDbTest
        using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        // 1. Генерируем и вставляем фейковый пост
        var fakePost = TestDataFactory.PostFaker.Generate();
        var insertPostSql = @"
            INSERT INTO posts (title, body, user_id) 
            VALUES (@Title, @Body, @UserId) 
            RETURNING id;";
        var postId = await connection.ExecuteScalarAsync<int>(insertPostSql, fakePost);

        // 2. Генерируем 3 комментария для этого поста через фабрику Bogus
        var fakeComments = TestDataFactory.CommentFaker(postId).Generate(3);

        // Dapper автоматически свяжет @body со свойством body с маленькой буквы
        var insertCommentSql = @"
            INSERT INTO comments (post_id, name, email, body) 
            VALUES (@PostId, @Name, @Email, @body);";
        await connection.ExecuteAsync(insertCommentSql, fakeComments);

        // 3. Выбираем комментарии из базы для валидации
        var dbComments = (await connection.QueryAsync<Comment>(
            "SELECT id, post_id as PostId, name, email, body as body FROM comments WHERE post_id = @PostId;",
            new { PostId = postId })).ToList();

        // Проверки (FluentAssertions)
        dbComments.Should().HaveCount(3);
        dbComments.ForEach(c =>
        {
            c.PostId.Should().Be(postId);
            c.Email.Should().Contain("@");
            c.body.Should().NotBeNullOrEmpty();
        });
    }

    [Test]
    public async Task DeletePost_ShouldCascadeDelete_AssociatedComments()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        // 1. Создаем тестовые связанные данные
        var fakePost = TestDataFactory.PostFaker.Generate();
        var postId = await connection.ExecuteScalarAsync<int>(
            "INSERT INTO posts (title, body, user_id) VALUES (@Title, @Body, @UserId) RETURNING id;", fakePost);

        var fakeComment = TestDataFactory.CommentFaker(postId).Generate();
        await connection.ExecuteAsync(
            "INSERT INTO comments (post_id, name, email, body) VALUES (@PostId, @Name, @Email, @body);", fakeComment);

        // 2. Удаляем пост (в БД настроен ON DELETE CASCADE)
        await connection.ExecuteAsync("DELETE FROM posts WHERE id = @Id;", new { Id = postId });

        // 3. Проверяем, что комментарии удалились автоматически
        var commentCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM comments WHERE post_id = @PostId;", new { PostId = postId });

        commentCount.Should().Be(0, "комментарии должны удаляться каскадно при удалении родительского поста");
    }
}
