using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Bookstore.Web.Data;
using Bookstore.Web.Models.DTOs;
using Bookstore.Web.Models.Entities;
using Bookstore.Web.Services.Interfaces;

namespace Bookstore.Web.Services.Implementations;

public class CartService : ICartService
{
    private readonly BookstoreDbContext _context;
    private readonly IBookService _bookService;

    public CartService(BookstoreDbContext context, IBookService bookService)
    {
        _context = context;
        _bookService = bookService;
    }

    public async Task AddToCartAsync(string bookId, int quantity, string azureAdObjectId)
    {
        await AddToCartAsync(bookId, quantity, azureAdObjectId, null, null);
    }

    public async Task AddToCartAsync(string bookId, int quantity, string azureAdObjectId, string? userEmail, string? userName)
    {
        var book = await _bookService.GetBookByIdAsync(bookId);
        if (book == null)
        {
            throw new ArgumentException("Book not found", nameof(bookId));
        }

        var customer = await GetOrCreateCustomerForUserAsync(azureAdObjectId, userEmail, userName);

        var existingItem = await _context.Carts
            .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId && c.BookId == bookId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            existingItem.TotalPrice = existingItem.Price * existingItem.Quantity;
        }
        else
        {
            var cartItem = new Cart
            {
                CustomerId = customer.CustomerId,
                BookId = bookId,
                Price = book.Price,
                Quantity = quantity,
                TotalPrice = book.Price * quantity
            };

            _context.Carts.Add(cartItem);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CartItemDto>> GetCartItemsAsync(string azureAdObjectId)
    {
        var customer = await GetCustomerForUserAsync(azureAdObjectId);
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
                BookId = c.BookId!,
                BookTitle = c.Book!.Title,
                ImagePath = c.Book.ImagePath,
                Price = c.Price,
                Quantity = c.Quantity,
                TotalPrice = c.TotalPrice
            })
            .ToListAsync();
    }

    public async Task<decimal> GetCartTotalAsync(string azureAdObjectId)
    {
        var customer = await GetCustomerForUserAsync(azureAdObjectId);
        if (customer == null)
        {
            return 0m;
        }

        return await _context.Carts
            .Where(c => c.CustomerId == customer.CustomerId)
            .SumAsync(c => c.TotalPrice);
    }

    public async Task EmptyCartAsync(string azureAdObjectId)
    {
        var customer = await GetCustomerForUserAsync(azureAdObjectId);
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

    public async Task RemoveCartItemAsync(int cartId, string azureAdObjectId)
    {
        var customer = await GetCustomerForUserAsync(azureAdObjectId);
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

    private async Task<Customer?> GetCustomerForUserAsync(string azureAdObjectId)
    {
        if (!Guid.TryParse(azureAdObjectId, out var guidValue))
        {
            return null;
        }

        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.AzureAdObjectId == guidValue);

        return user?.Customer;
    }

    private async Task<Customer> GetOrCreateCustomerForUserAsync(string azureAdObjectId, string? userEmail = null, string? userName = null)
    {
        var customer = await GetCustomerForUserAsync(azureAdObjectId);
        if (customer != null)
        {
            return customer;
        }

        if (!Guid.TryParse(azureAdObjectId, out var guidValue))
        {
            throw new ArgumentException("Invalid Azure AD Object ID", nameof(azureAdObjectId));
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.AzureAdObjectId == guidValue);

        // If user doesn't exist in database, create it (first-time Azure AD login)
        if (user == null)
        {
            user = new User
            {
                AzureAdObjectId = guidValue,
                Email = userEmail ?? "user@example.com",
                UserName = userName ?? "Azure AD User",
                CreatedAt = DateTimeOffset.UtcNow,
                LastLoginAt = DateTimeOffset.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Update last login time and optionally update email/username if provided
            user.LastLoginAt = DateTimeOffset.UtcNow;
            if (!string.IsNullOrEmpty(userEmail))
            {
                user.Email = userEmail;
            }
            if (!string.IsNullOrEmpty(userName))
            {
                user.UserName = userName;
            }
            await _context.SaveChangesAsync();
        }

        customer = new Customer
        {
            Name = user.UserName ?? "Guest",
            Email = user.Email,
            UserId = user.UserId
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return customer;
    }
}
