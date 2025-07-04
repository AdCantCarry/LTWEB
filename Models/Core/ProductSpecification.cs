using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechNova.Models.Core
{
    // Giá trị cụ thể cho từng sản phẩm
    public class ProductSpecification
    {
        [Key]
        public int ProductSpecificationId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        public int SpecificationItemId { get; set; }

        [ForeignKey("SpecificationItemId")]
        public SpecificationItem SpecificationItem { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị thông số")]
        [StringLength(500)]
        public string Value { get; set; } = string.Empty;
    }
}
