using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Bookstore.Web.Data;
using Bookstore.Web.Models.Entities;
using Bookstore.Web.Services.Interfaces;

namespace Bookstore.Web.Services.Implementations;

public class BookService : IBookService
{
    private readonly BookstoreDbContext _context;

    public BookService(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        return await _context.Books
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(string bookId)
    {
        return await _context.Books
            .FirstOrDefaultAsync(b => b.BookId == bookId);
    }
}
