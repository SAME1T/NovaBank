using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using NovaBank.Contracts.Accounts;
using NovaBank.Contracts.Transactions;
using NovaBank.Contracts.Reports;
using NovaBank.Contracts.Admin;

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

    /// <summary>
    /// Her request'e header ekle (X-Customer-Id, X-Role).
    /// </summary>
    private void AddAuthHeaders()
    {
        _http.DefaultRequestHeaders.Remove("X-Customer-Id");
        _http.DefaultRequestHeaders.Remove("X-Role");
        
        if (Session.CurrentCustomerId.HasValue)
        {
            _http.DefaultRequestHeaders.Add("X-Customer-Id", Session.CurrentCustomerId.Value.ToString());
        }
        
        if (Session.CurrentRole.HasValue)
        {
            _http.DefaultRequestHeaders.Add("X-Role", Session.CurrentRole.Value.ToString());
        }
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        AddAuthHeaders();
        return await _http.GetFromJsonAsync<T>(url);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body)
    {
        AddAuthHeaders();
        return await _http.PostAsJsonAsync(url, body);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        AddAuthHeaders();
        var response = await _http.PostAsJsonAsync(url, body);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<TResponse>();
        return default;
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T body)
    {
        AddAuthHeaders();
        return await _http.PutAsJsonAsync(url, body);
    }

    /// <summary>
    /// HTTP response'dan hata mesajını parse eder.
    /// </summary>
    public static async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
                return content;
        }
        catch { }
        
        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.NotFound => "Kayıt bulunamadı.",
            System.Net.HttpStatusCode.BadRequest => "Geçersiz istek.",
            System.Net.HttpStatusCode.Conflict => "Çakışma hatası.",
            System.Net.HttpStatusCode.Unauthorized => "Yetkisiz erişim.",
            _ => $"HTTP {response.StatusCode}: {response.ReasonPhrase}"
        };
    }

    // Hesap işlemleri
    public async Task<List<AccountResponse>?> GetAccountsByCustomerIdAsync(Guid customerId)
    {
        return await GetAsync<List<AccountResponse>>($"/api/v1/accounts/by-customer/{customerId}");
    }

    // Para işlemleri
    public async Task<HttpResponseMessage> DepositAsync(Guid accountId, decimal amount, string currency, string? description)
    {
        var currencyEnum = Enum.Parse<NovaBank.Core.Enums.Currency>(currency);
        var req = new DepositRequest(accountId, amount, currencyEnum, description);
        return await PostAsync("/api/v1/transactions/deposit", req);
    }

    public async Task<HttpResponseMessage> WithdrawAsync(Guid accountId, decimal amount, string currency, string? description)
    {
        var currencyEnum = Enum.Parse<NovaBank.Core.Enums.Currency>(currency);
        var req = new WithdrawRequest(accountId, amount, currencyEnum, description);
        return await PostAsync("/api/v1/transactions/withdraw", req);
    }

    // Transfer işlemleri
    public async Task<HttpResponseMessage> TransferInternalAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string currency, string? description)
    {
        var currencyEnum = Enum.Parse<NovaBank.Core.Enums.Currency>(currency);
        var req = new TransferInternalRequest(fromAccountId, toAccountId, amount, currencyEnum, description);
        return await PostAsync("/api/v1/transfers/internal", req);
    }

    public async Task<HttpResponseMessage> TransferExternalAsync(Guid fromAccountId, string toIban, decimal amount, string currency, string? description)
    {
        var currencyEnum = Enum.Parse<NovaBank.Core.Enums.Currency>(currency);
        var req = new TransferExternalRequest(fromAccountId, toIban, amount, currencyEnum, description);
        return await PostAsync("/api/v1/transfers/external", req);
    }

    // Ekstre
    public async Task<AccountStatementResponse?> GetStatementAsync(Guid accountId, DateTime from, DateTime to)
    {
        var url = $"/api/v1/reports/account-statement?accountId={accountId}&from={from:O}&to={to:O}";
        return await GetAsync<AccountStatementResponse>(url);
    }

    // Admin işlemleri
    public async Task<List<NovaBank.Contracts.Admin.CustomerSummaryResponse>> SearchCustomersAsync(string? search)
    {
        var url = "/api/v1/admin/customers";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"?search={Uri.EscapeDataString(search)}";
        return await GetAsync<List<NovaBank.Contracts.Admin.CustomerSummaryResponse>>(url) ?? new List<NovaBank.Contracts.Admin.CustomerSummaryResponse>();
    }

    public async Task<List<NovaBank.Contracts.Admin.AccountAdminResponse>> GetCustomerAccountsAsync(Guid customerId)
    {
        var url = $"/api/v1/admin/customers/{customerId}/accounts";
        return await GetAsync<List<NovaBank.Contracts.Admin.AccountAdminResponse>>(url) ?? new List<NovaBank.Contracts.Admin.AccountAdminResponse>();
    }

    public async Task<HttpResponseMessage> UpdateOverdraftLimitAsync(Guid accountId, decimal overdraftLimit)
    {
        var url = $"/api/v1/admin/accounts/{accountId}/overdraft";
        var request = new NovaBank.Contracts.Admin.UpdateOverdraftRequest(overdraftLimit);
        return await PutAsync(url, request);
    }

    public async Task<HttpResponseMessage> UpdateAccountStatusAsync(Guid accountId, string status)
    {
        var url = $"/api/v1/admin/accounts/{accountId}/status";
        var request = new NovaBank.Contracts.Admin.UpdateAccountStatusRequest(status);
        return await PutAsync(url, request);
    }
}
