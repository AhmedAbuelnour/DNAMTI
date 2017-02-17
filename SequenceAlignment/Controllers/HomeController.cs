using Microsoft.AspNetCore.Mvc;
using SequenceAlignment.ViewModels;

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
            // TODO - Sending an email to MTI IT 
            return View();
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}
