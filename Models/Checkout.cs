using TechNova.Models;
namespace TechNova.ViewModels
{
    public class Checkout
    {
        public List<CartItem> CartItems { get; set; }
        public List<Address> Addresses { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public string SelectedPaymentMethod { get; set; } // e.g. COD, Momo
        public string PaymentMethod { get; set; }  // thêm trường này


    }
}
