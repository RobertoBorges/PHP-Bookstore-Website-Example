```prompt
---
agent: agent
model: Claude Sonnet 4.5 (copilot)
tools: ['search/codebase', 'usages', 'vscodeAPI', 'problems', 'changes', 'testFailure', 'runCommands/terminalSelection', 'runCommands/terminalLastCommand', 'openSimpleBrowser', 'fetch', 'search/searchResults', 'githubRepo', 'extensions', 'runTests', 'edit/editFiles', 'runNotebooks', 'search', 'new', 'runCommands', 'runTasks', 'Azure MCP/*', 'Microsoft Docs/*']
---

# Phase 2: Detailed File-by-File Migration Plan

## Objective

Create a comprehensive, file-by-file migration plan that documents exactly how each PHP file with business logic will be migrated to .NET 10. This plan ensures the model has complete context when executing migrations.

**Prerequisites**: 
- Phase 0: `Application-Discovery-Report.md` completed
- Phase 1: `Technical-Assessment-Report.md` completed with user preferences

## Why This Phase is Critical

When migrating code, the model needs to understand:
1. **What** each file does (purpose, responsibilities)
2. **How** it interacts with other components (dependencies)
3. **Where** it should go in .NET (target structure)
4. **What patterns** to use (PHP → .NET mapping)
5. **What order** to migrate (dependency chain)

This phase creates that complete context.

---

## Step 1: Review Previous Phases

Read the reports from previous phases:

```
read_file: reports/Application-Discovery-Report.md
read_file: reports/Technical-Assessment-Report.md
```

Extract:
- Component inventory (controllers, models, services, views)
- Business logic locations
- User's target architecture preferences
- Technology mapping decisions

---

## Step 2: Define .NET Project Structure

Based on user preferences from Phase 1, define the target .NET 10 project structure:

### For ASP.NET Core MVC:
```
[ProjectName]/
├── [ProjectName].sln
├── src/
│   └── [ProjectName].Web/
│       ├── [ProjectName].Web.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── Controllers/
│       ├── Models/
│       │   ├── Entities/
│       │   ├── ViewModels/
│       │   └── DTOs/
│       ├── Services/
│       │   ├── Interfaces/
│       │   └── Implementations/
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   ├── Repositories/
│       │   └── Migrations/
│       ├── Views/
│       │   ├── Shared/
│       │   └── [Controller]/
│       ├── wwwroot/
│       │   ├── css/
│       │   ├── js/
│       │   └── images/
│       └── Infrastructure/
│           ├── Middleware/
│           └── Extensions/
└── tests/
    └── [ProjectName].Tests/
```

### For ASP.NET Core Web API:
```
[ProjectName]/
├── [ProjectName].sln
├── src/
│   └── [ProjectName].Api/
│       ├── [ProjectName].Api.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── Controllers/
│       ├── Models/
│       │   ├── Entities/
│       │   └── DTOs/
│       ├── Services/
│       ├── Data/
│       └── Infrastructure/
└── tests/
```

### For Blazor:
```
[ProjectName]/
├── [ProjectName].sln
├── src/
│   ├── [ProjectName].Client/         (Blazor WASM)
│   │   ├── Pages/
│   │   ├── Components/
│   │   └── Services/
│   └── [ProjectName].Server/
│       ├── Controllers/
│       ├── Services/
│       └── Data/
└── tests/
```

---

## Step 3: Create File-by-File Migration Plan

### 3.1 Controllers Migration Plan

For each PHP controller discovered in Phase 0:

```markdown
### Controller: [ControllerName]

| Property | Value |
|----------|-------|
| **Source File** | `app/Http/Controllers/UserController.php` |
| **Target File** | `src/ProjectName.Web/Controllers/UserController.cs` |
| **Purpose** | Handle user CRUD operations and authentication |
| **HTTP Methods** | GET, POST, PUT, DELETE |

#### Actions/Methods Mapping

