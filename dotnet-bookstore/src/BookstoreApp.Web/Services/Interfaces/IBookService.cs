using BookstoreApp.Data.Entities;

namespace BookstoreApp.Web.Services.Interfaces;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<(IEnumerable<Book> Books, int TotalCount)> GetBooksPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        string? category = null);
    Task<Book?> GetBookByIdAsync(string bookId);
    Task<IEnumerable<string>> GetCategoriesAsync();
}
