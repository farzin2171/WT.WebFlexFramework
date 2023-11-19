using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using WT.WebApplication.Data.Account;
using WT.WebApplication.ViewModels;

namespace WT.WebApplication.Pages.Account
{
    [Authorize]
    public class AuthenticatorWithMFASetupModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        [BindProperty]
        public SetupMFAViewModel viewModel { get; set; }

        [BindProperty]
        public bool Succeeded { get; set; }

        public AuthenticatorWithMFASetupModel(UserManager<User> userManager)
        {
            _userManager = userManager;
            viewModel = new SetupMFAViewModel();
            Succeeded = false;
        }
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                var key = await _userManager.GetAuthenticatorKeyAsync(user);
                viewModel.Key = key ?? String.Empty;
                viewModel.QRCodeBytes = GenerateQRCodeBytes(
                   "my web app",
                   this.viewModel.Key,
                   user.Email ?? string.Empty);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(base.User);
            if (user != null && await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    _userManager.Options.Tokens.AuthenticatorTokenProvider,
                    viewModel.SecurityCode))
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                this.Succeeded = true;
            }
            else
            {
                ModelState.AddModelError("AuthenticatorSetup", "Something went wrong with the authenticator setup.");
            }

            return Page();
        }

        private Byte[] GenerateQRCodeBytes(string provider, string key, string userEmail)
        {
            var qrCodeGenerator = new QRCodeGenerator();
            var qrCodeData = qrCodeGenerator.CreateQrCode(
                $"otpauth://totp/{provider}:{userEmail}?secret={key}&issuer={provider}",
                QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}
