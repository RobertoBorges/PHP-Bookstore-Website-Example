using System.Collections.Generic;
using Bookstore.Web.Models.DTOs;

namespace Bookstore.Web.Models.ViewModels;

public class CheckoutViewModel
{
    public IEnumerable<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    public decimal CartTotal { get; set; }
}
