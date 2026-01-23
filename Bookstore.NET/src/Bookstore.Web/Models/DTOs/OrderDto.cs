using System;

namespace Bookstore.Web.Models.DTOs;

public class OrderDto
{
    public int OrderId { get; set; }
    public DateTimeOffset PurchaseDate { get; set; }
    public string BookTitle { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = null!;
}
