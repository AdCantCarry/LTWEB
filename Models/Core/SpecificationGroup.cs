using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TechNova.Models.Core
{
    public class SpecificationGroup
    {
        public int SpecificationGroupId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhóm")]
        public string GroupName { get; set; }

        public int DisplayOrder { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [ValidateNever] // ✅ Không validate navigation property
        public Category Category { get; set; }

        [ValidateNever] // ✅ Không validate danh sách
        public List<SpecificationItem> Items { get; set; }
    }
}
