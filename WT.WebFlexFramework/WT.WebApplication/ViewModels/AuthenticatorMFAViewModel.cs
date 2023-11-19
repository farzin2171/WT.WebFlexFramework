using System.ComponentModel.DataAnnotations;

namespace WT.WebApplication.ViewModels
{
    public class AuthenticatorMFAViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string SecurityCode { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
       
    }
}