| PHP Method | HTTP | Route | .NET Method | Notes |
|------------|------|-------|-------------|-------|
| `index()` | GET | /users | `Index()` | Returns IActionResult with view |
| `show($id)` | GET | /users/{id} | `Details(int id)` | Validate id exists |
| `store(Request $request)` | POST | /users | `Create(CreateUserDto dto)` | Use DTO + FluentValidation |
| `update(Request $request, $id)` | PUT | /users/{id} | `Update(int id, UpdateUserDto dto)` | |
| `destroy($id)` | DELETE | /users/{id} | `Delete(int id)` | Soft delete if applicable |

#### Dependencies
- **Injected Services**: `IUserService`, `ILogger<UserController>`
- **Used Models**: `User`, `CreateUserDto`, `UpdateUserDto`
- **Middleware**: `[Authorize]` attribute

#### Business Logic in Controller
⚠️ If controller contains business logic, document for extraction:
| Logic | Current Location | Target Location |
|-------|-----------------|-----------------|
| Email validation | `store()` method | `UserService.ValidateEmail()` |
| Role assignment | `store()` method | `UserService.AssignDefaultRole()` |

#### Migration Complexity: [Low/Medium/High]
#### Estimated Effort: [X hours]
```

### 3.2 Models/Entities Migration Plan

For each PHP model:

```markdown
### Model: [ModelName]

| Property | Value |
|----------|-------|
| **Source File** | `app/Models/User.php` |
| **Target File** | `src/ProjectName.Web/Models/Entities/User.cs` |
| **Database Table** | `users` |
| **Primary Key** | `id` (auto-increment) |

#### Properties Mapping

| PHP Property | PHP Type | C# Property | C# Type | Notes |
|--------------|----------|-------------|---------|-------|
| `$id` | int | `Id` | `int` | Primary key |
| `$email` | string | `Email` | `string` | Required, unique |
| `$password` | string | `PasswordHash` | `string` | Hashed |
| `$name` | string | `Name` | `string` | Max 100 chars |
| `$created_at` | Carbon | `CreatedAt` | `DateTimeOffset` | |
| `$updated_at` | Carbon | `UpdatedAt` | `DateTimeOffset?` | Nullable |
| `$deleted_at` | Carbon | `DeletedAt` | `DateTimeOffset?` | Soft delete |

#### Relationships

| PHP Relationship | Type | Related Model | C# Navigation | Notes |
|------------------|------|---------------|---------------|-------|
| `orders()` | hasMany | Order | `ICollection<Order> Orders` | |
| `role()` | belongsTo | Role | `Role Role` + `int RoleId` | FK |
| `profile()` | hasOne | Profile | `Profile Profile` | |

#### Eloquent Scopes → EF Core Query Filters

| PHP Scope | Purpose | EF Core Implementation |
|-----------|---------|----------------------|
| `scopeActive($query)` | Only active users | Global query filter or extension method |
| `scopeAdmins($query)` | Only admin users | Extension method on `IQueryable<User>` |

#### Model Events → EF Core

| PHP Event | Purpose | EF Core Implementation |
|-----------|---------|----------------------|
| `creating` | Set defaults | Override `SaveChanges()` or use `IEntityTypeConfiguration` |
| `deleting` | Cascade soft delete | Configure in `OnModelCreating` |

#### Validation Rules
Document Laravel validation rules to convert:
| PHP Rule | C# Equivalent |
|----------|---------------|
| `required` | `[Required]` |
| `email` | `[EmailAddress]` |
| `max:100` | `[MaxLength(100)]` |
| `unique:users,email` | Unique index + service validation |

#### Migration Complexity: [Low/Medium/High]
#### Estimated Effort: [X hours]
```

### 3.3 Services Migration Plan

For each PHP service (this is where business logic lives):

```markdown
### Service: [ServiceName]

| Property | Value |
|----------|-------|
| **Source File** | `app/Services/PaymentService.php` |
| **Target Interface** | `src/ProjectName.Web/Services/Interfaces/IPaymentService.cs` |
| **Target Implementation** | `src/ProjectName.Web/Services/Implementations/PaymentService.cs` |
| **Purpose** | Handle all payment processing logic |

