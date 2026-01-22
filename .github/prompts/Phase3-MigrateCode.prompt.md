---
agent: agent
model: Claude Sonnet 4.5 (copilot)
tools: ['vscode/openSimpleBrowser', 'vscode/vscodeAPI', 'vscode/extensions', 'execute/testFailure', 'execute/runTask', 'execute/runInTerminal', 'execute/runTests', 'read/problems', 'read/readFile', 'read/terminalSelection', 'read/terminalLastCommand', 'edit/editFiles', 'search', 'web', 'azure-mcp/documentation', 'azure-mcp/search']
---

# Phase 3: Execute PHP to .NET 10 Code Migration

## Objective

Execute the migration from PHP to .NET 10 following the detailed file-by-file plan created in Phase 2. This phase creates the actual .NET 10 project and migrates all code.

**Prerequisites**:
- Phase 0: `Application-Discovery-Report.md` completed
- Phase 1: `Technical-Assessment-Report.md` completed
- Phase 2: `Migration-Plan-Detailed.md` completed

---

## Step 1: Review Migration Plan

Before starting, read the migration plan:

```
read_file: reports/Migration-Plan-Detailed.md
read_file: reports/Technical-Assessment-Report.md
```

Confirm you understand:
- [ ] Target .NET architecture (MVC/API/Blazor)
- [ ] Migration wave order
- [ ] All file mappings
- [ ] Business logic locations
- [ ] Package mappings

---

## Step 2: Create .NET 10 Project Structure

### 2.1 Create Project Folder

Create a new folder for the .NET 10 project:

```bash
# Do NOT create a new workspace, create a folder in existing workspace
mkdir [ProjectName].NET
cd [ProjectName].NET
```

### 2.2 Initialize .NET 10 Project

Based on the architecture chosen in Phase 1:

**For ASP.NET Core MVC:**
```bash
dotnet new mvc -n [ProjectName].Web -f net10.0
dotnet new sln -n [ProjectName]
dotnet sln add src/[ProjectName].Web/[ProjectName].Web.csproj
```

**For ASP.NET Core Web API:**
```bash
dotnet new webapi -n [ProjectName].Api -f net10.0
dotnet new sln -n [ProjectName]
dotnet sln add src/[ProjectName].Api/[ProjectName].Api.csproj
```

**For Blazor Server:**
```bash
dotnet new blazor -n [ProjectName].Web -f net10.0
```

### 2.3 Add Required NuGet Packages

Based on the package mapping from Phase 2:

```bash
# Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer  # or .MySQL, .PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Authentication (if using Identity)
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# Authentication (if using Entra ID)
dotnet add package Microsoft.Identity.Web
dotnet add package Microsoft.Identity.Web.UI

# Validation
dotnet add package FluentValidation.AspNetCore

# Add packages mapped from Composer dependencies
# [Add based on Phase 2 package mapping]
```

### 2.4 Build to Verify Setup

```bash
dotnet build
```

Use `get_errors` to check for any issues.

---

## Step 3: Execute Migration by Waves

**CRITICAL**: Follow the wave order from Phase 2's Migration Plan.

### Wave 1: Foundation

#### 1.1 Configure appsettings.json

