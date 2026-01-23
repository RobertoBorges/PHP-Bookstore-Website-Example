# Detailed File-by-File Migration Plan

**Application**: PHP Bookstore Website  
**Generated**: January 22, 2026 12:00 PM  
**Source**: PHP 7.4 (Vanilla, No Framework)  
**Target**: .NET 10 / ASP.NET Core MVC  
**Target Database**: Azure SQL Database  
**Authentication**: Microsoft Entra ID  
**Hosting**: Azure Kubernetes Service (AKS)  

---

## Executive Summary

This document provides a comprehensive, file-by-file migration plan for transforming the PHP Bookstore Website into a modern .NET 10 ASP.NET Core MVC application deployed on Azure Kubernetes Service with Entra ID authentication.

### Migration Statistics

| Metric | Count | Estimated Effort |
|--------|-------|------------------|
| **PHP Files to Migrate** | 9 files | - |
| **Controllers to Create** | 4 controllers | 24 hours |
| **Services to Create** | 3 services | 10 hours |
| **Entity Models** | 5 entities | 6 hours |
| **Razor Views** | 6 views | 16 hours |
| **Database Tables** | 5 tables | 8 hours |
| **Business Rules to Preserve** | 12 rules | - |
| **Migration Waves** | 6 waves | - |
| **Total Estimated Effort** | - | **227 hours** |

### Migration Approach

- **Strategy**: Wave-based migration following dependency order
- **Testing**: Build and validate after each wave
- **Business Logic**: Extract from pages into services
- **Authentication**: Complete redesign to Entra ID
- **Database**: Migrate schema and data to Azure SQL

---

## Target .NET 10 Project Structure

```
Bookstore/
├── Bookstore.sln
├── src/
│   └── Bookstore.Web/
│       ├── Bookstore.Web.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       │
│       ├── Controllers/
│       │   ├── HomeController.cs
│       │   ├── CartController.cs
│       │   ├── OrderController.cs
│       │   └── AccountController.cs
│       │
│       ├── Models/
│       │   ├── Entities/
│       │   │   ├── Book.cs
│       │   │   ├── User.cs
│       │   │   ├── Customer.cs
│       │   │   ├── Order.cs
│       │   │   └── Cart.cs
│       │   ├── ViewModels/
│       │   │   ├── BookCatalogViewModel.cs
│       │   │   ├── CartViewModel.cs
│       │   │   ├── CheckoutViewModel.cs
│       │   │   └── ProfileViewModel.cs
│       │   └── DTOs/
│       │       ├── AddToCartDto.cs
│       │       ├── CheckoutDto.cs
│       │       └── UpdateProfileDto.cs
│       │
│       ├── Services/
│       │   ├── Interfaces/
│       │   │   ├── IBookService.cs
│       │   │   ├── ICartService.cs
│       │   │   └── IOrderService.cs
│       │   └── Implementations/
│       │       ├── BookService.cs
│       │       ├── CartService.cs
│       │       └── OrderService.cs
│       │
│       ├── Data/
│       │   ├── BookstoreDbContext.cs
│       │   ├── DbInitializer.cs
│       │   └── Migrations/
│       │
│       ├── Views/
│       │   ├── _ViewStart.cshtml
│       │   ├── _ViewImports.cshtml
│       │   ├── Shared/
│       │   │   ├── _Layout.cshtml
│       │   │   ├── _Header.cshtml
│       │   │   ├── Error.cshtml
│       │   │   └── _ValidationScriptsPartial.cshtml
│       │   ├── Home/
│       │   │   └── Index.cshtml
│       │   ├── Cart/
│       │   │   └── Index.cshtml
│       │   ├── Order/
│       │   │   ├── Checkout.cshtml
│       │   │   └── Confirmation.cshtml
│       │   └── Account/
│       │       ├── Login.cshtml
│       │       ├── Profile.cshtml
│       │       └── AccessDenied.cshtml
│       │
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── site.css
│       │   ├── js/
│       │   │   └── site.js
│       │   └── images/
│       │       ├── logo.png
│       │       ├── travel.jpg
│       │       ├── food.jpg
│       │       ├── technical.jpg
│       │       └── technology.jpg
│       │
│       └── Infrastructure/
│           ├── Extensions/
│           │   └── ServiceCollectionExtensions.cs
│           └── Validators/
│               ├── CheckoutDtoValidator.cs
│               └── UpdateProfileDtoValidator.cs
│
├── tests/
│   └── Bookstore.Tests/
│       ├── Services/
│       ├── Controllers/
│       └── Integration/
│
├── infrastructure/
│   ├── bicep/
│   │   ├── main.bicep
│   │   ├── aks.bicep
│   │   ├── sql.bicep
│   │   └── keyvault.bicep
│   └── kubernetes/
│       ├── deployment.yaml
│       ├── service.yaml
│       ├── ingress.yaml
│       ├── configmap.yaml
│       ├── secret.yaml
│       └── hpa.yaml
│
├── Dockerfile
├── .dockerignore
├── azure.yaml
└── README.md
```

---

## File-by-File Migration Plan

## 1. DATABASE LAYER

### 1.1 Entity: Book

| Property | Value |
|----------|-------|
| **Source File** | `bookstore/database.sql` (Book table) |
| **Target File** | `src/Bookstore.Web/Models/Entities/Book.cs` |
| **Database Table** | `Book` → `Books` |
| **Primary Key** | `BookID` (varchar) → `BookId` (string) |
| **Purpose** | Represent book catalog items |

#### MySQL Schema
```sql
CREATE TABLE Book(
    BookID varchar(50),
    BookTitle varchar(200),
    ISBN varchar(20),
    Price double(12,2),
    Author varchar(128),
    Type varchar(128),
    Image varchar(128),
    PRIMARY KEY (BookID)
);
```

#### Properties Mapping

| MySQL Column | MySQL Type | C# Property | C# Type | Annotations | Notes |
|--------------|-----------|-------------|---------|-------------|-------|
| `BookID` | `varchar(50)` | `BookId` | `string` | `[Key]`, `[MaxLength(50)]` | Primary key |
| `BookTitle` | `varchar(200)` | `Title` | `string` | `[Required]`, `[MaxLength(200)]` | Required field |
| `ISBN` | `varchar(20)` | `ISBN` | `string` | `[MaxLength(20)]` | Optional |
| `Price` | `double(12,2)` | `Price` | `decimal` | `[Column(TypeName = "decimal(12,2)")]` | Money type |
| `Author` | `varchar(128)` | `Author` | `string` | `[Required]`, `[MaxLength(128)]` | Required field |
| `Type` | `varchar(128)` | `Type` | `string` | `[MaxLength(128)]` | Category/genre |
| `Image` | `varchar(128)` | `ImagePath` | `string` | `[MaxLength(256)]` | Relative path |

#### Relationships

| Relationship | Type | Related Entity | Navigation Property | Notes |
|--------------|------|----------------|---------------------|-------|
| Cart items | One-to-Many | `Cart` | `ICollection<Cart> CartItems` | Books in shopping carts |
| Orders | One-to-Many | `Order` | `ICollection<Order> Orders` | Books in orders |

