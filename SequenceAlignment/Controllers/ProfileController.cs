using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SequenceAlignment.Services;

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
        // GET: /<controller>/
        public IActionResult Index()
        {
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
