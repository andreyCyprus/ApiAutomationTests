using ApiAutomationTests.Infrastructure;
using ApiAutomationTests.Models; // Подключаем наши модели
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using Npgsql;
using System.Threading.Tasks;

namespace ApiAutomationTests.Tests.DB;

[TestFixture]
public class PostgresSmokeTests : BaseDbTest
{
    [Test]
    public async Task Container_ShouldBeAccessible_AndReturnData()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        var result = await connection.ExecuteScalarAsync<int>("SELECT 1;");
        result.Should().Be(1);
    }

    [Test]
    public async Task InsertAndSelect_Post_ShouldSuccessfullySaveAndRetrieve()
    {
        // Arrange
        using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        var testPost = new Post
        {
            Title = "Тестовый заголовок Docker",
            Body = "Содержимое поста для проверки Testcontainers",
            UserId = 42
        };

        // SQL для вставки с возвратом сгенерированного ID (RETURNING id)
        var insertSql = @"
            INSERT INTO posts (title, body, user_id) 
            VALUES (@Title, @Body, @UserId) 
            RETURNING id;";

        // Act
        // 1. Вставляем запись и сохраняем полученный ID в нашу модель
        var insertedId = await connection.ExecuteScalarAsync<int>(insertSql, testPost);
        testPost.Id = insertedId;

        // 2. Вычитываем запись обратно по ID
        // Используем псевдонимы (AS), чтобы Dapper автоматически смапил snake_case из базы на CamelCase в C#
        var selectSql = "SELECT id As Id, title As Title, body As Body, user_id As UserId FROM posts WHERE id = @Id;";
        var dbPost = await connection.QuerySingleOrDefaultAsync<Post>(selectSql, new { Id = insertedId });

        // Assert
        dbPost.Should().NotBeNull("запись должна быть физически найдена в базе данных контейнера");
        dbPost!.Id.Should().Be(testPost.Id);
        dbPost.Title.Should().Be(testPost.Title);
        dbPost.Body.Should().Be(testPost.Body);
        dbPost.UserId.Should().Be(testPost.UserId);
    }
}
