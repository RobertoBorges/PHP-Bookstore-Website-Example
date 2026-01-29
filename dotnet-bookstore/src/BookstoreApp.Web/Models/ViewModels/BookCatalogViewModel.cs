using BookstoreApp.Data.Entities;
using BookstoreApp.Web.Models.DTOs;

namespace BookstoreApp.Web.Models.ViewModels;

public class BookCatalogViewModel
{
    public IEnumerable<Book> Books { get; set; } = Enumerable.Empty<Book>();
    public ShoppingCart Cart { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>();
}
