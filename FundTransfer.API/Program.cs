using FundTransfer.Application.Interfaces;
using FundTransfer.Application.Services;
using FundTransfer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use InMemoryAccountStore for simplicity.
builder.Services.AddSingleton<IAccountStore, InMemoryAccountStore>();

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

app.Run();
