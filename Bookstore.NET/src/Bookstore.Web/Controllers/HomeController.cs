using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Web.Models;
using Bookstore.Web.Models.DTOs;
using Bookstore.Web.Models.ViewModels;
using Bookstore.Web.Services.Interfaces;

namespace Bookstore.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBookService _bookService;
    private readonly ICartService _cartService;

    public HomeController(
        ILogger<HomeController> logger,
        IBookService bookService,
        ICartService cartService)
    {
        _logger = logger;
        _bookService = bookService;
        _cartService = cartService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetAllBooksAsync();
        
        var viewModel = new BookCatalogViewModel
        {
            Books = books,
            CartItems = Enumerable.Empty<CartItemDto>(),
            CartTotal = 0m
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            // Azure AD provides Object ID in this claim
            var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            if (!string.IsNullOrEmpty(userId))
            {
                viewModel.CartItems = await _cartService.GetCartItemsAsync(userId);
                viewModel.CartTotal = await _cartService.GetCartTotalAsync(userId);
            }
        }

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(AddToCartDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for AddToCart. Errors: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            TempData["Error"] = "Invalid input";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Azure AD provides Object ID in this claim
            var userId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User authenticated but Object ID claim is missing. Claims: {Claims}", 
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                TempData["Error"] = "Authentication error. Please try logging in again.";
                return RedirectToAction(nameof(Index));
            }

            // Get user email and name from Azure AD claims
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? 
                           User.FindFirstValue("preferred_username") ?? 
                           "user@example.com";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? 
                          User.FindFirstValue("name") ?? 
                          "Azure AD User";

            _logger.LogInformation("Adding book {BookId} (qty: {Quantity}) to cart for user {UserId}", 
                dto.BookId, dto.Quantity, userId);
            
            await _cartService.AddToCartAsync(dto.BookId, dto.Quantity, userId, userEmail, userName);
            TempData["Success"] = "Item added to cart";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart for book {BookId}", dto.BookId);
            TempData["Error"] = "Failed to add item to cart";
        }

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
