using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaBank.Infrastructure.Persistence.DesignTime;

public sealed class BankDbContextFactory : IDesignTimeDbContextFactory<BankDbContext>
{
    public BankDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<BankDbContext>();
        builder.UseNpgsql("Host=localhost;Port=5432;Database=banka_db;Username=postgres;Password=postgres;Include Error Detail=true");
        return new BankDbContext(builder.Options);
    }
}
