using Microsoft.EntityFrameworkCore;
using BookstoreApp.Data;
using BookstoreApp.Data.Entities;
using BookstoreApp.Web.Services.Interfaces;

namespace BookstoreApp.Web.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly BookstoreDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(BookstoreDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Customer?> GetCustomerByEntraIdAsync(string entraIdObjectId)
    {
        _logger.LogInformation("Fetching customer by Entra ID {EntraId}", entraIdObjectId);
        return await _context.Customers.FirstOrDefaultAsync(c => c.EntraIdObjectId == entraIdObjectId);
    }

    public async Task<Customer> GetOrCreateCustomerAsync(string entraIdObjectId, string? email, string? name)
    {
        var customer = await GetCustomerByEntraIdAsync(entraIdObjectId);

        if (customer == null)
        {
            _logger.LogInformation("Creating new customer for Entra ID {EntraId}", entraIdObjectId);
            customer = new Customer
            {
                EntraIdObjectId = entraIdObjectId,
                Email = email,
                FullName = name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        _logger.LogInformation("Updating customer {CustomerId}", customer.CustomerId);
        customer.UpdatedAt = DateTime.UtcNow;
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return customer;
    }
}
