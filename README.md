# Lending API

A loan-application backend built on .NET 9, modelling the lifecycle of a
lending product: applications are created, submitted, and then approved or
rejected through a state machine, with repayment estimates for approved loans.

This is a **portfolio project**, built deliberately to deepen modern .NET
skills coming from a .NET Framework 4.8 background. The emphasis is on *how*
it's built — architecture, patterns, and design decisions — rather than on
breadth of features. Every decision below is one I can explain and defend.

---

## What it does

Loan applications move through a state machine:

```
Draft ──submit──▶ Submitted ──approve──▶ Approved
                      │
                      └────reject────▶ Rejected
```

- Create a loan application (applicant, amount, term)
- List applications with paging
- Retrieve a single application
- Drive the state machine (submit / approve / reject) with rule enforcement
- Calculate a monthly repayment estimate for approved loans

---

## Tech stack

- **.NET 9 / ASP.NET Core** — controller-based Web API
- **C# 13** — records, nullable reference types, switch expressions, file-scoped namespaces
- **Entity Framework Core 9** — SQL Server provider, code-first migrations
- **SQL Server** — LocalDB for local dev, containerized SQL Server for Docker
- **xUnit + FluentAssertions** — unit tests with SQLite in-memory
- **Docker + Docker Compose** — multi-stage build, fully self-contained stack
- **Serilog-style structured logging** via `ILogger<T>`

---

## Quick start

### Option 1 — Docker (fully self-contained, recommended)

Requires Docker Desktop. No .NET SDK or SQL Server installation needed.

```bash
git clone https://github.com/avishekhbharati/lending-api.git
cd lending-api
docker compose up --build
```

This builds the API, starts a SQL Server container, waits for it to be
healthy, applies migrations (with a retry loop for container startup
timing), seeds sample data, and serves the API on port 8080.

```bash
curl http://localhost:8080/loans
```

### Option 2 — Local (.NET SDK + LocalDB)

Requires .NET 9 SDK and SQL Server LocalDB (ships with Visual Studio).

```bash
git clone https://github.com/avishekhbharati/lending-api.git
cd lending-api
dotnet ef database update --project src/LendingApi
dotnet run --project src/LendingApi
```

API serves on `http://localhost:5141`.

---

## API endpoints

| Method | Path                       | Description                          | Status codes        |
|--------|----------------------------|--------------------------------------|---------------------|
| GET    | `/loans`                   | List applications (paged)            | 200                 |
| GET    | `/loans/{id}`              | Retrieve one application             | 200, 404            |
| POST   | `/loans`                   | Create a Draft application           | 201, 400            |
| POST   | `/loans/{id}/submit`       | Draft → Submitted (rule-checked)     | 200, 404, 409, 422  |
| POST   | `/loans/{id}/approve`      | Submitted → Approved                 | 200, 404, 409       |
| POST   | `/loans/{id}/reject`       | Submitted → Rejected                 | 200, 404, 409       |
| GET    | `/loans/{id}/repayment`    | Monthly repayment (Approved only)    | 200, 404, 409       |

Paging: `GET /loans?page=1&pageSize=20` (pageSize capped server-side at 100).

---

## Architecture & design decisions

### Layered architecture

```
HTTP request
   │
   ▼
LoansController      — HTTP concerns only (routing, status codes, binding)
   │
   ▼
ILoanService         — business logic, state machine, rule enforcement
   │
   ▼
LendingDbContext     — persistence (EF Core change tracking, unit of work)
   │
   ▼
SQL Server
```

Each layer depends on an abstraction of the layer below. Controllers know
nothing about EF Core; the service knows nothing about HTTP. This keeps the
business logic independently testable and makes the persistence layer
swappable.

### Errors as values, not exceptions

The state-machine operations return a `Result<T>` rather than throwing.
A loan being in the wrong state, or not existing, are *expected* business
outcomes — not exceptional conditions. Exceptions are reserved for genuinely
unexpected failures. The controller pattern-matches on the result and maps
each outcome to the correct HTTP status (404 / 409 / 422). This avoids
exceptions-as-control-flow and keeps failure modes visible in the type
signature.

### Money and time handled correctly

