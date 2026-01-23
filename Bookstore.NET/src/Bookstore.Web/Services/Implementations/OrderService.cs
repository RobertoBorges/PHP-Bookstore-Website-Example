using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Bookstore.Web.Data;
using Bookstore.Web.Models.DTOs;
using Bookstore.Web.Models.Entities;
using Bookstore.Web.Services.Interfaces;

namespace Bookstore.Web.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly BookstoreDbContext _context;

    public OrderService(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateOrderFromCartAsync(string azureAdObjectId, CheckoutDto checkoutInfo)
    {
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var customer = await GetOrCreateCustomerAsync(azureAdObjectId, checkoutInfo);

            var cartItems = await _context.Carts
                .Where(c => c.CustomerId == customer.CustomerId)
                .Include(c => c.Book)
                .ToListAsync();

            if (!cartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty");
            }

            var orders = new List<Order>();
            foreach (var cartItem in cartItems)
            {
                var order = new Order
                {
                    CustomerId = customer.CustomerId,
                    BookId = cartItem.BookId,
                    PurchaseDate = DateTimeOffset.UtcNow,
                    Quantity = cartItem.Quantity,
                    TotalPrice = cartItem.TotalPrice,
                    Status = "N"
                };
                orders.Add(order);
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

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
            .Include(o => o.Book)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customer.CustomerId);

        if (order == null)
        {
            return null;
        }

        return new OrderDetailsDto
        {
            OrderId = order.OrderId,
            CustomerName = order.Customer?.Name ?? "N/A",
            CustomerEmail = order.Customer?.Email ?? "N/A",
            CustomerAddress = order.Customer?.Address ?? "N/A",
            BookTitle = order.Book?.Title ?? "N/A",
            BookImage = order.Book?.ImagePath,
            Quantity = order.Quantity,
            TotalPrice = order.TotalPrice,
            PurchaseDate = order.PurchaseDate,
            Status = order.Status
        };
    }

    public async Task CompleteOrderAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = "y";
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

    private async Task<Customer> GetOrCreateCustomerAsync(string azureAdObjectId, CheckoutDto checkoutInfo)
    {
        if (!Guid.TryParse(azureAdObjectId, out var guidValue))
        {
            throw new ArgumentException("Invalid Azure AD Object ID", nameof(azureAdObjectId));
        }

        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.AzureAdObjectId == guidValue);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.Customer != null)
        {
            user.Customer.Name = checkoutInfo.Name;
            user.Customer.IdNumber = checkoutInfo.IdNumber;
            user.Customer.Email = checkoutInfo.Email;
            user.Customer.Phone = checkoutInfo.Phone;
            user.Customer.Gender = checkoutInfo.Gender;
            user.Customer.Address = checkoutInfo.Address;
            
            await _context.SaveChangesAsync();
            return user.Customer;
        }

        var customer = new Customer
        {
            Name = checkoutInfo.Name,
            IdNumber = checkoutInfo.IdNumber,
            Email = checkoutInfo.Email,
            Phone = checkoutInfo.Phone,
            Gender = checkoutInfo.Gender,
            Address = checkoutInfo.Address,
            UserId = user.UserId
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return customer;
    }
}
