using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JustHangingAround.Client.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "https://localhost:7137/api/";

        public async Task<(bool IsSuccess, string ResponseText)> PostAsync<T>(string endpoint, T data)
        {
            var json = JsonSerializer.Serialize(data);

            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync(BaseUrl + endpoint, content);
            var responseText = await response.Content.ReadAsStringAsync();

            return (response.IsSuccessStatusCode, responseText);
        }

        // 👇 НОВЫЙ МЕТОД
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(BaseUrl + endpoint);

            if (!response.IsSuccessStatusCode)
                return default;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}