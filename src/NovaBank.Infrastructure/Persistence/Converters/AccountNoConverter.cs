using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Infrastructure.Persistence.Converters;

public sealed class AccountNoConverter : ValueConverter<AccountNo,long>
{
    public AccountNoConverter() : base(
        v => v.Value,
        v => new AccountNo(v))
    { }
}