#### C# Entity Code Template

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Web.Models.Entities
{
    public class Book
    {
        [Key]
        [MaxLength(50)]
        public string BookId { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(20)]
        public string? ISBN { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(128)]
        public string Author { get; set; } = null!;

        [MaxLength(128)]
        public string? Type { get; set; }

        [MaxLength(256)]
        public string? ImagePath { get; set; }

        // Navigation properties
        public ICollection<Cart> CartItems { get; set; } = new List<Cart>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
```

#### Migration Complexity: **Low**
#### Estimated Effort: **1 hour**

---

### 1.2 Entity: User

| Property | Value |
|----------|-------|
| **Source File** | `bookstore/database.sql` (Users table) |
| **Target File** | `src/Bookstore.Web/Models/Entities/User.cs` |
| **Database Table** | `Users` → `Users` |
| **Primary Key** | `UserID` (int auto_increment) → `UserId` (int identity) |
| **Purpose** | Store user authentication data (modified for Entra ID) |

#### MySQL Schema (Original)
```sql
CREATE TABLE Users(
    UserID int not null AUTO_INCREMENT,
    UserName varchar(128),
    Password varchar(16),
    PRIMARY KEY (UserID)
);
```

#### **CRITICAL CHANGE**: Modified for Entra ID

The `Password` field will be **removed** since Entra ID handles authentication. We'll add `AzureAdObjectId` to link to Entra ID users.

#### Properties Mapping

| MySQL Column | MySQL Type | C# Property | C# Type | Annotations | Notes |
|--------------|-----------|-------------|---------|-------------|-------|
| `UserID` | `int` AUTO_INCREMENT | `UserId` | `int` | `[Key]` | Identity column |
| - | - | `AzureAdObjectId` | `Guid` | `[Required]` | **NEW**: Link to Entra ID |
| - | - | `Email` | `string` | `[Required]`, `[MaxLength(256)]` | **NEW**: From Entra ID |
| `UserName` | `varchar(128)` | `UserName` | `string` | `[MaxLength(128)]` | **Legacy**: Keep for migration |
| `Password` | `varchar(16)` | **REMOVED** | - | - | **Deleted**: No passwords stored |
| - | - | `CreatedAt` | `DateTimeOffset` | `[Required]` | **NEW**: Audit field |
| - | - | `LastLoginAt` | `DateTimeOffset?` | - | **NEW**: Track activity |

#### Relationships

| Relationship | Type | Related Entity | Navigation Property | Notes |
|--------------|------|----------------|---------------------|-------|
| Customer profile | One-to-One | `Customer` | `Customer? Customer` | Optional customer profile |

#### C# Entity Code Template

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Web.Models.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public Guid AzureAdObjectId { get; set; }

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [MaxLength(128)]
        public string? UserName { get; set; } // Legacy field

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? LastLoginAt { get; set; }

        // Navigation property
        public Customer? Customer { get; set; }
    }
}
```

#### Business Logic Notes

**Authentication Flow Change:**
- **PHP**: Username/password lookup in database
- **.NET**: Entra ID OAuth2 flow, token validation
- **User Creation**: On first login, create User record from Entra ID claims

#### Migration Complexity: **High** (Authentication redesign)
#### Estimated Effort: **3 hours**

---

### 1.3 Entity: Customer

| Property | Value |
|----------|-------|
| **Source File** | `bookstore/database.sql` (Customer table) |
| **Target File** | `src/Bookstore.Web/Models/Entities/Customer.cs` |
| **Database Table** | `Customer` → `Customers` |
| **Primary Key** | `CustomerID` (int auto_increment) → `CustomerId` (int identity) |
| **Purpose** | Store customer profile information |

#### MySQL Schema
```sql
CREATE TABLE Customer (
    CustomerID int not null AUTO_INCREMENT,
    CustomerName varchar(128),
    CustomerPhone varchar(12),
    CustomerIC varchar(14),
    CustomerEmail varchar(200),
    CustomerAddress varchar(200),
    CustomerGender varchar(10),
    UserID int,
    PRIMARY KEY (CustomerID),
    CONSTRAINT FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE SET NULL ON UPDATE CASCADE
);
```

#### Properties Mapping

| MySQL Column | MySQL Type | C# Property | C# Type | Annotations | Notes |
|--------------|-----------|-------------|---------|-------------|-------|
| `CustomerID` | `int` | `CustomerId` | `int` | `[Key]` | Identity |
| `CustomerName` | `varchar(128)` | `Name` | `string` | `[Required]`, `[MaxLength(128)]` | Full name |
| `CustomerPhone` | `varchar(12)` | `Phone` | `string` | `[Phone]`, `[MaxLength(20)]` | Validation |
| `CustomerIC` | `varchar(14)` | `IdNumber` | `string` | `[MaxLength(20)]` | ID/IC number |
| `CustomerEmail` | `varchar(200)` | `Email` | `string` | `[Required]`, `[EmailAddress]`, `[MaxLength(256)]` | Email |
| `CustomerAddress` | `varchar(200)` | `Address` | `string` | `[MaxLength(500)]` | Address |
| `CustomerGender` | `varchar(10)` | `Gender` | `string` | `[MaxLength(10)]` | Male/Female |
| `UserID` | `int` | `UserId` | `int?` | - | Foreign key (nullable) |

#### Relationships

| Relationship | Type | Related Entity | Navigation Property | Foreign Key | Notes |
|--------------|------|----------------|---------------------|-------------|-------|
| User account | Many-to-One | `User` | `User? User` | `UserId` | Optional |
| Cart items | One-to-Many | `Cart` | `ICollection<Cart> CartItems` | - | Shopping cart |
| Orders | One-to-Many | `Order` | `ICollection<Order> Orders` | - | Order history |

#### C# Entity Code Template

```csharp
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Web.Models.Entities
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = null!;

        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(20)]
        public string? IdNumber { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = null!;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        // Foreign key
        public int? UserId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
        
        public ICollection<Cart> CartItems { get; set; } = new List<Cart>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
```

#### Business Logic Notes

- **Guest Checkout**: Customer can exist without UserId (guest purchases)
- **Registered Users**: Customer linked to User via UserId
- **Email Sync**: If User exists, Customer.Email should match User.Email

#### Migration Complexity: **Low**
#### Estimated Effort: **1 hour**

---

### 1.4 Entity: Order

| Property | Value |
|----------|-------|
| **Source File** | `bookstore/database.sql` (Order table) |
| **Target File** | `src/Bookstore.Web/Models/Entities/Order.cs` |
| **Database Table** | `` Order`` → `Orders` (renamed to avoid keyword) |
| **Primary Key** | `OrderID` (int auto_increment) → `OrderId` (int identity) |
| **Purpose** | Store order/purchase records |

#### MySQL Schema
```sql
CREATE TABLE `Order`(
    OrderID int not null AUTO_INCREMENT,
    CustomerID int,
    BookID varchar(50),
    DatePurchase datetime,
    Quantity int,
    TotalPrice double(12,2),
    Status varchar(1),
    PRIMARY KEY (OrderID),
    CONSTRAINT FOREIGN KEY (BookID) REFERENCES Book(BookID) ON DELETE SET NULL ON UPDATE CASCADE,
    CONSTRAINT FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID) ON DELETE SET NULL ON UPDATE CASCADE
);
```

#### Properties Mapping

| MySQL Column | MySQL Type | C# Property | C# Type | Annotations | Notes |
|--------------|-----------|-------------|---------|-------------|-------|
| `OrderID` | `int` | `OrderId` | `int` | `[Key]` | Identity |
| `CustomerID` | `int` | `CustomerId` | `int?` | - | Foreign key (nullable) |
| `BookID` | `varchar(50)` | `BookId` | `string` | `[MaxLength(50)]` | Foreign key (nullable) |
| `DatePurchase` | `datetime` | `PurchaseDate` | `DateTimeOffset` | `[Required]` | Order timestamp |
| `Quantity` | `int` | `Quantity` | `int` | `[Range(1, 999)]` | Validation |
| `TotalPrice` | `double(12,2)` | `TotalPrice` | `decimal` | `[Column(TypeName = "decimal(12,2)")]` | Calculated |
| `Status` | `varchar(1)` | `Status` | `string` | `[MaxLength(1)]` | 'N' = pending, 'y' = completed |

#### Relationships

| Relationship | Type | Related Entity | Navigation Property | Foreign Key | Notes |
|--------------|------|----------------|---------------------|-------------|-------|
| Customer | Many-to-One | `Customer` | `Customer? Customer` | `CustomerId` | Who ordered |
| Book | Many-to-One | `Book` | `Book? Book` | `BookId` | What was ordered |

#### C# Entity Code Template

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Web.Models.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }

        [MaxLength(50)]
        public string? BookId { get; set; }

        [Required]
        public DateTimeOffset PurchaseDate { get; set; } = DateTimeOffset.UtcNow;

        [Range(1, 999)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalPrice { get; set; }

        [MaxLength(1)]
        public string Status { get; set; } = "N"; // N = pending, y = completed

        // Navigation properties
        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }
    }
}
```

#### Business Logic Notes

**Order Status Values:**
- `'N'` = Pending/New order
- `'y'` = Completed/Confirmed order

**Total Price Calculation:**
- PHP: `INSERT ... VALUES(..., Price * Quantity)`
- .NET: Calculate in service before saving: `order.TotalPrice = book.Price * quantity`

#### Migration Complexity: **Low**
#### Estimated Effort: **1 hour**

---

### 1.5 Entity: Cart

| Property | Value |
|----------|-------|
| **Source File** | `bookstore/database.sql` (Cart table) |
| **Target File** | `src/Bookstore.Web/Models/Entities/Cart.cs` |
| **Database Table** | `Cart` → `Carts` |
| **Primary Key** | `CartID` (int auto_increment) → `CartId` (int identity) |
| **Purpose** | Store shopping cart items (session-based or user-based) |

#### MySQL Schema
```sql
CREATE TABLE Cart(
    CartID int not null AUTO_INCREMENT,
    CustomerID int,
    BookID varchar(50),
    Price double(12,2),
    Quantity int,
    TotalPrice double(12,2),
    PRIMARY KEY (CartID),
    CONSTRAINT FOREIGN KEY (BookID) REFERENCES Book(BookID) ON DELETE SET NULL ON UPDATE CASCADE,
    CONSTRAINT FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID) ON DELETE SET NULL ON UPDATE CASCADE
);
```

#### Properties Mapping

| MySQL Column | MySQL Type | C# Property | C# Type | Annotations | Notes |
|--------------|-----------|-------------|---------|-------------|-------|
| `CartID` | `int` | `CartId` | `int` | `[Key]` | Identity |
| `CustomerID` | `int` | `CustomerId` | `int?` | - | Foreign key (nullable for guest) |
| `BookID` | `varchar(50)` | `BookId` | `string` | `[MaxLength(50)]` | Foreign key |
| `Price` | `double(12,2)` | `Price` | `decimal` | `[Column(TypeName = "decimal(12,2)")]` | Unit price at time of add |
| `Quantity` | `int` | `Quantity` | `int` | `[Range(1, 999)]` | Item quantity |
| `TotalPrice` | `double(12,2)` | `TotalPrice` | `decimal` | `[Column(TypeName = "decimal(12,2)")]` | Price * Quantity |

#### Relationships

| Relationship | Type | Related Entity | Navigation Property | Foreign Key | Notes |
|--------------|------|----------------|---------------------|-------------|-------|
| Customer | Many-to-One | `Customer` | `Customer? Customer` | `CustomerId` | Nullable for guest carts |
| Book | Many-to-One | `Book` | `Book? Book` | `BookId` | The book in cart |

#### C# Entity Code Template

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Web.Models.Entities
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public int? CustomerId { get; set; }

        [MaxLength(50)]
        public string? BookId { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Price { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalPrice { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }
    }
}
```

