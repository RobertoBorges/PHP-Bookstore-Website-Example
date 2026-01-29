using Microsoft.AspNetCore.Mvc;
using BookstoreApp.Web.Services.Interfaces;
using BookstoreApp.Web.Models.ViewModels;

namespace BookstoreApp.Web.Controllers;

/// <summary>
/// Handles book catalog display with search, filtering, and pagination.
/// Migrated from: index.php
/// </summary>
public class HomeController : Controller
{
    private readonly IBookService _bookService;
    private readonly ICartService _cartService;
    private readonly ILogger<HomeController> _logger;
    private const int DefaultPageSize = 8;

    public HomeController(
        IBookService bookService,
        ICartService cartService,
        ILogger<HomeController> logger)
    {
        _bookService = bookService;
        _cartService = cartService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the book catalog with optional search, category filter, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm = null,
        string? category = null,
        int page = 1,
        int pageSize = DefaultPageSize)
    {
        _logger.LogInformation(
            "Loading catalog - Search: {SearchTerm}, Category: {Category}, Page: {Page}",
            searchTerm, category, page);

        var (books, totalCount) = await _bookService.GetBooksPagedAsync(page, pageSize, searchTerm, category);
        var categories = await _bookService.GetCategoriesAsync();
        var cart = _cartService.GetCart(HttpContext.Session);

        var viewModel = new BookCatalogViewModel
        {
            Books = books,
            Cart = cart,
            SearchTerm = searchTerm,
            Category = category,
            Categories = categories,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return View(viewModel);
    }

    /// <summary>
    /// Displays details for a single book.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            _logger.LogWarning("Book not found: {BookId}", id);
            return NotFound();
        }

        return View(book);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
