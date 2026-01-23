using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Web.Services.Interfaces;

namespace Bookstore.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ILogger<CartController> _logger;
    private readonly ICartService _cartService;

    public CartController(ILogger<CartController> logger, ICartService cartService)
    {
        _logger = logger;
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        // Azure AD provides Object ID in this claim
        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var cartItems = await _cartService.GetCartItemsAsync(userId);
        var cartTotal = await _cartService.GetCartTotalAsync(userId);

        ViewBag.CartTotal = cartTotal;
        return View(cartItems);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int cartId)
    {
        // Azure AD provides Object ID in this claim
        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        try
        {
            await _cartService.RemoveCartItemAsync(cartId, userId);
            TempData["Success"] = "Item removed from cart";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from cart");
            TempData["Error"] = "Failed to remove item";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmptyCart()
    {
        // Azure AD provides Object ID in this claim
        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        try
        {
            await _cartService.EmptyCartAsync(userId);
            TempData["Success"] = "Cart emptied";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error emptying cart");
            TempData["Error"] = "Failed to empty cart";
        }

        return RedirectToAction(nameof(Index));
    }
}
