# Lending API — Learning Todo List

Last updated: 24 May 2026
Current position: Month 1, Week 2, Day 1 (EF Core)

Legend: [x] done · [ ] todo · [~] in progress · [?] decide later · [-] skipped (with reason)

---

## Month 1 — Modern .NET

### Week 1 — Environment + first running service ✅ COMPLETE

- [x] .NET SDK confirmed (9.0.305)
- [x] GitHub repo `lending-api` created and cloned
- [x] API scaffolded with subfolder layout (src/LendingApi)
- [x] .gitignore added
- [x] API runs, /weatherforecast returns JSON
- [x] Program.cs walked through line by line
- [ ] .csproj walked through line by line (carried over to Week 2)
- [x] Sample WeatherForecast code deleted
- [x] LoanApplication model created
- [x] LoanStatus enum created
- [x] GET /loans endpoint with hard-coded data
- [x] README written with design decisions section
- [x] Committed and pushed

Learning checkpoints:
- [x] Program.cs vs Global.asax/Startup.cs — answered
- [x] What .NET CLI gives me vs MSBuild — answered

---

### Week 2 — DI + EF Core + persistence

Carried over from Week 1:
- [ ] Walk through .csproj line by line (5 min)

EF Core setup:
- [x] Install dotnet-ef tool globally (`dotnet tool install --global dotnet-ef`)
- [x] Add NuGet packages:
  - [x] Microsoft.EntityFrameworkCore.SqlServer
  - [x] Microsoft.EntityFrameworkCore.Tools
  - [x] Microsoft.EntityFrameworkCore.Design
- [x] Create Data/LendingDbContext.cs with DbSet<LoanApplication>
- [x] Add connection string to appsettings.json (LocalDB)
- [x] Register DbContext in Program.cs via AddDbContext
- [x] Run first migration: `dotnet ef migrations add InitialCreate`
- [x] Open generated migration file and READ what EF Core produced
- [x] Apply migration: `dotnet ef database update`
- [x] Verify DB exists via SSMS or VS SQL Server Object Explorer

Service layer + real endpoints:
- [ ] Create LoanService class for business logic
- [ ] Register LoanService in DI (decide: Scoped or Transient — discuss)
- [ ] Decide: convert to controllers now, or stay on minimal API for Week 2?
- [ ] Implement POST /loans (create Draft application)
- [ ] Implement GET /loans/{id}
- [ ] Implement GET /loans with paging (page + pageSize query params)
- [ ] Use async EF Core methods throughout (ToListAsync, FindAsync, etc.)
- [ ] Replace hard-coded list in Program.cs with DB queries
- [ ] Commit and push after each working endpoint

Learning checkpoints:
- [ ] How does constructor injection work? Where is service lifetime set?
- [ ] What did the EF Core migration actually generate?
- [ ] Difference between IQueryable (in DB) and in-memory work (after ToList)?

Open questions for this week:
- [?] Service lifetime for LoanService: Scoped (matches DbContext) or Transient?
- [?] Switch to controllers now, or after the state machine in Week 3?
- [?] Add a database seeding step for dev data?

---

### Week 3 — Business logic, validation, state machine, logging

- [ ] Add validation attributes to LoanApplication
- [ ] Return clean 400 responses for invalid input
- [ ] Implement state machine in LoanService:
  - [ ] POST /loans/{id}/submit (Draft → Submitted, rules pass)
  - [ ] POST /loans/{id}/approve (Submitted → Approved)
  - [ ] POST /loans/{id}/reject (Submitted → Rejected)
  - [ ] Invalid transitions return 409 Conflict
- [ ] Move rule constants to appsettings.json
- [ ] Read config with options pattern (IOptions<T>)
- [ ] Implement GET /loans/{id}/repayment (monthly estimate for Approved)
- [ ] Add structured logging with ILogger<T> for key events
- [ ] Add custom request-timing middleware
- [ ] Commit and push

Learning checkpoints:
- [ ] What is the middleware pipeline and how does ordering affect behaviour?
- [ ] Options pattern vs ConfigurationManager (4.8)?
- [ ] Where should business logic live and why not in the controller?

---

### Week 4 — Testing, containerisation, polish

- [ ] Add xUnit test project: `dotnet new xunit -n LendingApi.Tests`
- [ ] Place in tests/ folder, add to solution
- [ ] Write unit tests for LoanService (state machine + validation)
- [ ] All tests pass with `dotnet test`
- [ ] Write multi-stage Dockerfile (SDK build + runtime)
- [ ] Build and run API in container
- [ ] (Optional) docker-compose.yml with API + SQL Server
- [ ] Polish README: tech stack, how-to-run, endpoints, lessons learned
- [ ] Final Month 1 commit; pin repo on GitHub profile

Learning checkpoints:
- [ ] Why a multi-stage Dockerfile? What does each stage do?
- [ ] Can I walk through the project in 2 minutes (what, stack, decision, regret)?

