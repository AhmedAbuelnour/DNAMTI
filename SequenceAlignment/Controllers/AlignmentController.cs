using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SequenceAlignment.ViewModels;
using Newtonsoft.Json;
using SequenceAlignment.Services;
using Microsoft.AspNetCore.Http;
using SequenceAlignment.Models;
using System.Security.Claims;
using BioEdge.MatricesHelper;
using BioEdge.Matrices;
using BioEdge.Alignment;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Authorization;

namespace SequenceAlignment.Controllers
{
    public class AlignmentController : Controller
    {
        private readonly SequenceAlignmentDbContext db;
        public AlignmentController(SequenceAlignmentDbContext _db)
        {
            db = _db;
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
        public async Task<IActionResult> Align(SequenceViewModel Model, IFormFile FirstFile , IFormFile SecondFile)
        {
            if (string.IsNullOrWhiteSpace(Model.FirstSequence))
                Model.FirstSequence = await Helper.ConvertFileByteToByteStringAsync(FirstFile);
            if (string.IsNullOrWhiteSpace(Model.SecondSequence))
                Model.SecondSequence = await Helper.ConvertFileByteToByteStringAsync(SecondFile);

            Sequence SeqFound = Helper.GetMatchedAlignment(db.Sequences, Model.FirstSequence, Model.SecondSequence, User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (SeqFound == null)
            {
                SeqFound = new Sequence();
                SeqFound.AlignmentID = Guid.NewGuid().ToString();
                SeqFound.FirstSequence = Model.FirstSequence;
                SeqFound.SecondSequence = Model.SecondSequence;
                string AlignmentResult = string.Empty;
                float AlignmentScore = 0.0f;
                await Task.Run(() =>
                {
                    SequenceAligner AlgorithmInstance = DynamicInvoke.GetAlgorithm(Model.Algorithm);
                    ScoringMatrix ScoringMatrixInstance = DynamicInvoke.GetScoreMatrix(Model.ScoringMatrix);
                    AlignedSequences Result = AlgorithmInstance.Align(Model.FirstSequence, Model.SecondSequence, ScoringMatrixInstance, Model.Gap);
                    AlignmentResult = Result.StandardFormat(210);
                    AlignmentScore = Result.AlignmentScore(ScoringMatrixInstance);
                });
                SeqFound.ByteText = Helper.GetDocument(AlignmentResult, AlignmentScore, SeqFound.AlignmentID, Model.Algorithm, Model.ScoringMatrix, Model.Gap, Model.GapOpenPenalty, Model.GapExtensionPenalty);
                SeqFound.UserFK = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await db.AddAsync(SeqFound);
                await db.SaveChangesAsync();
                return File(SeqFound.ByteText, "plain/text", $"{SeqFound.AlignmentID}Result.txt");
            }
            else
            {
                return File(SeqFound.ByteText, "plain/text", $"{SeqFound.AlignmentID}Result.txt");
            }
        }

        [HttpGet]
        public IActionResult Grid()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Grid(GridViewModel Model, IFormFile FirstSequenceFile, IFormFile SecondSequenceFile)
        {
            string FirstSequence = await Helper.ConvertFileByteToByteStringAsync(FirstSequenceFile);
            string SecondSequence = await Helper.ConvertFileByteToByteStringAsync(SecondSequenceFile);
            if (FirstSequence.Length <= 20000 || SecondSequence.Length <= 20000)
               return RedirectToAction("Align", "Alignment");
            // Check for earlier exist
            Sequence Exist = Helper.GetMatchedAlignment(db.Sequences, await Helper.ConvertFileByteToByteStringAsync(FirstSequenceFile),await Helper.ConvertFileByteToByteStringAsync(SecondSequenceFile), User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (Exist==null) // Means the user didn't not submit these sequences before.
            {
                // Storing in the database
                await db.AddAsync(new Sequence { FirstSequence = await Helper.ConvertFileByteToByteStringAsync(FirstSequenceFile),
                                                 SecondSequence = await Helper.ConvertFileByteToByteStringAsync(SecondSequenceFile),
                                                 UserFK = User.FindFirstValue(ClaimTypes.NameIdentifier) });
                await db.SaveChangesAsync();
                // Sending to the Grid, that there is a job is required from you
                var connection = new HubConnection(@"http://mtidna.azurewebsites.net"); // Setting the URL of the SignalR server
                var _hub = connection.CreateHubProxy("GridHub"); // Setting the Hub Communication
                await connection.Start(); // Start the connection 
                await _hub.Invoke("Alignment", Exist.AlignmentID); // Invoke Alignment SignalR Method, and pass the Job Id to the Grid.
                return View("Notify", Exist.AlignmentID);
            }
            else
            {
                if(Exist.ByteText == null) // a failure happened before sending all the data to the Grid
                {
                    var connection = new HubConnection(@"http://mtidna.azurewebsites.net"); // Setting the URL of the SignalR server
                    var _hub = connection.CreateHubProxy("GridHub"); // Setting the Hub Communication
                    // Re-Sending them
                    await connection.Start(); // Start the connection 
                    await _hub.Invoke("Alignment", Exist.AlignmentID); // Invoke Alignment SignalR Method, and pass the Job Id to the Grid.
                    return View("Notify", Exist.AlignmentID); // it must be handled in the grid that the user whatever how many times he requested the same two sequences in the time that the grid is working , it must search fist if the same alignment ID is passed before it do nothing
                }
                else // the grid already alignment them , so no action is required from the grid, the user can download a text file directly.
                {
                    return File(Exist.ByteText, "plain/text", $"{Exist.AlignmentID}_Clould_Result.txt");
                }
            }
        }

    }
}

