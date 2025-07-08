namespace TechNova.Models.Core
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; } // Đơn giá tại thời điểm đặt

        public string? Color { get; set; }     // Thêm màu
        public string? Storage { get; set; }   // Thêm dung lượng

        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
