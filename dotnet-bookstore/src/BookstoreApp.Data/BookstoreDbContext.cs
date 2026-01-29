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
