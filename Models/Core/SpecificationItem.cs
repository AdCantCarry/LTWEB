using System.ComponentModel.DataAnnotations;

namespace TechNova.Models.Core
{
    // Mỗi dòng thông số: ví dụ "Công nghệ cảm biến", "DPI", "Trọng lượng"
    public class SpecificationItem
    {
        public int SpecificationItemId { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public int GroupId { get; set; }
        public SpecificationGroup Group { get; set; }
        public int DisplayOrder { get; set; } = 0; // ✅ Thêm dòng này
        public int ItemName { get; set; } = 0; // ✅ Thêm dòng này

        public ICollection<ProductSpecification> ProductSpecifications { get; set; } = new List<ProductSpecification>();
    }

}
