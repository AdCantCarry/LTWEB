using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } // Thêm dòng này
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public ICollection<Address> Addresses { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        public string Role { get; set; } = "User"; // hoặc "Admin"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? AvatarUrl { get; set; } // ảnh đại diện

        public bool IsActive { get; set; } = true;
    }
}

