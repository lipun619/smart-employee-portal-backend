# Smart Employee Portal — Backend

ASP.NET Core Web API using Clean Architecture (Domain / Application / Infrastructure / API).

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server **or** Azure SQL (see connection string options below)

---

## Local Development Setup

### 1. Clone the repository

```bash
git clone <repo-url>
cd smart-employee-portal-backend
```

### 2. Configure the connection string

The file `appsettings.Development.json` ships with a **LocalDB placeholder** connection string.  
This works out of the box if you have Visual Studio / SQL Server LocalDB installed.

**Option A — Use LocalDB (no Azure access needed)**

No action required. The default in `appsettings.Development.json` will be used:
```
Server=(localdb)\mssqllocaldb;Database=SmartEmployeePortalDb;Trusted_Connection=True;TrustServerCertificate=True;
```

**Option B — Use the shared Azure SQL Dev database**

Contact a team member for the Azure SQL connection string, then store it in User Secrets (never commit passwords to Git):

```bash
cd src/SmartEmployeePortal.API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string-from-team-member>"
```

User Secrets are stored only on your local machine and override `appsettings.Development.json` automatically.

### 3. Run the API

```bash
cd src/SmartEmployeePortal.API
dotnet run
```

Swagger UI will be available at `https://localhost:7084/swagger`.

---

## Project Structure

```
src/
  SmartEmployeePortal.API/           → Controllers, Middleware, Program.cs
  SmartEmployeePortal.Application/   → CQRS handlers, DTOs, Validation
  SmartEmployeePortal.Domain/        → Entities, Interfaces, Enums
  SmartEmployeePortal.Infrastructure/→ EF Core, Repositories, DB Context
tests/
  SmartEmployeePortal.Application.Tests/
```

---

## Running Tests

```bash
dotnet test
```