#### Dependencies

| PHP Dependency | Injection Method | C# Equivalent |
|----------------|------------------|---------------|
| `StripeGateway` | Constructor | `IStripeClient` (from Stripe.net) |
| `OrderRepository` | Constructor | `IOrderRepository` |
| `Logger` | Facade | `ILogger<PaymentService>` |
| `Config` | Facade | `IOptions<PaymentSettings>` |

#### Methods Mapping

| PHP Method | Signature | C# Method | Signature | Notes |
|------------|-----------|-----------|-----------|-------|
| `processPayment` | `(Order $order, array $card): PaymentResult` | `ProcessPaymentAsync` | `(Order order, CardDto card): Task<PaymentResult>` | Make async |
| `refund` | `(string $transactionId, float $amount): bool` | `RefundAsync` | `(string transactionId, decimal amount): Task<bool>` | Use decimal for money |
| `validateCard` | `(array $card): ValidationResult` | `ValidateCard` | `(CardDto card): ValidationResult` | Can be sync |

#### Business Logic Documentation

**⚠️ CRITICAL: Document ALL business rules in this service:**

| Business Rule | PHP Implementation | Line Numbers | C# Implementation Notes |
|---------------|-------------------|--------------|------------------------|
| Minimum order $10 | `if ($order->total < 10) throw...` | L45-47 | Same logic, use FlamamValidation |
| Max refund 30 days | `if ($order->created_at->diffInDays(now()) > 30)` | L78-82 | Use DateTimeOffset comparison |
| Fraud check | `$this->fraudService->check($card)` | L55-60 | Inject IFraudService |
| Retry on failure | `retry(3, fn() => $stripe->charge(...))` | L65-70 | Use Polly for retry policy |

#### Error Handling

| PHP Exception | When Thrown | C# Exception |
|---------------|-------------|--------------|
| `PaymentFailedException` | Stripe returns error | `PaymentFailedException` (custom) |
| `InsufficientFundsException` | Card declined | `InsufficientFundsException` (custom) |
| `ValidationException` | Invalid card data | `ValidationException` |

#### External API Calls

| API | Method | Purpose | .NET Implementation |
|-----|--------|---------|---------------------|
| Stripe | `POST /v1/charges` | Charge card | Use Stripe.net SDK |
| Stripe | `POST /v1/refunds` | Process refund | Use Stripe.net SDK |

#### Migration Complexity: [Low/Medium/High]
#### Estimated Effort: [X hours]
```

### 3.4 Views Migration Plan

For each PHP view/template:

```markdown
### View: [ViewName]

| Property | Value |
|----------|-------|
| **Source File** | `resources/views/users/index.blade.php` |
| **Target File** | `src/ProjectName.Web/Views/Users/Index.cshtml` |
| **Layout** | `app.blade.php` → `_Layout.cshtml` |
| **Controller** | `UserController.Index()` |

#### View Components Used

| Blade Component | Purpose | Razor Equivalent |
|-----------------|---------|------------------|
| `@include('partials.sidebar')` | Navigation | `<partial name="_Sidebar" />` |
| `<x-alert type="success">` | Alert box | Tag Helper or View Component |
| `@component('card')` | Card wrapper | View Component |

#### Data Passed to View

| Variable | PHP Type | Description | C# ViewModel Property |
|----------|----------|-------------|----------------------|
| `$users` | Collection | List of users | `IEnumerable<UserViewModel> Users` |
| `$currentPage` | int | Pagination | `int CurrentPage` |
| `$totalPages` | int | Pagination | `int TotalPages` |

#### Blade → Razor Syntax Mapping

