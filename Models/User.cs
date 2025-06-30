using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; } = "";

        [Required]
        public string Email { get; set; } = "";

        public string? PhoneNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Address { get; set; }

        public ICollection<Address> Addresses { get; set; } = new List<Address>();

        [Required]
        public string Password { get; set; } = "";

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? AvatarUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public string? ResetCode { get; set; } // Cho phép null
        public DateTime? ResetCodeExpiry { get; set; }


    }
}
