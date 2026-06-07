# Lending API — Learning Todo List

Last updated: 31 May 2026
Current position: Month 1, Week 2 — COMPLETE. Ready for Week 3.

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
- [x] .csproj walked through line by line
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

### Week 2 — DI + EF Core + persistence ✅ COMPLETE

EF Core setup:
- [x] Install dotnet-ef tool globally
- [x] Add NuGet packages (SqlServer, Tools, Design) — all pinned to 9.0.0
- [x] Create Data/LendingDbContext.cs with DbSet<LoanApplication>
- [x] Add connection string to appsettings.json (LocalDB)
- [x] Register DbContext in Program.cs via AddDbContext
- [x] Run first migration: `dotnet ef migrations add InitialCreate`
- [x] Open generated migration file and READ what EF Core produced
- [x] Apply migration: `dotnet ef database update`
- [x] Verify DB exists via VS SQL Server Object Explorer and SSMS

Controllers + endpoints:
- [x] Convert from minimal API to controller-based API
- [x] AddControllers() + MapControllers() in Program.cs
- [x] Create LoansController with constructor injection of DbContext
- [x] Implement GET /loans (returns all)
- [x] Implement GET /loans/{id} (with 404 on not found)
- [x] Implement POST /loans (returns 201 + Location header via CreatedAtAction)
- [x] CreateLoanRequest DTO (separate from entity, protects server-controlled fields)
- [x] Use async EF Core methods (ToListAsync, FindAsync, SaveChangesAsync)
- [x] Replace hard-coded list with DB queries

Service layer:
- [x] Create ILoanService interface
- [x] Create LoanService implementation
- [x] Move CRUD logic from controller into service
- [x] Register LoanService as Scoped in DI
- [x] Controller now depends on ILoanService, not DbContext directly
- [x] Verified "thin controllers, fat services" pattern works end-to-end

Seed data:
- [x] OnModelCreating override in DbContext
- [x] HasData with three deterministic seed loans (fixed Guids, fixed dates)
- [x] One loan in each state: Draft (Avi), Submitted (Jane), Approved (Sandip)
- [x] Generated SeedLoanData migration with InsertData calls
- [x] Recovered from migration desync (drop + remove + regenerate sequence)

Paging:
- [x] Add PagedResult<T> wrapper (Items, Page, PageSize, TotalCount, TotalPages)
- [x] Update ILoanService.GetAllAsync to take page + pageSize
- [x] Service uses Skip/Take BEFORE ToListAsync (slicing happens in SQL)
- [x] OrderBy(CreatedAt) before Skip — required for deterministic paging
- [x] Two queries: one CountAsync (total), one paged ToListAsync (items)
- [x] Controller accepts [FromQuery] page=1, pageSize=20 defaults
- [x] Defensive clamping: page >= 1, pageSize between 1 and 100

Learning checkpoints (all answered, ready for README):
- [x] How does constructor injection work? Where is service lifetime set?
- [x] What did the EF Core migration actually generate?
- [x] Difference between IQueryable (in DB) and in-memory work (after ToList)?

---

### Week 3 — Business logic, validation, state machine, logging

Schema cleanup (deferred from Week 2 — knock out first):
- [ ] Add [MaxLength(200)] to ApplicantName (no more nvarchar(max))
- [ ] Configure decimal precision explicitly with HasPrecision(18, 2) in OnModelCreating
  (silences the EF Core startup warning)
