using FundTransfer.Application.Interfaces;
using FundTransfer.Application.Services;
using FundTransfer.Application.Validators;
using FundTransfer.Domain.Services;
using FundTransfer.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------
// ✅ Explicitly bind BOTH HTTP + HTTPS
// -----------------------
builder.WebHost.UseUrls(
    "http://localhost:5202",
    "https://localhost:7202"
);

// -----------------------
// Controllers + Validation
// -----------------------
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<FundTransfer.Application.DTOs.TransferRequest>, TransferRequestValidator>();

// -----------------------
// Swagger
// -----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------
// Database
// -----------------------
builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseInMemoryDatabase("FundTransferDb"));

// -----------------------
// Application Services
// -----------------------
builder.Services.AddScoped<TransferService>();
builder.Services.AddScoped<TransferDomainService>();

// -----------------------
// Infrastructure
// -----------------------
builder.Services.AddScoped<IAccountStore, EfAccountStore>();
builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
builder.Services.AddScoped<IOtpValidator, ConfigOtpValidator>();

// Fraud config
var fraudThreshold = builder.Configuration.GetValue<decimal>("FraudSettings:Threshold");
builder.Services.AddSingleton<IFraudService>(new SimpleThresholdFraudService(fraudThreshold));

// Audit logger
builder.Services.AddSingleton<IAuditLogger>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var path = Path.Combine(env.ContentRootPath, "audit.log");
    return new FileAuditLogger(path);
});

// -----------------------
// Build App
// -----------------------
var app = builder.Build();

// -----------------------
// Middleware
// -----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        // ✅ Always point Swagger to HTTPS endpoint
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FundTransfer API V1");
    });
}

// ✅ Centralized exception handling endpoint
app.UseExceptionHandler("/error");


builder.WebHost.UseUrls("http://localhost:5202");

app.UseAuthorization();

app.MapControllers();

// ✅ Optional: basic error endpoint (so ExceptionHandler works correctly)
app.Map("/error", (HttpContext context) =>
{
    return Results.Problem("An unexpected error occurred");
});

// -----------------------
// Seed Data (dev only)
// -----------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

    if (!db.Accounts.Any())
    {
        db.Accounts.AddRange(
            new FundTransfer.Domain.Entities.Account("ACC1", 10000),
            new FundTransfer.Domain.Entities.Account("ACC2", 5000)
        );

        db.SaveChanges();
    }
}

app.Run();
