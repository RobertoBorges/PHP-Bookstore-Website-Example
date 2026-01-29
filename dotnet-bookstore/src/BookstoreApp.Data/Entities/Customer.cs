using System.ComponentModel.DataAnnotations;

namespace BookstoreApp.Data.Entities;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    // Entra ID identifier - replaces UserName/Password
    [Required]
    [MaxLength(100)]
    public string EntraIdObjectId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? FullName { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(20)]
    public string? IdentificationNumber { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