- [ ] Convert LoanStatus storage from int to string via HasConversion<string>()
  → write the data migration BY HAND (don't auto-generate the int→string mapping)
- [ ] Evaluate DateTime → DateTimeOffset for CreatedAt
- [ ] Generate cleanup migration, READ it before applying, apply

Validation:
- [ ] Add validation attributes to CreateLoanRequest
  (Required on ApplicantName, Range on Amount, Range on TermMonths)
- [ ] Verify [ApiController] returns clean 400 responses for invalid input
- [ ] Test with invalid POSTs (missing fields, zero amount, negative term)

State machine:
- [ ] Implement in LoanService:
  - [ ] SubmitAsync(id): Draft → Submitted, only if rules pass
  - [ ] ApproveAsync(id): Submitted → Approved
  - [ ] RejectAsync(id): Submitted → Rejected
  - [ ] Invalid transitions return Conflict (controller maps to 409)
- [ ] POST /loans/{id}/submit endpoint
- [ ] POST /loans/{id}/approve endpoint
- [ ] POST /loans/{id}/reject endpoint
- [ ] Decide pattern for "service rejects operation" — exceptions vs Result<T>

Configuration:
- [ ] Create LoanRulesOptions class (MinAmount, MaxAmount, MinTermMonths, MaxTermMonths)
- [ ] Move rule constants to appsettings.json under "LoanRules"
- [ ] Bind via builder.Services.Configure<LoanRulesOptions>(...)
- [ ] Inject IOptions<LoanRulesOptions> into LoanService

Repayment:
- [ ] Implement GET /loans/{id}/repayment — monthly estimate for Approved loans only
- [ ] Logic in service, not controller
- [ ] Decide: hardcoded interest rate (config), or include in loan?

Logging:
- [ ] Add ILogger<LoanService> via DI
- [ ] Log structured events at key transitions (loan submitted, approved, rejected)
- [ ] Use scopes/structured params (don't string-concatenate into messages)

Custom middleware:
- [ ] Write a request-timing middleware (logs how long each request took)
- [ ] Register it in the middleware pipeline
- [ ] Understand ordering: where in the pipeline does it go and why?

Learning checkpoints:
- [ ] What is the middleware pipeline, and how does ordering affect behaviour?
- [ ] Options pattern vs ConfigurationManager (4.8)?
- [ ] Where should business logic live, and why not in the controller?

---

### Week 4 — Testing, containerisation, polish

- [ ] Add xUnit test project: `dotnet new xunit -n LendingApi.Tests`
- [ ] Place under tests/, add to solution
- [ ] Unit tests for LoanService (state machine + validation rules)
- [ ] Use in-memory DbContext provider OR mock ILoanService at controller layer
- [ ] All tests pass: `dotnet test`
- [ ] Write multi-stage Dockerfile (SDK build + runtime image)
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
- [ ] List view of loan applications (with paging — already supported by API!)
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
- [ ] Update CV with .NET 9 / EF Core after Week 2 ← DO THIS NOW, you have real work to point at
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
- [ ] Health check endpoint
- [ ] Rate limiting middleware demo
- [ ] Return 404 vs empty page for out-of-range paged queries (decide team convention)

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
- **Minimal API → controllers** — switched in Week 2 alongside EF Core, so
  the restructure happened once not twice.
- **Service layer with ILoanService interface** — testability, swap-ability,
  clean dependency direction (controller → service → DbContext).
- **Scoped lifetime for LoanService and DbContext** — matches HTTP request
  lifetime; avoids captive-dependency bugs.
- **DTO pattern (CreateLoanRequest)** — clients can only set fields the server
  allows them to set; server controls Id, Status, CreatedAt.
- **PagedResult<T> wrapper** — clients need items + total count; generic
  so it reuses anywhere paging is needed.
- **OrderBy before Skip/Take always** — non-deterministic paging is a bug.

---

## Things to research / read about (parking lot)

- [ ] EF Core change tracking — how it works under the hood
- [ ] Difference between AsNoTracking() and tracked queries (relevant for read-heavy GETs)
- [ ] ConfigureAwait(false) — does it still matter in ASP.NET Core?
- [ ] FluentValidation vs DataAnnotations — when to use which
- [ ] When to use Result<T> pattern vs exceptions (will hit this in Week 3 state machine)
- [ ] Repository pattern vs DbContext-as-abstraction — modern guidance says skip the repo
- [ ] Window function paging (single query with COUNT() OVER) — perf optimization for later

---

## Lessons learned (the hard way)

- **EF tooling reads the SAVED + BUILT model, not the editor buffer.**
  Empty migration → first check save, then build. (Lost 15 min to this.)
- **EF Core major version must match .NET major version.**
  net9.0 needs EF Core 9.x, not 10.x. Pin with `--version` if `dotnet add package`
  picks something newer than your runtime.
- **HTTPS redirect middleware returns 307 on HTTP requests.**
  When testing locally with curl, either hit the https URL with `-k`, or
  use `-L` to follow redirects. The 307 isn't a bug.
- **`CreatedAtAction(nameof(GetXxx), ...)` does NOT call GetXxx.**
  It uses the action's route metadata to build a Location URL. nameof is
  refactor-safe stringification, not a method reference.
- **Migration recovery: drop → remove → rebuild is the dev workflow.**
  In dev the database is disposable. Don't fight migration history; reset it.
  Never use `database drop` in production.
- **PendingModelChangesWarning means EF sees model changes not in any migration.**
  Fix: generate a new migration. Don't try to update around it.