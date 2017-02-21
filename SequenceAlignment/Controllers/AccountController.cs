using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SequenceAlignment.ViewModels;
using System.Net.Mail;
using System.Net;

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
                    IdentityUser MyUser = await UserManager.FindByEmailAsync(User.Email);
                    string ConfirmationToken = await UserManager.GenerateEmailConfirmationTokenAsync(MyUser);
                    string confirmationLink = Url.Action("ConfirmEmail", "Account", new { UserId = MyUser.Id, Token = ConfirmationToken },HttpContext.Request.Scheme);
                    MailMessage EMailMessage = new MailMessage();
                    EMailMessage.From = new MailAddress("A7medRamadan@outlook.com");
                    EMailMessage.To.Add(MyUser.Email);
                    EMailMessage.Subject = "Email Confirmation";
                    EMailMessage.IsBodyHtml = true;
                    EMailMessage.Body = $"Please Confirm your email by click this link {confirmationLink}";
                    SmtpClient SC = new SmtpClient("smtp-mail.outlook.com", 587);
                    SC.Credentials = new NetworkCredential("A7medRamadan@outlook.com", "A7med1994");
                    SC.EnableSsl = true;

                    SC.Send(EMailMessage);

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
        public async Task<IActionResult> ConfirmEmail(string UserId , string Token)
        {
            IdentityUser User = await UserManager.FindByIdAsync(UserId);
            IdentityResult result =  await UserManager.ConfirmEmailAsync(User, Token);
            if (result.Succeeded)
            {
                ViewBag.Message = "Email confirmed successfully!";
                return View("Success");
            }
            else
            {
                ViewBag.Message = "Error while confirming your email!";
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string Email)
        {
            IdentityUser User = await UserManager.FindByEmailAsync(Email);
            if(User != null)
            {
                string ConfirmationToken = await UserManager.GeneratePasswordResetTokenAsync(User);
                string confirmationLink = Url.Action("ResetPassword", "Account", new { UserId = User.Id, Token = ConfirmationToken }, HttpContext.Request.Scheme);
                MailMessage EMailMessage = new MailMessage();
                EMailMessage.From = new MailAddress("WebSiteEmailAccount@outlook.com");
                EMailMessage.To.Add(User.Email);
                EMailMessage.Subject = "Email Confirmation";
                EMailMessage.IsBodyHtml = false;
                EMailMessage.Body = $"Please Confirm your password reset by click this link {confirmationLink}";
                SmtpClient SC = new SmtpClient("smtp-mail.outlook.com", 587);
                SC.Credentials = new NetworkCredential("Email", "Password");
                SC.EnableSsl = true;
                SC.Send(EMailMessage);
                return View("CheckEmail");
            }
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string UserId , string Token)
        {
            ResetPasswordViewModel VM = new ResetPasswordViewModel { CodeToken = Token, UserId = UserId };
            return View(VM);
        }
        [HttpPost]
        public async Task<IActionResult> RestPassword(ResetPasswordViewModel Model)
        {
            if (ModelState.IsValid)
            {
                IdentityUser User = await UserManager.FindByIdAsync(Model.UserId);
                IdentityResult ResetResult = await UserManager.ResetPasswordAsync(User, Model.CodeToken, Model.Password);
                if (ResetResult.Succeeded)
                    return RedirectToAction("Index","Home");
                else
                {
                    ViewBag.Message = "Error while confirming your email!";
                    return View("Error");
                }
            }
            else
                return View(Model);

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
