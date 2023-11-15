using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WT.WebApplication.Data.Account;
using WT.WebApplication.Services;
using WT.WebApplication.ViewModels;

namespace WT.WebApplication.Pages.Account
{
    public class RegisterModel : PageModel
    {

        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        [BindProperty]
        public RegisterViewModel RegisterViewModel { get; set; } = new RegisterViewModel();

        public RegisterModel(UserManager<User> userManager , IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }
        
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Validate Email Address (optional)

            // Create the user
            var user = new User
            {
                Email = RegisterViewModel.Email,
                UserName = RegisterViewModel.Email,
            };

            var claimDepartment = new Claim("Department", RegisterViewModel.Department);
            var claimPosition = new Claim("Position", RegisterViewModel.Position);

            var result = await _userManager.CreateAsync(user, RegisterViewModel.Password);
            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, claimDepartment);
                await _userManager.AddClaimAsync(user, claimPosition);

                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                //////////////////////////////////////////////////////////////
                // To trigger the email confirmation flow, use the code below
                //////////////////////////////////////////////////////////////

                var confirmationLink = Url.PageLink(pageName: "/Account/ConfirmEmail",
                    values: new { userId = user.Id, token = confirmationToken }) ?? "";

                await _emailService.SendAsync("frankliu.associates@gmail.com",
                    user.Email,
                    "Please confirm your email",
                    $"Please click on this link to confirm your email address: {confirmationLink}");

                return RedirectToPage("/Account/Login");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }

                return Page();
            }
        }
    }
}
