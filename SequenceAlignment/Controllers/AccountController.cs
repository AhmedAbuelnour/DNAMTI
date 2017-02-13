using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SequenceAlignment.ViewModels;

namespace SequenceAlignment.Controllers
{
    public class AccountController : Controller
    {
        UserManager<IdentityUser> UserManager;
        SignInManager<IdentityUser> SignInManager;
        public AccountController(UserManager<IdentityUser> _UserManager, SignInManager<IdentityUser> _SignInManager)
        {
            UserManager = _UserManager;
            SignInManager = _SignInManager;
        }
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(RegistrationViewModel Model)
        {
            if (ModelState.IsValid)
            {
                IdentityUser User = new IdentityUser { UserName = Model.UserName, Email = Model.Email, PasswordHash = Model.Password };
                IdentityResult result = await UserManager.CreateAsync(User, User.PasswordHash);
                if (result.Succeeded)
                {
                    await SignInManager.PasswordSignInAsync(User.Email, User.PasswordHash, false, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    result.Errors.ToList().ForEach(x => ModelState.AddModelError(string.Empty, x.Description));
                    return View(User);
                }
            }
            return View(User);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel Model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var xx = await SignInManager.PasswordSignInAsync(await UserManager.FindByEmailAsync(Model.Email), Model.Password, Model.RememberMe, false);
                if (xx.Succeeded)
                {
                    if (string.IsNullOrEmpty(returnUrl))
                        return RedirectToAction("Index", "Home");
                    else
                        return Redirect(returnUrl);
                }
                else
                {
                    return View(Model);
                }
            }
            else
            {
                return View(Model);
            }
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> Logout()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
