using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Infrastructure.Persistence.Converters;

public sealed class IbanConverter : ValueConverter<Iban,string>
{
    public IbanConverter() : base(
        v => v.Value,
        v => new Iban(v))
    { }
}
