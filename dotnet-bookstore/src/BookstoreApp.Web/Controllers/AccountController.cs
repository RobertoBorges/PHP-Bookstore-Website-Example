using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookstoreApp.Web.Services.Interfaces;
using BookstoreApp.Web.Models.ViewModels;

namespace BookstoreApp.Web.Controllers;

/// <summary>
/// Handles user profile management.
/// Migrated from: edituser.php, register.php
/// Note: Authentication is handled by Entra ID via Microsoft.Identity.Web
/// </summary>
[Authorize]
public class AccountController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        ICustomerService customerService,
        ILogger<AccountController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the user profile page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var entraId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        var customer = entraId != null 
            ? await _customerService.GetCustomerByEntraIdAsync(entraId) 
            : null;
        
        var viewModel = new ProfileViewModel
        {
            FullName = customer?.FullName ?? User.FindFirst("name")?.Value ?? "",
            Email = customer?.Email ?? User.FindFirst("preferred_username")?.Value ?? "",
            PhoneNumber = customer?.PhoneNumber ?? "",
            IdentificationNumber = customer?.IdentificationNumber,
            Gender = customer?.Gender ?? "",
            Address = customer?.Address ?? ""
        };

        return View(viewModel);
    }

    /// <summary>
    /// Updates the user profile.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var entraId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value 
                ?? throw new InvalidOperationException("User not authenticated");
            
            var customer = await _customerService.GetOrCreateCustomerAsync(
                entraId,
                model.Email,
                model.FullName);
            
            customer.FullName = model.FullName;
            customer.Email = model.Email;
            customer.PhoneNumber = model.PhoneNumber;
            customer.IdentificationNumber = model.IdentificationNumber;
            customer.Gender = model.Gender;
            customer.Address = model.Address;
            
            await _customerService.UpdateCustomerAsync(customer);

            _logger.LogInformation("Profile updated for customer {CustomerId}", customer.CustomerId);
            TempData["Success"] = "Profile updated successfully.";
            
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            ModelState.AddModelError("", "An error occurred updating your profile. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Displays access denied page.
    /// </summary>
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
