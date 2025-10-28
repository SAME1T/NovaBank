using NovaBank.Infrastructure.Extensions;
using NovaBank.Infrastructure.Persistence;
using NovaBank.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddInfrastructure(conn);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapCustomers();
app.MapAccounts();
app.MapTransactions();
app.MapTransfers();
app.MapCards();
app.MapPaymentOrders();
app.MapLoans();
app.MapReports();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();
