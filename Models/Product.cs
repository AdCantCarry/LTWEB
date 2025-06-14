namespace TechNova.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int? DiscountPercent { get; set; }  // Giảm giá (%), có thể null

        public decimal DiscountedPrice =>
        DiscountPercent.HasValue
            ? Price * (1 - DiscountPercent.Value / 100m)
            : Price;

        public string ImageUrl { get; set; } = "/images/default.jpg";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

    }
}