#### **CRITICAL**: Cart State Management Change

| PHP Approach | .NET Approach | Notes |
|--------------|---------------|-------|
| All carts in database with NULL CustomerId | **Option A**: Database (same as PHP) | Requires cleanup job |
| Session stores cart temporarily | **Option B**: ASP.NET Core Session with Redis | More scalable |
| Cart associated on checkout | **Option C**: Distributed cache | Best for AKS |

**Recommended for AKS**: Use session-based cart (in-memory or Redis), persist to database only on checkout.

#### Migration Complexity: **Medium** (Session strategy change)
#### Estimated Effort: **2 hours**

---

### 1.6 DbContext: BookstoreDbContext

| Property | Value |
|----------|-------|
| **Source Files** | `connectDB.php`, `db_helper.php` |
| **Target File** | `src/Bookstore.Web/Data/BookstoreDbContext.cs` |
| **Purpose** | EF Core database context for all entities |
| **Connection** | Azure SQL Database with Managed Identity |

#### PHP Database Connection (Current)

**connectDB.php (PDO)**:
```php
$pdo = new PDO("mysql:host=$db_host;port=3306;dbname=$db_name", $db_user, $db_pass);
```

**db_helper.php (MySQLi)**:
```php
$conn = new mysqli($servername, $username, $password, $database);
```

#### C# DbContext Code Template

```csharp
using Microsoft.EntityFrameworkCore;
using Bookstore.Web.Models.Entities;

namespace Bookstore.Web.Data
{
    public class BookstoreDbContext : DbContext
    {
        public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Book configuration
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.BookId);
                entity.Property(e => e.Price).HasPrecision(12, 2);
                
                // Relationships
                entity.HasMany(e => e.CartItems)
                    .WithOne(e => e.Book)
                    .HasForeignKey(e => e.BookId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.Orders)
                    .WithOne(e => e.Book)
                    .HasForeignKey(e => e.BookId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.AzureAdObjectId).IsUnique();
                entity.HasIndex(e => e.Email);
                
                // One-to-one with Customer
                entity.HasOne(e => e.Customer)
                    .WithOne(e => e.User)
                    .HasForeignKey<Customer>(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                
                // Relationships
                entity.HasMany(e => e.CartItems)
                    .WithOne(e => e.Customer)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.Orders)
                    .WithOne(e => e.Customer)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.TotalPrice).HasPrecision(12, 2);
            });

            // Cart configuration
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.CartId);
                entity.Property(e => e.Price).HasPrecision(12, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(12, 2);
            });

            // Seed data (migrated from database.sql INSERT statements)
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    BookId = "B-001",
                    Title = "Lonely Planet Australia (Travel Guide)",
                    ISBN = "123-456-789-1",
                    Price = 136m,
                    Author = "Lonely Planet",
                    Type = "Travel",
                    ImagePath = "images/travel.jpg"
                },
                new Book
                {
                    BookId = "B-002",
                    Title = "Crew Resource Management, Second Edition",
                    ISBN = "123-456-789-2",
                    Price = 599m,
                    Author = "Barbara Kanki",
                    Type = "Technical",
                    ImagePath = "images/technical.jpg"
                },
                new Book
                {
                    BookId = "B-003",
                    Title = "CCNA Routing and Switching 200-125 Official Cert Guide Library",
                    ISBN = "123-456-789-3",
                    Price = 329m,
                    Author = "Cisco Press",
                    Type = "Technology",
                    ImagePath = "images/technology.jpg"
                },
                new Book
                {
                    BookId = "B-004",
                    Title = "Easy Vegetarian Slow Cooker Cookbook",
                    ISBN = "123-456-789-4",
                    Price = 75.9m,
                    Author = "Rockridge Press",
                    Type = "Food",
                    ImagePath = "images/food.jpg"
                }
            );
        }
    }
}
```

