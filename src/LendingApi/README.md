# Lending API

A mini lending application API built on .NET 9. Personal project to
deepen modern .NET skills (coming from a .NET Framework 4.8 background).

## What it does

Manages loan applications through a state machine:
`Draft → Submitted → Approved | Rejected`. Calculates monthly repayment
estimates for approved loans.

## Tech stack

- .NET 9 / ASP.NET Core
- C# 13
- EF Core (Week 2)
- SQL Server (Week 2)
- xUnit (Week 4)
- Docker (Week 4)

## Running locally

Prerequisites: .NET 9 SDK.

```bash
git clone https://github.com/avishekhbharati/lending-api.git
cd lending-api
dotnet run --project src/LendingApi
```

The API listens on `http://localhost:5141` by default.

## Endpoints

| Method | Path     | Description                  |
|--------|----------|------------------------------|
| GET    | `/loans` | List all loan applications   |

(More endpoints coming as the project develops.)

## Design decisions

### `Guid` for IDs instead of `int`
Sequential integer IDs in URLs leak business volume and let callers
guess valid IDs. Guids are unguessable and globally unique. The
storage cost (16 bytes vs 4) is irrelevant at this scale.

### `decimal` for money, never `double`
`double` is binary floating-point and can't exactly represent decimal
values — `0.1 + 0.2 != 0.3`. `decimal` is base-10 and exact. Using
`double` for money is a bug, not a style choice.

### UTC for all timestamps
`DateTime.UtcNow`, never `DateTime.Now`. Local time depends on the
server's timezone, which causes silent bugs when the dev machine
(AEST) and the deployed environment (UTC) disagree.

### Subfolder repo layout (`src/` + future `tests/`)
Conventional .NET layout. Leaves room for the test project (Week 4)
and a future React frontend (Month 3) as siblings without restructuring.

## What I'm learning

This project deliberately surfaces what's different in modern .NET vs
.NET Framework 4.8:

- Minimal hosting in `Program.cs` replaces `Global.asax` + `Startup.cs`
- Built-in DI replaces Autofac/Unity bootstrapping
- `appsettings.json` + options pattern replaces `web.config`
- Middleware pipeline replaces HTTP modules and handlers
- SDK-style `.csproj` replaces the verbose 4.8 project file
- EF Core async patterns replace EF6 synchronous calls

## Roadmap

- **Month 1** — modern .NET: EF Core, DI, validation, state machine,
  Docker, xUnit
- **Month 2** — Azure deployment, DDD, CQRS, message-based patterns
- **Month 3** — React + TypeScript frontend, AI integration