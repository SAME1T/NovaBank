using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly BankDbContext _context;

    public AuditLogRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuditLog>> QueryAsync(
        DateTime? from,
        DateTime? to,
        string? search,
        string? action,
        bool? success,
        int take,
        CancellationToken ct = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        // Tarih filtresi - UTC'ye çevir (PostgreSQL timestamptz için)
        if (from.HasValue)
        {
            var fromUtc = ToUtcSafe(from.Value.Date);
            query = query.Where(x => x.CreatedAt >= fromUtc);
        }

        if (to.HasValue)
        {
            // to.Date.AddDays(1) ile exclusive end yap (gün bazlı filtreleme için)
            var toUtcExclusive = ToUtcSafe(to.Value.Date.AddDays(1));
            query = query.Where(x => x.CreatedAt < toUtcExclusive);
        }

        // Action filtresi
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action);

        // Success filtresi
        if (success.HasValue)
            query = query.Where(x => x.Success == success.Value);

        // Search filtresi (Summary, EntityId, ActorRole, Action içinde arama)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = $"%{search.Trim()}%";
            query = query.Where(x =>
                (x.Summary != null && EF.Functions.ILike(x.Summary, searchTerm)) ||
                (x.EntityId != null && EF.Functions.ILike(x.EntityId, searchTerm)) ||
                (x.ActorRole != null && EF.Functions.ILike(x.ActorRole, searchTerm)) ||
                (x.Action != null && EF.Functions.ILike(x.Action, searchTerm)));
        }

        // Take clamp (min 1, max 1000)
        var takeClamped = Math.Clamp(take, 1, 1000);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(takeClamped)
            .ToListAsync(ct);
    }

    /// <summary>
    /// DateTime'ı UTC'ye çevirir. PostgreSQL timestamp with time zone için gereklidir.
    /// </summary>
    private static DateTime ToUtcSafe(DateTime dt)
    {
        if (dt.Kind == DateTimeKind.Utc) return dt;
        if (dt.Kind == DateTimeKind.Local) return dt.ToUniversalTime();
        return DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime();
    }
}

