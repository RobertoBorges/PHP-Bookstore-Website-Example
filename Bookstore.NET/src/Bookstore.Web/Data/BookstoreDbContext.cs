using Microsoft.EntityFrameworkCore;
using Bookstore.Web.Models.Entities;

namespace Bookstore.Web.Data;

public class BookstoreDbContext : DbContext
{
    public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId);
            entity.Property(e => e.Price).HasPrecision(12, 2);
            
            entity.HasMany(e => e.CartItems)
                .WithOne(e => e.Book)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Orders)
                .WithOne(e => e.Book)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.AzureAdObjectId).IsUnique();
            entity.HasIndex(e => e.Email);
            
            entity.HasOne(e => e.Customer)
                .WithOne(e => e.User)
                .HasForeignKey<Customer>(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            
            entity.HasMany(e => e.CartItems)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Orders)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.TotalPrice).HasPrecision(12, 2);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId);
            entity.Property(e => e.Price).HasPrecision(12, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(12, 2);
        });

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
