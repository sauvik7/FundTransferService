# FundTransferService

[![License: MIT](https://img.shields.io/badge/license-MIT-green)](https://opensource.org/licenses/MIT)
![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)
[![CI Build](https://github.com/sauvik7/FundTransferService/actions/workflows/ci.yml/badge.svg)](https://github.com/sauvik7/FundTransferService/actions)
[![Codecov](https://codecov.io/gh/sauvik7/FundTransferService/branch/main/graph/badge.svg)](https://codecov.io/gh/sauvik7/FundTransferService)

## Overview

FundTransferService is a .NET 8 Web API that simulates a simple fund transfer workflow with validation rules for OTP, balance, duplicate requests, fraud limits, same-account transfers, and missing account data.

## Environment

- .NET SDK 8.0
- C# / ASP.NET Core Web API
- xUnit for unit tests
- `XPlat Code Coverage` for coverage collection

## Project Initialization

The project was initialized with the following commands:

```bash
cd /home/sr125588/repos/FundTransferService
dotnet new sln -n FundTransferService
dotnet new webapi -o FundTransfer.API
dotnet new classlib -o FundTransfer.Application
dotnet new classlib -o FundTransfer.Domain
dotnet new classlib -o FundTransfer.Infrastructure
dotnet new xunit -n FundTransfer.Tests

dotnet sln add FundTransfer.API/FundTransfer.API.csproj
dotnet sln add FundTransfer.Application/FundTransfer.Application.csproj
dotnet sln add FundTransfer.Domain/FundTransfer.Domain.csproj
dotnet sln add FundTransfer.Infrastructure/FundTransfer.Infrastructure.csproj
dotnet sln add FundTransfer.Tests/FundTransfer.Tests.csproj

dotnet add FundTransfer.Tests/FundTransfer.Tests.csproj reference FundTransfer.Application/FundTransfer.Application.csproj
```

## Build and Run

To build the solution:

```bash
cd /home/sr125588/repos/FundTransferService
dotnet build FundTransfer.API/FundTransfer.API.csproj
```

To run the API locally:

```bash
cd /home/sr125588/repos/FundTransferService
dotnet run --project FundTransfer.API/FundTransfer.API.csproj
```

The API runs on HTTPS by default. For local testing, use the port shown in the console output.

## Test Targets

The primary test module is:

- `FundTransfer.Tests/FundTransfer.Tests.csproj`

This project contains service-level tests and controller tests.

## Run Tests

Run all unit tests with:

```bash
cd /home/sr125588/repos/FundTransferService
dotnet test --no-restore
```

## Generate Coverage

A reusable coverage script is available at `scripts/generate-coverage.sh`.

Run coverage for all test projects with:

```bash
cd /home/sr125588/repos/FundTransferService
./scripts/generate-coverage.sh
```

Coverage results are written to the `coverage/` folder, organized by test project name.

## VS Code Tasks

A VS Code task is configured in `.vscode/tasks.json` for:

- `build`
- `Generate coverage report`

You can run the coverage task from the VS Code Command Palette: `Tasks: Run Task` → `Generate coverage report`.

## Swagger

Swagger is enabled for the development environment.

To use Swagger UI:

1. Run the API locally
2. Open the URL shown in the console, typically `https://localhost:5001/swagger` or `https://localhost:5202/swagger`

Swagger lets you explore the `POST /api/transfer` endpoint and send requests directly from the browser.

## FundTransfer API Test Commands

### 1. SUCCESS CASE

Request JSON:

```json
{
  "fromAccount": "ACC1",
  "toAccount": "ACC2",
  "amount": 500,
  "requestId": "req-success",
  "otp": "123456"
}
```

### 2. INVALID OTP

Request JSON:

```json
{
  "fromAccount": "ACC1",
  "toAccount": "ACC2",
  "amount": 500,
  "requestId": "req-bad-otp",
  "otp": "000000"
}
```

### 3. INSUFFICIENT BALANCE

Request JSON:

```json
{
  "fromAccount": "ACC1",
  "toAccount": "ACC2",
  "amount": 20000,
  "requestId": "req-low-balance",
  "otp": "123456"
}
```

### 4. DUPLICATE REQUEST (run twice)

Request JSON:

```json
{
  "fromAccount": "ACC1",
  "toAccount": "ACC2",
  "amount": 100,
  "requestId": "req-duplicate",
  "otp": "123456"
}
```

### 5. FRAUD LIMIT EXCEEDED

Request JSON:

```json
{
  "fromAccount": "ACC1",
  "toAccount": "ACC2",
  "amount": 200000,
  "requestId": "req-fraud",
  "otp": "123456"
}
```

### 6. SAME ACCOUNT TRANSFER

Request JSON:

```json
{
  "fromAccount": "ACC1",
  "toAccount": "ACC1",
  "amount": 100,
  "requestId": "req-same-account",
  "otp": "123456"
}
```

### 7. INVALID AMOUNT

Request JSON:

```json
{
  "fromAccount": "ACC1",
  "toAccount": "ACC2",
  "amount": 0,
  "requestId": "req-invalid-amount",
  "otp": "123456"
}
```

### 8. MISSING ACCOUNT

Request JSON:

```json
{
  "fromAccount": "",
  "toAccount": "ACC2",
  "amount": 100,
  "requestId": "req-missing-account",
  "otp": "123456"
}
```
---

## Author

**Sauvik Roy** *(<https://www.zensar.com>)*

- GitHub: <https://github.com/sauvik7>  
- Email: <mailto:sauvik.roy@zensar.com>

---

## License

This project is licensed under the MIT License.

See the LICENSE file for full details.