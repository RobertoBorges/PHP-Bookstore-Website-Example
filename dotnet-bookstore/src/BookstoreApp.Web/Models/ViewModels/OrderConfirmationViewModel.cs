using BookstoreApp.Data.Entities;

namespace BookstoreApp.Web.Models.ViewModels;

public class OrderConfirmationViewModel
{
    public Customer Customer { get; set; } = null!;
    public IEnumerable<Order> Orders { get; set; } = Enumerable.Empty<Order>();
    public decimal TotalAmount => Orders.Sum(o => o.TotalPrice);
    public DateTime OrderDate { get; set; }
}
