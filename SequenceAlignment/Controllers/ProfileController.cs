using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SequenceAlignment.Controllers
{
    public class ProfileController : Controller
    {
        private readonly Models.SequenceAlignmentDbContext db;
        public ProfileController(Models.SequenceAlignmentDbContext _db)
        {
            db = _db;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View(db.Sequences.ToList());
        }

        [Route("[action]/AlignmentID")]   
        public IActionResult DownloadFile(string AlignmentID)
        {
            return File(db.Find<Models.Sequence>(AlignmentID).ByteText, "text/plain", AlignmentID + "_Alignment_Result.txt");
        }

        [Route("[action]/AlignmentID")]
        public IActionResult DeleteFile(string AlignmentID)
        {
            db.Remove(db.Find<Models.Sequence>(AlignmentID));
            db.SaveChanges();
            return RedirectToAction("Index", "Profile");
        }
    }
}
