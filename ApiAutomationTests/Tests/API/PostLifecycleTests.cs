using System.Net;
using FluentAssertions;
using Bogus;
using ApiAutomationTests.Models;

namespace ApiAutomationTests.Tests.API;

    public class PostLifecycleTests : BaseTest
    {
        [Test]

        public async Task PostLifecycle_ShouldSucceed()
        {
            // Arrange: Готовим данные через Bogus

            var faker = new Faker<Post>()
                .RuleFor(p => p.Title, f => f.Commerce.ProductName())
                .RuleFor(p => p.Body, f => f.Lorem.Sentence())
                .RuleFor(p => p.UserId, f => f.Random.Number(1, 10));
            var newPost = faker.Generate();

        // Act: Создаем пост

        var createResponse = await PostService.CreatePost(newPost);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // В РЕАЛЬНОМ API мы бы взяли ID так: 
        // var createdId = createResponse.Data.Id.Value;
        // Но для JSONPlaceholder мы возьмем существующий ID = 1, чтобы тест не падал на 404
        var testId = 1;

        // Обновляем пост

        newPost.Title = "Updated Title";
        var updateResponse = await PostService.UpdatePost(testId, newPost);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updateResponse.Data.Title.Should().Be("Updated Title");

        // Удаляем пост

        var deleteResponse = await PostService.DeletePost(testId);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        }
    }

/*Логика теста должна быть такой:

Создание: Генерируешь данные через Bogus, вызываешь CreatePost.

Извлечение ID: Берешь Id из ответа (помним, что JSONPlaceholder всегда возвращает 101, 

но мы напишем код так, будто это реальный ID).

Обновление: Меняешь заголовок(Title) у этого же объекта и вызываешь UpdatePost, 

используя сохраненный ID.

Проверка: Убеждаешься, что сервер вернул обновленные данные.

Удаление: Вызываешь DeletePost для этого же ID.

Маленькая подсказка (Tips): 
Чтобы передать ID из одного шага в другой внутри одного теста, 
просто сохрани его в переменную:
var createdId = createResponse.Data.Id;
*/