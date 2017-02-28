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
            MailMessage EMailMessage = new MailMessage(Model.Email,"mtidna2017@gmail.com");

            EMailMessage.Subject = "MTI DNA - Contact";
            EMailMessage.Body = Model.Message;
            EMailMessage.IsBodyHtml = false;
            using (SmtpClient SC = new SmtpClient("smtp.gmail.com", 587))
            {
                SC.DeliveryMethod = SmtpDeliveryMethod.Network;
                SC.Credentials = new NetworkCredential("mtidna2017@gmail.com", "Mti_dna2017");
                SC.EnableSsl = true;
                SC.Send(EMailMessage);
            }
            
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}
