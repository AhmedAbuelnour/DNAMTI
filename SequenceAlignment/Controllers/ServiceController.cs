using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using SequenceAlignment.ViewModels;
using SequenceAlignment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SequenceAlignment.Controllers
{
    public class ServiceController : Controller
    {
        // GET: /<controller>/
        [HttpGet,AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Clean()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Clean(CleanSequenceViewModel Model , IFormFile SequenceFile)
        {
            if (string.IsNullOrEmpty(Model.Sequence))
                if (SequenceFile is null)
                    return View(Model);
                else
                    Model.Sequence = await Helper.ConvertFileByteToByteStringAsync(SequenceFile);

            string CleanSequence = string.Empty;
            if (Model.Alphabet == "AmbiguousDNA")
                CleanSequence = Helper.CleanUp(Model.Sequence, Helper.AmbiguousDNA);
            else if (Model.Alphabet == "UnambiguousDNA")
                CleanSequence = Helper.CleanUp(Model.Sequence, Helper.UnambiguousDNA);
            else if (Model.Alphabet == "AmbiguousRNA")
                CleanSequence = Helper.CleanUp(Model.Sequence, Helper.AmbiguousRNA);
            else if (Model.Alphabet == "UnambiguousRNA")
                CleanSequence = Helper.CleanUp(Model.Sequence, Helper.UnambiguousRNA);
            else
                CleanSequence = Helper.CleanUp(Model.Sequence, Helper.Protein);
            return File(Encoding.UTF8.GetBytes(CleanSequence), "text/plain", $"{Guid.NewGuid()}_Clean.txt");
        }

        [HttpGet]
        public IActionResult Generate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Generate(GenerateSequenceViewModel Model)
        {
            Tuple<string, string> CleanSequence;
            if (Model.Alphabet == "AmbiguousDNA")
                CleanSequence = Helper.GenerateSequences(Model.SequenceLength, Helper.AmbiguousDNA,Model.ConsecutiveMatch,Model.Position);
            else if (Model.Alphabet == "UnambiguousDNA")
                CleanSequence = Helper.GenerateSequences(Model.SequenceLength, Helper.UnambiguousDNA, Model.ConsecutiveMatch, Model.Position);
            else if (Model.Alphabet == "AmbiguousRNA")
                CleanSequence = Helper.GenerateSequences(Model.SequenceLength, Helper.AmbiguousRNA, Model.ConsecutiveMatch, Model.Position);
            else if (Model.Alphabet == "UnambiguousRNA")
                CleanSequence = Helper.GenerateSequences(Model.SequenceLength, Helper.UnambiguousRNA, Model.ConsecutiveMatch, Model.Position);
            else
                CleanSequence = Helper.GenerateSequences(Model.SequenceLength, Helper.Protein, Model.ConsecutiveMatch, Model.Position);

            return File(Encoding.UTF8.GetBytes(new StringBuilder().Append(CleanSequence.Item1).
                                                                   Append(Environment.NewLine).
                                                                   Append(Environment.NewLine).
                                                                   Append(CleanSequence.Item2).ToString()),
                                                                   "text/plain",
                                                                   $"{Guid.NewGuid()}_GeneratedSequence{Model.SequenceLength}.txt");
        }

        [HttpGet]
        public IActionResult Similarity()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SimilarityAsync(SimilarityViewModel Model, IFormFile FirstFile, IFormFile SecondFile)
        {
            if (!string.IsNullOrEmpty(Model.FirstSequence))
                Model.FirstSequence = Model.FirstSequence.Trim();
            if (!string.IsNullOrEmpty(Model.SecondSequence))
                Model.SecondSequence = Model.SecondSequence.Trim();

            if (string.IsNullOrWhiteSpace(Model.FirstSequence) && FirstFile != null)
            {
                string FirstSequence = (await Helper.ConvertFileByteToByteStringAsync(FirstFile)).Trim();
                if (FirstSequence.Length > 20000)
                    return View(Model); // Can't be greater than 20K
                else
                    Model.FirstSequence = FirstSequence;
            }
            if (string.IsNullOrWhiteSpace(Model.SecondSequence) && SecondFile != null)
            {
                string SecondSequence = (await Helper.ConvertFileByteToByteStringAsync(FirstFile)).Trim();
                if (SecondSequence.Length > 20000)
                    return View(Model); // Can't be greater than 20K
                else
                    Model.SecondSequence = SecondSequence;
            }
            if ((Model.FirstSequence == null && FirstFile == null) || (Model.SecondSequence == null && SecondFile == null) || FirstFile.ContentType != "text/plain" || SecondFile.ContentType != "text/plain")
            {
                ModelState.AddModelError("", "You have to enter the sequence or either upload a file contains the sequence");
                return View(Model);
            }

            if (!Regex.IsMatch(Model.FirstSequence, @"^[a-zA-Z]+$") || !Regex.IsMatch(Model.SecondSequence, @"^[a-zA-Z]+$"))
                return View(Model);

            StringBuilder Sb = new StringBuilder();
            Sb.Append($"Similarity between your two sequences are: {BioEdge.MatricesHelper.Similarity.CalculateSimilarity(Model.FirstSequence, Model.SecondSequence) * 100} %");
            Sb.Append("Additional Information:");
            Sb.Append("Your First Submitted Sequence:");
            Sb.Append(Model.FirstSequence);
            Sb.Append("Your Second Submitted Sequence:");
            Sb.Append(Model.SecondSequence);
            return File(Encoding.UTF8.GetBytes(Sb.ToString()), "text/plain", $"{Guid.NewGuid()}_SimilaritySequence.txt");
        }

        [HttpGet]
        public IActionResult Splitter()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Splitter(SplitterViewModel Model, IFormFile SequenceFile)
        {
            if (string.IsNullOrEmpty(Model.Sequence))
                if (SequenceFile is null)
                    return View(Model);
                else
                    Model.Sequence = await Helper.ConvertFileByteToByteStringAsync(SequenceFile);

            IList<string> Sequences =  Helper.SequenceSpliter(Model.Sequence, Model.Divider).ToList();

            StringBuilder Sb = new StringBuilder();
            Sb.Append($"Your Sequences count:{Sequences.Count()}, Each sequence is {Model.Divider} length:");
            for (int i = 0; i < Sequences.Count; i++)
            {
                Sb.Append(Sequences[i]);
                Sb.Append(Environment.NewLine);
            }
            return File(Encoding.UTF8.GetBytes(Sb.ToString()), "text/plain", $"{Guid.NewGuid()}_SplitSequence.txt");
        }
    }
}