| Blade Syntax | Razor Equivalent |
|--------------|------------------|
| `{{ $variable }}` | `@variable` or `@Model.Variable` |
| `{!! $html !!}` | `@Html.Raw(variable)` |
| `@if($condition)` | `@if (condition)` |
| `@foreach($items as $item)` | `@foreach (var item in items)` |
| `@extends('layout')` | `@{ Layout = "_Layout"; }` |
| `@section('content')` | `@section Content { }` |
| `@yield('content')` | `@RenderSection("Content")` |
| `@include('partial')` | `<partial name="_Partial" />` |
| `@csrf` | `@Html.AntiForgeryToken()` |
| `@auth` | `@if (User.Identity.IsAuthenticated)` |

#### JavaScript/CSS Dependencies

| Asset | Source | Target Location |
|-------|--------|-----------------|
| `app.js` | `resources/js/app.js` | `wwwroot/js/app.js` |
| `app.css` | `resources/css/app.css` | `wwwroot/css/app.css` |

#### Migration Complexity: [Low/Medium/High]
#### Estimated Effort: [X hours]
```

### 3.5 Middleware Migration Plan

```markdown
### Middleware: [MiddlewareName]

| Property | Value |
|----------|-------|
| **Source File** | `app/Http/Middleware/CheckAge.php` |
| **Target File** | `src/ProjectName.Web/Infrastructure/Middleware/CheckAgeMiddleware.cs` |
| **Applied To** | Routes with `age.check` middleware |

#### PHP Implementation Analysis

```php
public function handle($request, Closure $next, $minAge = 18)
{
    if ($request->user()->age < $minAge) {
        abort(403, 'Age requirement not met');
    }
    return $next($request);
}
```

#### C# Implementation Plan

```csharp
public class CheckAgeMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Implementation here
        await next(context);
    }
}
```

#### Registration
- PHP: `Kernel.php` in `$routeMiddleware`
- C#: `Program.cs` with `app.UseMiddleware<T>()` or endpoint filter

#### Migration Complexity: [Low/Medium/High]
#### Estimated Effort: [X hours]
```

### 3.6 Background Jobs Migration Plan

```markdown
### Job: [JobName]

| Property | Value |
|----------|-------|
| **Source File** | `app/Jobs/SendWelcomeEmail.php` |
| **Target File** | `src/ProjectName.Web/BackgroundServices/SendWelcomeEmailJob.cs` |
| **Trigger** | Queue dispatch after user registration |

#### PHP Implementation

| Property | Value |
|----------|-------|
| Queue | `emails` |
| Delay | None |
| Retries | 3 |
| Timeout | 60 seconds |

#### C# Implementation Plan

**Option A: BackgroundService + Azure Service Bus**
```csharp
public class EmailQueueProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Process messages from Azure Service Bus
    }
}
```

**Option B: Hangfire (if not using Azure)**
```csharp
BackgroundJob.Enqueue<IEmailService>(x => x.SendWelcomeEmail(userId));
```

#### Migration Complexity: [Low/Medium/High]
#### Estimated Effort: [X hours]
```

---

## Step 4: Define Migration Order

**CRITICAL**: Define the order files should be migrated based on dependencies.

```markdown
## Migration Order

### Wave 1: Foundation (No dependencies)
| Order | Component | File | Depends On |
|-------|-----------|------|------------|
| 1.1 | Project Setup | Create .csproj, Program.cs | - |
| 1.2 | Configuration | appsettings.json | - |
| 1.3 | Base Entities | User.cs, BaseEntity.cs | - |
| 1.4 | DbContext | ApplicationDbContext.cs | Entities |

### Wave 2: Data Layer
| Order | Component | File | Depends On |
|-------|-----------|------|------------|
| 2.1 | Entities | All entity classes | Wave 1 |
| 2.2 | DTOs | All DTO classes | Entities |
| 2.3 | Repositories | Repository implementations | DbContext |

### Wave 3: Business Logic
| Order | Component | File | Depends On |
|-------|-----------|------|------------|
| 3.1 | Service Interfaces | IUserService.cs, etc. | DTOs |
| 3.2 | Service Implementations | UserService.cs, etc. | Interfaces, Repositories |
| 3.3 | Validators | FluentValidation rules | DTOs |

