using Microsoft.EntityFrameworkCore;
using BookstoreApp.Data;
using BookstoreApp.Data.Entities;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Services.Implementations;

public class BookService : IBookService
{
    private readonly BookstoreDbContext _context;
    private readonly ILogger<BookService> _logger;

    public BookService(BookstoreDbContext context, ILogger<BookService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        _logger.LogInformation("Fetching all books");
        return await _context.Books.AsNoTracking().ToListAsync();
    }

    public async Task<(IEnumerable<Book> Books, int TotalCount)> GetBooksPagedAsync(
        int page, int pageSize, string? searchTerm = null, string? category = null)
    {
        _logger.LogInformation("Fetching books page {Page} with size {PageSize}", page, pageSize);

        var query = _context.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(b => 
                b.BookTitle.ToLower().Contains(term) ||
                (b.Author != null && b.Author.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category == category);
        }

        var totalCount = await query.CountAsync();
        var books = await query
            .OrderBy(b => b.BookTitle)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (books, totalCount);
    }

    public async Task<Book?> GetBookByIdAsync(string bookId)
    {
        _logger.LogInformation("Fetching book {BookId}", bookId);
        return await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == bookId);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.Books.AsNoTracking()
            .Where(b => b.Category != null)
            .Select(b => b.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
}
