using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly UserStore _userStore;

    public LoginModel(UserStore userStore)
    {
        _userStore = userStore;
    }

    [BindProperty]
    public string Login { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        UserRecord? user = await _userStore.ValidateCredentialsAsync(Login, Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return Page();
        }

        await SignInAsync(user);
        return RedirectToPage("/Index");
    }

    private async Task SignInAsync(UserRecord user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Login),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
