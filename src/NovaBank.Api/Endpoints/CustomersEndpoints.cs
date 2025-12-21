using Microsoft.AspNetCore.Http.HttpResults;
using NovaBank.Application.Customers;
using NovaBank.Application.Common.Errors;
using NovaBank.Contracts.Customers;

namespace NovaBank.Api.Endpoints;
public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomers(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/customers");

        g.MapPost("/", async Task<Results<Created<CustomerResponse>, BadRequest<string>>> (CreateCustomerRequest req, ICustomersService service) =>
        {
            var result = await service.CreateCustomerAsync(req);
            if (!result.IsSuccess)
                return TypedResults.BadRequest(result.ErrorMessage ?? "Müşteri oluşturulamadı.");

            return TypedResults.Created($"/api/v1/customers/{result.Value!.Id}", result.Value);
        }).AllowAnonymous();

        g.MapGet("/{id:guid}", async Task<Results<Ok<CustomerResponse>, NotFound>> (Guid id, ICustomersService service) =>
        {
            var result = await service.GetByIdAsync(id);
            if (!result.IsSuccess)
                return TypedResults.NotFound();

            return TypedResults.Ok(result.Value!);
        });

        g.MapGet("/", async Task<Ok<List<CustomerResponse>>> (ICustomersService service) =>
        {
            var result = await service.GetAllAsync();
            return TypedResults.Ok(result.Value ?? new List<CustomerResponse>());
        });

        g.MapPost("/login", async Task<Results<Ok<LoginResponse>, BadRequest<string>, StatusCodeHttpResult>> (LoginRequest req, ICustomersService service) =>
        {
            var result = await service.LoginAsync(req);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.StatusCode(403),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Giriş başarısız.")
                };
            }

            return TypedResults.Ok(result.Value!);
        }).AllowAnonymous();

        g.MapPost("/password-reset/request", async Task<Results<Ok<PasswordResetRequestResponse>, ProblemHttpResult>> 
            (PasswordResetRequest req, ICustomersService service) =>
        {
            var result = await service.RequestPasswordResetAsync(req);
            if (!result.IsSuccess)
            {
                return TypedResults.Problem(
                    detail: result.ErrorMessage ?? "Şifre sıfırlama isteği başarısız.",
                    statusCode: result.ErrorCode == ErrorCodes.EmailSendFailed ? 500 : 400,
                    title: "Password Reset Failed",
                    extensions: new Dictionary<string, object?>
                    {
                        ["errorCode"] = result.ErrorCode,
                        ["errorMessage"] = result.ErrorMessage
                    });
            }
            return TypedResults.Ok(result.Value!);
        }).AllowAnonymous();

        g.MapPost("/password-reset/verify", async Task<Results<Ok<PasswordResetVerifyResponse>, BadRequest<string>>> 
            (PasswordResetVerifyRequest req, ICustomersService service) =>
        {
            var result = await service.VerifyPasswordResetAsync(req);
            if (!result.IsSuccess)
            {
                return TypedResults.BadRequest(result.ErrorMessage ?? "Kod doğrulama başarısız.");
            }
            return TypedResults.Ok(result.Value!);
        }).AllowAnonymous();

        g.MapPost("/password-reset/complete", async Task<Results<Ok<PasswordResetCompleteResponse>, BadRequest<string>>> 
            (PasswordResetCompleteRequest req, ICustomersService service) =>
        {
            var result = await service.CompletePasswordResetAsync(req);
            if (!result.IsSuccess)
            {
                return TypedResults.BadRequest(result.ErrorMessage ?? "Şifre sıfırlama başarısız.");
            }
            return TypedResults.Ok(result.Value!);
        }).AllowAnonymous();

        return app;
    }
}
