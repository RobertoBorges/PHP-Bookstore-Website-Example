using System;

namespace Bookstore.Web.Models.DTOs;

public class OrderDetailsDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string? CustomerAddress { get; set; }
    public string BookTitle { get; set; } = null!;
    public string? BookImage { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTimeOffset PurchaseDate { get; set; }
    public string Status { get; set; } = null!;
}
