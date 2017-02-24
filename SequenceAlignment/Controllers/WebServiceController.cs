using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using DataAccessLayer.Model;
using DataAccessLayer.Service;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BioEdge.MatricesHelper;
using SequenceAlignment.Services;
using BioEdge.Matrices;
using BioEdge.Alignment;
using System.Text;

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
        [HttpGet("[action]/{FirstSequence}/{SecondSequence}/{ScoringMatrixName}/{Email}")]
        public async Task<string> Align(string FirstSequence, string SecondSequence,string ScoringMatrixName, string Email)
        {
            if (string.IsNullOrWhiteSpace(FirstSequence) || string.IsNullOrWhiteSpace(SecondSequence))
                return "Sequence Can't be empty";
            if (FirstSequence.Length > 20000 || SecondSequence.Length > 20000)
                return "Sequence length Can't be greater than 20K";

            if (!Regex.IsMatch(FirstSequence, @"^[a-zA-Z]+$") || !Regex.IsMatch(SecondSequence, @"^[a-zA-Z]+$"))
                return "Sequence must contains only characters";

            IdentityUser MyUser = await UserManager.FindByEmailAsync(Email);
            if(MyUser == null)
                return "You have to sign-up first to be able to use or alignmnet serive";

            AlignmentJob JobFound = Repo.AreExist(FirstSequence, SecondSequence);
            if (JobFound == null)
            {
                JobFound = new AlignmentJob()
                {
                    AlignmentID = Guid.NewGuid().ToString(),
                    Algorithm = "ParallelNeedlemanWunsch",
                    ScoringMatrix = ScoringMatrixName.ToUpper(),
                    FirstSequenceHash = Helper.SHA1HashStringForUTF8String(FirstSequence),
                    SecondSequenceHash = Helper.SHA1HashStringForUTF8String(SecondSequence),
                    FirstSequenceName = "Web Service Call",
                    SecondSequenceName = "Web Service Call",
                    GapOpenPenalty = -2,
                    Gap = -8,
                    GapExtensionPenalty = -2
                };
                SequenceAligner AlgorithmInstance = DynamicInvoke.GetAlgorithm(JobFound.Algorithm);
                ScoringMatrix ScoringMatrixInstance = DynamicInvoke.GetScoreMatrix(JobFound.ScoringMatrix);
                string AlignmentResult = string.Empty;
                float AlignmentScore = 0.0f;
                await Task.Run(() =>
                {
                    AlignedSequences Result = AlgorithmInstance.Align(FirstSequence, SecondSequence, ScoringMatrixInstance, -8);
                    AlignmentResult = Result.StandardFormat(210);
                    AlignmentScore = Result.AlignmentScore(ScoringMatrixInstance);
                });
                JobFound.ByteText = Helper.GetText(AlignmentResult,
                                                   AlignmentScore,
                                                   JobFound.AlignmentID,
                                                   "ParallelNeedlemanWunsch",
                                                   ScoringMatrixName,
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
        [HttpGet("[action]/{Sequence}/{Alphabet}")]
        public string Clean(string Sequence, string Alphabet)
        {
            if (string.IsNullOrWhiteSpace(Sequence))
                return "Sequence Can't be empty";
            if(!Regex.IsMatch(Sequence, @"^[a-zA-Z]+$"))
                return "Sequence must contains only characters";

            string CleanSequence = string.Empty;
            if (Alphabet == "AmbiguousDNA")
                CleanSequence = Helper.CleanUp(Sequence, Helper.AmbiguousDNA);
            else if (Alphabet == "UnambiguousDNA")
                CleanSequence = Helper.CleanUp(Sequence, Helper.UnambiguousDNA);
            else if (Alphabet == "AmbiguousRNA")
                CleanSequence = Helper.CleanUp(Sequence, Helper.AmbiguousRNA);
            else if (Alphabet == "UnambiguousRNA")
                CleanSequence = Helper.CleanUp(Sequence, Helper.UnambiguousRNA);
            else
                CleanSequence = Helper.CleanUp(Sequence, Helper.Protein);
            return CleanSequence;
        }
        [HttpGet("[action]/{FirstSequence}/{SecondSequence}")]
        public string Similarity(string FirstSequence, string SecondSequence)
        {
            if (string.IsNullOrWhiteSpace(FirstSequence) || string.IsNullOrWhiteSpace(SecondSequence))
                return "Sequence Can't be empty";
            if (FirstSequence.Length > 20000 || SecondSequence.Length > 20000)
                return "Sequence length Can't be greater than 20K";

            if (!Regex.IsMatch(FirstSequence, @"^[a-zA-Z]+$") || !Regex.IsMatch(SecondSequence, @"^[a-zA-Z]+$"))
                return "Sequence must contains only characters";
            return $"{BioEdge.MatricesHelper.Similarity.CalculateSimilarity(FirstSequence,SecondSequence) * 100} %";
        }
        [HttpGet("[action]/{Sequence}/{ChunkLength}")]
        public string Splitter(string Sequence ,int ChunkLength)
        {
            if (string.IsNullOrWhiteSpace(Sequence))
                return "Sequence Can't be empty";
            if (!Regex.IsMatch(Sequence, @"^[a-zA-Z]+$"))
                return "Sequence must contains only characters";
            return JsonConvert.SerializeObject(Helper.SequenceSpliter(Sequence, ChunkLength).ToList());
        }
    }
}
