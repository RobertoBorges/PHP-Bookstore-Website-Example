using Microsoft.EntityFrameworkCore;
using BookstoreApp.Data;
using BookstoreApp.Data.Entities;
using BookstoreApp.Data.Enums;
using BookstoreApp.Web.Models.DTOs;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly BookstoreDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(BookstoreDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Order>> CreateOrdersFromCartAsync(Customer customer, ShoppingCart cart)
    {
        _logger.LogInformation("Creating orders for customer {CustomerId}", customer.CustomerId);

        var orders = new List<Order>();

        foreach (var item in cart.Items)
        {
            var order = new Order
            {
                CustomerId = customer.CustomerId,
                BookId = item.BookId,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                TotalPrice = item.TotalPrice,
                PurchaseDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            _context.Orders.Add(order);
            orders.Add(order);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Created {Count} orders for customer {CustomerId}", orders.Count, customer.CustomerId);

        return orders;
    }

    public async Task<IEnumerable<Order>> GetOrderHistoryAsync(int customerId)
    {
        _logger.LogInformation("Fetching order history for customer {CustomerId}", customerId);

        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Book)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.PurchaseDate)
            .ToListAsync();
    }
}