#### Connection String Configuration

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:bookstore-sql.database.windows.net,1433;Initial Catalog=bookstore;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;"
  }
}
```

**Note**: Uses Managed Identity (no password in connection string)

#### Migration Complexity: **Low**
#### Estimated Effort: **3 hours**

---

## 2. BUSINESS LOGIC LAYER (SERVICES)

### 2.1 Service: BookService

| Property | Value |
|----------|-------|
| **Source File** | `index.php` (lines 26-34, 60-92) |
| **Target Interface** | `src/Bookstore.Web/Services/Interfaces/IBookService.cs` |
| **Target Implementation** | `src/Bookstore.Web/Services/Implementations/BookService.cs` |
| **Purpose** | Handle book catalog operations |

#### Extracted Business Logic from index.php

| PHP Code Location | Purpose | Line Numbers |
|-------------------|---------|--------------|
| `SELECT * FROM Book` | Get all books for catalog | L28 |
| Book display logic | Format book cards | L60-92 |

#### Methods to Implement

| Method | Signature | Purpose | Source |
|--------|-----------|---------|--------|
| `GetAllBooksAsync` | `Task<IEnumerable<Book>> GetAllBooksAsync()` | Retrieve all books | Line 28 |
| `GetBookByIdAsync` | `Task<Book?> GetBookByIdAsync(string bookId)` | Get specific book | Needed for cart |
| `SearchBooksAsync` | `Task<IEnumerable<Book>> SearchBooksAsync(string query)` | Search catalog | Future enhancement |

#### C# Interface Code Template

```csharp
using Bookstore.Web.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Web.Services.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(string bookId);
        Task<IEnumerable<Book>> SearchBooksAsync(string query);
    }
}
```

#### C# Implementation Code Template

```csharp
using Bookstore.Web.Data;
using Bookstore.Web.Models.Entities;
using Bookstore.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Web.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly BookstoreDbContext _context;

        public BookService(BookstoreDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<Book?> GetBookByIdAsync(string bookId)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.BookId == bookId);
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllBooksAsync();
            }

            return await _context.Books
                .Where(b => b.Title.Contains(query) || 
                           b.Author.Contains(query) ||
                           b.Type.Contains(query))
                .ToListAsync();
        }
    }
}
```

#### Migration Complexity: **Low**
#### Estimated Effort: **2 hours**

---

### 2.2 Service: CartService

| Property | Value |
|----------|-------|
| **Source File** | `index.php` (lines 6-24, 94-115) |
| **Target Interface** | `src/Bookstore.Web/Services/Interfaces/ICartService.cs` |
| **Target Implementation** | `src/Bookstore.Web/Services/Implementations/CartService.cs` |
| **Purpose** | Manage shopping cart operations |

#### Extracted Business Logic from index.php

| PHP Code Location | Business Rule | Line Numbers | Migration Notes |
|-------------------|---------------|--------------|-----------------|
| `INSERT INTO Cart(BookID, Quantity, Price, TotalPrice)` | Add item to cart | L14 | Calculate TotalPrice |
| `Price * Quantity` calculation | Total price logic | L14 | Preserve in service |
| `DELETE FROM Cart` | Empty entire cart | L20 | Clear all items |
| `SELECT... FROM Book,Cart WHERE...` | Get cart with book details | L96 | Join query |
| Total calculation loop | Sum all cart items | L103-109 | LINQ Sum() |

#### Business Rules to Preserve

| Rule | PHP Implementation | C# Implementation |
|------|-------------------|-------------------|
| **Total Price Calculation** | `Price * Quantity` in SQL | Calculate in service: `Price * Quantity` |
| **Cart Total** | Loop sum in PHP | `cart.Sum(c => c.TotalPrice)` |
| **Empty Cart** | `DELETE FROM Cart` (all rows) | Filter by user/session and delete |

#### Methods to Implement

| Method | Signature | Purpose | Source Line |
|--------|-----------|---------|-------------|
| `AddToCartAsync` | `Task AddToCartAsync(string bookId, int quantity, string userId)` | Add book to cart | L6-16 |
| `GetCartItemsAsync` | `Task<IEnumerable<CartItemDto>> GetCartItemsAsync(string userId)` | Get cart with book details | L94-109 |
| `GetCartTotalAsync` | `Task<decimal> GetCartTotalAsync(string userId)` | Calculate cart total | L103-109 |
| `EmptyCartAsync` | `Task EmptyCartAsync(string userId)` | Clear all cart items | L18-21 |
| `RemoveCartItemAsync` | `Task RemoveCartItemAsync(int cartId)` | Remove single item | New method |
| `UpdateQuantityAsync` | `Task UpdateQuantityAsync(int cartId, int quantity)` | Update item quantity | New method |

#### C# Interface Code Template

```csharp
using Bookstore.Web.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Web.Services.Interfaces
{
    public interface ICartService
    {
        Task AddToCartAsync(string bookId, int quantity, string userId);
        Task<IEnumerable<CartItemDto>> GetCartItemsAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
        Task EmptyCartAsync(string userId);
        Task RemoveCartItemAsync(int cartId, string userId);
        Task UpdateQuantityAsync(int cartId, int quantity, string userId);
    }
}
```

#### C# Implementation Code Template

```csharp
using Bookstore.Web.Data;
using Bookstore.Web.Models.DTOs;
using Bookstore.Web.Models.Entities;
using Bookstore.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Web.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly BookstoreDbContext _context;
        private readonly IBookService _bookService;

        public CartService(BookstoreDbContext context, IBookService bookService)
        {
            _context = context;
            _bookService = bookService;
        }

        public async Task AddToCartAsync(string bookId, int quantity, string userId)
        {
            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book == null)
            {
                throw new ArgumentException("Book not found", nameof(bookId));
            }

            var customer = await GetOrCreateCustomerForUserAsync(userId);

            // Check if item already in cart
            var existingItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId && c.BookId == bookId);

            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.Price * existingItem.Quantity;
            }
            else
            {
                // Add new cart item
                var cartItem = new Cart
                {
                    CustomerId = customer.CustomerId,
                    BookId = bookId,
                    Price = book.Price,
                    Quantity = quantity,
                    TotalPrice = book.Price * quantity // Business logic: Price * Quantity
                };

                _context.Carts.Add(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CartItemDto>> GetCartItemsAsync(string userId)
        {
            var customer = await GetCustomerForUserAsync(userId);
            if (customer == null)
            {
                return Enumerable.Empty<CartItemDto>();
            }

            return await _context.Carts
                .Where(c => c.CustomerId == customer.CustomerId)
                .Include(c => c.Book)
                .Select(c => new CartItemDto
                {
                    CartId = c.CartId,
                    BookId = c.BookId,
                    BookTitle = c.Book!.Title,
                    ImagePath = c.Book.ImagePath,
                    Price = c.Price,
                    Quantity = c.Quantity,
                    TotalPrice = c.TotalPrice
                })
                .ToListAsync();
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            var customer = await GetCustomerForUserAsync(userId);
            if (customer == null)
            {
                return 0m;
            }

            return await _context.Carts
                .Where(c => c.CustomerId == customer.CustomerId)
                .SumAsync(c => c.TotalPrice); // Business logic: Sum all TotalPrice
        }

        public async Task EmptyCartAsync(string userId)
        {
            var customer = await GetCustomerForUserAsync(userId);
            if (customer == null)
            {
                return;
            }

            var cartItems = await _context.Carts
                .Where(c => c.CustomerId == customer.CustomerId)
                .ToListAsync();

            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(int cartId, string userId)
        {
            var customer = await GetCustomerForUserAsync(userId);
            if (customer == null)
            {
                return;
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.CustomerId == customer.CustomerId);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateQuantityAsync(int cartId, int quantity, string userId)
        {
            var customer = await GetCustomerForUserAsync(userId);
            if (customer == null)
            {
                return;
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.CustomerId == customer.CustomerId);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                cartItem.TotalPrice = cartItem.Price * quantity; // Recalculate
                await _context.SaveChangesAsync();
            }
        }

        private async Task<Customer?> GetCustomerForUserAsync(string azureAdObjectId)
        {
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.AzureAdObjectId.ToString() == azureAdObjectId);

            return user?.Customer;
        }

        private async Task<Customer> GetOrCreateCustomerForUserAsync(string azureAdObjectId)
        {
            var customer = await GetCustomerForUserAsync(azureAdObjectId);
            if (customer != null)
            {
                return customer;
            }

            // Create customer on first cart add
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.AzureAdObjectId.ToString() == azureAdObjectId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            customer = new Customer
            {
                Email = user.Email,
                UserId = user.UserId
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return customer;
        }
    }
}
```

#### Migration Complexity: **Medium**
#### Estimated Effort: **4 hours**

---

### 2.3 Service: OrderService

| Property | Value |
|----------|-------|
| **Source File** | `checkout.php` (lines 11-34, 123-148) |
| **Target Interface** | `src/Bookstore.Web/Services/Interfaces/IOrderService.cs` |
| **Target Implementation** | `src/Bookstore.Web/Services/Implementations/OrderService.cs` |
| **Purpose** | Handle order creation and management |

#### Extracted Business Logic from checkout.php

| PHP Code Location | Business Rule | Line Numbers | Migration Notes |
|-------------------|---------------|--------------|-----------------|
| Get Customer from session | Find customer by UserID | L12-16 | Use Entra ID object ID |
| Update Cart with CustomerID | Associate cart to customer | L18-19 | No longer needed if cart already linked |
| Cart → Order conversion | Create orders from cart items | L21-30 | **CRITICAL**: Preserve this logic |
| Delete from Cart | Clear cart after checkout | L31-32 | Same logic |
| Update Order status | Mark orders as completed | L87, 291 | Status 'N' → 'y' |
| Guest checkout | Create customer + orders | L123-168 | Modified for Entra ID users only |

#### Business Rules to Preserve

| Rule | PHP Implementation | C# Implementation | Criticality |
|------|-------------------|-------------------|-------------|
| **Cart to Order** | Loop cart items, INSERT into Order | Service method with transaction | 🔴 Critical |
| **Order Status** | 'N' = new, 'y' = completed | Enum or const strings | 🟡 Medium |
| **Customer Association** | Update Cart.CustomerID before orders | Cart already linked to Customer | 🟢 Low |
| **Clear Cart** | DELETE FROM Cart after order | Remove cart items in same transaction | 🔴 Critical |

#### Methods to Implement

| Method | Signature | Purpose | Source Line |
|--------|-----------|---------|-------------|
| `CreateOrderFromCartAsync` | `Task<int> CreateOrderFromCartAsync(string userId)` | Convert cart to orders | L11-34 |
| `GetOrdersByUserAsync` | `Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(string userId)` | Get user's order history | New |
| `GetOrderDetailsAsync` | `Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId)` | Get single order | L38-86 |
| `CompleteOrderAsync` | `Task CompleteOrderAsync(int orderId)` | Mark order as completed | L87, 291 |

#### C# Interface Code Template

```csharp
using Bookstore.Web.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Web.Services.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrderFromCartAsync(string userId, CheckoutDto checkoutInfo);
        Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(string userId);
        Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId, string userId);
        Task CompleteOrderAsync(int orderId);
    }
}
```

#### C# Implementation Code Template (Partial - Key Methods)

```csharp
using Bookstore.Web.Data;
using Bookstore.Web.Models.DTOs;
using Bookstore.Web.Models.Entities;
using Bookstore.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Web.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly BookstoreDbContext _context;
        private readonly ICartService _cartService;

        public OrderService(BookstoreDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        /// <summary>
        /// CRITICAL BUSINESS LOGIC: Convert cart items to orders
        /// Source: checkout.php lines 21-32
        /// </summary>
        public async Task<int> CreateOrderFromCartAsync(string azureAdObjectId, CheckoutDto checkoutInfo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get customer
                var customer = await GetOrCreateCustomerAsync(azureAdObjectId, checkoutInfo);

                // Get cart items
                var cartItems = await _context.Carts
                    .Where(c => c.CustomerId == customer.CustomerId)
                    .Include(c => c.Book)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    throw new InvalidOperationException("Cart is empty");
                }

                // BUSINESS LOGIC: Convert each cart item to an order
                // PHP: INSERT INTO `Order`(CustomerID, BookID, DatePurchase, Quantity, TotalPrice, Status)
                var orders = new List<Order>();
                foreach (var cartItem in cartItems)
                {
                    var order = new Order
                    {
                        CustomerId = customer.CustomerId,
                        BookId = cartItem.BookId,
                        PurchaseDate = DateTimeOffset.UtcNow,
                        Quantity = cartItem.Quantity,
                        TotalPrice = cartItem.TotalPrice, // Preserved from cart
                        Status = "N" // N = New/Pending
                    };
                    orders.Add(order);
                }

                _context.Orders.AddRange(orders);
                await _context.SaveChangesAsync();

                // BUSINESS LOGIC: Clear cart after creating orders
                // PHP: DELETE FROM Cart
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Return first order ID (or could return list)
                return orders.First().OrderId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(string azureAdObjectId)
        {
            var customer = await GetCustomerForUserAsync(azureAdObjectId);
            if (customer == null)
            {
                return Enumerable.Empty<OrderDto>();
            }

            return await _context.Orders
                .Where(o => o.CustomerId == customer.CustomerId)
                .Include(o => o.Book)
                .OrderByDescending(o => o.PurchaseDate)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    PurchaseDate = o.PurchaseDate,
                    BookTitle = o.Book!.Title,
                    Quantity = o.Quantity,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status
                })
                .ToListAsync();
        }

        public async Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId, string azureAdObjectId)
        {
            var customer = await GetCustomerForUserAsync(azureAdObjectId);
            if (customer == null)
            {
                return null;
            }

            var order = await _context.Orders
                .Where(o => o.OrderId == orderId && o.CustomerId == customer.CustomerId)
                .Include(o => o.Book)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return null;
            }

            return new OrderDetailsDto
            {
                OrderId = order.OrderId,
                CustomerName = order.Customer!.Name,
                CustomerEmail = order.Customer.Email,
                CustomerAddress = order.Customer.Address,
                BookTitle = order.Book!.Title,
                BookImage = order.Book.ImagePath,
                Quantity = order.Quantity,
                TotalPrice = order.TotalPrice,
                PurchaseDate = order.PurchaseDate,
                Status = order.Status
            };
        }

        /// <summary>
        /// BUSINESS LOGIC: Mark order as completed
        /// Source: checkout.php line 87, 291
        /// </summary>
        public async Task CompleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = "y"; // y = completed
                await _context.SaveChangesAsync();
            }
        }

        private async Task<Customer?> GetCustomerForUserAsync(string azureAdObjectId)
        {
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.AzureAdObjectId.ToString() == azureAdObjectId);

            return user?.Customer;
        }

        private async Task<Customer> GetOrCreateCustomerAsync(string azureAdObjectId, CheckoutDto checkoutInfo)
        {
            var customer = await GetCustomerForUserAsync(azureAdObjectId);
            if (customer != null)
            {
                return customer;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.AzureAdObjectId.ToString() == azureAdObjectId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            customer = new Customer
            {
                UserId = user.UserId,
                Name = checkoutInfo.Name,
                Email = user.Email,
                Phone = checkoutInfo.Phone,
                Address = checkoutInfo.Address,
                Gender = checkoutInfo.Gender,
                IdNumber = checkoutInfo.IdNumber
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return customer;
        }
    }
}
```

#### Migration Complexity: **Medium-High** (Critical business logic)
#### Estimated Effort: **6 hours**

---

## 3. PRESENTATION LAYER (CONTROLLERS)

### 3.1 Controller: HomeController

| Property | Value |
|----------|-------|
| **Source File** | `index.php` |
| **Target File** | `src/Bookstore.Web/Controllers/HomeController.cs` |
| **Purpose** | Display book catalog and handle cart operations |
| **Routes** | `/` (GET), `/AddToCart` (POST), `/EmptyCart` (POST) |

#### PHP Actions Mapping

| PHP Action | HTTP Method | Route | .NET Action | Purpose |
|------------|-------------|-------|-------------|---------|
| Page load (lines 26-115) | GET | `/` | `Index()` | Display catalog + cart |
| `isset($_POST['ac'])` | POST | `/AddToCart` | `AddToCart(AddToCartDto dto)` | Add to cart |
| `isset($_POST['delc'])` | POST | `/EmptyCart` | `EmptyCart()` | Clear cart |

#### Business Logic in Controller (TO EXTRACT)

| Logic | Lines | Action | Notes |
|-------|-------|--------|-------|
| Add to cart | L6-16 | Move to CartService | Already extracted |
| Empty cart | L18-21 | Move to CartService | Already extracted |
| Display books | L28, L60-92 | Move to BookService + View | Already extracted |
| Display cart | L94-115 | Move to CartService + View | Already extracted |

#### C# Controller Code Template

```csharp
using Bookstore.Web.Models.DTOs;
using Bookstore.Web.Models.ViewModels;
using Bookstore.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookstore.Web.Controllers
{
    [Authorize] // Require Entra ID authentication
    public class HomeController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ICartService _cartService;

        public HomeController(IBookService bookService, ICartService cartService)
        {
            _bookService = bookService;
            _cartService = cartService;
        }

        [HttpGet]
        [Route("/")]
        [Route("/Home")]
        [Route("/Home/Index")]
        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAllBooksAsync();
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? User.FindFirstValue("oid"); // Azure AD Object ID

            var cartItems = await _cartService.GetCartItemsAsync(userId!);
            var cartTotal = await _cartService.GetCartTotalAsync(userId!);

            var viewModel = new BookCatalogViewModel
            {
                Books = books,
                CartItems = cartItems,
                CartTotal = cartTotal
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("/Home/AddToCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? User.FindFirstValue("oid");

            await _cartService.AddToCartAsync(dto.BookId, dto.Quantity, userId!);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("/Home/EmptyCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmptyCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? User.FindFirstValue("oid");

            await _cartService.EmptyCartAsync(userId!);

            return RedirectToAction(nameof(Index));
        }
    }
}
```

#### Migration Complexity: **Low**
#### Estimated Effort: **3 hours**

---

### 3.2 Controller: OrderController

| Property | Value |
|----------|-------|
| **Source File** | `checkout.php` |
| **Target File** | `src/Bookstore.Web/Controllers/OrderController.cs` |
| **Purpose** | Handle checkout and order confirmation |
| **Routes** | `/Checkout` (GET/POST), `/Confirmation` (GET) |

#### PHP Actions Mapping

| PHP Action | HTTP Method | Route | .NET Action | Purpose |
|------------|-------------|-------|-------------|---------|
| Logged-in checkout (lines 9-39) | POST | `/Checkout` | `Checkout(CheckoutDto dto)` | Create orders |
| Guest checkout (lines 92-189) | POST | `/Checkout` | **REMOVED** (Entra ID only) | N/A |
| Order confirmation (lines 40-89) | GET | `/Confirmation/{orderId}` | `Confirmation(int orderId)` | Show order details |

#### **CRITICAL CHANGE**: No Guest Checkout

With Entra ID authentication, all users must be logged in. The guest checkout form (lines 256-298 in checkout.php) will be **removed**.

#### C# Controller Code Template

```csharp
using Bookstore.Web.Models.DTOs;
using Bookstore.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookstore.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public OrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue("oid")!;
            
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                CartTotal = await _cartService.GetCartTotalAsync(userId)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = User.FindFirstValue("oid")!;

            try
            {
                // CRITICAL BUSINESS LOGIC: Create orders from cart
                // Source: checkout.php lines 11-34
                var orderId = await _orderService.CreateOrderFromCartAsync(userId, dto);
                await _orderService.CompleteOrderAsync(orderId);

                return RedirectToAction(nameof(Confirmation), new { orderId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Checkout failed: {ex.Message}");
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            var userId = User.FindFirstValue("oid")!;
            
            var orderDetails = await _orderService.GetOrderDetailsAsync(orderId, userId);
            if (orderDetails == null)
            {
                return NotFound();
            }

            return View(orderDetails);
        }
    }
}
```

#### Migration Complexity: **Medium**
#### Estimated Effort: **4 hours**

---

### 3.3 Controller: AccountController

| Property | Value |
|----------|-------|
| **Source Files** | `login.php`, `checklogin.php`, `logout.php`, `edituser.php`, `register.php` |
| **Target File** | `src/Bookstore.Web/Controllers/AccountController.cs` |
| **Purpose** | Handle authentication and profile management with Entra ID |
| **Routes** | `/Account/Login`, `/Account/Logout`, `/Account/Profile` |

#### **CRITICAL**: Complete Authentication Redesign

| PHP File | Function | .NET Replacement | Notes |
|----------|----------|------------------|-------|
| `login.php` | Login form | **Redirect to Entra ID** | External Microsoft login page |
| `checklogin.php` | Validate credentials | **OAuth2 middleware** | Automatic token validation |
| `register.php` | User registration | **Admin provisioning in Azure AD** | No self-registration |
| `logout.php` | Session destroy | `SignOut()` method | Entra ID sign out |
| `edituser.php` | Edit profile | `Profile()` method | Update custom fields only |

#### C# Controller Code Template

```csharp
using Bookstore.Web.Data;
using Bookstore.Web.Models.DTOs;
using Bookstore.Web.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookstore.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly BookstoreDbContext _context;

        public AccountController(BookstoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            // Redirect to Entra ID login
            return Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Sign out from both app and Entra ID
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var azureAdObjectId = User.FindFirstValue("oid")!;
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.AzureAdObjectId.ToString() == azureAdObjectId);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
            {
                Email = user.Email,
                Name = user.Customer?.Name,
                Phone = user.Customer?.Phone,
                Address = user.Customer?.Address,
                Gender = user.Customer?.Gender,
                IdNumber = user.Customer?.IdNumber
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var azureAdObjectId = User.FindFirstValue("oid")!;
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.AzureAdObjectId.ToString() == azureAdObjectId);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Customer == null)
            {
                user.Customer = new Customer
                {
                    UserId = user.UserId,
                    Email = user.Email
                };
                _context.Customers.Add(user.Customer);
            }

            // Update profile
            user.Customer.Name = dto.Name;
            user.Customer.Phone = dto.Phone;
            user.Customer.Address = dto.Address;
            user.Customer.Gender = dto.Gender;
            user.Customer.IdNumber = dto.IdNumber;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
