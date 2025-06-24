namespace TechNova.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Color { get; set; }
        public string Storage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ShippingMethod { get; set; }
        public decimal ShippingFee { get; set; }

        public decimal TotalPrice => (Price * Quantity) + ShippingFee;
    }

}
