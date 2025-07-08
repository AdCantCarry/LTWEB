using TechNova.Models.Auth;

namespace TechNova.Models.Core
{
    public class ProductReview
    {
        public int ProductReviewId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int UserId { get; set; } // 👈 Sửa lại từ string → int

        public User User { get; set; } // 👈 Thêm navigation property

        public int Rating { get; set; }
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }


}
