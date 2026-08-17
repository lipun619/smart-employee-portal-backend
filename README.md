# Smart Employee Portal — Backend

ASP.NET Core Web API and Azure Functions backend using Clean Architecture (Domain / Application / Infrastructure / API).

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Azure Functions Core Tools v4
- SQL Server **or** Azure SQL
- Azure Storage account for queue and blob usage
- Azure Communication Services Email connection (if testing email sending)

---

## Local Development Setup

### 1. Clone the repository

```bash
git clone <repo-url>
cd smart-employee-portal-backend
```

### 2. Set up the API local configuration

`appsettings.Development.json` is **gitignored** (it contains secrets). Copy the checked-in template and fill in your values:

**macOS / Linux**
```bash
cp src/SmartEmployeePortal.API/appsettings.Development.example.json \
   src/SmartEmployeePortal.API/appsettings.Development.json
```

**Windows (PowerShell)**
```powershell
copy src\SmartEmployeePortal.API\appsettings.Development.example.json `
     src\SmartEmployeePortal.API\appsettings.Development.json
```

### 3. Fill in the API placeholder values

Open `src/SmartEmployeePortal.API/appsettings.Development.json` and update:

| Key | Where to get it |
|-----|-----------------|
| `ConnectionStrings:DefaultConnection` | Azure SQL connection string |
| `BlobStorage:ConnectionString` | Azure Storage account access key connection string |
| `ACS:ConnectionString` | Azure Communication Services connection string |
| `ACS:SenderAddress` | Email sender address from ACS |

### 4. Set up the Azure Functions local configuration

Create or update `src/SmartEmployeePortal.Functions/local.settings.json` in the Functions project folder.

**Where to get the values from Azure:**

- `AzureWebJobsStorage`:
  - Azure Portal → your Storage Account → **Security + networking** → **Access keys**
  - Copy the **Connection string** for key1 or key2

- `ConnectionString--DefaultConnection`:
  - Azure Portal → SQL Server / Azure SQL Database
  - Open the database → **Connection strings**
  - Copy the SQL connection string and paste it here

- `ACS--ConnectionString`:
  - Azure Portal → your Communication Services resource
  - Go to **Keys** or **Connection strings**
  - Copy the primary connection string

- `ACS--SenderAddress`:
  - Azure Portal → Communication Services resource
  - Open **Email Communication Service** / **Domains**
  - Copy the sender address such as `donotreply@<resource>.azurecomm.net`

- `Queue-ConnectionString`:
  - Same value as `AzureWebJobsStorage` if you are using the same Azure Storage account for queue operations

Example:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=yourstorage;AccountKey=...;BlobEndpoint=https://yourstorage.blob.core.windows.net/;QueueEndpoint=https://yourstorage.queue.core.windows.net/;TableEndpoint=https://yourstorage.table.core.windows.net/;",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DOTNET_ENVIRONMENT": "Development",
    "ConnectionString--DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=yourdb;Persist Security Info=False;User ID=youruser;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "ACS--ConnectionString": "endpoint=https://youracsresource.communication.azure.com/;accesskey=your-key",
    "ACS--SenderAddress": "donotreply@yourdomain.azurecomm.net",
    "Queue-ConnectionString": "DefaultEndpointsProtocol=https;AccountName=yourstorage;AccountKey=...;BlobEndpoint=https://yourstorage.blob.core.windows.net/;QueueEndpoint=https://yourstorage.queue.core.windows.net/;TableEndpoint=https://yourstorage.table.core.windows.net/;"
  }
}
```
---

## Run the API locally

```bash
cd src/SmartEmployeePortal.API
dotnet run
```

The API will start with the ASP.NET Core development environment. Swagger is typically available at:

```text
https://localhost:7084/swagger
```

or:

```text
http://localhost:5048/swagger
```

Depending on the configured launch profile.

---

## Run the Azure Functions app locally

From the project folder:

```bash
cd src/SmartEmployeePortal.Functions
func start --verbose
```

This starts the Azure Functions host and loads the function triggers.

### Local settings needed for Functions

Ensure `src/SmartEmployeePortal.Functions/local.settings.json` exists and contains at least:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "<your-storage-connection-string>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DOTNET_ENVIRONMENT": "Development",
    "ConnectionString--DefaultConnection": "<your-sql-connection-string>",
    "ACS--ConnectionString": "<your-acs-connection-string>",
    "ACS--SenderAddress": "<your-acs-sender-address>",
    "Queue-ConnectionString": "<your-storage-connection-string>"
  }
}
```

### Verify the Functions app is running

Look for output similar to:

```text
Functions:
  EmployeeOnboardingQueueFunction: queueTrigger
  BirthdayAnniversaryTimerFunction: timerTrigger
```

If the timer is configured as a 2-minute schedule, it will fire every 2 minutes while the host is running.

---

## Local testing flow

### API testing

- Start the API with:

```bash
dotnet run --project src/SmartEmployeePortal.API/SmartEmployeePortal.API.csproj
```

- Open Swagger in the browser
- Test the controller endpoints such as employee or department APIs
- Confirm data is returned from SQL and the API responds successfully

### Function testing

For the Azure Functions app:

```bash
cd src/SmartEmployeePortal.Functions
func start --verbose
```

Then validate one of the following:

1. Timer trigger:
   - Wait for the configured cron/time to fire
   - Check the function logs for execution
   - Set breakpoints in the trigger and service methods when debugging locally

2. Queue trigger:
   - Add a message to the `employee-tasks` queue in Azure Storage or a local emulator
   - Confirm the queue trigger executes and the email service is invoked

---

## Project Structure

```
src/
  SmartEmployeePortal.API/           → Controllers, Middleware, Program.cs
  SmartEmployeePortal.Application/   → CQRS handlers, DTOs, Validation
  SmartEmployeePortal.Domain/        → Entities, Interfaces, Enums
  SmartEmployeePortal.Infrastructure/→ EF Core, Repositories, DB Context
  SmartEmployeePortal.Functions/      → Azure Function triggers and local runtime entry

tests/
  SmartEmployeePortal.Application.Tests/
```

---

## Running Tests

```bash
dotnet test
```

---

## Useful notes

- API and Functions are separate runtime apps and are not deployed to the same Azure resource type.
- For local debugging, start the API directly and start the Functions host separately.
- If using email sending locally, ACS may throttle requests with `429 TooManyRequests`; handle this gracefully in the service layer.