```

#### Entra ID Authentication Setup Required

**Program.cs configuration**:
```csharp
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
```

**appsettings.json**:
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourcompany.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-oidc"
  }
}
```

#### Migration Complexity: **High** (Complete authentication redesign)
#### Estimated Effort: **8 hours**

---

## 4. PRESENTATION LAYER (VIEWS)

### 4.1 View: Home/Index.cshtml

| Property | Value |
|----------|-------|
| **Source File** | `index.php` |
| **Target File** | `src/Bookstore.Web/Views/Home/Index.cshtml` |
| **Layout** | `_Layout.cshtml` |
| **Purpose** | Display book catalog and shopping cart |

#### PHP View Analysis

| Section | Lines | Purpose | Razor Equivalent |
|---------|-------|---------|------------------|
| HTML head + CDN | L1-7 | Meta, CSS, Font Awesome | `_Layout.cshtml` |
| Header with nav | L43-57 | Logo, Login/Logout buttons | `_Header.cshtml` partial |
| Book catalog grid | L60-92 | Display all books | `@foreach` loop with cards |
| Shopping cart sidebar | L94-115 | Cart items + total | Partial view `_CartSidebar.cshtml` |
| Add to cart form | L87-91 | POST form | `<form asp-action="AddToCart">` |

#### Blade → Razor Syntax Mapping