Month 1 Definition of Done:
- [ ] .NET 9 Web API builds and runs, backed by real DB
- [ ] DI, EF Core, middleware, validation, logging all used
- [ ] Meaningful xUnit tests, passing
- [ ] Working Dockerfile, API runs in container
- [ ] Clear README with design-decisions section
- [ ] Repo public and pinned on GitHub
- [ ] Can talk through project + modern .NET vs 4.8 without notes

---

## Month 2 — Azure + Architecture Patterns

### Week 1 — Real Azure mapping + deploy
- [ ] Audit Azure services actually used at Dayforce, write down
- [ ] Update CV: replace generic "Azure" with specific service names
- [ ] Deploy Lending API to Azure App Service
- [ ] Provision Azure SQL, update connection string
- [ ] Add Application Insights

### Week 2 — DDD basics
- [ ] Learn: entities, value objects, aggregates, bounded contexts
- [ ] Refactor: LoanApplication becomes proper aggregate
- [ ] Add Money value object
- [ ] Enforce invariants in domain, not service

### Week 3 — CQRS + message-based thinking
- [ ] Learn CQRS (command/query separation)
- [ ] Refactor to in-process mediator (no heavy framework)
- [ ] Learn async message-based systems and Service Bus
- [ ] (Optional) Wire one event (loan approved → message published)

### Week 4 — AZ-204 + consolidate
- [ ] Work through AZ-204 study material
- [ ] Decide on exam booking
- [ ] Update README + CV for architecture refactor

Month 2 Definition of Done:
- [ ] API runs on real Azure with observability
- [ ] DDD + CQRS reflected in code
- [ ] Can explain all 3 patterns + tradeoffs in interview
- [ ] Concrete AZ-204 plan

---

## Month 3 — Full-stack + AI

### Weeks 1-2 — React + TypeScript frontend
- [ ] Set up Vite + React + TS project
- [ ] List view of loan applications
- [ ] Create-application form
- [ ] Detail view with status
- [ ] State machine buttons (submit/approve/reject)
- [ ] Repayment estimate display
- [ ] Typed API client (interfaces matching API contracts)
- [ ] Routing, loading/error states

### Weeks 3-4 — AI feature
- [ ] Pick one: summarise loan / explain decision / build MCP server
- [ ] Implement using agentic patterns (tool use, reflection, structured output)
- [ ] Handle model failure gracefully
- [ ] Document in README
- [ ] Final demo-ready deploy

Month 3 Definition of Done:
- [ ] Full-stack: React/TS frontend + .NET API
- [ ] Deployed, demoable
- [ ] At least one working AI feature
- [ ] Can demo the whole thing and discuss any layer

---

## Cross-cutting / Anytime

CV + job search:
- [ ] Update CV with .NET 9 / EF Core after Week 2
- [ ] Start light job-market scanning after Month 1
- [ ] First interviews around Month 2 (don't wait until Month 3)
- [ ] Update CV with DDD/CQRS/Azure specifics after Month 2
- [ ] LinkedIn: pin Lending API project after Month 1
- [ ] LinkedIn: post about the build (one update per month is plenty)

Interview prep:
- [ ] Practice 2-min project walkthrough after Month 1
- [ ] Practice "modern .NET vs 4.8" answer after Month 1
- [ ] Practice DDD/CQRS explanations after Month 2
- [ ] Have repayment calculation explanation ready (financial domain credibility)

Stretch / nice-to-haves (only if ahead of schedule):
- [ ] OpenAPI/Swagger UI polish (good descriptions on endpoints)
- [ ] GitHub Actions: build + test on push
- [ ] Database seeding for dev environment
- [ ] Health check endpoint
- [ ] Rate limiting middleware demo

---

## Decisions log (so I remember WHY)

- **Subfolder layout (src/ + tests/ later)** — leaves room for tests project
  and frontend without restructuring.
- **SQL Server LocalDB for dev** — matches Azure SQL deploy in Month 2 and
  matches target employers (lender shops). Avoids provider-specific
  surprises later.
- **Guid for Id, not int** — unguessable, doesn't leak business volume.
- **decimal for Amount, never double** — exact base-10, mandatory for money.
- **DateTime.UtcNow everywhere** — never local time, avoids dev/prod
  timezone bugs.
- **Minimal API for now, controllers later** — fewer moving parts during
  EF Core learning; convert deliberately in Week 2 or Week 3.

---

## Things to research / read about (parking lot)

- [ ] EF Core change tracking — how it works under the hood
- [ ] Difference between AsNoTracking() and tracked queries
- [ ] ConfigureAwait(false) — does it still matter in ASP.NET Core?
- [ ] FluentValidation vs DataAnnotations — when to use which
- [ ] When to use Result<T> pattern vs exceptions
- [ ] switch LoanStatus to string storage
- [ ] constrain string column lengths
- [ ] evaluate DateTimeOffset vs DateTime