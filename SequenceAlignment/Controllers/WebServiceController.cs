using BioEdge.Alignment;
using BioEdge.Matrices;
using BioEdge.MatricesHelper;
using DataAccessLayer.Models;
using DataAccessLayer.Services;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SequenceAlignment.APIModel;
using SequenceAlignment.GridServiceAPI;
using SequenceAlignment.Services;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SequenceAlignment.Controllers
{
    [Route("api/[controller]")]
    public class WebServiceController : Controller 
    {
        private readonly IRepository Repo;
        private readonly UserManager<IdentityUser> UserManager;
        public WebServiceController(IRepository _Repo, UserManager<IdentityUser> _UserManager)
        {
            Repo = _Repo;
            UserManager = _UserManager;
        }
        [HttpGet("[action]")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("[action]")]
        public IActionResult Align()
        {
            return View();
        }
        [HttpGet("[action]")]
        public IActionResult Clean()
        {
            return View();
        }
        [HttpGet("[action]")]
        public IActionResult Similarity()
        {
            return View();
        }
        [HttpGet("[action]")]
        public IActionResult Splitter()
        {
            return View();
        }
        [HttpGet("[action]")]
        public IActionResult Generate()
        {
            return View();
        }
        [HttpPost("[action]")]
        public async Task<string> Align([FromBody]AlignAPIModel Model)
        {
            if (string.IsNullOrWhiteSpace(Model.FirstSequence) || string.IsNullOrWhiteSpace(Model.SecondSequence))
                return "Sequence Can't be empty";
            if (Model.FirstSequence.Length > 20000 || Model.SecondSequence.Length > 20000)
                return "Sequence length Can't be greater than 20K";

            if (!Regex.IsMatch(Model.FirstSequence, @"^[a-zA-Z]+$") || !Regex.IsMatch(Model.SecondSequence, @"^[a-zA-Z]+$"))
                return "Sequence must contains only characters";

            IdentityUser MyUser = await UserManager.FindByEmailAsync(Model.Email);
            if(MyUser == null)
                return "You have to sign-up first to be able to use our alignmnet serive";

            AlignmentJob JobFound = Repo.AreExist(Model.FirstSequence, Model.SecondSequence,Model.ScoringMatrixName,-8);
            if (JobFound == null)
            {
                JobFound = new AlignmentJob()
                {
                    AlignmentID = Guid.NewGuid().ToString(),
                    Algorithm = "ParallelNeedlemanWunsch",
                    ScoringMatrix = Model.ScoringMatrixName.ToUpper(),
                    FirstSequenceHash = Helper.SHA1HashStringForUTF8String(Model.FirstSequence),
                    SecondSequenceHash = Helper.SHA1HashStringForUTF8String(Model.SecondSequence),
                    FirstSequenceName = "Web Service Call",
                    SecondSequenceName = "Web Service Call",
                    GapOpenPenalty = -2,
                    Gap = -8,
                    GapExtensionPenalty = -2
                };
                SequenceAligner AlgorithmInstance = DynamicInvoke.GetAlgorithm(JobFound.Algorithm);
                ScoringMatrix ScoringMatrixInstance;
                try
                {
                    ScoringMatrixInstance = DynamicInvoke.GetScoreMatrix(JobFound.ScoringMatrix);
                }
                catch
                {
                    return "The Score Matrix Name is invalid.";
                }
                string AlignmentResult = string.Empty;
                float AlignmentScore = 0.0f;
                await Task.Run(() =>
                {
                    AlignedSequences Result = AlgorithmInstance.Align(Model.FirstSequence, Model.SecondSequence, ScoringMatrixInstance, -8);
                    AlignmentResult = Result.StandardFormat(210);
                    AlignmentScore = Result.AlignmentScore(ScoringMatrixInstance);
                });
                JobFound.ByteText = Helper.GetText(AlignmentResult,
                                                   AlignmentScore,
                                                   JobFound.AlignmentID,
                                                   "ParallelNeedlemanWunsch",
                                                    Model.ScoringMatrixName,
                                                   -8,
                                                   -2,
                                                   -2);
                JobFound.UserFK = MyUser.Id;
                await Repo.AddAlignmentJobAsync(JobFound);
                return Encoding.UTF8.GetString(JobFound.ByteText);
            }
            else
                return Encoding.UTF8.GetString(JobFound.ByteText);
        } 
        [HttpPost("[action]")]
        public string Clean([FromBody]CleanAPIModel Model)
        {
            if (string.IsNullOrWhiteSpace(Model.Sequence))
                return "Sequence Can't be empty";
            if(!Regex.IsMatch(Model.Sequence, @"^[a-zA-Z]+$"))
                return "Sequence must contains only characters";

            string CleanSequence = string.Empty;
            if (Model.Alphabet == "UnambiguousDNA")
                CleanSequence = Helper.CleanUp(Model.Sequence, Helper.UnambiguousDNA);
            else if (Model.Alphabet == "UnambiguousRNA")
                CleanSequence = Helper.CleanUp(Model.Sequence, Helper.UnambiguousRNA);
            else
                CleanSequence = Helper.CleanUp(Model.Sequence, Helper.Protein);
            return CleanSequence;
        }
        [HttpPost("[action]")]
        public string Similarity([FromBody]SimilarityAPIModel Model)
        {
            if (string.IsNullOrWhiteSpace(Model.FirstSequence) || string.IsNullOrWhiteSpace(Model.SecondSequence))
                return "Sequence Can't be empty";
            if (Model.FirstSequence.Length > 20000 || Model.SecondSequence.Length > 20000)
                return "Sequence length Can't be greater than 20K";
            if (!Regex.IsMatch(Model.FirstSequence, @"^[a-zA-Z]+$") || !Regex.IsMatch(Model.SecondSequence, @"^[a-zA-Z]+$"))
                return "Sequence must contains only characters";
            return $"{BioEdge.MatricesHelper.Similarity.CalculateSimilarity(Model.FirstSequence, Model.SecondSequence) * 100} %";
        }
        [HttpPost("[action]")]
        public string Splitter([FromBody]SplitterAPIModel Model)
        {
            if (string.IsNullOrWhiteSpace(Model.Sequence))
                return "Sequence Can't be empty";
            if (!Regex.IsMatch(Model.Sequence, @"^[a-zA-Z]+$"))
                return "Sequence must contains only characters";
            return JsonConvert.SerializeObject(Helper.SequenceSpliter(Model.Sequence, Model.ChunkLength).ToList());
        }
        [HttpPost("[action]")]
        public string Generate([FromBody]GenerateAPIModel Model)
        {
            Tuple<string, string> GeneratedSequences;
            if (Model.SequencesLength < 1)
                return "Generated Sequences must be greater than 0";
            if (string.IsNullOrWhiteSpace(Model.Alphabet))
                return "Alphabet Can't be empty";
            if (!Regex.IsMatch(Model.Alphabet, @"^[a-zA-Z]+$"))
                return "Sequence must contains only characters";

          
            if (Model.Alphabet == "UnambiguousDNA")
                GeneratedSequences = Helper.GenerateSequences(Model.SequencesLength, Helper.UnambiguousDNA, Model.ConsecutiveMatch, Model.Position);
            else if (Model.Alphabet == "UnambiguousRNA")
                GeneratedSequences = Helper.GenerateSequences(Model.SequencesLength, Helper.UnambiguousRNA, Model.ConsecutiveMatch, Model.Position);
            else
                GeneratedSequences = Helper.GenerateSequences(Model.SequencesLength, Helper.Protein, Model.ConsecutiveMatch, Model.Position);

            return $"'SequenceA:{GeneratedSequences.Item1}',SequenceB:{GeneratedSequences.Item2}";
        }




        [HttpPost]
        public async Task<IActionResult> Grid([FromBody]GridAPI Model)
        {
            if (Model.FirstSequence == null || Model.SecondSequence == null)
                return BadRequest();
            if (!Regex.IsMatch(Model.FirstSequence, @"^[a-zA-Z]+$") || !Regex.IsMatch(Model.FirstSequence, @"^[a-zA-Z]+$"))
                return BadRequest();
            //if (Model.FirstSequence.Length <= 20000 || Model.FirstSequence.Length <= 20000)
            //    return BadRequest();

            // Check for earlier exist
            AlignmentJob SeqFound = Repo.AreExist(Model.FirstSequence, Model.SecondSequence, Model.ScoringMatrix,Model.Gap);
            if (SeqFound == null) // Means the user didn't  submit these sequences before.
            {
                string AlignmentID = Guid.NewGuid().ToString();
                // Storing in the database
                await Repo.AddAlignmentJobAsync(new AlignmentJob
                {
                    AlignmentID = AlignmentID,
                    ScoringMatrix = Model.ScoringMatrix,
                    Algorithm = "Edge",
                    FirstSequenceHash = Helper.SHA1HashStringForUTF8String(Model.FirstSequence),
                    SecondSequenceHash = Helper.SHA1HashStringForUTF8String(Model.SecondSequence),
                    FirstSequenceName = "Sequence First Name",
                    SecondSequenceName = "Sequence Second Name",
                    IsAlignmentCompleted = false,
                    ByteText = Helper.SetTextGrid(Model.FirstSequence, Model.SecondSequence),
                    UserFK = (await UserManager.FindByEmailAsync(Model.Email)).Id
                });
                // Sending to the Grid, that there is a job is required from you
                // New up instance of SignalR HubConnection with the URL of the SignalR Server Hosting.
                HubConnection connection = new HubConnection(@"http://mtidna.azurewebsites.net");
                // Connect to the Grid SignalR Hub.
                IHubProxy _hub = connection.CreateHubProxy("GridHub");
                // Connecting to the server.
                await connection.Start(); // Start the connection 
                await _hub.Invoke("SendToGrid", Newtonsoft.Json.JsonConvert.SerializeObject(new GridInfo { AlignmentJobId = AlignmentID, Email = Model.Email })); // Invoke Alignment SignalR Method, and pass the Job Id to the Grid.
                return Ok("Grid OK");
            }
            else
            {
                if (SeqFound.IsAlignmentCompleted == false)
                {
                    return Ok("Grid Wait"); // Returning the same Alignment ID
                }
                else // the grid already alignment them , so no action is required from the grid, the user can download a text file directly.
                {
                    return Ok("Grid Check History");
                }
            }
        }



        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]LoginAPI Model)
        {
            if (Model.Email == null || Model.Password == null)
                return BadRequest();
            IdentityUser User = await UserManager.FindByEmailAsync(Model.Email);
            if (User is null)
                return BadRequest("Email Not Correct");
            if(await UserManager.CheckPasswordAsync(User, Model.Password))  return Ok();
            else  return BadRequest("Password Not Correct");
        }
    }
}
