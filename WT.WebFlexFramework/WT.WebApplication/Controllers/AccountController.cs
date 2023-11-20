using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WT.WebApplication.Data.Account;
using WT.WebApplication.Services;

namespace WT.WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly IEmailService emailService;


        public AccountController(SignInManager<User> signInManager,
                                 UserManager<User> userManager,
                                 IEmailService emailService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailService = emailService;
        }


        public async Task<IActionResult> ExternalLoginCallback()
        {

           var result = await HttpContext.AuthenticateAsync("temp");

            if (!result.Succeeded)
            {
                throw new Exception("external login failed");
            }


            var extUser = result.Principal;

            var sub = extUser?.FindFirst(ClaimTypes.NameIdentifier).Value;

            var issuer = result.Properties.Items["scheme"];


            var emailClaim = result.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            var userClaim = result.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);

            if (emailClaim != null && userClaim != null)
            {
                var user = await userManager.FindByEmailAsync(emailClaim.Value);

                if(user != null)
                {
                    await signInManager.SignInAsync(user, false);
                    await HttpContext.SignOutAsync("temp");
                }
                else
                {
                    // Create the user
                    var newUser = new User
                    {
                        Email = emailClaim.Value,
                        UserName = userClaim.Value.Replace(" ","-"),
                    };

                    var claimDepartment = new Claim("Department", "Guest");
                    var claimPosition = new Claim("Position", "Guest");

                    var newUserResult = await userManager.CreateAsync(newUser, "qazWSX_123");

                    if (result.Succeeded)
                    {
                        await userManager.AddClaimAsync(newUser, claimDepartment);
                        await userManager.AddClaimAsync(newUser, claimPosition);

                        await emailService.SendAsync("Admin@webtech.com",
                            newUser.Email,
                            "You are new In the system",
                            $"If you want to loging with login page use this password: qazWSX_123");

                        await signInManager.SignInAsync(newUser, false);
                        await HttpContext.SignOutAsync("temp");
                    }
                }
               
            }


            return RedirectToPage("/Index");
        }

    }
}
