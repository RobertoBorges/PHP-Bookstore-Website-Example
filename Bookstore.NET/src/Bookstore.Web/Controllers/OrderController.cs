using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Web.Models.DTOs;
using Bookstore.Web.Services.Interfaces;

namespace Bookstore.Web.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly ILogger<OrderController> _logger;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public OrderController(
        ILogger<OrderController> logger,
        IOrderService orderService,
        ICartService cartService)
    {
        _logger = logger;
        _orderService = orderService;
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        // Azure AD provides Object ID in this claim
        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var cartItems = await _cartService.GetCartItemsAsync(userId);
        if (!cartItems.Any())
        {
            TempData["Error"] = "Your cart is empty";
            return RedirectToAction("Index", "Home");
        }

        var cartTotal = await _cartService.GetCartTotalAsync(userId);
        ViewBag.CartTotal = cartTotal;
        ViewBag.CartItems = cartItems;

        var dto = new CheckoutDto
        {
            Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutDto dto)
    {
        if (!ModelState.IsValid)
        {
            // Azure AD provides Object ID in this claim
            var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            if (!string.IsNullOrEmpty(userId))
            {
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var cartTotal = await _cartService.GetCartTotalAsync(userId);
                ViewBag.CartTotal = cartTotal;
                ViewBag.CartItems = cartItems;
            }
            return View(dto);
        }

        try
        {
            // Azure AD provides Object ID in this claim
            var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var orderId = await _orderService.CreateOrderFromCartAsync(userId, dto);
            TempData["Success"] = "Order placed successfully";
            return RedirectToAction(nameof(Confirmation), new { orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            ModelState.AddModelError("", "Failed to create order. Please try again.");
            
            // Azure AD provides Object ID in this claim
            var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            if (!string.IsNullOrEmpty(userId))
            {
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var cartTotal = await _cartService.GetCartTotalAsync(userId);
                ViewBag.CartTotal = cartTotal;
                ViewBag.CartItems = cartItems;
            }
            
            return View(dto);
        }
    }

    public async Task<IActionResult> Confirmation(int orderId)
    {
        // Azure AD provides Object ID in this claim
        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var orderDetails = await _orderService.GetOrderDetailsAsync(orderId, userId);
        if (orderDetails == null)
        {
            return NotFound();
        }

        return View(orderDetails);
    }

    public async Task<IActionResult> History()
    {
        // Azure AD provides Object ID in this claim
        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var orders = await _orderService.GetOrdersByUserAsync(userId);
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        // Azure AD provides Object ID in this claim
        var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var orderDetails = await _orderService.GetOrderDetailsAsync(id, userId);
        if (orderDetails == null)
        {
            return NotFound();
        }

        return View(orderDetails);
    }
}
