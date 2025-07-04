using System.ComponentModel.DataAnnotations.Schema;
namespace TechNova.Models.Core

    {

    public class Product
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public decimal Price { get; set; }
        public int? DiscountPercent { get; set; }

        public decimal DiscountedPrice =>
            DiscountPercent.HasValue
                ? Price * (1 - DiscountPercent.Value / 100m)
                : Price;

        [Column("ImageUrl")] // ánh xạ cột DB tên là ImageUrl
        public string MainImageUrl { get; set; } = "/images/default.jpg";

        public string? SubImage1Url { get; set; }
        public string? SubImage2Url { get; set; }
        public string? SubImage3Url { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Color { get; set; }
        public bool HasColor { get; set; } // true nếu sản phẩm có lựa chọn màu

        public string Storage { get; set; }
        public bool HasStorage { get; set; }

        public int? BrandId { get; set; } // FK
        public Brand? Brand { get; set; } // Navigation
        public int StockQuantity { get; set; } = 0; // Số lượng tồn kho

        public bool IsActive { get; set; } = true; // Đang bán hay ngừng bán

        public DateTime? UpdatedAt { get; set; }
    }
}
