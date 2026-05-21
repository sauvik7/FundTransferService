using FundTransfer.Application.Interfaces;
using FundTransfer.Application.Services;
using FundTransfer.Infrastructure;
using FluentValidation.AspNetCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<FundTransfer.Application.DTOs.TransferRequest>, FundTransfer.Application.Validators.TransferRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use InMemoryAccountStore for simplicity.
builder.Services.AddSingleton<IAccountStore, InMemoryAccountStore>();

// Idempotency and fraud services
builder.Services.AddSingleton<FundTransfer.Application.Interfaces.IIdempotencyStore, FundTransfer.Infrastructure.InMemoryIdempotencyStore>();
builder.Services.AddSingleton<FundTransfer.Application.Interfaces.IFraudService>(_ => new FundTransfer.Infrastructure.SimpleThresholdFraudService());
// Audit logger (file-based) - write to app content root
builder.Services.AddSingleton<FundTransfer.Application.Interfaces.IAuditLogger>(_ =>
    new FundTransfer.Infrastructure.FileAuditLogger(Path.Combine(builder.Environment.ContentRootPath, "audit.log")));

// Read OTP secret from configuration.
builder.Services.AddScoped<IOtpValidator, ConfigOtpValidator>();

builder.Services.AddScoped<TransferService>();
// TransferService depends on IIdempotencyStore and IFraudService now; DI will resolve them.

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

app.Run();
