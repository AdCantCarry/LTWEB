using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public class Brand
    {
        public int BrandId { get; set; }

        [Required]
        public string Name { get; set; } = ""; // VD: Apple, Samsung, SKQ, etc.

        public string? Description { get; set; }

        // Optional: Logo, quốc gia, ngành công nghệ...
        public string? LogoUrl { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
