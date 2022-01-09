using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty, DisplayName("Имя пользователя")]
        public string? UserName { get; set; }

        [BindProperty, DataType(DataType.Password), DisplayName("Пароль")]
        public string? Password { get; set; }

        public string? Message { get; set; }


        public async Task<IActionResult> OnPost()
        {
            var user = _configuration.GetSection("SiteUser").Get<SiteUser>();

            if (user is null) throw new Exception("User configuration not found");

            if (UserName == user.UserName)
            {
                var passwordHasher = new PasswordHasher<string>();

                if (passwordHasher.VerifyHashedPassword(user.UserName, user.Password, Password) == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, UserName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return Redirect("/");
                }
            }
            Message = "Неправильный логин и/или пароль";
            return Page();
        }
    }
}
