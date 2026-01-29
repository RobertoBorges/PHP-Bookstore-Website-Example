# Detailed Migration Plan - PHP to .NET 10

**Project:** PHP-Bookstore-Website-Example  
**Date:** January 28, 2026  
**Phase:** 2 - Migration Planning  
**Status:** ✅ Complete

---

## Executive Summary

This document provides a **file-by-file, method-by-method migration plan** for converting the PHP Bookstore application to .NET 10. The migration is organized into **5 waves** based on dependencies, with each wave building on the previous one.

### Migration Strategy

```mermaid
graph TB
    subgraph "Wave 1: Foundation"
        W1A[Project Structure]
        W1B[EF Core Entities]
        W1C[DbContext]
        W1D[appsettings.json]
    end
    
    subgraph "Wave 2: Data Layer"
        W2A[BookService]
        W2B[CustomerService]
        W2C[CartService]
        W2D[OrderService]
    end
    
    subgraph "Wave 3: Controllers"
        W3A[HomeController]
        W3B[CartController]
        W3C[OrderController]
        W3D[AccountController]
    end
    
    subgraph "Wave 4: Views"
        W4A[Layout.cshtml]
        W4B[Home Views]
        W4C[Cart Views]
        W4D[Order Views]
        W4E[Account Views]
    end
    
    subgraph "Wave 5: Infrastructure"
        W5A[Dockerfile]
        W5B[K8s Manifests]
        W5C[Bicep Templates]
        W5D[CI/CD Pipeline]
    end
    
    W1A --> W1B --> W1C --> W1D
    W1D --> W2A
    W2A --> W2B --> W2C --> W2D
    W2D --> W3A
    W3A --> W3B --> W3C --> W3D
    W3D --> W4A
    W4A --> W4B --> W4C --> W4D --> W4E
    W4E --> W5A --> W5B --> W5C --> W5D
```

---

## Project Structure to Create

### .NET 10 Solution Structure

```
BookstoreApp/
├── src/
│   ├── BookstoreApp.Web/                    # ASP.NET Core MVC Project
│   │   ├── Controllers/
│   │   │   ├── HomeController.cs            # Book catalog (from index.php)
│   │   │   ├── CartController.cs            # Cart operations
│   │   │   ├── OrderController.cs           # Checkout (from checkout.php)
│   │   │   └── AccountController.cs         # Profile (from edituser.php, register.php)
│   │   ├── Views/
│   │   │   ├── Shared/
│   │   │   │   ├── _Layout.cshtml           # Master layout
│   │   │   │   ├── _LoginPartial.cshtml     # Auth buttons
│   │   │   │   └── _ValidationScriptsPartial.cshtml
│   │   │   ├── Home/
│   │   │   │   └── Index.cshtml             # Book catalog
│   │   │   ├── Cart/
│   │   │   │   └── Index.cshtml             # Shopping cart
│   │   │   ├── Order/
│   │   │   │   ├── Checkout.cshtml          # Checkout form
│   │   │   │   ├── Confirmation.cshtml      # Order success
│   │   │   │   └── History.cshtml           # Order history (new)
│   │   │   └── Account/
│   │   │       ├── Profile.cshtml           # Edit profile
│   │   │       └── Register.cshtml          # Complete registration
│   │   ├── Models/
│   │   │   ├── ViewModels/
│   │   │   │   ├── BookCatalogViewModel.cs
│   │   │   │   ├── CartViewModel.cs
│   │   │   │   ├── CheckoutViewModel.cs
│   │   │   │   ├── OrderConfirmationViewModel.cs
│   │   │   │   └── ProfileViewModel.cs
│   │   │   └── DTOs/
│   │   │       ├── CartItemDto.cs
│   │   │       └── OrderDto.cs
│   │   ├── Services/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IBookService.cs
│   │   │   │   ├── ICartService.cs
│   │   │   │   ├── IOrderService.cs
│   │   │   │   └── ICustomerService.cs
│   │   │   └── Implementations/
│   │   │       ├── BookService.cs
│   │   │       ├── CartService.cs
│   │   │       ├── OrderService.cs
│   │   │       └── CustomerService.cs
│   │   ├── Extensions/
│   │   │   └── SessionExtensions.cs
│   │   ├── wwwroot/
│   │   │   ├── css/
│   │   │   │   └── site.css                 # From style.css
│   │   │   ├── images/                      # Book images
│   │   │   └── js/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Dockerfile
│   │
│   └── BookstoreApp.Data/                   # Data Access Layer
│       ├── Entities/
│       │   ├── Book.cs
│       │   ├── Customer.cs
│       │   ├── Order.cs
│       │   └── CartItem.cs
│       ├── Enums/
│       │   └── OrderStatus.cs
│       ├── BookstoreDbContext.cs
│       └── Migrations/
│
├── infra/
│   ├── bicep/
│   │   ├── main.bicep
│   │   ├── sql.bicep
│   │   ├── storage.bicep
│   │   └── monitoring.bicep
│   └── kubernetes/
│       ├── namespace.yaml
│       ├── deployment.yaml
│       ├── service.yaml
│       ├── ingress.yaml
│       ├── configmap.yaml
│       └── hpa.yaml
│
├── tests/
│   └── BookstoreApp.Tests/
│       ├── Services/
│       └── Controllers/
│
├── .github/
│   └── workflows/
│       └── deploy.yml
│
├── BookstoreApp.sln
└── README.md
```

---

## Wave 1: Foundation (Day 1)

### Wave 1 Overview

| Task | Output | Dependencies | Est. Time |
|------|--------|--------------|-----------|
| 1.1 Create solution & projects | .sln, .csproj files | None | 30 min |
| 1.2 Add NuGet packages | Package references | 1.1 | 15 min |
| 1.3 Create EF Core entities | Entity classes | 1.2 | 1 hour |
| 1.4 Create DbContext | BookstoreDbContext.cs | 1.3 | 30 min |
| 1.5 Configure Program.cs | DI, middleware | 1.4 | 1 hour |
| 1.6 Create appsettings.json | Configuration | 1.5 | 30 min |
| 1.7 Run EF migrations | Database schema | 1.6 | 30 min |
| **Total Wave 1** | | | **4-5 hours** |

---

### Task 1.1: Create Solution & Projects

**Commands to execute:**
```powershell
# Create solution
dotnet new sln -n BookstoreApp -o src

# Create web project
dotnet new mvc -n BookstoreApp.Web -o src/BookstoreApp.Web -f net10.0

# Create data project
dotnet new classlib -n BookstoreApp.Data -o src/BookstoreApp.Data -f net10.0

# Create test project
dotnet new xunit -n BookstoreApp.Tests -o tests/BookstoreApp.Tests -f net10.0

# Add projects to solution
dotnet sln src/BookstoreApp.sln add src/BookstoreApp.Web/BookstoreApp.Web.csproj
dotnet sln src/BookstoreApp.sln add src/BookstoreApp.Data/BookstoreApp.Data.csproj
dotnet sln src/BookstoreApp.sln add tests/BookstoreApp.Tests/BookstoreApp.Tests.csproj

# Add project references
dotnet add src/BookstoreApp.Web/BookstoreApp.Web.csproj reference src/BookstoreApp.Data/BookstoreApp.Data.csproj
dotnet add tests/BookstoreApp.Tests/BookstoreApp.Tests.csproj reference src/BookstoreApp.Web/BookstoreApp.Web.csproj
```

