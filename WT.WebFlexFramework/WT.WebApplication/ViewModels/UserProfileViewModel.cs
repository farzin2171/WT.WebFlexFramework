using System.ComponentModel.DataAnnotations;

namespace WT.WebApplication.ViewModels
{
    public class UserProfileViewModel
    {
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;
    }
}