### Wave 4: Controllers
| Order | Component | File | Depends On |
|-------|-----------|------|------------|
| 4.1 | Base Controller | BaseController.cs | - |
| 4.2 | All Controllers | UserController.cs, etc. | Services, DTOs |

### Wave 5: UI Layer
| Order | Component | File | Depends On |
|-------|-----------|------|------------|
| 5.1 | Layout | _Layout.cshtml | - |
| 5.2 | Partials | All partial views | Layout |
| 5.3 | Views | All views | Partials, ViewModels |
| 5.4 | Static Assets | CSS, JS, images | - |

### Wave 6: Infrastructure
| Order | Component | File | Depends On |
|-------|-----------|------|------------|
| 6.1 | Middleware | All middleware | - |
| 6.2 | Background Services | All jobs | Services |
| 6.3 | Extensions | DI registration | All services |
```

---

## Step 5: Generate Migration Plan Document

Create `reports/Migration-Plan-Detailed.md`:

```markdown
# Detailed Migration Plan

**Application**: [Name]
**Generated**: [Date/Time]
**Source**: PHP [Version] / [Framework]
**Target**: .NET 10 / ASP.NET Core

## Executive Summary

- **Total PHP Files**: [X]
- **Files with Business Logic**: [X]
- **Estimated Total Effort**: [X hours]
- **Migration Waves**: 6

## Target Project Structure

[Full project structure diagram]

## Migration Plan by Component

### Controllers ([X] files)
[All controller migration plans]

### Models/Entities ([X] files)
[All model migration plans]

### Services ([X] files)
[All service migration plans]

### Views ([X] files)
[All view migration plans]

### Middleware ([X] files)
[All middleware migration plans]

### Background Jobs ([X] files)
[All job migration plans]

## Migration Order

[Complete wave-by-wave migration order]

## Business Logic Preservation Checklist

| Business Rule | Source File | Source Method | Target File | Target Method | Status |
|---------------|-------------|---------------|-------------|---------------|--------|
| [Rule 1] | [file.php] | [method] | [File.cs] | [Method] | ⏳ |

## Dependency Migration

| Composer Package | NuGet Package | Notes |
|------------------|---------------|-------|
| [package] | [package] | [notes] |

## Configuration Migration

| PHP Config | .NET Config | Notes |
|------------|-------------|-------|
| `.env` key | `appsettings.json` path | |

## Next Steps

Proceed to code migration:

**Run**: `/phase3-migratecode`

The migration will follow the wave order defined in this plan.
```

---

## Step 6: Update Status Report

Update `reports/Report-Status.md`:

```markdown
## Phase 2 Summary

- **Migration Plan**: Complete
- **Files Documented**: [X]
- **Business Rules Mapped**: [X]
- **Estimated Effort**: [X] hours

## Next Step

Run `/phase3-migratecode` to execute the migration following this plan.
```

---

## Rules & Constraints

### Documentation Depth
- Document EVERY file with business logic
- Include exact line numbers for business rules
- Map every PHP pattern to its .NET equivalent
- Define clear migration order based on dependencies

### Business Logic Priority
- **CRITICAL**: Capture ALL business rules
- Document WHERE logic lives (file, method, line)
- Identify logic in wrong places (controllers instead of services)
- Plan for refactoring if needed

### File Reading
- Read **2000 lines at a time** for context
- Use `semantic_search` to find related code
- Use `grep_search` for specific patterns

### Do NOT
- Do NOT start writing .NET code yet
- Do NOT create the .NET project yet
- Focus ONLY on creating the detailed plan

---

## Deliverables

At the end of Phase 2, you should have:

1. ✅ `reports/Migration-Plan-Detailed.md` - Complete file-by-file plan
2. ✅ Every controller mapped with actions
3. ✅ Every model mapped with properties and relationships
4. ✅ Every service mapped with methods and business logic
5. ✅ Every view mapped with Razor equivalents
6. ✅ Migration order defined by waves
7. ✅ Business logic preservation checklist
8. ✅ `reports/Report-Status.md` updated

**Next Step**: `/phase3-migratecode` to execute the migration.
```
