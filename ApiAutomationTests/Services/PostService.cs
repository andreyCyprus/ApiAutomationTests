using ApiAutomationTests.Models;
using RestSharp;

namespace ApiAutomationTests.Services;

public class PostService
{
    private readonly RestClient _client;

    public PostService(RestClient client)
    {
        _client = client;
    }

    // Метод для получения поста
    public async Task<RestResponse<Post>> GetPost(int id)
    {
        var request = new RestRequest($"/posts/{id}", Method.Get);
        return await _client.ExecuteAsync<Post>(request);
    }

    // Метод для создания поста
    public async Task<RestResponse<Post>> CreatePost(Post post)
    {
        var request = new RestRequest("/posts", Method.Post);
        request.AddJsonBody(post);
        return await _client.ExecuteAsync<Post>(request);
    }

    // Метод для удаления
    public async Task<RestResponse> DeletePost(int id)
    {
        var request = new RestRequest($"/posts/{id}", Method.Delete);
        return await _client.ExecuteAsync(request);
    }

    //Задание: Попробуй самостоятельно добавить
    //в PostService метод для получения списка
    //всех постов (GET /posts). Помни, что этот метод
    //должен возвращать List<Post>.

    public async Task<RestResponse<List<Post>>> GetAllPosts()
    {
        var request = new RestRequest("/posts", Method.Get);
        return await _client.ExecuteAsync<List<Post>>(request);
    }

    public async Task<RestResponse<Post>> UpdatePost(int id, Post post)
    {
        var request = new RestRequest($"/posts/{id}", Method.Put);
        request.AddJsonBody(post);
        return await _client.ExecuteAsync<Post>(request);
    }

}