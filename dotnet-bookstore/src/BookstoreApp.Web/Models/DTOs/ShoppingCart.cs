namespace BookstoreApp.Web.Models.DTOs;

public class ShoppingCart
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public int ItemCount => Items.Sum(i => i.Quantity);
}
