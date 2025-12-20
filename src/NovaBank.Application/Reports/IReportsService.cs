using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Reports;

namespace NovaBank.Application.Reports;

public interface IReportsService
{
    Task<Result<AccountStatementResponse>> GetAccountStatementAsync(Guid accountId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<CustomerSummaryResponse>> GetCustomerSummaryAsync(Guid customerId, CancellationToken ct = default);
}

