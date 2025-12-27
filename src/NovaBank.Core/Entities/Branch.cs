using NovaBank.Core.Abstractions;

namespace NovaBank.Core.Entities;

/// <summary>
/// Banka şubesi.
/// </summary>
public sealed class Branch : Entity
{
    private Branch() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string? District { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public Guid? ManagerId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Branch(string code, string name, string city, string? district = null, string? address = null, string? phone = null, Guid? managerId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Şube kodu gerekli.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Şube adı gerekli.", nameof(name));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Şehir gerekli.", nameof(city));

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        City = city.Trim();
        District = district?.Trim();
        Address = address?.Trim();
        Phone = phone?.Trim();
        ManagerId = managerId;
    }

    public void Update(string name, string city, string? district, string? address, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Şube adı gerekli.", nameof(name));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Şehir gerekli.", nameof(city));

        Name = name.Trim();
        City = city.Trim();
        District = district?.Trim();
        Address = address?.Trim();
        Phone = phone?.Trim();
        TouchUpdated();
    }

    public void AssignManager(Guid managerId)
    {
        ManagerId = managerId;
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
