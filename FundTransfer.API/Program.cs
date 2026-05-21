using FundTransfer.Application.Interfaces;
using FundTransfer.Application.Services;
using FundTransfer.Domain.Entities;
using FundTransfer.Infrastructure;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<FundTransfer.Application.DTOs.TransferRequest>, FundTransfer.Application.Validators.TransferRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core repository - use in-memory database instead of PostgreSQL
builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseInMemoryDatabase("FundTransferDb"));

builder.Services.AddScoped<IAccountStore, EfAccountStore>();

// Idempotency and fraud services
builder.Services.AddSingleton<FundTransfer.Application.Interfaces.IIdempotencyStore, FundTransfer.Infrastructure.InMemoryIdempotencyStore>();
builder.Services.AddSingleton<FundTransfer.Application.Interfaces.IFraudService>(_ => new FundTransfer.Infrastructure.SimpleThresholdFraudService());
// Audit logger (file-based) - write to app content root
builder.Services.AddSingleton<FundTransfer.Application.Interfaces.IAuditLogger>(_ =>
    new FundTransfer.Infrastructure.FileAuditLogger(Path.Combine(builder.Environment.ContentRootPath, "audit.log")));

// Read OTP secret from configuration.
builder.Services.AddScoped<IOtpValidator, ConfigOtpValidator>();

builder.Services.AddScoped<TransferService>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    context.Database.EnsureCreated();

    if (!context.Accounts.Any())
    {
        context.Accounts.AddRange(
            new Account { AccountId = "ACC1", Balance = 1000m },
            new Account { AccountId = "ACC2", Balance = 1000m }
        );
        context.SaveChanges();
    }
}

app.Run();
