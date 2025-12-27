using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// Fatura kurumu.
/// </summary>
public sealed class BillInstitution : Entity
{
    private BillInstitution() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public BillCategory Category { get; private set; }
    public string? LogoUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    public BillInstitution(string code, string name, BillCategory category, string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Kurum kodu gerekli.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kurum adı gerekli.", nameof(name));

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Category = category;
        LogoUrl = logoUrl?.Trim();
    }

    public void Update(string name, BillCategory category, string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kurum adı gerekli.", nameof(name));

        Name = name.Trim();
        Category = category;
        LogoUrl = logoUrl?.Trim();
        TouchUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        TouchUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        TouchUpdated();
    }
}
