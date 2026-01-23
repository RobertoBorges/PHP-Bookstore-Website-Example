using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Web.Models.Entities;

public class Book
{
    [Key]
    [MaxLength(50)]
    public string BookId { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(20)]
    public string? ISBN { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(128)]
    public string Author { get; set; } = null!;

    [MaxLength(128)]
    public string? Type { get; set; }

    [MaxLength(256)]
    public string? ImagePath { get; set; }

    // Navigation properties
    public ICollection<Cart> CartItems { get; set; } = new List<Cart>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
