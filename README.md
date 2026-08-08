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

### 2. Create your local settings file

`appsettings.Development.json` is **gitignored** (it contains secrets). Copy the checked-in template and fill in your values:

**Windows (PowerShell)**
```powershell
copy src\SmartEmployeePortal.API\appsettings.Development.example.json `
     src\SmartEmployeePortal.API\appsettings.Development.json
```

**macOS / Linux**
```bash
cp src/SmartEmployeePortal.API/appsettings.Development.example.json \
   src/SmartEmployeePortal.API/appsettings.Development.json
```

### 3. Fill in the placeholder values

Open `appsettings.Development.json` and replace the two placeholders:

| Key | Where to get it |
|-----|-----------------|
| `ConnectionStrings:DefaultConnection` | Azure Portal → **SQL databases** → your database → **Settings → Connection strings** → **ADO.NET** tab → copy the string → replace `{your_username}` and `{your_password}` |
| `BlobStorage:ConnectionString` | Azure Portal → **Storage accounts** → your storage account (`stsepdev`) → **Security + networking → Access keys** → copy **key1 Connection string** |

### 4. Run the API

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
