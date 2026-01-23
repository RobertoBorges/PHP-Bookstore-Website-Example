using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Web.Models.Entities;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = null!;

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? IdNumber { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
    
    public ICollection<Cart> CartItems { get; set; } = new List<Cart>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
