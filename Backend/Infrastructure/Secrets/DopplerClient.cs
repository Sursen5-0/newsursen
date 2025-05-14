using Application.Secrets;
using System.Net.Http.Headers;

namespace Infrastructure.Secrets
{
    public class DopplerClient : ISecretClient
    {
        private readonly HttpClient _httpClient;
        public DopplerClient(string token) {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.doppler.com/v3/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        public string GetSecret(string key)
        {
            _httpClient.GetAsync("",)
            return string.Empty;
        }
    }
}
