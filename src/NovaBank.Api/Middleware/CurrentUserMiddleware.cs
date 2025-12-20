using NovaBank.Application.Common;
using NovaBank.Core.Enums;

namespace NovaBank.Api.Middleware;

/// <summary>
/// Header-based authentication middleware (MVP seviyesinde).
/// X-Customer-Id ve X-Role header'larını okuyup CurrentUser'a set eder.
/// </summary>
public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CurrentUser currentUser)
    {
        // Header'lardan customer ID ve role oku
        if (context.Request.Headers.TryGetValue("X-Customer-Id", out var customerIdHeader))
        {
            if (Guid.TryParse(customerIdHeader.ToString(), out var customerId))
            {
                currentUser.CustomerId = customerId;
            }
        }

        if (context.Request.Headers.TryGetValue("X-Role", out var roleHeader))
        {
            if (Enum.TryParse<UserRole>(roleHeader.ToString(), out var role))
            {
                currentUser.Role = role;
            }
        }

        await _next(context);
    }
}

