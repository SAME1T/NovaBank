using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NovaBank.Application.Extensions;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Infrastructure.Extensions;
using NovaBank.Infrastructure.Persistence;
using NovaBank.Infrastructure.Persistence.Repositories;
using NovaBank.Infrastructure.Persistence.Seeding;
using NovaBank.Api.Endpoints;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// JSON Enum String Converter - String olarak gelen enum değerlerini parse etmek için
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var conn = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddHttpContextAccessor(); // Required for AuditLogger and CurrentUser
builder.Services.AddInfrastructure(conn, builder.Configuration);
builder.Services.AddApplication();

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", opt =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opt.AddPolicy("AdminOrBranchManager", p => p.RequireRole("Admin", "BranchManager"));
    opt.AddPolicy("AnyUser", p => p.RequireAuthenticatedUser());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(t => (t.FullName ?? t.Name).Replace("+", "."));
    
    // JWT Bearer Security Definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
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
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

// Public endpoints (login, register) - AllowAnonymous already set in CustomersEndpoints
app.MapCustomers();

// Protected endpoints - RequireAuthorization will be set in each endpoint method
app.MapAccounts();
app.MapTransactions();
app.MapTransfers();
app.MapCards();
app.MapPaymentOrders();
app.MapLoans();
app.MapReports();

// Admin-only endpoints - RequireAuthorization will be set in MapAdmin method
app.MapAdmin();
app.MapCreditCards();
app.MapApprovalWorkflows();
app.MapLimits();
app.MapCommissions();
app.MapKyc();
app.MapBills();
app.MapNotifications();
app.MapCurrencyExchangeEndpoints();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();
