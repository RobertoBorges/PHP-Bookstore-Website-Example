using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Web.Models.DTOs;

namespace Bookstore.Web.Services.Interfaces;

public interface ICartService
{
    Task AddToCartAsync(string bookId, int quantity, string userId);
    Task AddToCartAsync(string bookId, int quantity, string userId, string userEmail, string userName);
    Task<IEnumerable<CartItemDto>> GetCartItemsAsync(string userId);
    Task<decimal> GetCartTotalAsync(string userId);
    Task EmptyCartAsync(string userId);
    Task RemoveCartItemAsync(int cartId, string userId);
}
