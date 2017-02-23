using Microsoft.AspNetCore.Mvc;
using SequenceAlignment.ViewModels;
using System.Net.Mail;
using System.Net;
namespace SequenceAlignment.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Contact(ContactViewModel Model)
        {
            MailMessage EMailMessage = new MailMessage();
            EMailMessage.From = new MailAddress(Model.Email);
            EMailMessage.To.Add("A7medRamadan@outlook.com");
            EMailMessage.Subject = "Hello I'm Ahmed From MVC";
            EMailMessage.Body = Model.Message;

            SmtpClient SC = new SmtpClient("smtp-mail.outlook.com", 587);
            SC.Credentials = new NetworkCredential("A7medRamadan@outlook.com", "Password");
            SC.EnableSsl = true;

            SC.Send(EMailMessage);

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}
