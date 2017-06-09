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
            ViewData["Title"] = "Index";

            return View();
        }
        public IActionResult About()
        {
            ViewData["Title"] = "About";

            return View();
        }
        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact";

            return View();
        }
        [HttpPost]
        public IActionResult Contact(ContactViewModel Model)
        {
            MailMessage EMailMessage = new MailMessage(Model.Email,"mtidna2017@gmail.com");

            EMailMessage.Subject = "MTI DNA - Contact";
            EMailMessage.Body = Model.Message;
            EMailMessage.IsBodyHtml = false;
            using (SmtpClient SC = new SmtpClient("smtp.gmail.com", 587))
            {
                SC.DeliveryMethod = SmtpDeliveryMethod.Network;
                SC.Credentials = new NetworkCredential("mtidna2017@gmail.com", "Mti_dna2017");
                SC.EnableSsl = true;
                try
                {
                    SC.Send(EMailMessage);
                }
                catch
                {
                    return View("Error", new ErrorViewModel { Message = "Couldn't Send the email", Solution = "Try again later" });
                }
            }
            
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}
