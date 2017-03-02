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
                    MailMessage EMailMessage = new MailMessage("mtidna2017@gmail.com", MyUser.Email);
                    EMailMessage.Subject = "Email Confirmation";
                    EMailMessage.IsBodyHtml = true;

                    EMailMessage.Body = $"Please Confirm your email by click this link <a href='{confirmationLink}'> Confirm Me </a>";
                    SmtpClient SC = new SmtpClient("smtp.gmail.com", 587);
                    SC.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SC.Credentials = new NetworkCredential("mtidna2017@gmail.com", "Mti_dna2017");
                    SC.EnableSsl = true;

                    SC.Send(EMailMessage);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    result.Errors.ToList().ForEach(x => ModelState.AddModelError(string.Empty, x.Description));
                    return View(Model);
                }
            }
            return View(Model);
        }
        public async Task<IActionResult> SendConfirmationEmail(string UserID)
        {
            IdentityUser MyUser = await UserManager.FindByIdAsync(UserID);
            string ConfirmationToken = await UserManager.GenerateEmailConfirmationTokenAsync(MyUser);
            string confirmationLink = Url.Action("ConfirmEmail", "Account", new { UserId = MyUser.Id, Token = ConfirmationToken }, HttpContext.Request.Scheme);
            MailMessage EMailMessage = new MailMessage("mtidna2017@gmail.com", MyUser.Email);
            EMailMessage.Subject = "Email Confirmation";
            EMailMessage.IsBodyHtml = true;

            EMailMessage.Body = $"Please Confirm your email by click this link <a href='{confirmationLink}'> Confirm Me </a>";
            SmtpClient SC = new SmtpClient("smtp.gmail.com", 587);
            SC.DeliveryMethod = SmtpDeliveryMethod.Network;
            SC.Credentials = new NetworkCredential("mtidna2017@gmail.com", "Mti_dna2017");
            SC.EnableSsl = true;

            SC.Send(EMailMessage);
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string UserId , string Token)
        {
            IdentityUser MyUser = await UserManager.FindByIdAsync(UserId);
            IdentityResult result =  await UserManager.ConfirmEmailAsync(MyUser, Token);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View("Error",new ErrorViewModel { Message = "We Can't Confirm your email",Solution = "You may need to resend the confirmation email again" ,Link = $"<a class='btn btn-info' href='{Url.Action("SendConfirmationEmail", "Account", new { UserId = UserManager.GetUserId(User) }, HttpContext.Request.Scheme)}'> Resend Email Confirmation </a>" } );
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
                MailMessage EMailMessage = new MailMessage("mtidna2017@gmail.com", User.Email);
                EMailMessage.Subject = "Password Confirmation";
                EMailMessage.IsBodyHtml = true;
                EMailMessage.Body = $"Please Confirm your password reset by click this link <a href='{confirmationLink}'> Reset Me </a>";
                SmtpClient SC = new SmtpClient("smtp.gmail.com", 587);
                SC.DeliveryMethod = SmtpDeliveryMethod.Network;
                SC.Credentials = new NetworkCredential("mtidna2017@gmail.com", "Mti_dna2017");
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
                    return View("Error", new ErrorViewModel { Message = "We Can't reset your password ", Solution = "You may need to try again later"});
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
                var SignInResult = await SignInManager.PasswordSignInAsync(await UserManager.FindByEmailAsync(Model.Email), Model.Password, Model.RememberMe, false);
                if (SignInResult.Succeeded)
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
