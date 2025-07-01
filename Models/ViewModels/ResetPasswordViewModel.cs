using System.ComponentModel.DataAnnotations;

namespace TechNova.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
