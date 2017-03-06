using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DataAccessLayer.Services;
using System.Text;

namespace SequenceAlignment.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IRepository Repo;
        private readonly UserManager<IdentityUser> UserManager;
        public ProfileController(IRepository _Repo , UserManager<IdentityUser> _UserManager)
        {
            Repo = _Repo;
            UserManager = _UserManager;
        }

        [HttpGet("[action]/{AlignmentID}")]
        public IActionResult Display(string AlignmentID)
        {
            return View("Display",AlignmentID);         
        }
        [HttpGet("[action]/{AlignmentID}")]
        public virtual IActionResult GetFile(string AlignmentID)
        {
            return Content(Encoding.UTF8.GetString(Repo.GetAlignmentJobById(AlignmentID).ByteText));
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "History";

            return View(Repo.GetHistory(UserManager.GetUserId(User)));
        }

        [Route("[action]/{AlignmentID}")]   
        public IActionResult DownloadFile(string AlignmentID)
        {
            return File(Repo.GetAlignmentJobById(AlignmentID).ByteText, "text/plain", AlignmentID + "_Alignment_Result.txt");
        }

        [Route("[action]/{AlignmentID}")]
        public IActionResult DeleteFile(string AlignmentID)
        {
            Repo.DeleteAlignmentJob(AlignmentID);
            return RedirectToAction("Index", "Profile");
        }
    }
}
