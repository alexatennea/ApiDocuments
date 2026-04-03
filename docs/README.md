# Documents API – Technical Documentation

## Overview

The **ApiDocuments** API is a .NET 10 RESTful service for uploading, retrieving, and deleting documents. Document binary content is stored in **Azure Blob Storage**. Document metadata and a full audit history are stored in **Azure SQL Database**.

---

## Table of Contents

1. [Architecture](#architecture)
2. [API Endpoints](#api-endpoints)
3. [Data Model](#data-model)
4. [Project Structure](#project-structure)
5. [Configuration](#configuration)
6. [Infrastructure (Terraform)](#infrastructure-terraform)
7. [CI/CD Pipelines](#cicd-pipelines)
8. [Local Development](#local-development)
9. [Running Tests](#running-tests)
10. [Required GitHub Secrets](#required-github-secrets)

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    ApiDocuments.Api                     │
│              (ASP.NET Core 10 Controller)               │
└──────────────────────┬──────────────────────────────────┘
                       │ IDocumentService
┌──────────────────────▼──────────────────────────────────┐
│               ApiDocuments.Infrastructure               │
│   DocumentService  │  BlobStorageService                │
│   DocumentRepository                                    │
└───────┬───────────────────────────┬─────────────────────┘
        │ IDocumentRepository       │ IBlobStorageService
        ▼                           ▼
  Azure SQL Database          Azure Blob Storage
  (Documents + Audits)        (document files)
```

### Design principles

| Principle | Implementation |
|-----------|---------------|
| Thin controllers | Controllers only validate input and delegate to `IDocumentService` |
| Service layer | `DocumentService` contains all business logic |
| Repository pattern | `DocumentRepository` wraps all EF Core data access |
| XML documentation | All public types and members carry `/// <summary>` comments |
| Soft deletes | Documents are marked `IsDeleted = true`; EF query filters exclude them |
| Audit trail | Every create, read (download) and delete action writes a `DocumentAudit` row |

---

## API Endpoints

Base path: `/api/documents`

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/documents` | List all documents (metadata only) |
| `GET` | `/api/documents/{id}` | Get metadata for a single document |
| `GET` | `/api/documents/{id}/download` | Download the document binary |
| `POST` | `/api/documents` | Upload a new document (`multipart/form-data`) |
| `DELETE` | `/api/documents/{id}` | Soft-delete a document |
| `GET` | `/api/documents/{id}/audit` | Retrieve the audit history for a document |

Swagger UI is available at `/swagger` when the application is running in the Development environment.

### Upload example

```bash
curl -X POST https://<host>/api/documents \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/document.pdf"
```

### Response format (DocumentDto)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "document.pdf",
  "contentType": "application/pdf",
  "fileSizeBytes": 204800,
  "createdAtUtc": "2024-01-15T10:30:00Z",
  "updatedAtUtc": "2024-01-15T10:30:00Z"
}
```

---

## Data Model

### Documents table

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `uniqueidentifier` | Primary key |
| `FileName` | `nvarchar(512)` | Original file name |
| `ContentType` | `nvarchar(256)` | MIME type |
| `BlobPath` | `nvarchar(1024)` | Path within the blob container |
| `FileSizeBytes` | `bigint` | File size in bytes |
| `CreatedAtUtc` | `datetime2` | Creation timestamp (UTC) |
| `UpdatedAtUtc` | `datetime2` | Last update timestamp (UTC) |
| `IsDeleted` | `bit` | Soft-delete flag |

### DocumentAudits table

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `uniqueidentifier` | Primary key |
| `DocumentId` | `uniqueidentifier` | Foreign key → Documents.Id |
| `Action` | `nvarchar(64)` | Action name: `Uploaded`, `Downloaded`, or `Deleted` |
| `Details` | `nvarchar(2048)` | Optional detail text |
| `PerformedAtUtc` | `datetime2` | Timestamp (UTC) |
| `PerformedBy` | `nvarchar(256)` | User or system identifier |

---

## Project Structure

```
ApiDocuments/
├── src/
│   ├── ApiDocuments.Api/               # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   └── DocumentsController.cs  # Thin controller
│   │   └── Program.cs                  # Dependency injection & middleware
│   │
│   ├── ApiDocuments.Core/              # Domain layer (no framework dependencies)
│   │   ├── DTOs/
│   │   │   ├── DocumentDto.cs
│   │   │   ├── DocumentAuditDto.cs
│   │   │   └── UploadDocumentRequest.cs
│   │   ├── Interfaces/
│   │   │   ├── IDocumentRepository.cs
│   │   │   ├── IDocumentService.cs
│   │   │   └── IBlobStorageService.cs
│   │   └── Models/
│   │       ├── Document.cs
│   │       └── DocumentAudit.cs
│   │
│   ├── ApiDocuments.Infrastructure/    # Data access & external services
│   │   ├── Data/
│   │   │   └── AppDbContext.cs
│   │   ├── Repositories/
│   │   │   └── DocumentRepository.cs
│   │   └── Services/
│   │       ├── BlobStorageService.cs
│   │       ├── DocumentService.cs
│   │       └── AuditActions.cs
│   │
│   └── ApiDocuments.Tests/             # xUnit unit tests
│       ├── Controllers/
│       │   └── DocumentsControllerTests.cs
│       ├── Services/
│       │   └── DocumentServiceTests.cs
│       └── Repositories/
│           └── DocumentRepositoryTests.cs
│
├── terraform/                          # Infrastructure as Code
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   └── environments/
│       ├── dev.tfvars
│       └── prod.tfvars
│
├── .github/
│   └── workflows/
│       ├── ci.yml                      # CI: build + test on every PR
│       ├── deploy-dev.yml              # Auto-deploy to dev on merge to main
│       └── deploy-prod.yml             # Manual production deployment
│
└── docs/
    └── README.md                       # This file
```

---

## Configuration

The application uses the standard ASP.NET Core configuration system. The following keys are required:

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | Azure SQL Database connection string |
| `ConnectionStrings:BlobStorage` | Azure Storage account connection string |
| `BlobStorage:ContainerName` | Name of the blob container (default: `documents`) |

In Azure App Service these are set by Terraform as **Connection Strings** and **App Settings**. Secrets should never be stored in source control.

---

## Infrastructure (Terraform)

All Azure resources are defined in the `terraform/` directory and are applied automatically by the deployment pipelines.

### Resources created per environment

| Resource | SKU / Tier | Notes |
|----------|-----------|-------|
| Resource Group | – | Groups all resources |
| Storage Account | Standard LRS | Cheapest blob storage option |
| Blob Container | Private | Stores document files |
| SQL Server | – | Azure SQL logical server |
| SQL Database | Basic (5 DTU) | Cheapest available SQL Database |
| App Service Plan | B1 (configurable) | Linux, .NET 10 |
| App Service | – | Hosts the API |
| App Configuration | Free | Environment settings |

### Terraform state

Terraform state is stored remotely in an Azure Storage Account. The backend is configured via `-backend-config` flags in the pipeline using the following secrets:

- `TF_BACKEND_RESOURCE_GROUP`
- `TF_BACKEND_STORAGE_ACCOUNT`

### Initialising the Terraform backend manually

Before the first pipeline run, you must create the backend storage account once:

```bash
az group create --name tfstate-rg --location uksouth
az storage account create --name <unique-name> --resource-group tfstate-rg --sku Standard_LRS
az storage container create --name tfstate --account-name <unique-name>
```

---

## CI/CD Pipelines

### `ci.yml` – Continuous Integration

Triggered on every push and pull request targeting `main`. Runs restore, build, and test. Uploads the test results and publish artifact.

### `deploy-dev.yml` – Development deployment

Triggered automatically on every push to `main`. Steps:

1. Build and test the solution
2. Run `terraform init`, `plan`, and `apply` against the `dev` environment
3. Deploy the published API artifact to the `apidocs-dev-api` App Service

### `deploy-prod.yml` – Production deployment

Triggered manually via `workflow_dispatch`. Requires typing `deploy` in the confirmation input to prevent accidental runs. Uses the `prod` GitHub Environment (which can require manual approval). Steps are identical to the dev pipeline but target the `prod` environment.

---

## Local Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Storage Emulator (Azurite)](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) or a real Azure Storage account
- SQL Server (LocalDB or full instance) or Azure SQL

### Setup

1. Clone the repository
2. Start Azurite: `azurite --silent --location /tmp/azurite`
3. Update `src/ApiDocuments.Api/appsettings.Development.json` with your SQL connection string if needed
4. Apply EF Core migrations:

```bash
cd src/ApiDocuments.Api
dotnet ef database update --project ../ApiDocuments.Infrastructure
```

5. Run the API:

```bash
dotnet run --project src/ApiDocuments.Api
```

6. Open Swagger UI at `https://localhost:<port>/swagger`

### Adding EF Core migrations

```bash
dotnet ef migrations add <MigrationName> \
  --project src/ApiDocuments.Infrastructure \
  --startup-project src/ApiDocuments.Api
```

---

## Running Tests

```bash
dotnet test
```

To run with code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

The test suite covers:

- **DocumentsControllerTests** – all controller actions (mock service)
- **DocumentServiceTests** – all service methods (mock repository + blob storage)
- **DocumentRepositoryTests** – all repository methods (EF Core in-memory database)

---

## Required GitHub Secrets

The following secrets must be configured in GitHub repository settings before the deployment pipelines will work:

| Secret | Description |
|--------|-------------|
| `AZURE_CREDENTIALS` | Service principal JSON for `azure/login` |
| `ARM_CLIENT_ID` | Azure service principal client ID (for Terraform) |
| `ARM_CLIENT_SECRET` | Azure service principal client secret (for Terraform) |
| `ARM_SUBSCRIPTION_ID` | Azure subscription ID |
| `ARM_TENANT_ID` | Azure tenant ID |
| `TF_BACKEND_RESOURCE_GROUP` | Resource group of the Terraform state storage account |
| `TF_BACKEND_STORAGE_ACCOUNT` | Name of the Terraform state storage account |
| `SQL_ADMIN_USERNAME` | SQL Server administrator login name |
| `SQL_ADMIN_PASSWORD` | SQL Server administrator password |

GitHub Environments `dev` and `prod` should be configured in repository Settings → Environments. The `prod` environment should have a required reviewer configured for additional safety.
