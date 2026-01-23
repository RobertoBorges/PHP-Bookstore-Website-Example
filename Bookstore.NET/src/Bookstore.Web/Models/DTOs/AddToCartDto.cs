using System.ComponentModel.DataAnnotations;

namespace Bookstore.Web.Models.DTOs;

public class AddToCartDto
{
    [Required]
    [MaxLength(50)]
    public string BookId { get; set; } = null!;

    [Required]
    [Range(1, 999)]
    public int Quantity { get; set; } = 1;
}
