using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Web.Models.Entities;

namespace Bookstore.Web.Services.Interfaces;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(string bookId);
}
