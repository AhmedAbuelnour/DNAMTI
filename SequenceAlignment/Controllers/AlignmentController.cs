using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SequenceAlignment.ViewModels;
using SequenceAlignment.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using BioEdge.MatricesHelper;
using BioEdge.Matrices;
using BioEdge.Alignment;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SequenceAlignment.Models;
using SequenceAlignment.Json;

namespace SequenceAlignment.Controllers
{
    [Authorize]
    public class AlignmentController : Controller
    {
        private readonly IRepository Repo;
        private readonly UserManager<IdentityUser> UserManager;
        public AlignmentController(IRepository _Repo, UserManager<IdentityUser> _UserManager)
        {
            Repo = _Repo;
            UserManager = _UserManager;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Align()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Align(SequenceViewModel Model, IFormFile FirstFile, IFormFile SecondFile)
        {              
            if(!string.IsNullOrWhiteSpace(Model.FirstSequence))
                Model.FirstSequence = Model.FirstSequence.Trim().Replace(" ", string.Empty);
            if (!string.IsNullOrWhiteSpace(Model.SecondSequence))
                Model.SecondSequence = Model.SecondSequence.Trim().Replace(" ", string.Empty).ToUpper();
            if (string.IsNullOrWhiteSpace(Model.FirstSequence) && FirstFile != null)
            {
                if(FirstFile.ContentType == "text/plain")
                {
                    string FirstSequence = (await Helper.ConvertFileByteToByteStringAsync(FirstFile)).Trim().Replace(" ", string.Empty).ToUpper();
                    if (FirstSequence.Length > 20000)
                        return RedirectToAction("Grid", "Alignment");
                    else if (FirstSequence.Length == 0)
                        return View("Error", new ErrorViewModel { Message = "You Can't send a sequence of 0 Length", Solution = "You should send a sequence greater than 0 length" });
                    else
                        Model.FirstSequence = FirstSequence;
                }
                else
                {
                    return View("Error", new ErrorViewModel { Message = "You Can't upload a file of any type rather than txt file format", Solution = "You should upload a file of txt file format" });
                }
            }
            if (string.IsNullOrWhiteSpace(Model.SecondSequence) && SecondFile != null)
            {
                if (FirstFile.ContentType == "text/plain")
                {
                    string SecondSequence = (await Helper.ConvertFileByteToByteStringAsync(FirstFile)).Trim();
                    if (SecondSequence.Length > 20000)
                        return RedirectToAction("Grid", "Alignment");
                    else if (SecondSequence.Length == 0)
                        return View("Error", new ErrorViewModel { Message = "You Can't send a sequence of 0 Length", Solution = "You should send a sequence greater than 0 length" });
                    else
                        Model.SecondSequence = SecondSequence;
                }
                else
                {
                    return View("Error", new ErrorViewModel { Message = "You Can't upload a file of any type rather than txt file format", Solution = "You should upload a file of txt file format" });
                }
            }
            if ((Model.FirstSequence == null && FirstFile == null) || (Model.SecondSequence == null && SecondFile == null))
            {
                return View("Error", new ErrorViewModel { Message = "You Can't empty sequence", Solution = "You have to enter the sequence or either upload a file contains the sequence" });
            }
            if (!Regex.IsMatch(Model.FirstSequence, @"^[a-zA-Z]+$") || !Regex.IsMatch(Model.SecondSequence, @"^[a-zA-Z]+$"))
                return View(Model);
            AlignmentJob JobFound = Repo.AreExist(Model.FirstSequence,Model.SecondSequence);
            if (JobFound == null)
            {
                JobFound = new AlignmentJob()
                {
                    AlignmentID = Guid.NewGuid().ToString(),
                    Algorithm = Model.Algorithm,
                    ScoringMatrix = Model.ScoringMatrix,
                    FirstSequenceHash = Helper.SHA1HashStringForUTF8String(Model.FirstSequence),
                    SecondSequenceHash = Helper.SHA1HashStringForUTF8String(Model.SecondSequence),
                    FirstSequenceName = Model.FirstSequenceName,
                    SecondSequenceName = Model.SecomdSequenceName,
                    GapOpenPenalty = Model.GapOpenPenalty,
                    Gap = Model.Gap,
                    GapExtensionPenalty = Model.GapExtensionPenalty
                };
                SequenceAligner AlgorithmInstance = DynamicInvoke.GetAlgorithm(Model.Algorithm);
                ScoringMatrix ScoringMatrixInstance = DynamicInvoke.GetScoreMatrix(Model.ScoringMatrix);
                string AlignmentResult = string.Empty;
                float AlignmentScore = 0.0f;

                AlignedSequences Result = AlgorithmInstance.Align(Model.FirstSequence, Model.SecondSequence, ScoringMatrixInstance, Model.Gap);
                AlignmentResult = Result.StandardFormat(210);
                AlignmentScore = Result.AlignmentScore(ScoringMatrixInstance);
                JobFound.ByteText = Helper.GetText(AlignmentResult,
                                                  AlignmentScore,
                                                  JobFound.AlignmentID,
                                                  Model.Algorithm,
                                                  Model.ScoringMatrix,
                                                  Model.Gap,
                                                  Model.GapOpenPenalty,
                                                  Model.GapExtensionPenalty);
                JobFound.UserFK = UserManager.GetUserId(User);
                await Repo.AddAlignmentJobAsync(JobFound);
                return File(JobFound.ByteText, "text/plain", $"{JobFound.AlignmentID}_Alignment_Result.txt");
            }
            else
            {
                return File(JobFound.ByteText, "text/plain", $"{JobFound.AlignmentID}_Alignment_Result.txt");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Grid()
        {
            IdentityUser CurrentUser = await UserManager.FindByIdAsync(UserManager.GetUserId(User));
            bool IsEmailConfirmed = await UserManager.IsEmailConfirmedAsync(CurrentUser);
            if (IsEmailConfirmed)
                return View();
            else
                return View("Error",new ErrorViewModel { Message = "You Did not Confirm your email yet.", Solution = $"Confirm Your E-Mail", Link =$"<a class='btn btn-info' href='{Url.Action("SendConfirmationEmail", "Account", new { UserId = UserManager.GetUserId(User) }, HttpContext.Request.Scheme)}'> Resend Email Confirmation </a>" } ); 
        }
        [HttpPost]
        public async Task<IActionResult> Grid(GridViewModel Model, IFormFile FirstFile, IFormFile SecondFile)
        {
            if (Model.FirstSequenceName.Length > 50 || Model.SecomdSequenceName.Length > 50)
                return View("Error", new ErrorViewModel { Message = "You Can't enter a sequence name greater than 50 character", Solution = "You should name your sequence with a name less than or equal to 50 character" });
            if (FirstFile == null || SecondFile == null || FirstFile.ContentType != "text/plain" || SecondFile.ContentType != "text/plain")
                return View("Error", new ErrorViewModel { Message = "You Can't empty sequence", Solution = "You have to enter the sequence or either upload a file contains the sequence" });
            string FirstSequence = (await Helper.ConvertFileByteToByteStringAsync(FirstFile)).Trim().Replace(" ", string.Empty).ToUpper(); 
            string SecondSequence = (await Helper.ConvertFileByteToByteStringAsync(SecondFile)).Trim().Replace(" ", string.Empty).ToUpper(); 

            if (!Regex.IsMatch(FirstSequence, @"^[a-zA-Z]+$") || !Regex.IsMatch(SecondSequence, @"^[a-zA-Z]+$"))
                return View("Error", new ErrorViewModel { Message = "Your sequence must contains only characters", Solution = "Send sequence contains only characters" });

            if (FirstSequence.Length <= 20000 || SecondSequence.Length <= 20000)
               return RedirectToAction("Align", "Alignment");
            // Check for earlier exist
            AlignmentJob SeqFound = Repo.AreExist(FirstSequence,SecondSequence,Model.ScoringMatrix);
            if (SeqFound == null) // Means the user didn't  submit these sequences before.
            {
                string AlignmentID = Guid.NewGuid().ToString();
                // Storing in the database
                await Repo.AddAlignmentJobAsync(new AlignmentJob {
                                                 AlignmentID = AlignmentID,
                                                 ScoringMatrix = Model.ScoringMatrix,
                                                 Algorithm = "Edge",
                                                 FirstSequenceHash = Helper.SHA1HashStringForUTF8String(FirstSequence),
                                                 SecondSequenceHash = Helper.SHA1HashStringForUTF8String(SecondSequence),
                                                 FirstSequenceName = Model.FirstSequenceName,
                                                 SecondSequenceName = Model.SecomdSequenceName,
                                                 UserFK = User.FindFirstValue(ClaimTypes.NameIdentifier) });
                // Sending to the Grid, that there is a job is required from you
                var connection = new HubConnection(@"http://mtidna.azurewebsites.net"); // Setting the URL of the SignalR server
                var _hub = connection.CreateHubProxy("GridHub"); // Setting the Hub Communication
                await connection.Start(); // Start the connection 
                await _hub.Invoke("SendToGrid", Newtonsoft.Json.JsonConvert.SerializeObject(new GridInfo { AlignmentJobId = AlignmentID, Email = (await UserManager.FindByIdAsync(Repo.GetAlignmentJobById(AlignmentID).UserFK)).Email })); // Invoke Alignment SignalR Method, and pass the Job Id to the Grid.
                return View("Notify", AlignmentID);
            }
            else
            {
                if(SeqFound.ByteText == null) // a failure happened before sending all the data to the Grid or the user re-submitted the same two sequences before finishing 
                {
                    return View("Notify", SeqFound.AlignmentID); // Returning the same Alignment ID
                }
                else // the grid already alignment them , so no action is required from the grid, the user can download a text file directly.
                {
                    return File(SeqFound.ByteText, "text/plain", $"{SeqFound.AlignmentID}_Clould_Result.txt");
                }
            }
        }
    }
}

