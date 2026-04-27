using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Client.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public bool LastRequestWasUnauthorized { get; private set; }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ServerConfig.ApiUrl + "Auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<LoginResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<(bool IsSuccess, string ResponseText)> PostAsync<T>(string endpoint, T data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ServerConfig.ApiUrl + endpoint, content);
            var responseText = await response.Content.ReadAsStringAsync();

            return (response.IsSuccessStatusCode, responseText);
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            LastRequestWasUnauthorized = false;

            var response = await _httpClient.GetAsync(ServerConfig.ApiUrl + endpoint);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                LastRequestWasUnauthorized = true;
                return default;
            }

            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}