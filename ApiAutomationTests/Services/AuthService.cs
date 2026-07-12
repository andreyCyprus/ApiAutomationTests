using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;

namespace ApiAutomationTests.Services
{
    public class AuthService
    {
        private readonly RestClient _client;

        public AuthService(RestClient client)
        {
            _client = client;
        }

        public string GetToken()
        {
            // Имитируем логику: в реальности тут был бы запрос к API
            // Но сейчас мы просто возвращаем строку для проверки механики
            return "fake-jwt-token-12345";
        }
    }
}
