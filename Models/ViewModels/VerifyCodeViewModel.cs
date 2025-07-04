using System.ComponentModel.DataAnnotations;

namespace TechNova.Models.ViewModels
{
    public class VerifyCodeViewModel
    {
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã gồm 6 chữ số")]
        public string Code { get; set; }
    }

}
