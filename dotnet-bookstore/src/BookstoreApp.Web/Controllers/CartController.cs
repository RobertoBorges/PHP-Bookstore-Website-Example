using Microsoft.AspNetCore.Mvc;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Controllers;

/// <summary>
/// Handles shopping cart operations: add, update quantity, remove items.
/// Migrated from: index.php cart functionality
/// </summary>
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IBookService _bookService;
    private readonly ILogger<CartController> _logger;

    public CartController(
        ICartService cartService,
        IBookService bookService,
        ILogger<CartController> logger)
    {
        _cartService = cartService;
        _bookService = bookService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the shopping cart contents.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        return View(cart);
    }

    /// <summary>
    /// Adds a book to the cart.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string bookId, int quantity = 1, string? returnUrl = null)
    {
        var book = await _bookService.GetBookByIdAsync(bookId);
        if (book == null)
        {
            _logger.LogWarning("Attempted to add non-existent book to cart: {BookId}", bookId);
            TempData["Error"] = "Book not found.";
            return RedirectToLocal(returnUrl);
        }

        await _cartService.AddToCartAsync(HttpContext.Session, bookId, quantity);
        _logger.LogInformation("Added book {BookId} to cart, quantity: {Quantity}", bookId, quantity);
        
        TempData["Success"] = $"'{book.BookTitle}' added to cart.";
        return RedirectToLocal(returnUrl);
    }

    /// <summary>
    /// Updates the quantity of a cart item.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(string bookId, int quantity)
    {
        if (quantity < 1)
        {
            _cartService.RemoveFromCart(HttpContext.Session, bookId);
            TempData["Success"] = "Item removed from cart.";
        }
        else
        {
            _cartService.UpdateQuantity(HttpContext.Session, bookId, quantity);
            TempData["Success"] = "Cart updated.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(string bookId)
    {
        _cartService.RemoveFromCart(HttpContext.Session, bookId);
        _logger.LogInformation("Removed book {BookId} from cart", bookId);
        
        TempData["Success"] = "Item removed from cart.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Clears all items from the cart.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _cartService.ClearCart(HttpContext.Session);
        _logger.LogInformation("Cart cleared");
        
        TempData["Success"] = "Cart cleared.";
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Gets cart summary for AJAX requests (mini-cart in header).
    /// </summary>
    [HttpGet]
    public IActionResult Summary()
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        return Json(new { itemCount = cart.ItemCount, totalAmount = cart.TotalAmount });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}
