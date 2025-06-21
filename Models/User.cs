using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public string? Address { get; set; }

        public ICollection<Address> Addresses { get; set; } = new List<Address>();

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        //[MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = "";

        public string Role { get; set; } = "User"; // Hoặc "Admin"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? AvatarUrl { get; set; } // Đường dẫn ảnh đại diện

        public bool IsActive { get; set; } = true;
    }
}
