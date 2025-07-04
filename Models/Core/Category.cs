namespace TechNova.Models.Core
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string MainImageUrl { get; set; } // ✅ Thêm dòng này
        public string? SvgUrl { get; set; } // hoặc SvgContent nếu bạn muốn lưu nội dung SVG
        public string? Description { get; set; } // ✅ Thêm dòng này

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation
        public List<Product>? Products { get; set; }
    }

}
