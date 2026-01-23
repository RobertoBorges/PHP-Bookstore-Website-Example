using System.ComponentModel.DataAnnotations;

namespace Bookstore.Web.Models.DTOs;

public class CheckoutDto
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(128)]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
    public string Name { get; set; } = null!;

    [MaxLength(20)]
    [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "IC number can only contain numbers and dashes")]
    public string? IdNumber { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Phone(ErrorMessage = "Invalid phone number")]
    [MaxLength(20)]
    [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "Phone can only contain numbers and dashes")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = null!;

    [Required(ErrorMessage = "Address is required")]
    [MaxLength(500)]
    public string Address { get; set; } = null!;
}
