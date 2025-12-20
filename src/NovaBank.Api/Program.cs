using NovaBank.Application.Extensions;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Infrastructure.Extensions;
using NovaBank.Infrastructure.Persistence;
using NovaBank.Infrastructure.Persistence.Repositories;
using NovaBank.Infrastructure.Persistence.Seeding;
using NovaBank.Api.Endpoints;
using NovaBank.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddInfrastructure(conn);
builder.Services.AddApplication();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(t => (t.FullName ?? t.Name).Replace("+", "."));
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Seed sistem hesapları ve admin (idempotent)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    var accountRepo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
    var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
    
    var systemSeeder = new SystemAccountSeeder(context, accountRepo, customerRepo);
    await systemSeeder.SeedSystemAccountsAsync();
    
    var adminSeeder = new AdminSeeder(context, customerRepo);
    await adminSeeder.SeedAdminAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// CurrentUser middleware (header-based auth)
app.UseMiddleware<CurrentUserMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapCustomers();
app.MapAccounts();
app.MapTransactions();
app.MapTransfers();
app.MapCards();
app.MapPaymentOrders();
app.MapLoans();
app.MapReports();
app.MapAdmin();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();
