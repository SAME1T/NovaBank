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
    /// Her request'e Authorization Bearer header ekle.
    /// </summary>
    private void AddAuthHeaders()
    {
        _http.DefaultRequestHeaders.Remove("Authorization");
        
        if (!string.IsNullOrWhiteSpace(Session.AccessToken))
        {
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.AccessToken);
        }
    }

    /// <summary>
    /// HttpRequestMessage oluşturur ve Bearer token ekler.
    /// </summary>
    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        if (!string.IsNullOrWhiteSpace(Session.AccessToken))
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.AccessToken);
        return req;
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
    /// HTTP response'dan hata mesajını parse eder. ProblemDetails JSON formatını destekler.
    /// </summary>
    public static async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
            {
                // ProblemDetails JSON formatını kontrol et
                if (content.TrimStart().StartsWith("{") && content.Contains("\"detail\"") || content.Contains("\"title\""))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(content);
                        var root = doc.RootElement;
                        
                        // detail varsa onu kullan
                        if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            var detailStr = detail.GetString();
                            if (!string.IsNullOrWhiteSpace(detailStr))
                                return detailStr;
                        }
                        
                        // title varsa onu kullan
                        if (root.TryGetProperty("title", out var title) && title.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            var titleStr = title.GetString();
                            if (!string.IsNullOrWhiteSpace(titleStr))
                                return titleStr;
                        }
                        
                        // errorMessage extension varsa onu kullan
                        if (root.TryGetProperty("errorMessage", out var errorMsg) && errorMsg.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            var errorMsgStr = errorMsg.GetString();
                            if (!string.IsNullOrWhiteSpace(errorMsgStr))
                                return errorMsgStr;
                        }
                    }
                    catch
                    {
                        // JSON parse hatası olursa raw content döndür
                    }
                }
                
                // JSON değilse veya parse edilemezse raw content döndür
                return content;
            }
        }
        catch { }
        
        // Fallback: Status code'a göre genel mesaj
        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.NotFound => "Kayıt bulunamadı.",
            System.Net.HttpStatusCode.BadRequest => "Geçersiz istek.",
            System.Net.HttpStatusCode.Conflict => "Çakışma hatası.",
            System.Net.HttpStatusCode.Unauthorized => "Yetkisiz erişim.",
            System.Net.HttpStatusCode.InternalServerError => "Sunucu hatası oluştu.",
            _ => $"HTTP {(int)response.StatusCode} {response.StatusCode}: {response.ReasonPhrase ?? "Bilinmeyen hata"}"
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

    public async Task<HttpResponseMessage> UpdateCustomerActiveAsync(Guid customerId, bool isActive)
    {
        var url = $"/api/v1/admin/customers/{customerId}/active";
        var request = new NovaBank.Contracts.Admin.UpdateCustomerActiveRequest(isActive);
        return await PutAsync(url, request);
    }

    public async Task<NovaBank.Contracts.Admin.ResetCustomerPasswordResponse?> ResetCustomerPasswordAsync(Guid customerId)
    {
        var url = $"/api/v1/admin/customers/{customerId}/reset-password";
        AddAuthHeaders();
        var response = await _http.PostAsync(url, null);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<NovaBank.Contracts.Admin.ResetCustomerPasswordResponse>();
        return null;
    }

    public async Task<List<AuditLogResponse>> GetAuditLogsAsync(DateTime? from, DateTime? to, string? search, string? action, bool? success, int take = 200)
    {
        var queryParams = new List<string>();
        
        // from/to varsa UTC'ye çevirip ISO 8601 formatında ekle
        if (from.HasValue)
        {
            var fromUtc = from.Value.ToUniversalTime().ToString("O");
            queryParams.Add($"from={Uri.EscapeDataString(fromUtc)}");
        }
        if (to.HasValue)
        {
            var toUtc = to.Value.ToUniversalTime().ToString("O");
            queryParams.Add($"to={Uri.EscapeDataString(toUtc)}");
        }
        
        // search varsa ekle
        if (!string.IsNullOrWhiteSpace(search))
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        
        // action ALL/boş ise ekleme, değilse ekle
        if (!string.IsNullOrWhiteSpace(action))
            queryParams.Add($"action={Uri.EscapeDataString(action)}");
        
        // success ALL (null) ise ekleme, değilse ekle
        if (success.HasValue)
            queryParams.Add($"success={(success.Value ? "true" : "false")}");
        
        // take değerini clamp et
        take = Math.Clamp(take, 1, 1000);
        queryParams.Add($"take={take}");

        var url = "/api/v1/admin/audit-logs";
        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        // CreateRequest kullanarak istek oluştur (Bearer token otomatik eklenir)
        var request = CreateRequest(HttpMethod.Get, url);
        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await GetErrorMessageAsync(response);
            throw new Exception($"HTTP {(int)response.StatusCode}: {errorMessage}");
        }

        var result = await response.Content.ReadFromJsonAsync<List<AuditLogResponse>>();
        return result ?? new List<AuditLogResponse>();
    }

    // Password Reset
    public async Task<HttpResponseMessage> PasswordResetRequestAsync(string emailOrTc)
    {
        var url = "/api/v1/customers/password-reset/request";
        var req = new NovaBank.Contracts.Customers.PasswordResetRequest(emailOrTc);
        // Anonymous endpoint, token gerekmez
        return await PostAsync(url, req);
    }

    public async Task<HttpResponseMessage> PasswordResetVerifyAsync(string emailOrTc, string code)
    {
        var url = "/api/v1/customers/password-reset/verify";
        var req = new NovaBank.Contracts.Customers.PasswordResetVerifyRequest(emailOrTc, code);
        // Anonymous endpoint, token gerekmez
        return await PostAsync(url, req);
    }

    public async Task<HttpResponseMessage> PasswordResetCompleteAsync(string emailOrTc, string code, string newPassword)
    {
        var url = "/api/v1/customers/password-reset/complete";
        var req = new NovaBank.Contracts.Customers.PasswordResetCompleteRequest(emailOrTc, code, newPassword);
        // Anonymous endpoint, token gerekmez
        return await PostAsync(url, req);
    }
}
