using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Web.Models.Entities;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    [MaxLength(50)]
    public string? BookId { get; set; }

    [Required]
    public DateTimeOffset PurchaseDate { get; set; } = DateTimeOffset.UtcNow;

    [Range(1, 999)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal TotalPrice { get; set; }

    [MaxLength(1)]
    public string Status { get; set; } = "N";

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    [ForeignKey(nameof(BookId))]
    public Book? Book { get; set; }
}
