using System.ComponentModel.DataAnnotations;
using BookstoreApp.Web.Models.DTOs;

namespace BookstoreApp.Web.Models.ViewModels;

public class CheckoutViewModel
{
    public ShoppingCart Cart { get; set; } = new();

    [Required(ErrorMessage = "Please enter your full name")]
    [MaxLength(128)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your phone number")]
    [MaxLength(20)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "ID Number")]
    public string? IdentificationNumber { get; set; }

    [Required(ErrorMessage = "Please select your gender")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your address")]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
}
