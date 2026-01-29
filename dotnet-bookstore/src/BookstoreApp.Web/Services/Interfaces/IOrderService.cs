using BookstoreApp.Data.Entities;
using BookstoreApp.Web.Models.DTOs;

namespace BookstoreApp.Web.Services.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<Order>> CreateOrdersFromCartAsync(Customer customer, ShoppingCart cart);
    Task<IEnumerable<Order>> GetOrderHistoryAsync(int customerId);
}
