using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookstoreApp.Web.Services.Interfaces;
using BookstoreApp.Web.Models.ViewModels;

namespace BookstoreApp.Web.Controllers;

/// <summary>
/// Handles checkout process and order history.
/// Migrated from: checkout.php
/// </summary>
[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        ICartService cartService,
        ICustomerService customerService,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _cartService = cartService;
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the checkout page with cart items and customer info.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        if (cart.ItemCount == 0)
        {
            TempData["Warning"] = "Your cart is empty.";
            return RedirectToAction("Index", "Home");
        }

        var entraId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        var customer = entraId != null 
            ? await _customerService.GetCustomerByEntraIdAsync(entraId) 
            : null;
        
        var viewModel = new CheckoutViewModel
        {
            Cart = cart,
            FullName = customer?.FullName ?? User.FindFirst("name")?.Value ?? "",
            Email = customer?.Email ?? User.FindFirst("preferred_username")?.Value ?? "",
            PhoneNumber = customer?.PhoneNumber ?? "",
            IdentificationNumber = customer?.IdentificationNumber,
            Gender = customer?.Gender ?? "",
            Address = customer?.Address ?? ""
        };

        return View(viewModel);
    }

    /// <summary>
    /// Processes the checkout and creates orders.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        model.Cart = cart;

        if (cart.ItemCount == 0)
        {
            TempData["Warning"] = "Your cart is empty.";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var entraId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value 
                ?? throw new InvalidOperationException("User not authenticated");
            
            // Get or create customer
            var customer = await _customerService.GetOrCreateCustomerAsync(
                entraId, 
                model.Email, 
                model.FullName);
            
            // Update customer info
            customer.FullName = model.FullName;
            customer.Email = model.Email;
            customer.PhoneNumber = model.PhoneNumber;
            customer.IdentificationNumber = model.IdentificationNumber;
            customer.Gender = model.Gender;
            customer.Address = model.Address;
            customer = await _customerService.UpdateCustomerAsync(customer);

            // Create orders for each cart item
            var orders = await _orderService.CreateOrdersFromCartAsync(customer, cart);
            
            _logger.LogInformation(
                "Created {OrderCount} orders for customer {CustomerId}",
                orders.Count(), customer.CustomerId);

            // Clear the cart
            _cartService.ClearCart(HttpContext.Session);

            // Redirect to confirmation
            return RedirectToAction(nameof(Confirmation), new { customerId = customer.CustomerId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing checkout");
            ModelState.AddModelError("", "An error occurred processing your order. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Displays order confirmation after successful checkout.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Confirmation(int customerId)
    {
        var entraId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (entraId == null)
        {
            return NotFound();
        }

        var customer = await _customerService.GetCustomerByEntraIdAsync(entraId);
        if (customer == null || customer.CustomerId != customerId)
        {
            return NotFound();
        }

        var orders = await _orderService.GetOrderHistoryAsync(customer.CustomerId);
        var recentOrders = orders.Where(o => o.PurchaseDate >= DateTime.UtcNow.AddMinutes(-1)).ToList();
        
        if (!recentOrders.Any())
        {
            return NotFound();
        }

        var viewModel = new OrderConfirmationViewModel
        {
            Customer = customer,
            Orders = recentOrders,
            OrderDate = recentOrders.First().PurchaseDate
        };

        return View(viewModel);
    }

    /// <summary>
    /// Displays order history for the current customer.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var entraId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (entraId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var customer = await _customerService.GetCustomerByEntraIdAsync(entraId);
        if (customer == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var orders = await _orderService.GetOrderHistoryAsync(customer.CustomerId);
        return View(orders);
    }
}
