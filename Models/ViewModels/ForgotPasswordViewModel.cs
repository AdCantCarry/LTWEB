using System.ComponentModel.DataAnnotations;

namespace TechNova.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
