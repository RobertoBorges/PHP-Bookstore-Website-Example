using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookstoreApp.Data.Entities;

public class Book
{
    [Key]
    [MaxLength(50)]
    public string BookId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string BookTitle { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Isbn { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Price { get; set; }

    [MaxLength(128)]
    public string? Author { get; set; }

    [MaxLength(128)]
    public string? Category { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
