using TechNova.Models.Auth;
using TechNova.Models.Core;

namespace TechNova.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public User User { get; set; }
        public List<Address> Addresses { get; set; }
        public List<OrderViewModel> Orders { get; set; }
    }

    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public string Image { get; set; }
    }

}