| PHP/Blade | Razor |
|-----------|-------|
| `<?php if(isset($_SESSION['id'])){ ?>` | `@if (User.Identity!.IsAuthenticated)` |
| `<?php echo $row['BookTitle']; ?>` | `@book.Title` |
| `<form action="" method="post">` | `<form asp-action="AddToCart" method="post">` |
| `<input type="hidden" value="<?php echo $row['BookID']; ?>" name="ac"/>` | `<input type="hidden" asp-for="BookId" value="@book.BookId" />` |
| `RM<?php echo $row['Price']; ?>` | `@book.Price.ToString("C")` |

#### Razor View Code Template

```cshtml
@model Bookstore.Web.Models.ViewModels.BookCatalogViewModel
@{
    ViewData["Title"] = "Bookstore - Home";
}

<div class="container-fluid">
    <div class="row">
        <!-- Book Catalog (80% width) -->
        <div class="col-md-9">
            <div class="row">
                @foreach (var book in Model.Books)
                {
                    <div class="col-md-4 mb-4">
                        <div class="card">
                            <img src="~/@book.ImagePath" class="card-img-top" alt="@book.Title" />
                            <div class="card-body">
                                <h5 class="card-title">@book.Title</h5>
                                <p class="card-text">
                                    <strong>ISBN:</strong> @book.ISBN<br />
                                    <strong>Author:</strong> @book.Author<br />
                                    <strong>Type:</strong> @book.Type<br />
                                    <strong>Price:</strong> RM @book.Price.ToString("F2")
                                </p>
                                
                                <form asp-action="AddToCart" method="post">
                                    <div class="input-group mb-2">
                                        <label for="quantity-@book.BookId" class="me-2">Quantity:</label>
                                        <input type="number" 
                                               id="quantity-@book.BookId" 
                                               name="quantity" 
                                               value="1" 
                                               min="1" 
                                               class="form-control form-control-sm" 
                                               style="width: 60px;" />
                                    </div>
                                    <input type="hidden" name="bookId" value="@book.BookId" />
                                    @Html.AntiForgeryToken()
                                    <button type="submit" class="btn btn-primary">Add to Cart</button>
                                </form>
                            </div>
                        </div>
                    </div>
                }
            </div>
        </div>

        <!-- Shopping Cart Sidebar (20% width) -->
        <div class="col-md-3">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="fa fa-shopping-cart"></i> Cart
                    </h5>
                    @if (Model.CartItems.Any())
                    {
                        <form asp-action="EmptyCart" method="post" class="d-inline">
                            @Html.AntiForgeryToken()
                            <button type="submit" class="btn btn-sm btn-outline-danger">
                                Empty Cart
                            </button>
                        </form>
                    }
                </div>
                <div class="card-body">
                    @if (!Model.CartItems.Any())
                    {
                        <p class="text-muted">Your cart is empty.</p>
                    }
                    else
                    {
                        @foreach (var item in Model.CartItems)
                        {
                            <div class="cart-item mb-3">
                                <img src="~/@item.ImagePath" alt="@item.BookTitle" class="img-thumbnail" style="width: 50px;" />
                                <div class="cart-item-details">
                                    <strong>@item.BookTitle</strong><br />
                                    RM @item.Price.ToString("F2")<br />
                                    Quantity: @item.Quantity<br />
                                    <strong>Total: RM @item.TotalPrice.ToString("F2")</strong>
                                </div>
                            </div>
                            <hr />
                        }
                        
                        <div class="cart-total">
                            <strong>Cart Total: RM @Model.CartTotal.ToString("F2")</strong>
                        </div>
                        
                        <a asp-controller="Order" asp-action="Checkout" class="btn btn-success w-100 mt-3">
                            CHECKOUT
                        </a>
                    }
                </div>
            </div>
        </div>
    </div>
</div>
```

#### Migration Complexity: **Medium**
#### Estimated Effort: **4 hours**

---

### 4.2 View: Order/Checkout.cshtml

| Property | Value |
|----------|-------|
| **Source File** | `checkout.php` (lines 256-298 for form) |
| **Target File** | `src/Bookstore.Web/Views/Order/Checkout.cshtml` |
| **Purpose** | Checkout form (authenticated users only) |

#### PHP Form Fields → Razor

| PHP Field | Validation | Razor Field | Validation Attributes |
|-----------|-----------|-------------|----------------------|
| `name` | Letters/spaces | `Name` | `[Required]`, `[RegularExpression]` |
| `ic` | Numbers/dashes | `IdNumber` | `[RegularExpression]` |
| `email` | Email format | `Email` | `[Required]`, `[EmailAddress]` |
| `contact` | Numbers/dashes | `Phone` | `[Phone]` |
| `gender` | Required | `Gender` | `[Required]` |
| `address` | Required | `Address` | `[Required]` |

#### Razor View Code Template

