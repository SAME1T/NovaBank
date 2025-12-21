using Microsoft.AspNetCore.Http.HttpResults;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Reports;
using NovaBank.Contracts.Reports;

namespace NovaBank.Api.Endpoints;
public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReports(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/reports").RequireAuthorization("AnyUser");

        g.MapGet("/account-statement", async Task<Results<Ok<AccountStatementResponse>, BadRequest<string>, NotFound>>
        (Guid accountId, DateTime from, DateTime to, IReportsService service) =>
        {
            var result = await service.GetAccountStatementAsync(accountId, from, to);
            if (!result.IsSuccess)
            {
                if (result.ErrorCode == ErrorCodes.NotFound)
                    return TypedResults.NotFound();
                return TypedResults.BadRequest(result.ErrorMessage ?? "Ekstre alınamadı.");
            }

            return TypedResults.Ok(result.Value!);
        });

        g.MapGet("/customer-summary", async Task<Results<Ok<CustomerSummaryResponse>, NotFound>>
        (Guid customerId, IReportsService service) =>
        {
            var result = await service.GetCustomerSummaryAsync(customerId);
            if (!result.IsSuccess)
                return TypedResults.NotFound();

            return TypedResults.Ok(result.Value!);
        });

        return app;
    }
}
