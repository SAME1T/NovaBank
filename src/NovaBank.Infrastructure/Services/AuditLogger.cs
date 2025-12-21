using Microsoft.AspNetCore.Http;
using NovaBank.Application.Common;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Infrastructure.Persistence;
using System.Text.Json;

namespace NovaBank.Infrastructure.Services;

public sealed class AuditLogger : IAuditLogger
{
    private readonly BankDbContext _dbContext;
    private readonly CurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogger(
        BankDbContext dbContext,
        CurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        string action,
        bool success,
        string? entityType = null,
        string? entityId = null,
        string? summary = null,
        object? metadata = null,
        string? errorCode = null,
        CancellationToken ct = default)
    {
        var metadataJson = metadata != null
            ? JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = false })
            : null;

        var actorRole = _currentUser.Role?.ToString() ?? "Anonymous";
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

        // Summary ve UserAgent'ı 256 karaktere kısalt (DB sınırı)
        var truncatedSummary = summary != null && summary.Length > 256
            ? summary.Substring(0, 253) + "..."
            : summary;
        
        var truncatedUserAgent = userAgent != null && userAgent.Length > 256
            ? userAgent.Substring(0, 253) + "..."
            : userAgent;

        var log = new AuditLog(
            actorCustomerId: _currentUser.CustomerId,
            actorRole: actorRole,
            action: action,
            success: success,
            entityType: entityType,
            entityId: entityId,
            summary: truncatedSummary,
            metadataJson: metadataJson,
            ipAddress: ipAddress,
            userAgent: truncatedUserAgent,
            errorCode: errorCode
        );

        _dbContext.AuditLogs.Add(log);
        await _dbContext.SaveChangesAsync(ct);
    }
}