```cshtml
@model Bookstore.Web.Models.ViewModels.CheckoutViewModel
@{
    ViewData["Title"] = "Checkout";
}

<div class="container mt-4">
    <div class="row">
        <div class="col-md-6">
            <h2>Checkout</h2>
            
            <form asp-action="Checkout" method="post">
                <div class="mb-3">
                    <label for="Name" class="form-label">Full Name</label>
                    <input type="text" class="form-control" id="Name" name="Name" required />
                    <span asp-validation-for="Name" class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label for="IdNumber" class="form-label">IC Number</label>
                    <input type="text" class="form-control" id="IdNumber" name="IdNumber" placeholder="xxxxxx-xx-xxxx" />
                    <span asp-validation-for="IdNumber" class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label for="Email" class="form-label">E-mail</label>
                    <input type="email" class="form-control" id="Email" name="Email" required />
                    <span asp-validation-for="Email" class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label for="Phone" class="form-label">Mobile Number</label>
                    <input type="tel" class="form-control" id="Phone" name="Phone" placeholder="012-3456789" />
                    <span asp-validation-for="Phone" class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label class="form-label">Gender</label><br />
                    <div class="form-check form-check-inline">
                        <input class="form-check-input" type="radio" name="Gender" id="GenderMale" value="Male" required />
                        <label class="form-check-label" for="GenderMale">Male</label>
                    </div>
                    <div class="form-check form-check-inline">
                        <input class="form-check-input" type="radio" name="Gender" id="GenderFemale" value="Female" required />
                        <label class="form-check-label" for="GenderFemale">Female</label>
                    </div>
                    <span asp-validation-for="Gender" class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label for="Address" class="form-label">Address</label>
                    <textarea class="form-control" id="Address" name="Address" rows="3" required></textarea>
                    <span asp-validation-for="Address" class="text-danger"></span>
                </div>

                @Html.AntiForgeryToken()
                
                <button type="submit" class="btn btn-primary">Place Order</button>
                <a asp-controller="Home" asp-action="Index" class="btn btn-secondary">Cancel</a>
            </form>
        </div>

        <div class="col-md-6">
            <h4>Order Summary</h4>
            <table class="table">
                <thead>
                    <tr>
                        <th>Item</th>
                        <th>Quantity</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in Model.CartItems)
                    {
                        <tr>
                            <td>@item.BookTitle</td>
                            <td>@item.Quantity</td>
                            <td>RM @item.TotalPrice.ToString("F2")</td>
                        </tr>
                    }
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan="2"><strong>Total:</strong></td>
                        <td><strong>RM @Model.CartTotal.ToString("F2")</strong></td>
                    </tr>
                </tfoot>
            </table>
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

#### Migration Complexity: **Low**
#### Estimated Effort: **3 hours**

---

### 4.3 View: Shared/_Layout.cshtml

| Property | Value |
|----------|-------|
| **Source File** | Header sections in `index.php`, `login.php`, etc. |
| **Target File** | `src/Bookstore.Web/Views/Shared/_Layout.cshtml` |
| **Purpose** | Main layout template for all pages |

#### Razor Layout Code Template

```cshtml
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Bookstore</title>
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
    <header class="bg-primary text-white p-3">
        <div class="container-fluid d-flex justify-content-between align-items-center">
            <a asp-controller="Home" asp-action="Index">
                <img src="~/images/logo.png" alt="Bookstore Logo" style="height: 50px;" />
            </a>
            
            <div>
                @if (User.Identity!.IsAuthenticated)
                {
                    <span class="me-3">Welcome, @User.Identity.Name</span>
                    <a asp-controller="Account" asp-action="Profile" class="btn btn-light me-2">Edit Profile</a>
                    <a asp-controller="Account" asp-action="Logout" class="btn btn-light">Logout</a>
                }
                else
                {
                    <a asp-controller="Account" asp-action="Login" class="btn btn-light">Login</a>
                }
            </div>
        </div>
    </header>

    <main role="main" class="pb-3">
        @if (TempData["Success"] != null)
        {
            <div class="alert alert-success alert-dismissible fade show" role="alert">
                @TempData["Success"]
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        }
        @if (TempData["Error"] != null)
        {
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                @TempData["Error"]
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        }
        
        @RenderBody()
    </main>

    <footer class="border-top footer text-muted">
        <div class="container text-center">
            &copy; 2026 - Bookstore - <a asp-controller="Home" asp-action="Privacy">Privacy</a>
        </div>
    </footer>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

#### Migration Complexity: **Low**
#### Estimated Effort: **2 hours**

---

## 5. DTOs AND VIEWMODELS

### DTOs to Create

| DTO | Purpose | Properties | Source |
|-----|---------|------------|--------|
| `AddToCartDto` | Add item to cart | `BookId` (string), `Quantity` (int) | index.php form |
| `CheckoutDto` | Checkout information | `Name`, `Email`, `Phone`, `Address`, `Gender`, `IdNumber` | checkout.php form |
| `UpdateProfileDto` | Update user profile | `Name`, `Phone`, `Address`, `Gender`, `IdNumber` | edituser.php form |
| `CartItemDto` | Cart item with book details | `CartId`, `BookId`, `BookTitle`, `ImagePath`, `Price`, `Quantity`, `TotalPrice` | index.php cart display |
| `OrderDto` | Order summary | `OrderId`, `PurchaseDate`, `BookTitle`, `Quantity`, `TotalPrice`, `Status` | New |
| `OrderDetailsDto` | Full order details | Customer info + Book info + Order info | checkout.php confirmation |

### ViewModels to Create

| ViewModel | Purpose | Properties | Source |
|-----------|---------|------------|--------|
| `BookCatalogViewModel` | Home page | `Books` (IEnumerable<Book>), `CartItems` (IEnumerable<CartItemDto>), `CartTotal` (decimal) | index.php |
| `CheckoutViewModel` | Checkout page | `CartItems` (IEnumerable<CartItemDto>), `CartTotal` (decimal) | checkout.php |
| `ProfileViewModel` | Profile page | `Email`, `Name`, `Phone`, `Address`, `Gender`, `IdNumber` | edituser.php |

---

## 6. MIGRATION ORDER (WAVES)

### Wave 1: Foundation & Setup (Day 1-2)

| Order | Task | File(s) to Create | Dependencies | Estimated Hours |
|-------|------|-------------------|--------------|-----------------|
| 1.1 | Create .NET 10 project | Bookstore.sln, Bookstore.Web.csproj | None | 1h |
| 1.2 | Install NuGet packages | Microsoft.EntityFrameworkCore.SqlServer, Microsoft.Identity.Web, etc. | None | 1h |
| 1.3 | Configure appsettings.json | appsettings.json, appsettings.Development.json | None | 1h |
| 1.4 | Create entity models | Book.cs, User.cs, Customer.cs, Order.cs, Cart.cs | None | 6h |
| 1.5 | Create DbContext | BookstoreDbContext.cs | Entities | 3h |
| 1.6 | Generate initial migration | `dotnet ef migrations add InitialCreate` | DbContext | 1h |
| 1.7 | Configure Entra ID in Program.cs | Program.cs | None | 2h |

**Wave 1 Total: 15 hours**

#### Validation Checkpoint
- ✅ Project builds without errors
- ✅ EF Core migration generated successfully
- ✅ Connection to Azure SQL Database works
- ✅ Entra ID authentication configured (not yet tested)

---

### Wave 2: Data Layer & Services (Day 3-5)

| Order | Task | File(s) to Create | Dependencies | Estimated Hours |
|-------|------|-------------------|--------------|-----------------|
| 2.1 | Create DTOs | AddToCartDto.cs, CheckoutDto.cs, UpdateProfileDto.cs, CartItemDto.cs, OrderDto.cs, OrderDetailsDto.cs | Entities | 4h |
| 2.2 | Create service interfaces | IBookService.cs, ICartService.cs, IOrderService.cs | DTOs | 2h |
| 2.3 | Implement BookService | BookService.cs | IBookService, DbContext | 2h |
| 2.4 | Implement CartService | CartService.cs | ICartService, DbContext, IBookService | 4h |
| 2.5 | Implement OrderService | OrderService.cs | IOrderService, DbContext, ICartService | 6h |
| 2.6 | Register services in DI | Program.cs | Services | 1h |
| 2.7 | Create validators | CheckoutDtoValidator.cs, UpdateProfileDtoValidator.cs | FluentValidation | 3h |

**Wave 2 Total: 22 hours**

#### Validation Checkpoint
- ✅ All services implement interfaces
- ✅ Dependency injection configured
- ✅ Unit tests for critical business logic pass
- ✅ Cart-to-order conversion logic preserved

---

### Wave 3: Controllers & Authentication (Day 6-8)

| Order | Task | File(s) to Create | Dependencies | Estimated Hours |
|-------|------|-------------------|--------------|-----------------|
| 3.1 | Create ViewModels | BookCatalogViewModel.cs, CheckoutViewModel.cs, ProfileViewModel.cs | DTOs | 2h |
| 3.2 | Create AccountController | AccountController.cs | DbContext, Entra ID | 8h |
| 3.3 | Create HomeController | HomeController.cs | IBookService, ICartService | 3h |
| 3.4 | Create OrderController | OrderController.cs | IOrderService, ICartService | 4h |
| 3.5 | Test Entra ID authentication | Login/logout flow | Entra ID app registration | 3h |
| 3.6 | Create user provisioning helper | Middleware or service | DbContext | 2h |

**Wave 3 Total: 22 hours**

#### Validation Checkpoint
- ✅ Entra ID login redirects to Microsoft login page
- ✅ User authenticated successfully
- ✅ User record created in database on first login
- ✅ Claims populated correctly (oid, email, name)

---

### Wave 4: Views & UI (Day 9-11)

| Order | Task | File(s) to Create | Dependencies | Estimated Hours |
|-------|------|-------------------|--------------|-----------------|
| 4.1 | Create shared layout | _Layout.cshtml, _ViewStart.cshtml, _ViewImports.cshtml | None | 2h |
| 4.2 | Migrate CSS | site.css | style.css | 2h |
| 4.3 | Copy static assets | Images (logo, book covers) | None | 1h |
| 4.4 | Create Home/Index view | Index.cshtml | HomeController | 4h |
| 4.5 | Create Order/Checkout view | Checkout.cshtml | OrderController | 3h |
| 4.6 | Create Order/Confirmation view | Confirmation.cshtml | OrderController | 2h |
| 4.7 | Create Account/Profile view | Profile.cshtml | AccountController | 2h |
| 4.8 | Create Account/Login view | Login.cshtml | AccountController | 1h |
| 4.9 | Create Account/AccessDenied view | AccessDenied.cshtml | AccountController | 1h |
| 4.10 | Test all views | Manual testing | All views | 2h |

**Wave 4 Total: 20 hours**

