using BookstoreApp.Data.Entities;

namespace BookstoreApp.Web.Services.Interfaces;

public interface ICustomerService
{
    Task<Customer?> GetCustomerByEntraIdAsync(string entraIdObjectId);
    Task<Customer> GetOrCreateCustomerAsync(string entraIdObjectId, string? email, string? name);
    Task<Customer> UpdateCustomerAsync(Customer customer);
}