- **`decimal` for all monetary values**, never `double` — `decimal` is exact
  base-10; `double` cannot represent `0.10` exactly, which is unacceptable
  for money.
- **`DateTimeOffset` for all timestamps**, never `DateTime` — the timezone
  offset is stored in the data, making correctness structural rather than
  conventional. Maps to SQL `datetimeoffset`.

### Enum storage and serialization

`LoanStatus` is stored as a **string** in the database (via
`HasConversion<string>()`) and serialized as a **string** in JSON (via
`JsonStringEnumConverter`) — not as integers. A self-documenting database
and human-readable API, with no risk of an enum reorder silently corrupting
stored data.

### Configuration via the options pattern

Loan rules (min/max amount, term bounds, interest rate) live in
`appsettings.json`, bound to a strongly-typed `LoanRulesOptions` class and
injected as `IOptions<LoanRulesOptions>`. No magic strings, no manual
parsing — a clean contrast with `ConfigurationManager.AppSettings` from the
.NET Framework world.

### DTO boundary

Incoming requests use a `CreateLoanRequest` DTO that exposes only the fields
a client is allowed to set. Server-controlled fields (`Id`, `Status`,
`CreatedAt`) cannot be supplied by the caller — the server owns them. This
is enforced structurally, not by validation.

### Identifiers

Loans use `Guid` primary keys rather than sequential integers — unguessable
in URLs, and they don't leak business volume.

### Migrations as the source of truth

Schema is defined by EF Core migrations. The one non-trivial data migration
(converting `LoanStatus` from int to string storage) was hand-edited rather
than auto-generated, because the automatic conversion would have corrupted
existing rows. The migration uses an explicit `CASE` mapping to convert int
values to their string equivalents.

---

## Testing

Unit tests cover the service layer — primarily the state machine, which is
the core domain logic.

- **xUnit** as the test framework
- **SQLite in-memory** as the test database — a real relational database
  (real SQL, real constraints, real transactions), fast and disposable, with
  one fresh instance per test for isolation
- **FluentAssertions** for readable assertions

The state-machine coverage is exhaustive — every valid transition is tested,
and every *invalid* transition (from every wrong starting state) is verified
to return the correct error. Data-driven `[Theory]` tests cover the
invalid-transition matrix compactly.

```bash
dotnet test
```

---

## Notable challenges (and what they taught me)

- **EF Core major version must match the .NET major version.** A `dotnet add
  package` pulled EF Core 10 into a .NET 9 project; pinning to 9.x fixed it.
- **A column-type change is a schema migration AND a data migration.** EF Core
  generates the schema change automatically but converts existing data
  naively — the int→string enum conversion needed a hand-written `CASE`.
- **Three independent layers represent an enum** — database column type,
  in-memory C# type, and JSON wire format — each configured separately.
- **Containerized SQL Server reports "healthy" before it can accept DDL.**
  The startup migration needed a retry loop with an exception filter
  (retry transient failures, let the final attempt crash loudly).
- **The IQueryable → IEnumerable boundary determines where work happens.**
  `Skip`/`Take` before `ToListAsync` pages in the database; after it pages
  in memory. The difference is invisible at three rows and catastrophic at
  three million.

---

## Roadmap

This project is the foundation for a longer skills-upgrade plan:

- **Done** — modern .NET, EF Core, DI, state machine, validation, structured
  logging, custom middleware, testing, containerization
- **Next** — deploy to Azure (App Service + Azure SQL + Application Insights);
  refactor toward DDD and a CQRS read/write split
- **Later** — a React + TypeScript frontend consuming this API; one AI-backed
  feature (loan summarization / decision explanation)

---

## Project structure

```
lending-api/
├── Dockerfile                  multi-stage build
├── docker-compose.yml          API + SQL Server
├── global.json                 pins the .NET SDK version
├── src/LendingApi/
│   ├── Common/                 Result<T>
│   ├── Controllers/            thin HTTP adapters
│   ├── Data/                   DbContext + seed data
│   ├── Middleware/             request-timing middleware
│   ├── Migrations/             EF Core migrations
│   ├── Models/                 entities + DTOs
│   ├── Options/                strongly-typed configuration
│   └── Services/               business logic + state machine
└── tests/LendingApi.Tests/     xUnit + SQLite in-memory
```