#### Validation Checkpoint
- ✅ All pages render correctly
- ✅ Forms submit successfully
- ✅ Validation errors display properly
- ✅ Entra ID authentication works end-to-end
- ✅ Cart functionality works
- ✅ Checkout creates orders successfully

---

### Wave 5: Database Migration & Seeding (Day 12)

| Order | Task | Activities | Estimated Hours |
|-------|------|------------|-----------------|
| 5.1 | Export MySQL data | Export Book, Customer data to CSV | 1h |
| 5.2 | Apply EF Core migrations | `dotnet ef database update` | 1h |
| 5.3 | Import seed data | Run DbInitializer or bulk insert | 2h |
| 5.4 | Validate data migration | Query Azure SQL, verify counts | 1h |
| 5.5 | Test application with real data | Full end-to-end testing | 3h |

**Wave 5 Total: 8 hours**

#### Validation Checkpoint
- ✅ Azure SQL database schema created
- ✅ All book data migrated
- ✅ Application works with real data
- ✅ No data loss or corruption

---

### Wave 6: Infrastructure & Deployment (Day 13-17)

| Order | Task | File(s) to Create | Estimated Hours |
|-------|------|-------------------|-----------------|
| 6.1 | Create Dockerfile | Dockerfile, .dockerignore | 3h |
| 6.2 | Build and test Docker image locally | docker build, docker run | 2h |
| 6.3 | Create Bicep templates | main.bicep, aks.bicep, sql.bicep, keyvault.bicep | 16h |
| 6.4 | Create Kubernetes manifests | deployment.yaml, service.yaml, ingress.yaml, configmap.yaml, secret.yaml, hpa.yaml | 12h |
| 6.5 | Deploy infrastructure to Azure | Run Bicep deployment | 4h |
| 6.6 | Deploy application to AKS | kubectl apply | 3h |
| 6.7 | Configure DNS and SSL | Custom domain, TLS certificate | 3h |
| 6.8 | Test deployed application | Full end-to-end in production | 2h |

**Wave 6 Total: 45 hours**

#### Validation Checkpoint
- ✅ AKS cluster provisioned
- ✅ Azure SQL Database provisioned
- ✅ Azure Key Vault configured
- ✅ Application deployed to AKS
- ✅ HTTPS working with valid certificate
- ✅ Entra ID authentication works in production
- ✅ Application Insights telemetry flowing

---

## 7. BUSINESS LOGIC PRESERVATION CHECKLIST

**⚠️ CRITICAL**: Every business rule from PHP must be preserved in .NET.

| # | Business Rule | Source File | Source Line | Target File | Target Method | Status |
|---|---------------|-------------|-------------|-------------|---------------|--------|
| 1 | **Cart Total Calculation** | index.php | L103-109 | CartService.cs | `GetCartTotalAsync()` | ⏳ Pending |
| 2 | **Add to Cart - Price Capture** | index.php | L14 | CartService.cs | `AddToCartAsync()` | ⏳ Pending |
| 3 | **TotalPrice = Price * Quantity** | index.php | L14 | Cart entity, CartService | Multiple locations | ⏳ Pending |
| 4 | **Empty Cart** | index.php | L20 | CartService.cs | `EmptyCartAsync()` | ⏳ Pending |
| 5 | **Get Customer from User** | checkout.php | L12-16 | OrderService.cs | `GetCustomerForUserAsync()` | ⏳ Pending |
| 6 | **Cart to Order Conversion** | checkout.php | L21-30 | OrderService.cs | `CreateOrderFromCartAsync()` | ⏳ Pending |
| 7 | **Order Status: 'N' = Pending** | checkout.php | L27 | Order entity | Constructor default | ⏳ Pending |
| 8 | **Order Status: 'y' = Completed** | checkout.php | L87, L291 | OrderService.cs | `CompleteOrderAsync()` | ⏳ Pending |
| 9 | **Clear Cart After Checkout** | checkout.php | L31-32 | OrderService.cs | `CreateOrderFromCartAsync()` | ⏳ Pending |
| 10 | **Name Validation: Letters/Spaces** | register.php | L11-15 | UpdateProfileDto | `[RegularExpression]` | ⏳ Pending |
| 11 | **IC Validation: Numbers/Dashes** | register.php | L35-39 | UpdateProfileDto | `[RegularExpression]` | ⏳ Pending |
| 12 | **Email Validation** | register.php | L43-47 | UpdateProfileDto | `[EmailAddress]` | ⏳ Pending |
| 13 | **Phone Validation: Numbers/Dashes** | register.php | L49-53 | UpdateProfileDto | `[Phone]` | ⏳ Pending |
| 14 | **Gender Required** | register.php | L57-61 | CheckoutDto | `[Required]` | ⏳ Pending |
| 15 | **Address Required** | register.php | L63-67 | CheckoutDto | `[Required]` | ⏳ Pending |

---

## 8. ENTRA ID CONFIGURATION CHECKLIST

### Azure AD App Registration

- [ ] Create App Registration in Azure Portal
- [ ] Set Display Name: "Bookstore Application"
- [ ] Set Supported Account Types: "Single tenant"
- [ ] Add Redirect URI: `https://yourdomain.com/signin-oidc`
- [ ] Add Front-channel logout URL: `https://yourdomain.com/signout-oidc`
- [ ] Enable ID tokens and access tokens
- [ ] Add API Permissions: `User.Read`, `Directory.Read.All` (optional)
- [ ] Create Client Secret (save to Azure Key Vault)
- [ ] Note Tenant ID and Client ID

### Azure AD Groups

- [ ] Create Azure AD Group: "Bookstore_Users"
- [ ] Create Azure AD Group: "Bookstore_Admins" (for future use)
- [ ] Assign corporate users to appropriate groups
- [ ] Configure group claims in App Registration

### Application Configuration

- [ ] Update `appsettings.json` with Tenant ID, Client ID
- [ ] Store Client Secret in Azure Key Vault
- [ ] Configure Managed Identity for Key Vault access
- [ ] Test authentication in development environment
- [ ] Test authentication in production environment

---

## 9. TESTING STRATEGY

### Unit Tests

| Test Suite | Coverage | Estimated Hours |
|------------|----------|-----------------|
| BookService tests | All methods | 2h |
| CartService tests | Cart operations, business logic | 4h |
| OrderService tests | **CRITICAL**: Order creation logic | 4h |
| Validator tests | All validation rules | 2h |

**Total Unit Testing: 12 hours**

### Integration Tests

| Test Suite | Coverage | Estimated Hours |
|------------|----------|-----------------|
| Controller tests | All actions | 4h |
| Database tests | EF Core operations | 2h |
| End-to-end tests | Full user journeys | 2h |

**Total Integration Testing: 8 hours**

### Manual Testing Checklist

- [ ] Entra ID login works
- [ ] User created in database on first login
- [ ] Browse books catalog
- [ ] Add book to cart
- [ ] Update cart quantity
- [ ] Remove cart item
- [ ] Empty cart
- [ ] Checkout with valid data
- [ ] Order confirmation displayed
- [ ] View order history
- [ ] Update profile
- [ ] Logout

---

## 10. DEPLOYMENT CHECKLIST

### Pre-Deployment

- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Code reviewed and approved
- [ ] Azure subscription ready
- [ ] Entra ID app registration complete
- [ ] Azure AD users provisioned

### Infrastructure Deployment

- [ ] Run Bicep deployment: `az deployment group create --resource-group bookstore-rg --template-file infrastructure/bicep/main.bicep`
- [ ] Validate AKS cluster created
- [ ] Validate Azure SQL Database created
- [ ] Validate Azure Key Vault created
- [ ] Configure Managed Identity
- [ ] Store secrets in Key Vault

### Application Deployment

- [ ] Build Docker image: `docker build -t bookstore:latest .`
- [ ] Push to Azure Container Registry: `docker push acrbookstore.azurecr.io/bookstore:latest`
- [ ] Apply Kubernetes manifests: `kubectl apply -f infrastructure/kubernetes/`
- [ ] Validate pods running: `kubectl get pods`
- [ ] Validate service created: `kubectl get svc`
- [ ] Configure ingress with TLS certificate
- [ ] Test application URL

### Post-Deployment Validation

- [ ] Application accessible via HTTPS
- [ ] Entra ID authentication works
- [ ] Database connectivity works
- [ ] All features functional
- [ ] Application Insights telemetry flowing
- [ ] No errors in logs

---

## Next Steps

✅ **Phase 2 Complete - Migration Plan Created**

**Proceed to Phase 3: Code Migration**

Run the command: **`/phase3-migratecode`**

Phase 3 will execute this plan, migrating each component wave by wave, building and validating after each wave to ensure quality and correctness.

---

**Plan Generated**: January 22, 2026  
**Total Estimated Effort**: 227 hours  
**Waves**: 6  
**Business Rules Tracked**: 15  
**Ready for Execution**: ✅ Yes
