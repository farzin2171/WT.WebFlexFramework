using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WT.WebApplication.Infrastructure.Authorization;

namespace WT.WebApplication.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        [BindProperty]
        public Credential Credential { get; set; } = new Credential();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Verify the credential
            if (Credential.UserName == "admin" && Credential.Password == "password")
            {
                // Creating the security context
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim(ClaimTypes.Email, "admin@mywebsite.com"),
                    new Claim("Department", "HR"),
                    new Claim("Admin", "true"),
                    new Claim("Manager", "true"),
                    new Claim("EmploymentDate", "2023-01-01")
                };
                var identity = new ClaimsIdentity(claims, "pwd", ClaimTypes.Name, "role");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = Credential.RememberMe
                };

                await HttpContext.SignInAsync(claimsPrincipal, authProperties);

                return LocalRedirect(ReturnUrl);
            }

            return Page();
        }
    }
}
