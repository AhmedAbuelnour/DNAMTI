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
        public async Task<IActionResult> Align(SequenceViewModel Model)
        {
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
                SeqFound.PDFData = Helper.GetDocument(AlignmentResult, AlignmentScore, SeqFound.AlignmentID, Model.Algorithm, Model.ScoringMatrix, Model.Gap, Model.GapOpenPenalty, Model.GapExtensionPenalty);
                SeqFound.UserFK = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await db.AddAsync(SeqFound);
                await db.SaveChangesAsync();
                return File(SeqFound.PDFData, "plain/text", $"{SeqFound.AlignmentID}Result.txt");
            }
            else
            {
                return File(SeqFound.PDFData, "plain/text", $"{SeqFound.AlignmentID}Result.txt");
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
            AlignmentViewModel Object = new AlignmentViewModel
            {
                FirstSequence = await Helper.ConvertFileByteToByteStringAsync(FirstSequenceFile),
                SecondSequence = await Helper.ConvertFileByteToByteStringAsync(SecondSequenceFile),
                Algorithm = Model.Algorithm,
                ScoreMatrix = Model.ScoringMatrix,
                Gap = Model.Gap,
                GapOpenPenalty = Model.GapOpenPenalty,
                GapExtensionPenalty = Model.GapExtensionPenalty
            };
            string Json = JsonConvert.SerializeObject(Object);
            var connection = new HubConnection(@"http://mtidna.azurewebsites.net"); // Setting the URL of the SignalR server
            var _hub = connection.CreateHubProxy("GridHub"); // Setting the Hub Communication 
            await connection.Start(); // Start the connection 
            await _hub.Invoke("Alignment", Json); // Invoke Alignment SignalR Method, and pass the Alignment Parameters to the Grid.
            return View("GridRedirect");
        }

    }
}

