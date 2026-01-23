using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Web.Models.DTOs;

namespace Bookstore.Web.Services.Interfaces;

public interface IOrderService
{
    Task<int> CreateOrderFromCartAsync(string userId, CheckoutDto checkoutInfo);
    Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(string userId);
    Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId, string userId);
    Task CompleteOrderAsync(int orderId);
}