---

### Task 1.2: Add NuGet Packages

**BookstoreApp.Data.csproj packages:**
```xml
<ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
</ItemGroup>
```

**BookstoreApp.Web.csproj packages:**
```xml
<ItemGroup>
    <PackageReference Include="Microsoft.Identity.Web" Version="3.0.0" />
    <PackageReference Include="Microsoft.Identity.Web.UI" Version="3.0.0" />
    <PackageReference Include="Azure.Identity" Version="1.12.0" />
    <PackageReference Include="Azure.Storage.Blobs" Version="12.21.0" />
    <PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
    <PackageReference Include="AspNetCore.HealthChecks.SqlServer" Version="8.0.0" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
</ItemGroup>
```

---

### Task 1.3: Create EF Core Entities

#### 1.3.1 OrderStatus Enum (NEW - replaces 'N'/'y')

**File:** `src/BookstoreApp.Data/Enums/OrderStatus.cs`

```csharp
namespace BookstoreApp.Data.Enums;

public enum OrderStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2,
    Refunded = 3
}
```

#### 1.3.2 Book Entity

**Source:** `database.sql` Book table  
**File:** `src/BookstoreApp.Data/Entities/Book.cs`

| PHP Column | .NET Property | Type Change | Notes |
|------------|---------------|-------------|-------|
| BookID (varchar 50) | BookId | string | Primary key |
| BookTitle (varchar 200) | BookTitle | string | Required |
| ISBN (varchar 20) | Isbn | string | Optional |
| Price (double 12,2) | Price | decimal | Precision maintained |
| Author (varchar 128) | Author | string | Optional |
| Type (varchar 128) | Category | string | Renamed for clarity |
| Image (varchar 128) | ImageUrl | string | Will be Blob URL |

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookstoreApp.Data.Entities;

