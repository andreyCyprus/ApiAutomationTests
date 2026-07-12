using System.Net;
using FluentAssertions;
using Bogus;
using ApiAutomationTests.Models;

namespace ApiAutomationTests.Tests.API;

public class JsonPlaceholderTests : BaseTest
{
    [Test]
    public async Task CreatePost_Refactored_ShouldSucceed()
    {
        // Arrange: Готовим данные через Bogus
        var faker = new Faker<Post>()
            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
            .RuleFor(p => p.Body, f => f.Lorem.Sentence())
            .RuleFor(p => p.UserId, f => f.Random.Number(1, 10));

        var newPost = faker.Generate();

        // Act: Просто вызываем метод сервиса
        var response = await PostService.CreatePost(newPost);

        // Assert: Проверяем результат
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Data.Title.Should().Be(newPost.Title);
    }

    [Test]
    public async Task DeletePost_Refactored_ShouldSucceed()
    {
        // Act
        var response = await PostService.DeletePost(1);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetAllPosts_ShouldReturnListOfPosts()
    { 
        // Act
        var response = await PostService.GetAllPosts();
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Should().BeOfType<List<Post>>();
        // Проверяем, что в списке есть элементы
        response.Data.Should().NotBeEmpty();

        // Проверяем, что у первого элемента заполнены обязательные поля
        response.Data.First().Id.Should().NotBeNull();
        response.Data.First().Title.Should().NotBeNullOrWhiteSpace();

        // Можно даже проверить количество, если оно фиксировано
        // (в JSONPlaceholder их обычно 100)
        response.Data.Count.Should().Be(100);
    }

}
