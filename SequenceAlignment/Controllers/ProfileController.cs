using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Service;

namespace SequenceAlignment.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IRepository Repo;
        public ProfileController(IRepository _Repo)
        {
            Repo = _Repo;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View(Repo.GetAlignmentJobs());
        }

        [Route("[action]/AlignmentID")]   
        public IActionResult DownloadFile(string AlignmentID)
        {
            return File(Repo.GetAlignmentJobById(AlignmentID).ByteText, "text/plain", AlignmentID + "_Alignment_Result.txt");
        }

        [Route("[action]/AlignmentID")]
        public IActionResult DeleteFile(string AlignmentID)
        {
            Repo.DeleteAlignmentJob(AlignmentID);
            return RedirectToAction("Index", "Profile");
        }
    }
}
