using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechNova.Models.Auth;

namespace TechNova.Models.Core
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Street { get; set; }

        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }

        public bool IsDefault { get; set; }

        // Khóa ngoại liên kết User
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
