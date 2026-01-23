using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Web.Models.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    public Guid AzureAdObjectId { get; set; }

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [MaxLength(128)]
    public string? UserName { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastLoginAt { get; set; }

    // Navigation property
    public Customer? Customer { get; set; }
}