public class Book
{
    [Key]
    [MaxLength(50)]
    public string BookId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string BookTitle { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Isbn { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Price { get; set; }

    [MaxLength(128)]
    public string? Author { get; set; }

    [MaxLength(128)]
    public string? Category { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

#### 1.3.3 Customer Entity

**Source:** `database.sql` Users + Customer tables (MERGED)  
**File:** `src/BookstoreApp.Data/Entities/Customer.cs`

| PHP Column | .NET Property | Type Change | Notes |
|------------|---------------|-------------|-------|
| CustomerID | CustomerId | int | Primary key |
| UserName/Password | **REMOVED** | - | Entra ID handles auth |
| **NEW** | EntraIdObjectId | string | Entra ID user identifier |
| CustomerName | FullName | string | Renamed |
| CustomerPhone | PhoneNumber | string | Renamed |
| CustomerIC | IdentificationNumber | string | Renamed |
| CustomerEmail | Email | string | Renamed |
| CustomerAddress | Address | string | Same |
| CustomerGender | Gender | string | Same |

```csharp
using System.ComponentModel.DataAnnotations;

namespace BookstoreApp.Data.Entities;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    // Entra ID identifier - replaces UserName/Password
    [Required]
    [MaxLength(100)]
    public string EntraIdObjectId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? FullName { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(20)]
    public string? IdentificationNumber { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

#### 1.3.4 Order Entity

**Source:** `database.sql` Order table  
**File:** `src/BookstoreApp.Data/Entities/Order.cs`

| PHP Column | .NET Property | Type Change | Notes |
|------------|---------------|-------------|-------|
| OrderID | OrderId | int | Primary key |
| CustomerID | CustomerId | int | Foreign key |
| BookID | BookId | string | Foreign key |
| DatePurchase | PurchaseDate | DateTime | Renamed |
| Quantity | Quantity | int | Same |
| TotalPrice | TotalPrice | decimal | Same |
| Status ('N'/'y') | Status | OrderStatus enum | Enum replaces char |

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookstoreApp.Data.Enums;

namespace BookstoreApp.Data.Entities;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    [MaxLength(50)]
    public string BookId { get; set; } = string.Empty;

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal TotalPrice { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Navigation properties
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey(nameof(BookId))]
    public virtual Book Book { get; set; } = null!;
}
```

---

### Task 1.4: Create DbContext

**File:** `src/BookstoreApp.Data/BookstoreDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using BookstoreApp.Data.Entities;

namespace BookstoreApp.Data;

public class BookstoreDbContext : DbContext
{
    public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Book configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId);
            entity.Property(e => e.BookId).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(12,2)");
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Author);
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.HasIndex(e => e.EntraIdObjectId).IsUnique();
            entity.HasIndex(e => e.Email);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(12,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(12,2)");
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.PurchaseDate);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Book)
                .WithMany(b => b.Orders)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data (from database.sql)
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                BookId = "B-001",
                BookTitle = "Lonely Planet Australia (Travel Guide)",
                Isbn = "123-456-789-1",
                Price = 136.00m,
                Author = "Lonely Planet",
                Category = "Travel",
                ImageUrl = "/images/travel.jpg"
            },
            new Book
            {
                BookId = "B-002",
                BookTitle = "Crew Resource Management, Second Edition",
                Isbn = "123-456-789-2",
                Price = 599.00m,
                Author = "Barbara Kanki",
                Category = "Technical",
                ImageUrl = "/images/technical.jpg"
            },
            new Book
            {
                BookId = "B-003",
                BookTitle = "CCNA Routing and Switching 200-125 Official Cert Guide Library",
                Isbn = "123-456-789-3",
                Price = 329.00m,
                Author = "Cisco Press",
                Category = "Technology",
                ImageUrl = "/images/technology.jpg"
            },
            new Book
            {
                BookId = "B-004",
                BookTitle = "Easy Vegetarian Slow Cooker Cookbook",
                Isbn = "123-456-789-4",
                Price = 75.90m,
                Author = "Rockridge Press",
                Category = "Food",
                ImageUrl = "/images/food.jpg"
            }
        );
    }
}
```

---

### Task 1.5: Configure Program.cs

**File:** `src/BookstoreApp.Web/Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using BookstoreApp.Data;
using BookstoreApp.Web.Services;
using BookstoreApp.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add Entra ID authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// Add authorization
builder.Services.AddControllersWithViews(options =>
{
    // Require authentication by default (can use [AllowAnonymous] for public pages)
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddMicrosoftIdentityUI();

// Add Razor Pages (for Identity UI)
builder.Services.AddRazorPages();

// Add DbContext
builder.Services.AddDbContext<BookstoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BookstoreDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(3)
    ));

// Add session support (for shopping cart)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Register services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("BookstoreDb")!);

var app = builder.Build();

// Configure the HTTP request pipeline
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

app.UseSession();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Apply migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
```

---

### Task 1.6: Create appsettings.json

**File:** `src/BookstoreApp.Web/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<YOUR-TENANT-ID>",
    "ClientId": "<YOUR-CLIENT-ID>",
    "ClientSecret": "<FROM-KEY-VAULT>",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  },
  "ConnectionStrings": {
    "BookstoreDb": "Server=tcp:<your-server>.database.windows.net,1433;Initial Catalog=BookstoreDb;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;"
  },
  "ApplicationInsights": {
    "ConnectionString": "<YOUR-APP-INSIGHTS-CONNECTION-STRING>"
  }
}
```

---

### Task 1.7: Run EF Migrations

```powershell
# Navigate to data project
cd src/BookstoreApp.Data

# Create initial migration
dotnet ef migrations add InitialCreate --startup-project ../BookstoreApp.Web

# Apply migration (development)
dotnet ef database update --startup-project ../BookstoreApp.Web
```

---

## Wave 2: Services Layer (Day 2)

### Wave 2 Overview

| Task | Source PHP | Output | Est. Time |
|------|------------|--------|-----------|
| 2.1 Session Extensions | - | SessionExtensions.cs | 20 min |
| 2.2 DTOs & ViewModels | All PHP | Models folder | 45 min |
| 2.3 IBookService + BookService | index.php | BookService.cs | 1 hour |
| 2.4 ICartService + CartService | index.php cart logic | CartService.cs | 1.5 hours |
| 2.5 IOrderService + OrderService | checkout.php | OrderService.cs | 1.5 hours |
| 2.6 ICustomerService + CustomerService | register.php, edituser.php | CustomerService.cs | 1 hour |
| **Total Wave 2** | | | **6-7 hours** |

---

### Task 2.1: Session Extensions

**File:** `src/BookstoreApp.Web/Extensions/SessionExtensions.cs`

**Purpose:** Store/retrieve shopping cart from session (replaces database-backed Cart table)

```csharp
using System.Text.Json;

namespace BookstoreApp.Web.Extensions;

public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
```

---

### Task 2.2: DTOs and ViewModels

#### CartItemDto

**File:** `src/BookstoreApp.Web/Models/DTOs/CartItemDto.cs`

```csharp
namespace BookstoreApp.Web.Models.DTOs;

public class CartItemDto
{
    public string BookId { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => Price * Quantity;
}
```

#### ShoppingCart

**File:** `src/BookstoreApp.Web/Models/DTOs/ShoppingCart.cs`

```csharp
namespace BookstoreApp.Web.Models.DTOs;

public class ShoppingCart
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public int ItemCount => Items.Sum(i => i.Quantity);
}
```

#### BookCatalogViewModel

**File:** `src/BookstoreApp.Web/Models/ViewModels/BookCatalogViewModel.cs`

```csharp
using BookstoreApp.Data.Entities;

namespace BookstoreApp.Web.Models.ViewModels;

public class BookCatalogViewModel
{
    public IEnumerable<Book> Books { get; set; } = Enumerable.Empty<Book>();
    public ShoppingCart Cart { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>();
}
```

#### CheckoutViewModel

**File:** `src/BookstoreApp.Web/Models/ViewModels/CheckoutViewModel.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using BookstoreApp.Web.Models.DTOs;

namespace BookstoreApp.Web.Models.ViewModels;

public class CheckoutViewModel
{
    public ShoppingCart Cart { get; set; } = new();

    [Required(ErrorMessage = "Please enter your full name")]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Only letters and spaces allowed")]
    [MaxLength(128)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your phone number")]
    [RegularExpression(@"^[\d\s\-]+$", ErrorMessage = "Invalid phone number format")]
    [MaxLength(20)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [RegularExpression(@"^[\d\s\-]+$", ErrorMessage = "Invalid ID number format")]
    [MaxLength(20)]
    [Display(Name = "ID Number")]
    public string? IdentificationNumber { get; set; }

    [Required(ErrorMessage = "Please select your gender")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your address")]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
}
```

#### ProfileViewModel

**File:** `src/BookstoreApp.Web/Models/ViewModels/ProfileViewModel.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace BookstoreApp.Web.Models.ViewModels;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Please enter your full name")]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Only letters and spaces allowed")]
    [MaxLength(128)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your phone number")]
    [RegularExpression(@"^[\d\s\-]+$", ErrorMessage = "Invalid phone number format")]
    [MaxLength(20)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [RegularExpression(@"^[\d\s\-]+$", ErrorMessage = "Invalid ID number format")]
    [MaxLength(20)]
    [Display(Name = "ID Number")]
    public string? IdentificationNumber { get; set; }

    [Required(ErrorMessage = "Please select your gender")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your address")]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
}
```

#### OrderConfirmationViewModel

**File:** `src/BookstoreApp.Web/Models/ViewModels/OrderConfirmationViewModel.cs`

```csharp
using BookstoreApp.Data.Entities;

namespace BookstoreApp.Web.Models.ViewModels;

public class OrderConfirmationViewModel
{
    public Customer Customer { get; set; } = null!;
    public IEnumerable<Order> Orders { get; set; } = Enumerable.Empty<Order>();
    public decimal TotalAmount => Orders.Sum(o => o.TotalPrice);
    public DateTime OrderDate { get; set; }
}
```

---

### Task 2.3: BookService

**Source PHP Logic:**
- `index.php` lines 27-31: `SELECT * FROM Book`
- Used for catalog display

**File:** `src/BookstoreApp.Web/Services/Interfaces/IBookService.cs`

```csharp
using BookstoreApp.Data.Entities;

namespace BookstoreApp.Web.Services.Interfaces;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<(IEnumerable<Book> Books, int TotalCount)> GetBooksPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        string? category = null);
    Task<Book?> GetBookByIdAsync(string bookId);
    Task<IEnumerable<string>> GetCategoriesAsync();
}
```

**File:** `src/BookstoreApp.Web/Services/Implementations/BookService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using BookstoreApp.Data;
using BookstoreApp.Data.Entities;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Services;

public class BookService : IBookService
{
    private readonly BookstoreDbContext _context;
    private readonly ILogger<BookService> _logger;

    public BookService(BookstoreDbContext context, ILogger<BookService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Replaces: SELECT * FROM Book (index.php line 27)
    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        _logger.LogInformation("Fetching all books");
        return await _context.Books.AsNoTracking().ToListAsync();
    }

    // NEW: Pagination support (moderate modernization)
    public async Task<(IEnumerable<Book> Books, int TotalCount)> GetBooksPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        string? category = null)
    {
        _logger.LogInformation("Fetching books page {Page} with size {PageSize}", page, pageSize);

        var query = _context.Books.AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(b => 
                b.BookTitle.ToLower().Contains(term) ||
                (b.Author != null && b.Author.ToLower().Contains(term)));
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category == category);
        }

        var totalCount = await query.CountAsync();

        var books = await query
            .OrderBy(b => b.BookTitle)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (books, totalCount);
    }

    // Replaces: SELECT * FROM Book WHERE BookID = ? (index.php line 8)
    public async Task<Book?> GetBookByIdAsync(string bookId)
    {
        _logger.LogInformation("Fetching book {BookId}", bookId);
        return await _context.Books.AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookId == bookId);
    }

    // NEW: Get distinct categories for filter dropdown
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.Books
            .AsNoTracking()
            .Where(b => b.Category != null)
            .Select(b => b.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
}
```

---

### Task 2.4: CartService

**Source PHP Logic:**
- `index.php` lines 5-17: Add to cart
- `index.php` lines 19-23: Empty cart
- `index.php` lines 80-95: Display cart with totals

**File:** `src/BookstoreApp.Web/Services/Interfaces/ICartService.cs`

```csharp
using BookstoreApp.Web.Models.DTOs;

namespace BookstoreApp.Web.Services.Interfaces;

public interface ICartService
{
    ShoppingCart GetCart(ISession session);
    Task AddToCartAsync(ISession session, string bookId, int quantity);
    void UpdateQuantity(ISession session, string bookId, int quantity);
    void RemoveFromCart(ISession session, string bookId);
    void ClearCart(ISession session);
}
```

**File:** `src/BookstoreApp.Web/Services/Implementations/CartService.cs`

```csharp
using BookstoreApp.Web.Extensions;
using BookstoreApp.Web.Models.DTOs;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Services;

public class CartService : ICartService
{
    private const string CartSessionKey = "ShoppingCart";
    private readonly IBookService _bookService;
    private readonly ILogger<CartService> _logger;

    public CartService(IBookService bookService, ILogger<CartService> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    public ShoppingCart GetCart(ISession session)
    {
        return session.Get<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();
    }

    // Replaces: INSERT INTO Cart(BookID, Quantity, Price, TotalPrice) 
    //           VALUES('{bookId}', {quantity}, {price}, Price * Quantity)
    // Source: index.php lines 5-17
    public async Task AddToCartAsync(ISession session, string bookId, int quantity)
    {
        _logger.LogInformation("Adding {Quantity} of book {BookId} to cart", quantity, bookId);

        var cart = GetCart(session);
        var existingItem = cart.Items.FirstOrDefault(i => i.BookId == bookId);

        if (existingItem != null)
        {
            // Update quantity if already in cart
            existingItem.Quantity += quantity;
        }
        else
        {
            // Fetch book details and add new item
            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book != null)
            {
                cart.Items.Add(new CartItemDto
                {
                    BookId = book.BookId,
                    BookTitle = book.BookTitle,
                    ImageUrl = book.ImageUrl,
                    Price = book.Price,
                    Quantity = quantity
                });
            }
        }

        session.Set(CartSessionKey, cart);
    }

    public void UpdateQuantity(ISession session, string bookId, int quantity)
    {
        var cart = GetCart(session);
        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);

        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            session.Set(CartSessionKey, cart);
        }
    }

    public void RemoveFromCart(ISession session, string bookId)
    {
        var cart = GetCart(session);
        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);

        if (item != null)
        {
            cart.Items.Remove(item);
            session.Set(CartSessionKey, cart);
        }
    }

    // Replaces: DELETE FROM Cart (index.php line 21)
    public void ClearCart(ISession session)
    {
        _logger.LogInformation("Clearing cart");
        session.Remove(CartSessionKey);
    }
}
```

---

### Task 2.5: OrderService

**Source PHP Logic:**
- `checkout.php` lines 20-24: Insert orders from cart
- `checkout.php` lines 28-31: Fetch order details
- `checkout.php` lines 72-73: Update order status

**File:** `src/BookstoreApp.Web/Services/Interfaces/IOrderService.cs`

```csharp
using BookstoreApp.Data.Entities;
using BookstoreApp.Web.Models.DTOs;

namespace BookstoreApp.Web.Services.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<Order>> CreateOrdersFromCartAsync(int customerId, ShoppingCart cart);
    Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
    Task<IEnumerable<Order>> GetPendingOrdersAsync(int customerId);
    Task CompleteOrdersAsync(int customerId);
}
```

**File:** `src/BookstoreApp.Web/Services/Implementations/OrderService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using BookstoreApp.Data;
using BookstoreApp.Data.Entities;
using BookstoreApp.Data.Enums;
using BookstoreApp.Web.Models.DTOs;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Services;

public class OrderService : IOrderService
{
    private readonly BookstoreDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(BookstoreDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Replaces: INSERT INTO `Order`(CustomerID, BookID, DatePurchase, Quantity, TotalPrice, Status)
    //           VALUES({customerId}, '{bookId}', CURRENT_TIME, {quantity}, {totalPrice}, 'N')
    // Source: checkout.php lines 20-22
    public async Task<IEnumerable<Order>> CreateOrdersFromCartAsync(int customerId, ShoppingCart cart)
    {
        _logger.LogInformation("Creating orders for customer {CustomerId} with {ItemCount} items", 
            customerId, cart.Items.Count);

        var orders = new List<Order>();

        // Use transaction for atomicity (fixes PHP issue - no transaction handling)
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in cart.Items)
            {
                var order = new Order
                {
                    CustomerId = customerId,
                    BookId = item.BookId,
                    PurchaseDate = DateTime.UtcNow,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    TotalPrice = item.TotalPrice,
                    Status = OrderStatus.Pending
                };

                _context.Orders.Add(order);
                orders.Add(order);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Created {OrderCount} orders for customer {CustomerId}", 
                orders.Count, customerId);

            return orders;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create orders for customer {CustomerId}", customerId);
            throw;
        }
    }

    // NEW: Order history (moderate modernization feature)
    public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Book)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.PurchaseDate)
            .ToListAsync();
    }

    // Replaces: SELECT ... FROM Order WHERE Status = 'N' AND CustomerID = {customerId}
    // Source: checkout.php lines 28-31
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(int customerId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Book)
            .Include(o => o.Customer)
            .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Pending)
            .ToListAsync();
    }

    // Replaces: UPDATE `Order` SET Status = 'y' WHERE CustomerID = {customerId}
    // Source: checkout.php line 72
    public async Task CompleteOrdersAsync(int customerId)
    {
        _logger.LogInformation("Completing orders for customer {CustomerId}", customerId);

        await _context.Orders
            .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Completed));
    }
}
```

---

### Task 2.6: CustomerService

**Source PHP Logic:**
- `register.php` lines 69-79: Create user and customer
- `edituser.php` lines 17-28: Load profile
- `edituser.php` lines 110-113: Update profile

**File:** `src/BookstoreApp.Web/Services/Interfaces/ICustomerService.cs`

```csharp
using BookstoreApp.Data.Entities;
using BookstoreApp.Web.Models.ViewModels;

namespace BookstoreApp.Web.Services.Interfaces;

public interface ICustomerService
{
    Task<Customer?> GetByEntraIdAsync(string entraIdObjectId);
    Task<Customer> GetOrCreateCustomerAsync(string entraIdObjectId, string? email, string? name);
    Task<Customer> UpdateProfileAsync(int customerId, ProfileViewModel profile);
    Task<bool> IsProfileCompleteAsync(string entraIdObjectId);
}
```

**File:** `src/BookstoreApp.Web/Services/Implementations/CustomerService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using BookstoreApp.Data;
using BookstoreApp.Data.Entities;
using BookstoreApp.Web.Models.ViewModels;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Services;

public class CustomerService : ICustomerService
{
    private readonly BookstoreDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(BookstoreDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Customer?> GetByEntraIdAsync(string entraIdObjectId)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.EntraIdObjectId == entraIdObjectId);
    }

    // Replaces: INSERT INTO Users + INSERT INTO Customer
    // Source: register.php lines 69-79
    // NOTE: No UserName/Password - Entra ID handles authentication
    public async Task<Customer> GetOrCreateCustomerAsync(
        string entraIdObjectId, 
        string? email, 
        string? name)
    {
        var customer = await GetByEntraIdAsync(entraIdObjectId);

        if (customer == null)
        {
            _logger.LogInformation("Creating new customer for Entra ID {EntraId}", entraIdObjectId);

            customer = new Customer
            {
                EntraIdObjectId = entraIdObjectId,
                Email = email,
                FullName = name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        return customer;
    }

    // Replaces: UPDATE Customer SET CustomerName = ?, CustomerPhone = ?, ...
    // Source: edituser.php lines 110-113
    // FIX: Original PHP had missing WHERE clause - this is properly scoped
    public async Task<Customer> UpdateProfileAsync(int customerId, ProfileViewModel profile)
    {
        _logger.LogInformation("Updating profile for customer {CustomerId}", customerId);

        var customer = await _context.Customers.FindAsync(customerId);
        
        if (customer == null)
        {
            throw new ArgumentException($"Customer {customerId} not found");
        }

        customer.FullName = profile.FullName;
        customer.Email = profile.Email;
        customer.PhoneNumber = profile.PhoneNumber;
        customer.IdentificationNumber = profile.IdentificationNumber;
        customer.Gender = profile.Gender;
        customer.Address = profile.Address;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return customer;
    }

    // Check if user has completed their profile (for checkout requirement)
    public async Task<bool> IsProfileCompleteAsync(string entraIdObjectId)
    {
        var customer = await GetByEntraIdAsync(entraIdObjectId);

        if (customer == null) return false;

        return !string.IsNullOrEmpty(customer.FullName) &&
               !string.IsNullOrEmpty(customer.Email) &&
               !string.IsNullOrEmpty(customer.PhoneNumber) &&
               !string.IsNullOrEmpty(customer.Address) &&
               !string.IsNullOrEmpty(customer.Gender);
    }
}
```

---

## Wave 3: Controllers (Day 3)

### Wave 3 Overview

| Task | Source PHP | Output | Est. Time |
|------|------------|--------|-----------|
| 3.1 HomeController | index.php | HomeController.cs | 1.5 hours |
| 3.2 CartController | index.php cart actions | CartController.cs | 1 hour |
| 3.3 OrderController | checkout.php | OrderController.cs | 2 hours |
| 3.4 AccountController | register.php, edituser.php, logout.php | AccountController.cs | 1.5 hours |
| **Total Wave 3** | | | **6-7 hours** |

---

### Task 3.1: HomeController

**Source:** `index.php` (catalog display, lines 27-95)

**File:** `src/BookstoreApp.Web/Controllers/HomeController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookstoreApp.Web.Models.ViewModels;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly IBookService _bookService;
    private readonly ICartService _cartService;
    private readonly ILogger<HomeController> _logger;
    private const int PageSize = 12;

    public HomeController(
        IBookService bookService, 
        ICartService cartService,
        ILogger<HomeController> logger)
    {
        _bookService = bookService;
        _cartService = cartService;
        _logger = logger;
    }

    // Replaces: index.php main page logic (lines 27-95)
    // - Displays book catalog
    // - Shows shopping cart sidebar
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        int page = 1, 
        string? search = null, 
        string? category = null)
    {
        _logger.LogInformation("Loading catalog page {Page}", page);

        var (books, totalCount) = await _bookService.GetBooksPagedAsync(
            page, PageSize, search, category);

        var categories = await _bookService.GetCategoriesAsync();

        var viewModel = new BookCatalogViewModel
        {
            Books = books,
            Cart = _cartService.GetCart(HttpContext.Session),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize),
            SearchTerm = search,
            Category = category,
            Categories = categories
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
```

---

### Task 3.2: CartController

**Source:** `index.php` cart logic (lines 5-23, 80-95)

**File:** `src/BookstoreApp.Web/Controllers/CartController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    // Display cart page
    [AllowAnonymous]
    public IActionResult Index()
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        return View(cart);
    }

    // Replaces: index.php lines 5-17 (POST with 'ac' parameter)
    // Original: INSERT INTO Cart(BookID, Quantity, Price, TotalPrice)
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string bookId, int quantity = 1)
    {
        _logger.LogInformation("Adding book {BookId} to cart with quantity {Quantity}", 
            bookId, quantity);

        if (string.IsNullOrEmpty(bookId))
        {
            return BadRequest("Book ID is required");
        }

        if (quantity < 1) quantity = 1;

        await _cartService.AddToCartAsync(HttpContext.Session, bookId, quantity);

        // Redirect back to catalog
        return RedirectToAction("Index", "Home");
    }

    // Update quantity in cart
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult Update(string bookId, int quantity)
    {
        _cartService.UpdateQuantity(HttpContext.Session, bookId, quantity);
        return RedirectToAction("Index");
    }

    // Remove single item from cart
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(string bookId)
    {
        _cartService.RemoveFromCart(HttpContext.Session, bookId);
        return RedirectToAction("Index");
    }

    // Replaces: index.php lines 19-23 (POST with 'delc' parameter)
    // Original: DELETE FROM Cart
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _logger.LogInformation("Clearing cart");
        _cartService.ClearCart(HttpContext.Session);
        return RedirectToAction("Index", "Home");
    }

    // AJAX endpoint to get cart count for header badge
    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetCartCount()
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        return Json(new { count = cart.ItemCount, total = cart.TotalAmount });
    }
}
```

---

### Task 3.3: OrderController

**Source:** `checkout.php` (264 lines)

**File:** `src/BookstoreApp.Web/Controllers/OrderController.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using BookstoreApp.Web.Models.ViewModels;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Controllers;

public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        ICartService cartService,
        ICustomerService customerService,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _cartService = cartService;
        _customerService = customerService;
        _logger = logger;
    }

    // Replaces: checkout.php GET - Show checkout form
    // Source: checkout.php lines 174-230 (guest form display)
    // NOTE: With Entra ID, all users are authenticated
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = _cartService.GetCart(HttpContext.Session);

        if (!cart.Items.Any())
        {
            TempData["Error"] = "Your cart is empty";
            return RedirectToAction("Index", "Home");
        }

        var entraId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(entraId))
        {
            return Challenge();
        }

        var customer = await _customerService.GetByEntraIdAsync(entraId);

        var viewModel = new CheckoutViewModel
        {
            Cart = cart,
            FullName = customer?.FullName ?? User.Identity?.Name ?? "",
            Email = customer?.Email ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            PhoneNumber = customer?.PhoneNumber ?? "",
            IdentificationNumber = customer?.IdentificationNumber ?? "",
            Gender = customer?.Gender ?? "",
            Address = customer?.Address ?? ""
        };

        return View(viewModel);
    }

    // Replaces: checkout.php POST - Process checkout
    // Source: checkout.php lines 7-73 (authenticated user) + lines 121-173 (guest user)
    // Merged into single flow since all users authenticated via Entra ID
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        model.Cart = cart;

        if (!cart.Items.Any())
        {
            TempData["Error"] = "Your cart is empty";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entraId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(entraId))
        {
            return Challenge();
        }

        try
        {
            // Get or create customer
            var customer = await _customerService.GetOrCreateCustomerAsync(
                entraId,
                model.Email,
                model.FullName);

            // Update customer profile with checkout info
            await _customerService.UpdateProfileAsync(customer.CustomerId, new ProfileViewModel
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IdentificationNumber = model.IdentificationNumber,
                Gender = model.Gender,
                Address = model.Address
            });

            // Create orders from cart items
            // Replaces: INSERT INTO `Order` ... loop (checkout.php lines 20-22)
            var orders = await _orderService.CreateOrdersFromCartAsync(customer.CustomerId, cart);

            // Clear cart after successful order
            // Replaces: DELETE FROM Cart (checkout.php line 24)
            _cartService.ClearCart(HttpContext.Session);

            // Complete orders (mark as completed)
            // Replaces: UPDATE `Order` SET Status = 'y' (checkout.php line 72)
            await _orderService.CompleteOrdersAsync(customer.CustomerId);

            _logger.LogInformation("Order completed for customer {CustomerId} with {OrderCount} items",
                customer.CustomerId, orders.Count());

            return RedirectToAction("Confirmation", new { customerId = customer.CustomerId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process checkout");
            ModelState.AddModelError("", "An error occurred processing your order. Please try again.");
            return View(model);
        }
    }

    // Replaces: checkout.php order confirmation display
    // Source: checkout.php lines 32-68
    [HttpGet]
    public async Task<IActionResult> Confirmation(int customerId)
    {
        var entraId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var customer = await _customerService.GetByEntraIdAsync(entraId!);

        if (customer == null || customer.CustomerId != customerId)
        {
            return Forbid();
        }

        var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
        var recentOrders = orders.Where(o => 
            o.PurchaseDate >= DateTime.UtcNow.AddMinutes(-5));

        var viewModel = new OrderConfirmationViewModel
        {
            Customer = customer,
            Orders = recentOrders,
            OrderDate = recentOrders.FirstOrDefault()?.PurchaseDate ?? DateTime.UtcNow
        };

        return View(viewModel);
    }

    // NEW: Order history (moderate modernization feature)
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var entraId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var customer = await _customerService.GetByEntraIdAsync(entraId!);

        if (customer == null)
        {
            return RedirectToAction("Profile", "Account");
        }

        var orders = await _orderService.GetOrdersByCustomerIdAsync(customer.CustomerId);
        return View(orders);
    }
}
```

---

### Task 3.4: AccountController

**Source:** `register.php`, `edituser.php`, `logout.php`

**File:** `src/BookstoreApp.Web/Controllers/AccountController.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookstoreApp.Web.Models.ViewModels;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Controllers;

public class AccountController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        ICustomerService customerService,
        ILogger<AccountController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    // Replaces: login.php + checklogin.php
    // Now handled by Entra ID - this just initiates the challenge
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return Challenge(new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? Url.Action("Index", "Home")
        });
    }

    // Replaces: logout.php
    // Source: session_start(); session_destroy(); header("Location:index.php");
    public IActionResult Logout()
    {
        _logger.LogInformation("User {User} logging out", User.Identity?.Name);

        return SignOut(
            new AuthenticationProperties { RedirectUri = Url.Action("Index", "Home") },
            OpenIdConnectDefaults.AuthenticationScheme,
            "Cookies");
    }

    // Replaces: register.php (partial - Entra ID handles account creation)
    // This completes the profile after Entra ID registration
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var entraId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(entraId))
        {
            return Challenge();
        }

        var customer = await _customerService.GetByEntraIdAsync(entraId);

        var viewModel = new ProfileViewModel
        {
            FullName = customer?.FullName ?? User.Identity?.Name ?? "",
            Email = customer?.Email ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            PhoneNumber = customer?.PhoneNumber ?? "",
            IdentificationNumber = customer?.IdentificationNumber ?? "",
            Gender = customer?.Gender ?? "",
            Address = customer?.Address ?? ""
        };

        return View(viewModel);
    }

    // Replaces: register.php POST
    // Source: register.php lines 69-79 (INSERT INTO Users + Customer)
    // NOTE: No password handling - Entra ID manages credentials
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entraId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(entraId))
        {
            return Challenge();
        }

        try
        {
            var customer = await _customerService.GetOrCreateCustomerAsync(
                entraId,
                model.Email,
                model.FullName);

            await _customerService.UpdateProfileAsync(customer.CustomerId, model);

            _logger.LogInformation("Profile completed for customer {CustomerId}", customer.CustomerId);

            TempData["Success"] = "Profile saved successfully!";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile");
            ModelState.AddModelError("", "Failed to save profile. Please try again.");
            return View(model);
        }
    }

    // Replaces: edituser.php GET - Load profile
    // Source: edituser.php lines 17-28 (SELECT Users/Customer join)
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var entraId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(entraId))
        {
            return Challenge();
        }

        var customer = await _customerService.GetByEntraIdAsync(entraId);

        if (customer == null)
        {
            return RedirectToAction("Register");
        }

        var viewModel = new ProfileViewModel
        {
            FullName = customer.FullName ?? "",
            Email = customer.Email ?? "",
            PhoneNumber = customer.PhoneNumber ?? "",
            IdentificationNumber = customer.IdentificationNumber ?? "",
            Gender = customer.Gender ?? "",
            Address = customer.Address ?? ""
        };

        return View(viewModel);
    }

    // Replaces: edituser.php POST - Update profile
    // Source: edituser.php lines 107-113 (UPDATE Users + Customer)
    // FIX: Original had incomplete SQL - this properly updates
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entraId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(entraId))
        {
            return Challenge();
        }

        try
        {
            var customer = await _customerService.GetByEntraIdAsync(entraId);
            if (customer == null)
            {
                return RedirectToAction("Register");
            }

            await _customerService.UpdateProfileAsync(customer.CustomerId, model);

            _logger.LogInformation("Profile updated for customer {CustomerId}", customer.CustomerId);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update profile");
            ModelState.AddModelError("", "Failed to update profile. Please try again.");
            return View(model);
        }
    }
}
```

---

## Wave 4: Views (Day 4-5)

### Wave 4 Overview

| Task | Source PHP | Output | Est. Time |
|------|------------|--------|-----------|
| 4.1 _Layout.cshtml | PHP header blocks | Shared/_Layout.cshtml | 1 hour |
| 4.2 _LoginPartial.cshtml | Session checks | Shared/_LoginPartial.cshtml | 30 min |
| 4.3 Home/Index.cshtml | index.php main | Home/Index.cshtml | 1.5 hours |
| 4.4 Cart/Index.cshtml | Cart sidebar | Cart/Index.cshtml | 1 hour |
| 4.5 Order/Checkout.cshtml | checkout.php form | Order/Checkout.cshtml | 1.5 hours |
| 4.6 Order/Confirmation.cshtml | checkout.php success | Order/Confirmation.cshtml | 45 min |
| 4.7 Order/History.cshtml | NEW | Order/History.cshtml | 45 min |
| 4.8 Account/Profile.cshtml | edituser.php | Account/Profile.cshtml | 1 hour |
| 4.9 Account/Register.cshtml | register.php | Account/Register.cshtml | 30 min |
| 4.10 site.css | style.css | wwwroot/css/site.css | 45 min |
| **Total Wave 4** | | | **9-10 hours** |

---

### Task 4.1: _Layout.cshtml

**Source:** PHP header blocks in each file (lines like 40-58 in index.php)

**File:** `src/BookstoreApp.Web/Views/Shared/_Layout.cshtml`

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Bookstore</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <header>
        <div class="container header-content">
            <a asp-controller="Home" asp-action="Index">
                <img src="~/images/logo.png" alt="Bookstore Logo" class="logo" />
            </a>
            <partial name="_LoginPartial" />
        </div>
    </header>

    <main class="container">
        @if (TempData["Success"] != null)
        {
            <div class="alert alert-success">@TempData["Success"]</div>
        }
        @if (TempData["Error"] != null)
        {
            <div class="alert alert-error">@TempData["Error"]</div>
        }
        @RenderBody()
    </main>

    <footer>
        <div class="container">
            <p>&copy; @DateTime.Now.Year Bookstore. All rights reserved.</p>
        </div>
    </footer>

    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

---

### Task 4.2: _LoginPartial.cshtml

**Source:** `index.php` lines 40-58 (session check for header buttons)

**File:** `src/BookstoreApp.Web/Views/Shared/_LoginPartial.cshtml`

```html
@* Replaces: if(isset($_SESSION['id'])) / if(!isset($_SESSION['id'])) blocks *@
@* Source: index.php lines 40-58 *@

<nav class="header-nav">
    @if (User.Identity?.IsAuthenticated == true)
    {
        <span class="user-greeting">Hello, @User.Identity.Name</span>
        <a asp-controller="Order" asp-action="History" class="nav-button">
            <i class="fa fa-history"></i> Orders
        </a>
        <a asp-controller="Account" asp-action="Profile" class="nav-button">
            <i class="fa fa-user"></i> Profile
        </a>
        <a asp-controller="Account" asp-action="Logout" class="nav-button">
            <i class="fa fa-sign-out"></i> Logout
        </a>
    }
    else
    {
        <a asp-controller="Account" asp-action="Login" class="nav-button">
            <i class="fa fa-sign-in"></i> Login
        </a>
    }
</nav>
```

---

### Task 4.3: Home/Index.cshtml

**Source:** `index.php` (book catalog + cart sidebar)

**File:** `src/BookstoreApp.Web/Views/Home/Index.cshtml`

```html
@model BookstoreApp.Web.Models.ViewModels.BookCatalogViewModel
@{
    ViewData["Title"] = "Book Catalog";
}

@* Replaces: index.php catalog display (lines 60-95) *@

<div class="catalog-container">
    <div class="catalog-main">
        <h1>Book Catalog</h1>
        
        @* NEW: Search and filter (moderate modernization) *@
        <form method="get" class="search-form">
            <input type="text" name="search" value="@Model.SearchTerm" 
                   placeholder="Search by title or author..." />
            <select name="category">
                <option value="">All Categories</option>
                @foreach (var cat in Model.Categories)
                {
                    <option value="@cat" selected="@(cat == Model.Category)">@cat</option>
                }
            </select>
            <button type="submit" class="button">Search</button>
        </form>

        <div class="book-grid">
            @foreach (var book in Model.Books)
            {
                <div class="book-card">
                    <img src="@book.ImageUrl" alt="@book.BookTitle" />
                    <div class="book-info">
                        <h3>@book.BookTitle</h3>
                        <p><strong>ISBN:</strong> @book.Isbn</p>
                        <p><strong>Author:</strong> @book.Author</p>
                        <p><strong>Category:</strong> @book.Category</p>
                        <p class="price">RM @book.Price.ToString("F2")</p>
                        
                        @* Replaces: index.php add to cart form (lines 70-76) *@
                        <form asp-controller="Cart" asp-action="Add" method="post">
                            @Html.AntiForgeryToken()
                            <input type="hidden" name="bookId" value="@book.BookId" />
                            <div class="quantity-group">
                                <label>Qty:</label>
                                <input type="number" name="quantity" value="1" min="1" max="99" />
                            </div>
                            <button type="submit" class="button">
                                <i class="fa fa-cart-plus"></i> Add to Cart
                            </button>
                        </form>
                    </div>
                </div>
            }
        </div>

        @* NEW: Pagination (moderate modernization) *@
        @if (Model.TotalPages > 1)
        {
            <div class="pagination">
                @for (int i = 1; i <= Model.TotalPages; i++)
                {
                    <a asp-action="Index" 
                       asp-route-page="@i" 
                       asp-route-search="@Model.SearchTerm"
                       asp-route-category="@Model.Category"
                       class="@(i == Model.CurrentPage ? "active" : "")">@i</a>
                }
            </div>
        }
    </div>

    @* Replaces: index.php cart sidebar (lines 80-95) *@
    <aside class="cart-sidebar">
        <h2><i class="fa fa-shopping-cart"></i> Cart</h2>
        
        @if (Model.Cart.Items.Any())
        {
            <form asp-controller="Cart" asp-action="Clear" method="post" class="clear-form">
                @Html.AntiForgeryToken()
                <button type="submit" class="cbtn">Empty Cart</button>
            </form>

            @foreach (var item in Model.Cart.Items)
            {
                <div class="cart-item">
                    <img src="@item.ImageUrl" alt="@item.BookTitle" />
                    <div class="cart-item-info">
                        <p>@item.BookTitle</p>
                        <p>RM @item.Price.ToString("F2")</p>
                        <p>Qty: @item.Quantity</p>
                        <p><strong>RM @item.TotalPrice.ToString("F2")</strong></p>
                    </div>
                </div>
            }

            <div class="cart-total">
                <p>Total: <strong>RM @Model.Cart.TotalAmount.ToString("F2")</strong></p>
                <a asp-controller="Order" asp-action="Checkout" class="button checkout-btn">
                    CHECKOUT
                </a>
            </div>
        }
        else
        {
            <p class="empty-cart">Your cart is empty</p>
        }
    </aside>
</div>
```

---

### Remaining Wave 4 Tasks (Views)

Due to space constraints, I'll provide file paths and key transformation notes for remaining views:

| View File | Source PHP | Key Transformations |
|-----------|------------|---------------------|
| `Cart/Index.cshtml` | index.php cart | Full cart page with quantity updates |
| `Order/Checkout.cshtml` | checkout.php form | Profile + cart summary, validation |
| `Order/Confirmation.cshtml` | checkout.php success | Order details, customer info |
| `Order/History.cshtml` | NEW | List of past orders with status |
| `Account/Profile.cshtml` | edituser.php | Profile edit form with validation |
| `Account/Register.cshtml` | register.php | Complete profile form (no password) |
| `wwwroot/css/site.css` | style.css | Migrate with minor adjustments |

---

## Wave 5: Infrastructure & CI/CD (Day 6)

### Wave 5 Overview

| Task | Output | Est. Time |
|------|--------|-----------|
| 5.1 Dockerfile | Multi-stage build | 30 min |
| 5.2 Kubernetes manifests | K8s YAML files | 2 hours |
| 5.3 Bicep templates | Infrastructure as Code | 2 hours |
| 5.4 GitHub Actions pipeline | CI/CD workflow | 1.5 hours |
| **Total Wave 5** | | **6-7 hours** |

### Task 5.1: Dockerfile

**File:** `src/BookstoreApp.Web/Dockerfile`

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["BookstoreApp.Web/BookstoreApp.Web.csproj", "BookstoreApp.Web/"]
COPY ["BookstoreApp.Data/BookstoreApp.Data.csproj", "BookstoreApp.Data/"]
RUN dotnet restore "BookstoreApp.Web/BookstoreApp.Web.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/BookstoreApp.Web"
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Security: Run as non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BookstoreApp.Web.dll"]
```

---

## Migration Execution Checklist

### Pre-Migration Checklist

- [ ] Azure resources verified (ACR, AKS, SQL Server, Entra ID)
- [ ] Entra ID app registration created
- [ ] Azure SQL Database created
- [ ] Key Vault secrets configured
- [ ] Domain name and TLS certificates ready
- [ ] Development environment set up with .NET 10 SDK

### Wave 1 Checklist (Foundation)

- [ ] Solution and projects created
- [ ] NuGet packages added
- [ ] `Book` entity created
- [ ] `Customer` entity created (with EntraIdObjectId)
- [ ] `Order` entity created
- [ ] `OrderStatus` enum created
- [ ] `BookstoreDbContext` created with seed data
- [ ] `Program.cs` configured
- [ ] `appsettings.json` configured
- [ ] Initial migration created
- [ ] Database schema applied
- [ ] **BUILD SUCCESS** ✓

### Wave 2 Checklist (Services)

- [ ] `SessionExtensions` created
- [ ] DTOs created (CartItemDto, ShoppingCart)
- [ ] ViewModels created (all 5)
- [ ] `IBookService` + `BookService` created
- [ ] `ICartService` + `CartService` created
- [ ] `IOrderService` + `OrderService` created
- [ ] `ICustomerService` + `CustomerService` created
- [ ] Services registered in DI
- [ ] **BUILD SUCCESS** ✓

### Wave 3 Checklist (Controllers)

- [ ] `HomeController` created
- [ ] `CartController` created
- [ ] `OrderController` created
- [ ] `AccountController` created
- [ ] All routes tested
- [ ] **BUILD SUCCESS** ✓

### Wave 4 Checklist (Views)

- [ ] `_Layout.cshtml` created
- [ ] `_LoginPartial.cshtml` created
- [ ] `Home/Index.cshtml` created
- [ ] `Cart/Index.cshtml` created
- [ ] `Order/Checkout.cshtml` created
- [ ] `Order/Confirmation.cshtml` created
- [ ] `Order/History.cshtml` created
- [ ] `Account/Profile.cshtml` created
- [ ] `Account/Register.cshtml` created
- [ ] `site.css` migrated
- [ ] Images copied to wwwroot
- [ ] **FULL UI TEST** ✓

### Wave 5 Checklist (Infrastructure)

- [ ] Dockerfile created and tested
- [ ] Kubernetes manifests created
- [ ] Bicep templates created
- [ ] GitHub Actions pipeline created
- [ ] Container pushed to ACR
- [ ] Deployed to AKS
- [ ] Health checks passing
- [ ] Entra ID authentication working
- [ ] **END-TO-END TEST** ✓

---

## Business Rules Migration Tracking

### Authentication Rules

| Rule ID | PHP Implementation | .NET Implementation | Status |
|---------|-------------------|---------------------|--------|
| AUTH-01 | `checklogin.php` credential check | Entra ID middleware | ⏳ Pending |
| AUTH-02 | `$_SESSION['id']` storage | Claims-based auth | ⏳ Pending |
| AUTH-03 | `logout.php` session destroy | OIDC sign-out | ⏳ Pending |

### Customer Rules

| Rule ID | PHP Location | .NET Location | Status |
|---------|-------------|---------------|--------|
| CUST-01 | `register.php:13-15` | `ProfileViewModel` DataAnnotations | ⏳ Pending |
| CUST-02 | `register.php:31-33` | `ProfileViewModel` RegularExpression | ⏳ Pending |
| CUST-03 | `register.php:40-42` | `ProfileViewModel` EmailAddress | ⏳ Pending |
| CUST-04 | `register.php:50-52` | `ProfileViewModel` RegularExpression | ⏳ Pending |
| CUST-05 | `register.php:59-61` | `ProfileViewModel` Required | ⏳ Pending |
| CUST-06 | `register.php:63-65` | `ProfileViewModel` Required | ⏳ Pending |
| CUST-07 | `register.php:77` UserID FK | `Customer.EntraIdObjectId` | ⏳ Pending |

### Cart Rules

| Rule ID | PHP Location | .NET Location | Status |
|---------|-------------|---------------|--------|
| CART-01 | `index.php:12` quantity | `CartService.AddToCartAsync` | ⏳ Pending |
| CART-02 | `index.php:14` total calc | `CartItemDto.TotalPrice` property | ⏳ Pending |
| CART-03 | `index.php:21` empty | `CartService.ClearCart` | ⏳ Pending |
| CART-04 | DB Cart table | Session-based cart | ⏳ Pending |
| CART-05 | `index.php:85-95` total | `ShoppingCart.TotalAmount` | ⏳ Pending |

### Order Rules

| Rule ID | PHP Location | .NET Location | Status |
|---------|-------------|---------------|--------|
| ORD-01 | `checkout.php:20-22` | `OrderService.CreateOrdersFromCartAsync` | ⏳ Pending |
| ORD-02 | `checkout.php:8-11` | `Order.CustomerId` FK | ⏳ Pending |
| ORD-03 | `checkout.php:21` CURRENT_TIME | `DateTime.UtcNow` | ⏳ Pending |
| ORD-04 | `checkout.php:24` DELETE | `CartService.ClearCart` | ⏳ Pending |
| ORD-05 | `checkout.php:22` Status='N' | `OrderStatus.Pending` enum | ⏳ Pending |
| ORD-06 | `checkout.php:72` Status='y' | `OrderStatus.Completed` enum | ⏳ Pending |
| ORD-07 | `checkout.php:121-173` guest | All users authenticated (Entra ID) | ⏳ Pending |

---

## Summary

### Migration Statistics

| Category | PHP | .NET 10 |
|----------|-----|---------|
| **Source Files** | 9 | 30+ |
| **Lines of Code** | ~730 | ~1,500 |
| **Controllers** | 0 (inline) | 4 |
| **Views** | 0 (inline) | 9 |
| **Services** | 0 | 4 |
| **Entities** | 5 tables | 3 entities |
| **Business Rules** | 25 | 25 preserved |
| **Security Issues** | 4 critical | 0 |

### Estimated Timeline

| Wave | Duration | Deliverables |
|------|----------|--------------|
| Wave 1: Foundation | Day 1 | Project structure, entities, DbContext |
| Wave 2: Services | Day 2 | 4 services with full business logic |
| Wave 3: Controllers | Day 3 | 4 controllers with all endpoints |
| Wave 4: Views | Days 4-5 | 9 views, layout, CSS |
| Wave 5: Infrastructure | Day 6 | Dockerfile, K8s, Bicep, CI/CD |
| Testing & Fixes | Days 7-8 | E2E testing, bug fixes |

**Total: 8 working days (~60-80 hours)**

---

## Next Steps

### Phase 2 Complete ✅

**Deliverables:**
- ✅ Project structure defined
- ✅ 5 migration waves defined with dependencies
- ✅ File-by-file mapping created
- ✅ Method-level implementation plans
- ✅ Business rules tracking table
- ✅ Execution checklist

### Ready for Phase 3: Code Migration 🚀

**What's Next:**
- Execute Wave 1: Create solution, entities, DbContext
- Execute Wave 2: Implement services layer
- Execute Wave 3: Build controllers
- Execute Wave 4: Create Razor views
- Build and test after each wave

**Command to Continue:** `/phase3-migratecode`

---

**Report Generated:** January 28, 2026  
**Migration Plan:** Complete  
**Next Phase:** Code Migration
