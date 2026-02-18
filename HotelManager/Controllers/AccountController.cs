using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HotelManager.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace HotelManager.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginInputModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
    public async Task<IActionResult> Login(LoginInputModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
    if (result.Succeeded) return LocalRedirect(model.ReturnUrl ?? Url.Action("Index", "Home") ?? "/");

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterInputModel());

    [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
    public async Task<IActionResult> Register(RegisterInputModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
        foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
