using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace NovaBank.WinForms.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    public string BaseUrl { get; }
    public ApiClient()
    {
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
        BaseUrl = cfg["ApiBaseUrl"] ?? "http://localhost:5221";
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task<T?> GetAsync<T>(string url) => await _http.GetFromJsonAsync<T>(url);
    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body) => await _http.PostAsJsonAsync(url, body);
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        var response = await _http.PostAsJsonAsync(url, body);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<TResponse>();
        return default;
    }
}
