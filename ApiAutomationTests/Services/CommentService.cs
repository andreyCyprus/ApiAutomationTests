using ApiAutomationTests.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiAutomationTests.Services
{
    public class CommentService
    {
        private readonly RestClient _client;
        public CommentService(RestClient client)
        {
            _client = client;
        }

        // Метод для получения комментариев по ID поста
        public async Task<RestResponse<List<Comment>>> GetCommentsByPostId(int postId)
        {
            var request = new RestRequest($"/comments?postId={postId}", Method.Get);
            var response = await _client.ExecuteAsync<List<Comment>>(request);
            return response;
        }

    }
}
/*Задание «Сборная солянка»:

Модель: Создай новую модель Comment.cs (у JSONPlaceholder есть эндпоинт /comments). 
В ней должны быть поля: id, name, email, body, postId.
Сервис: Добавь в PostService (или создай CommentService) 
метод GetCommentsByPostId(int postId). 
Эндпоинт: /comments?postId={id}.
Тест:
Через PostService получи пост №1.
Возьми его Id.
Через новый метод получи все комментарии к этому посту.
Assert: Проверь, что в списке комментариев больше 0 элементов 
и что у каждого комментария поле Email содержит символ @.

я лишь уточню правильный ли я путь выбрал: 
1. я уже создал класс public class CommentService с методом public List<Comment> GetCommentsByPostId(int postId) внутри. 
2. я уже добавил в классе BaseTest : protected CommentService CommentService; и инициализировал CommentService = new CommentService(Client); 
3. создам класс public class CommentServiceTests: BaseTest по аналогии с public class PostLifecycleTests : BaseTest
4. в CommentServiceTests реализую : 
// Arrange: Готовим данные через Bogus,
// Act: получаю пост №1, беру его Id, Через новый метод получаю все комментарии к этому посту
// Assert: Проверяю, что в списке комментариев больше 0 элементов 
и что у каждого комментария поле Email содержит символ @.
Дай мне знать если я не прав в где-то? 
*/ 