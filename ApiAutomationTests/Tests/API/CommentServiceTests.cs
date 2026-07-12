using System.Net;
using FluentAssertions;
using Bogus;
using RestSharp;

namespace ApiAutomationTests.Tests.API
{
    public class CommentServiceTests : BaseTest
    {
                [Test]
                public async Task GetCommentsByPostId_ShouldReturnComments()
                {
                    // Arrange: Получаем пост №1, берем его Id

                    var postResponse = await PostService.GetPost(1);
                    int postId = postResponse.Data?.Id ?? throw new System.InvalidOperationException("Post or Post.Id is null");

                    // Act: Получаем все комментарии к этому посту

                    var commentsResponse = await CommentService.GetCommentsByPostId(postId);
                    var comments = commentsResponse.Data;

                    // Assert: Проверяем, что в списке комментариев больше 0 элементов 
                    // и что у каждого комментария поле Email содержит символ @.

                    comments.Should().NotBeEmpty();
                    comments.Should().OnlyContain(c => c.Email.Contains("@"));
                }
    }
}