Migrate from PHP `.env` and `config/*.php`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;"
  },
  "AppSettings": {
    // Migrate from PHP config
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

#### 1.2 Configure Program.cs

Set up dependency injection, middleware, and services:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services from Phase 2 plan
// builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

### Wave 2: Data Layer

#### 2.1 Create Entity Classes

For each PHP model documented in Phase 2:

**Read the PHP model:**
```
read_file: [PHP source path from Migration Plan]
```

**Create the C# entity following the mapping:**

```csharp
// Example: User entity migrated from app/Models/User.php
namespace ProjectName.Web.Models.Entities;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }  // Soft delete
    
    // Relationships (from Eloquent relationships)
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

#### 2.2 Create DbContext

```csharp
namespace ProjectName.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    // Add all entities
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure relationships
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Soft delete query filter (replaces Eloquent SoftDeletes)
            entity.HasQueryFilter(e => e.DeletedAt == null);
            
            // Relationships
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleId);
        });
    }
    
    // Override SaveChanges for timestamps (replaces Eloquent events)
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            
        foreach (var entry in entries)
        {
            if (entry.Entity is IHasTimestamps entity)
            {
                if (entry.State == EntityState.Added)
                    entity.CreatedAt = DateTimeOffset.UtcNow;
                entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
```

#### 2.3 Create DTOs

For each entity, create corresponding DTOs:

```csharp
namespace ProjectName.Web.Models.DTOs;

public record CreateUserDto(
    string Email,
    string Password,
    string Name,
    int RoleId
);

public record UpdateUserDto(
    string? Email,
    string? Name,
    int? RoleId
);

public record UserDto(
    int Id,
    string Email,
    string Name,
    string RoleName,
    DateTimeOffset CreatedAt
);
```

#### 2.4 Generate Migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update  # If testing locally
```

**Build and validate:**
```bash
dotnet build
```

Use `get_errors` to check for issues.

---

### Wave 3: Business Logic (Services)

**⚠️ CRITICAL**: This is where business logic lives. Follow the Phase 2 plan exactly.

#### 3.1 Create Service Interfaces

```csharp
namespace ProjectName.Web.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto> UpdateAsync(int id, UpdateUserDto dto);
    Task DeleteAsync(int id);
    
    // Business logic methods from PHP service
    Task<bool> ValidateEmailAsync(string email);
    Task AssignDefaultRoleAsync(User user);
}
```

#### 3.2 Implement Services

For each PHP service in the Migration Plan:

1. **Read the PHP service:**
```
read_file: [PHP service path - read 2000 lines at a time]
```

2. **Migrate each method following the plan:**

```csharp
namespace ProjectName.Web.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    
    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    // Migrated from PHP: UserService::getAll()
    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.Name,
                u.Role.Name,
                u.CreatedAt
            ))
            .ToListAsync();
    }
    
    // Migrated from PHP: UserService::create($data)
    // Business logic preserved from lines XX-YY
    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        // Business rule: Validate email uniqueness (from PHP line XX)
        if (!await ValidateEmailAsync(dto.Email))
        {
            throw new ValidationException("Email already exists");
        }
        
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Name = dto.Name,
            RoleId = dto.RoleId
        };
        
        // Business rule: Assign default role if not specified (from PHP line XX)
        if (dto.RoleId == 0)
        {
            await AssignDefaultRoleAsync(user);
        }
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return new UserDto(user.Id, user.Email, user.Name, user.Role.Name, user.CreatedAt);
    }
    
    // Additional business logic methods...
}
```

#### 3.3 Register Services

In `Program.cs`:

```csharp
// Register all services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
// ... add all services from Phase 2 plan
```

**Build and validate:**
```bash
dotnet build
```

---

### Wave 4: Controllers

For each PHP controller in the Migration Plan:

#### 4.1 Read PHP Controller

```
read_file: [PHP controller path from Migration Plan]
```

#### 4.2 Create .NET Controller

```csharp
namespace ProjectName.Web.Controllers;

public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    
    // Migrated from: UserController::index()
    // Route: GET /users
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();
        return View(users);
    }
    
    // Migrated from: UserController::show($id)
    // Route: GET /users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        return View(user);
    }
    
    // Migrated from: UserController::store(Request $request)
    // Route: POST /users
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);
            
        try
        {
            var user = await _userService.CreateAsync(dto);
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }
    
    // ... additional actions following Phase 2 mapping
}
```

**Build and validate:**
```bash
dotnet build
```

---

### Wave 5: UI Layer (Views)

#### 5.1 Create Layout

Migrate from `resources/views/layouts/app.blade.php`:

**Read PHP layout:**
```
read_file: resources/views/layouts/app.blade.php
```

**Create `Views/Shared/_Layout.cshtml`:**

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - @Configuration["AppName"]</title>
    <link rel="stylesheet" href="~/css/app.css" />
</head>
<body>
    <header>
        <!-- Migrate navigation from Blade -->
        <partial name="_Navigation" />
    </header>
    
    <main class="container">
        @RenderBody()
    </main>
    
    <footer>
        <!-- Footer content -->
    </footer>
    
    <script src="~/js/app.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

#### 5.2 Migrate Views

For each Blade view in the Migration Plan:

**Read PHP view:**
```
read_file: [Blade template path]
```

**Apply Blade → Razor conversion:**

| Blade | Razor |
|-------|-------|
| `{{ $var }}` | `@Model.Var` or `@var` |
| `{!! $html !!}` | `@Html.Raw(html)` |
| `@if($cond)` | `@if (cond)` |
| `@foreach($items as $item)` | `@foreach (var item in items)` |
| `@extends('layout')` | `@{ Layout = "_Layout"; }` |
| `@section('content')` | Content goes in body |
| `@include('partial')` | `<partial name="_Partial" />` |
| `@csrf` | `@Html.AntiForgeryToken()` |
| `@auth` | `@if (User.Identity?.IsAuthenticated == true)` |
| `{{ route('name') }}` | `@Url.Action("Action", "Controller")` |
| `{{ asset('path') }}` | `~/path` |

#### 5.3 Copy Static Assets

```bash
# Copy CSS
cp -r resources/css/* wwwroot/css/

# Copy JS
cp -r resources/js/* wwwroot/js/

# Copy images
cp -r public/images/* wwwroot/images/
```

---

### Wave 6: Infrastructure

#### 6.1 Middleware

For each PHP middleware in the Migration Plan:

```csharp
namespace ProjectName.Web.Infrastructure.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Request: {Method} {Path}", 
            context.Request.Method, 
            context.Request.Path);
            
        await _next(context);
    }
}
```

#### 6.2 Background Services

For each PHP job in the Migration Plan:

```csharp
namespace ProjectName.Web.BackgroundServices;

public class EmailQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailQueueProcessor> _logger;
    
    public EmailQueueProcessor(IServiceProvider serviceProvider, ILogger<EmailQueueProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            
            // Process queue
            await emailService.ProcessPendingEmailsAsync();
            
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

---

## Step 4: Validate Migration

### 4.1 Build Solution

```bash
dotnet build
```

Use `get_errors` to identify and fix any issues.

### 4.2 Run Application

```bash
dotnet run
```

### 4.3 Validate Business Logic

Check each business rule documented in Phase 2:

| Business Rule | Expected Behavior | Status |
|---------------|-------------------|--------|
| [Rule 1] | [Expected] | ✅/❌ |

---

## Step 5: Containerization (If Required)

If specified in Phase 1, create Docker support:

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/ProjectName.Web/ProjectName.Web.csproj", "ProjectName.Web/"]
RUN dotnet restore "ProjectName.Web/ProjectName.Web.csproj"
COPY . .
WORKDIR "/src/ProjectName.Web"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectName.Web.dll"]
```

---

## Step 6: Update Status Report

Update `reports/Report-Status.md`:

```markdown
## Phase 3 Summary

- **Migration Status**: Complete
- **Files Migrated**: [X]
- **Build Status**: ✅ Passing
- **Business Logic Preserved**: ✅ All rules migrated

## Next Step

Run `/phase4-generateinfra` to create Azure infrastructure.
```

---

## Rules & Constraints

### Code Reading
- Read **2000 lines at a time** for sufficient context
- Use `semantic_search` for cross-file pattern discovery
- Always read PHP source before writing C# equivalent

### Build Frequently
- Build after each wave
- Use `get_errors` to catch issues early
- Fix errors before proceeding

### Business Logic Priority
- **CRITICAL**: Preserve ALL business logic from PHP
- Reference exact locations from Phase 2 plan
- Add comments noting which PHP method/line was migrated

### Do NOT
- Do NOT skip steps in the wave order
- Do NOT deviate from the Phase 2 plan
- Do NOT ignore build errors

---

## Deliverables

At the end of Phase 3:

1. ✅ Complete .NET 10 project structure
2. ✅ All entities migrated with relationships
3. ✅ All services migrated with business logic
4. ✅ All controllers migrated with actions
5. ✅ All views migrated from Blade/Twig to Razor
6. ✅ Middleware and background services
7. ✅ Static assets copied
8. ✅ Application builds successfully
9. ✅ Docker support (if required)
10. ✅ `reports/Report-Status.md` updated

**Next Step**: `/phase4-generateinfra` to create Azure infrastructure.
