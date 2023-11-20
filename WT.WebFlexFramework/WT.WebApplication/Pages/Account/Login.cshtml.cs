using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WT.WebApplication.Data.Account;
using WT.WebApplication.Infrastructure.Authorization;
using WT.WebApplication.ViewModels;

namespace WT.WebApplication.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        [BindProperty]
        public CredentialViewModel Credential { get; set; } = new CredentialViewModel();

        [BindProperty]
        public IEnumerable<AuthenticationScheme> ExternalLoginProviders { get; set; }

        private readonly SignInManager<User> _signInManager;

        public LoginModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            ReturnUrl = "/";
        }


        public async Task OnGetAsync()
        {
            this.ExternalLoginProviders = await _signInManager.GetExternalAuthenticationSchemesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.PasswordSignInAsync(
                this.Credential.Email,
                this.Credential.Password,
                this.Credential.RememberMe,
                false);

            if (result.Succeeded)
            {
                return LocalRedirect(ReturnUrl);
            }
            else
            {
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("/Account/LoginTwoFactorWithAuthenticator",
                        new
                        {
                            RememberMe = Credential.RememberMe
                        });
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login", "You are locked out.");
                }
                else
                {
                    ModelState.AddModelError("Login", "Failed to login.");
                }

                return Page();
            }
        }

        public IActionResult OnPostLoginExternally(string provider)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("ExternalLoginCallback", "Account"),
                Items =
                {
                    { "uru", ReturnUrl },
                    { "scheme", "Google" }
                }
            };

            return Challenge(props,"Google");
        }
    }
}
