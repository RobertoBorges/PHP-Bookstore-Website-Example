using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookstoreApp.Data.Enums;

namespace BookstoreApp.Data.Entities;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    [MaxLength(50)]
    public string BookId { get; set; } = string.Empty;

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal TotalPrice { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Navigation properties
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey(nameof(BookId))]
    public virtual Book Book { get; set; } = null!;
}
