using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Infrastructure.Persistence.Converters;

public sealed class NationalIdConverter : ValueConverter<NationalId,string>
{
    public NationalIdConverter() : base(
        v => v.Value,
        v => new NationalId(v))
    { }
}
