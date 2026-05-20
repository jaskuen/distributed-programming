using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserStore _userStore;

    public RegisterModel(UserStore userStore)
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
        var result = await _userStore.RegisterAsync(Login, Password);
        if (!result.Success || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Не удалось зарегистрировать пользователя.");
            return Page();
        }

        await SignInAsync(result.User);
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
