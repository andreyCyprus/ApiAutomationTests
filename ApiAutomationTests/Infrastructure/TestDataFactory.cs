using ApiAutomationTests.Models;
using Bogus;

namespace ApiAutomationTests.Infrastructure;

public static class TestDataFactory
{
    // Генератор для Постов
    public static Faker<Post> PostFaker => new Faker<Post>()
        .RuleFor(p => p.Title, f => f.Lorem.Sentence(3, 5))
        .RuleFor(p => p.Body, f => f.Lorem.Paragraphs(1, 3))
        .RuleFor(p => p.UserId, f => f.Random.Number(1, 100));

    // Генератор для Комментариев под вашу модель
    public static Faker<Comment> CommentFaker(int postId) => new Faker<Comment>()
        .RuleFor(c => c.PostId, _ => postId)
        .RuleFor(c => c.Name, f => f.Lorem.Sentence(2, 4))
        .RuleFor(c => c.Email, f => f.Internet.Email())
        .RuleFor(c => c.body, f => f.Lorem.Paragraph()); // Матчинг свойства body с маленькой буквы
}
