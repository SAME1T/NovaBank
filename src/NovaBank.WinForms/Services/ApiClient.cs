#nullable enable
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using NovaBank.Contracts.Accounts;
using NovaBank.Contracts.Transactions;
using NovaBank.Contracts.Reports;
using NovaBank.Contracts.Admin;

namespace NovaBank.WinForms.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
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
        return await _http.GetFromJsonAsync<T>(url, _jsonOptions);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body)
    {
        AddAuthHeaders();
        return await _http.PostAsJsonAsync(url, body, _jsonOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        AddAuthHeaders();
        var response = await _http.PostAsJsonAsync(url, body, _jsonOptions);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
        return default;
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T body)
    {
        AddAuthHeaders();
        return await _http.PutAsJsonAsync(url, body, _jsonOptions);
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

    public async Task<List<AccountResponse>?> GetAllAccountsAsync()
    {
        return await GetAsync<List<AccountResponse>>("/api/v1/accounts");
    }

    public async Task<HttpResponseMessage> CreateAccountAsync(CreateAccountRequest request)
    {
        return await PostAsync("/api/v1/accounts", request);
    }

    public async Task<List<AccountResponse>?> GetAccountsAsync()
    {
        return await GetAsync<List<AccountResponse>>("/api/v1/accounts");
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

    // Son işlemler
    public async Task<List<TransactionDto>?> GetTransactionsAsync(int limit = 10)
    {
        return await GetAsync<List<TransactionDto>>($"/api/v1/transactions?limit={limit}");
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
            return await response.Content.ReadFromJsonAsync<NovaBank.Contracts.Admin.ResetCustomerPasswordResponse>(_jsonOptions);
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

        var result = await response.Content.ReadFromJsonAsync<List<AuditLogResponse>>(_jsonOptions);
        return result ?? new List<AuditLogResponse>();
    }

    // Pending Approvals
    public async Task<List<PendingApprovalResponse>> GetPendingApprovalsAsync()
    {
        var url = "/api/v1/admin/pending-approvals";
        return await GetAsync<List<PendingApprovalResponse>>(url) ?? new List<PendingApprovalResponse>();
    }

    public async Task<HttpResponseMessage> ApproveCustomerAsync(Guid customerId)
    {
        var url = $"/api/v1/admin/customers/{customerId}/approve";
        AddAuthHeaders();
        return await _http.PostAsync(url, null);
    }

    public async Task<HttpResponseMessage> RejectCustomerAsync(Guid customerId)
    {
        var url = $"/api/v1/admin/customers/{customerId}/reject";
        AddAuthHeaders();
        return await _http.PostAsync(url, null);
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

    // Approval Workflow Methods
    public async Task<List<NovaBank.Contracts.ApprovalWorkflows.ApprovalWorkflowResponse>> GetApprovalWorkflowsAsync(NovaBank.Core.Enums.UserRole? role = null)
    {
        var url = "/api/approval-workflows/pending";
        if (role.HasValue)
            url += $"?role={role.Value}";
            
        return await GetAsync<List<NovaBank.Contracts.ApprovalWorkflows.ApprovalWorkflowResponse>>(url) ?? new List<NovaBank.Contracts.ApprovalWorkflows.ApprovalWorkflowResponse>();
    }

    public async Task<HttpResponseMessage> ApproveWorkflowAsync(Guid id)
    {
        var url = $"/api/approval-workflows/{id}/approve";
        return await PostAsync(url, new { });
    }

    public async Task<HttpResponseMessage> RejectWorkflowAsync(Guid id, string reason)
    {
        var url = $"/api/approval-workflows/{id}/reject?reason={Uri.EscapeDataString(reason)}";
        return await PostAsync(url, new { });
    }

    // Limits
    public async Task<List<NovaBank.Contracts.Limits.TransactionLimitResponse>> GetLimitsAsync()
    {
        return await GetAsync<List<NovaBank.Contracts.Limits.TransactionLimitResponse>>("/api/limits") ?? new();
    }

    public async Task<HttpResponseMessage> CreateLimitAsync(NovaBank.Contracts.Limits.CreateLimitRequest request)
    {
        return await PostAsync("/api/limits", request);
    }
    
    // Commissions
    public async Task<List<NovaBank.Contracts.Commissions.CommissionResponse>> GetCommissionsAsync(NovaBank.Core.Enums.CommissionType type)
    {
        return await GetAsync<List<NovaBank.Contracts.Commissions.CommissionResponse>>($"/api/commissions?type={type}") ?? new();
    }

    public async Task<HttpResponseMessage> CreateCommissionAsync(NovaBank.Contracts.Commissions.CreateCommissionRequest request)
    {
        return await PostAsync("/api/commissions", request);
    }

    // KYC
    public async Task<HttpResponseMessage> SubmitKycAsync(NovaBank.Contracts.Kyc.CreateKycVerificationRequest request)
    {
        return await PostAsync("/api/kyc", request);
    }

    public async Task<List<NovaBank.Contracts.Kyc.KycVerificationResponse>> GetMyKycAsync()
    {
        return await GetAsync<List<NovaBank.Contracts.Kyc.KycVerificationResponse>>("/api/kyc/my") ?? new();
    }

    public async Task<List<NovaBank.Contracts.Kyc.KycVerificationResponse>> GetPendingKycAsync()
    {
        return await GetAsync<List<NovaBank.Contracts.Kyc.KycVerificationResponse>>("/api/kyc/pending") ?? new();
    }

    public async Task<HttpResponseMessage> VerifyKycAsync(Guid id)
    {
        return await PostAsync($"/api/kyc/{id}/verify", new { });
    }

    public async Task<HttpResponseMessage> RejectKycAsync(Guid id, string reason)
    {
        return await PostAsync($"/api/kyc/{id}/reject?reason={Uri.EscapeDataString(reason)}", new { });
    }

    // Bills
    public async Task<List<NovaBank.Contracts.Bills.BillInstitutionResponse>> GetBillInstitutionsAsync()
    {
        return await GetAsync<List<NovaBank.Contracts.Bills.BillInstitutionResponse>>("/api/bills/institutions") ?? new();
    }

    public async Task<NovaBank.Contracts.Bills.BillInquiryResponse?> InquireBillAsync(NovaBank.Contracts.Bills.BillInquiryRequest request)
    {
        var resp = await PostAsync<NovaBank.Contracts.Bills.BillInquiryRequest, NovaBank.Contracts.Bills.BillInquiryResponse>("/api/bills/inquire", request);
        return resp;
    }

    public async Task<HttpResponseMessage> PayBillAsync(NovaBank.Contracts.Bills.PayBillRequest request)
    {
        return await PostAsync("/api/bills/pay", request);
    }

    public async Task<List<NovaBank.Contracts.Bills.BillPaymentResponse>> GetMyBillHistoryAsync()
    {
        return await GetAsync<List<NovaBank.Contracts.Bills.BillPaymentResponse>>("/api/bills/my-history") ?? new();
    }

    // Admin: Manage Bill Institutions
    public async Task<HttpResponseMessage> CreateBillInstitutionAsync(NovaBank.Contracts.Bills.CreateBillInstitutionRequest request)
    {
        return await PostAsync("/api/bills/institutions", request);
    }

    public async Task<HttpResponseMessage> DeleteBillInstitutionAsync(Guid institutionId)
    {
        AddAuthHeaders();
        return await _http.DeleteAsync($"/api/bills/institutions/{institutionId}");
    }
    
    // Notifications
    public async Task<List<NovaBank.Contracts.Notifications.NotificationResponse>> GetMyNotificationsAsync(int take = 50)
    {
         return await GetAsync<List<NovaBank.Contracts.Notifications.NotificationResponse>>($"/api/notifications?take={take}") ?? new();
    }
    
    public async Task<int> GetUnreadNotificationCountAsync()
    {
        return await GetAsync<int>("/api/notifications/unread-count");
    }
    
    public async Task MarkNotificationReadAsync(Guid id)
    {
        await PostAsync($"/api/notifications/{id}/read", new { });
    }

    // ===== CREDIT CARD =====
    
    /// <summary>Kredi kartı başvurusu yap</summary>
    public async Task<HttpResponseMessage> ApplyCreditCardAsync(decimal requestedLimit, decimal monthlyIncome)
    {
        return await PostAsync("/api/v1/credit-cards/apply", new { RequestedLimit = requestedLimit, MonthlyIncome = monthlyIncome });
    }

    /// <summary>Kullanıcının kartlarını getir</summary>
    public async Task<List<CreditCardSummaryDto>> GetMyCardsAsync()
    {
        return await GetAsync<List<CreditCardSummaryDto>>("/api/v1/credit-cards/my-cards") ?? new();
    }

    /// <summary>Kullanıcının kredi kartı başvurularını getir</summary>
    public async Task<List<CreditCardApplicationDto>> GetMyCardApplicationsAsync()
    {
        return await GetAsync<List<CreditCardApplicationDto>>("/api/v1/credit-cards/my-applications") ?? new();
    }

    /// <summary>Kredi kartı borcu öde</summary>
    public async Task<HttpResponseMessage> PayCardDebtAsync(Guid cardId, decimal amount, Guid fromAccountId)
    {
        return await PostAsync($"/api/v1/credit-cards/{cardId}/payment", new { Amount = amount, FromAccountId = fromAccountId });
    }

    // Admin: Bekleyen kredi kartı başvuruları
    public async Task<List<CreditCardApplicationDto>> GetPendingCardApplicationsAsync()
    {
        return await GetAsync<List<CreditCardApplicationDto>>("/api/v1/admin/credit-card-applications") ?? new();
    }

    public async Task<HttpResponseMessage> ApproveCardApplicationAsync(Guid applicationId, decimal approvedLimit)
    {
        return await PostAsync($"/api/v1/admin/credit-card-applications/{applicationId}/approve", new { ApprovedLimit = approvedLimit });
    }

    public async Task<HttpResponseMessage> RejectCardApplicationAsync(Guid applicationId, string reason)
    {
        return await PostAsync($"/api/v1/admin/credit-card-applications/{applicationId}/reject", new { Reason = reason });
    }

    // ========== CURRENCY EXCHANGE ==========
    
    public async Task<HttpResponseMessage> BuyCurrencyAsync(string currency, decimal amount, Guid fromTryAccountId, Guid toForeignAccountId, string? description = null)
    {
        return await PostAsync("/api/v1/currency-exchange/buy", new 
        { 
            Currency = currency, 
            Amount = amount, 
            FromTryAccountId = fromTryAccountId, 
            ToForeignAccountId = toForeignAccountId,
            Description = description
        });
    }

    public async Task<HttpResponseMessage> SellCurrencyAsync(string currency, decimal amount, Guid fromForeignAccountId, Guid toTryAccountId, string? description = null)
    {
        return await PostAsync("/api/v1/currency-exchange/sell", new 
        { 
            Currency = currency, 
            Amount = amount, 
            FromForeignAccountId = fromForeignAccountId, 
            ToTryAccountId = toTryAccountId,
            Description = description
        });
    }

    public async Task<CurrencyPositionsDto?> GetCurrencyPositionsAsync()
    {
        return await GetAsync<CurrencyPositionsDto>("/api/v1/currency-exchange/positions");
    }

    public async Task<CurrencyRateDto?> GetCurrentRateAsync(string currency)
    {
        return await GetAsync<CurrencyRateDto>($"/api/v1/currency-exchange/rate/{currency}");
    }

    public async Task<HttpResponseMessage> SaveExchangeRatesAsync(DateTime rateDate, List<ExchangeRateItemDto> rates)
    {
        return await PostAsync("/api/v1/currency-exchange/rates", new 
        { 
            RateDate = rateDate, 
            Rates = rates 
        });
    }

    // BranchManager yönetimi (Sadece Admin)
    public async Task<HttpResponseMessage> CreateBranchManagerAsync(CreateBranchManagerRequest request)
    {
        return await PostAsync("/api/v1/admin/branch-managers", request);
    }

    public async Task<CreateBranchManagerResponse?> CreateBranchManagerWithResponseAsync(CreateBranchManagerRequest request)
    {
        var resp = await PostAsync("/api/v1/admin/branch-managers", request);
        if (resp.IsSuccessStatusCode)
            return await resp.Content.ReadFromJsonAsync<CreateBranchManagerResponse>(_jsonOptions);
        return null;
    }

    public async Task<HttpResponseMessage> UpdateCustomerRoleAsync(Guid customerId, string role)
    {
        return await PutAsync($"/api/v1/admin/customers/{customerId}/role", new UpdateCustomerRoleRequest(role));
    }

    // Silme işlemleri (Sadece Admin)
    public async Task<HttpResponseMessage> DeleteAccountAsync(Guid accountId)
    {
        AddAuthHeaders();
        return await _http.DeleteAsync($"/api/v1/admin/accounts/{accountId}");
    }

    public async Task<HttpResponseMessage> DeleteCustomerAsync(Guid customerId)
    {
        AddAuthHeaders();
        return await _http.DeleteAsync($"/api/v1/admin/customers/{customerId}");
    }
}

// DTO'lar
public record CreditCardSummaryDto(
    Guid CardId, 
    string MaskedPan, 
    decimal CreditLimit, 
    decimal AvailableLimit,
    decimal CurrentDebt, 
    DateTime? MinPaymentDueDate,
    decimal? MinPaymentAmount,
    string Status,
    int ExpiryMonth,
    int ExpiryYear);

public record CreditCardApplicationDto(
    Guid ApplicationId,
    Guid CustomerId,
    string CustomerName,
    decimal MonthlyIncome,
    decimal RequestedLimit,
    decimal? ApprovedLimit,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    string? RejectionReason);

// Currency Exchange DTO'ları
public record CurrencyPositionDto(
    string Currency,
    decimal TotalAmount,
    decimal AverageCostRate,
    decimal TotalCostTry,
    decimal CurrentRate,
    decimal CurrentValue,
    decimal UnrealizedPnlTry,
    decimal UnrealizedPnlPercent);

public record CurrencyPositionsDto(
    List<CurrencyPositionDto> Positions,
    decimal TotalCostTry,
    decimal TotalCurrentValue,
    decimal TotalUnrealizedPnlTry,
    decimal TotalUnrealizedPnlPercent);

public record CurrencyRateDto(
    decimal BuyRate,
    decimal SellRate,
    DateTime RateDate);

public record CurrencyExchangeResultDto(
    Guid TransactionId,
    string ReferenceCode,
    string Currency,
    decimal Amount,
    decimal ExchangeRate,
    decimal TryAmount,
    decimal Commission,
    decimal NetTryAmount,
    decimal? RealizedPnlTry,
    decimal? RealizedPnlPercent,
    PositionSnapshotDto NewPosition);

public record PositionSnapshotDto(
    decimal TotalAmount,
    decimal AverageCostRate,
    decimal TotalCostTry);

public record ExchangeRateItemDto(
    string CurrencyCode,
    decimal BuyRate,
    decimal SellRate);

// Transaction DTO
public record TransactionDto(
    Guid Id,
    Guid AccountId,
    string Type,
    decimal Amount,
    string Currency,
    string? Description,
    DateTime CreatedAt);

