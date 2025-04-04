using CleanArch.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace CleanArch.Infrastructure.WebServices
{
    public class HttpClientBase
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger<HttpClientBase> _logger;
        protected readonly ISerializer _serializer;
        protected readonly HttpClientSettings _settings;

        public HttpClientBase(
            HttpClient httpClient,
            ILogger<HttpClientBase> logger,
            ISerializer serializer,
            IOptions<HttpClientSettings> settings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serializer = serializer;
            _settings = settings.Value;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        public void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_settings.AuthScheme, token);
        }

        public void ClearAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        protected async Task<TResponse?> GetAsync<TResponse>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return _serializer.Deserialize<TResponse>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
                throw;
            }
        }

        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var json = _serializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return _serializer.Deserialize<TResponse>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
                throw;
            }
        }

        protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var json = _serializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return _serializer.Deserialize<TResponse>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PUT request to {Endpoint}", endpoint);
                throw;
            }
        }

        protected async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DELETE request to {Endpoint}", endpoint);
                throw;
            }
        }
    }
}