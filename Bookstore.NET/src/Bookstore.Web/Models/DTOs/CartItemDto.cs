namespace Bookstore.Web.Models.DTOs;

public class CartItemDto
{
    public int CartId { get; set; }
    public string BookId { get; set; } = null!;
    public string BookTitle { get; set; } = null!;
    public string? ImagePath { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
