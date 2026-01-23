using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Web.Models.Entities;

public class Cart
{
    [Key]
    public int CartId { get; set; }

    public int? CustomerId { get; set; }

    [MaxLength(50)]
    public string? BookId { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal TotalPrice { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    [ForeignKey(nameof(BookId))]
    public Book? Book { get; set; }
}
