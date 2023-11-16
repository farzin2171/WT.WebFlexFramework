using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WT.WebApplication.Data.Account;
using WT.WebApplication.Services;
using WT.WebApplication.ViewModels;

namespace WT.WebApplication.Pages.Account
{
    public class LoginTwoFactorModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly SignInManager<User> _signInManager;

        [BindProperty]
        public EmailMFA EmailMFA { get; set; } = new EmailMFA();

        public LoginTwoFactorModel(UserManager<User> userManager , IEmailService emailService , SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
        }
        public async Task OnGetAsync(string email , string rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);

            var securityCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            await _emailService.SendAsync("Admin@webtech.com", email, "Web App OPT", $"Please use this code as OTP : {securityCode}");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.TwoFactorSignInAsync("Email",
                this.EmailMFA.SecurityCode,
                this.EmailMFA.RememberMe,
                false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login2FA", "You are locked out.");
                }
                else
                {
                    ModelState.AddModelError("Login2FA", "Failed to login.");
                }

                return Page();
            }
        }
    }
}
