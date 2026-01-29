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
