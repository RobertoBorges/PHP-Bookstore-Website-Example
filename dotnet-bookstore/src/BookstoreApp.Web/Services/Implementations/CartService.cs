using BookstoreApp.Web.Extensions;
using BookstoreApp.Web.Models.DTOs;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Services.Implementations;

public class CartService : ICartService
{
    private const string CartSessionKey = "ShoppingCart";
    private readonly IBookService _bookService;
    private readonly ILogger<CartService> _logger;

    public CartService(IBookService bookService, ILogger<CartService> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    public ShoppingCart GetCart(ISession session)
    {
        return session.Get<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();
    }

    public async Task AddToCartAsync(ISession session, string bookId, int quantity)
    {
        _logger.LogInformation("Adding {Quantity} of book {BookId} to cart", quantity, bookId);

        var cart = GetCart(session);
        var existingItem = cart.Items.FirstOrDefault(i => i.BookId == bookId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book != null)
            {
                cart.Items.Add(new CartItemDto
                {
                    BookId = book.BookId,
                    BookTitle = book.BookTitle,
                    ImageUrl = book.ImageUrl,
                    Price = book.Price,
                    Quantity = quantity
                });
            }
        }

        session.Set(CartSessionKey, cart);
    }

    public void UpdateQuantity(ISession session, string bookId, int quantity)
    {
        var cart = GetCart(session);
        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            session.Set(CartSessionKey, cart);
        }
    }

    public void RemoveFromCart(ISession session, string bookId)
    {
        var cart = GetCart(session);
        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);
        if (item != null)
        {
            cart.Items.Remove(item);
            session.Set(CartSessionKey, cart);
        }
    }

    public void ClearCart(ISession session)
    {
        session.Remove(CartSessionKey);
    }
}
