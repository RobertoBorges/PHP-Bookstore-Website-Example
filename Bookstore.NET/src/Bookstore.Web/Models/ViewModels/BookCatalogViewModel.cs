using System.Collections.Generic;
using Bookstore.Web.Models.Entities;
using Bookstore.Web.Models.DTOs;

namespace Bookstore.Web.Models.ViewModels;

public class BookCatalogViewModel
{
    public IEnumerable<Book> Books { get; set; } = new List<Book>();
    public IEnumerable<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    public decimal CartTotal { get; set; }
}
