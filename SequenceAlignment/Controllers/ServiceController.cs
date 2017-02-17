using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using SequenceAlignment.ViewModels;
using SequenceAlignment.Services;
using Microsoft.AspNetCore.Authorization;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        public IActionResult Clean(CleanSequenceViewModel Model)
        {
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
            return File(Encoding.UTF8.GetBytes(CleanSequence), "plain/text", $"{Guid.NewGuid()}_Clean.txt");
        }

        [HttpGet]
        public IActionResult Generate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Generate(GenerateSequenceViewModel Model)
        {
            string CleanSequence = string.Empty;
            if (Model.Alphabet == "AmbiguousDNA")
                CleanSequence = Helper.GenerateSequence(Model.SequenceLength, Helper.AmbiguousDNA);
            else if (Model.Alphabet == "UnambiguousDNA")
                CleanSequence = Helper.GenerateSequence(Model.SequenceLength, Helper.UnambiguousDNA);
            else if (Model.Alphabet == "AmbiguousRNA")
                CleanSequence = Helper.GenerateSequence(Model.SequenceLength, Helper.AmbiguousRNA);
            else if (Model.Alphabet == "UnambiguousRNA")
                CleanSequence = Helper.GenerateSequence(Model.SequenceLength, Helper.UnambiguousRNA);
            else
                CleanSequence = Helper.GenerateSequence(Model.SequenceLength, Helper.Protein);
            return File(Encoding.UTF8.GetBytes(CleanSequence), "plain/text", $"{Guid.NewGuid()}_GeneratedSequence{Model.SequenceLength}.txt");
        }

        [HttpGet]
        public IActionResult Similarity()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Similarity(SimilarityViewModel Model)
        {
            StringBuilder Sb = new StringBuilder();
            Sb.Append($"Similarity between your two sequences are: {BioEdge.MatricesHelper.Similarity.CalculateSimilarity(Model.FirstSequence, Model.SecondSequence) * 100} %");
            Sb.Append("Additional Information:");
            Sb.Append("Your First Submitted Sequence:");
            Sb.Append(Model.FirstSequence);
            Sb.Append("Your Second Submitted Sequence:");
            Sb.Append(Model.SecondSequence);
            return File(Encoding.UTF8.GetBytes(Sb.ToString()), "plain/text", $"{Guid.NewGuid()}_SimilaritySequence.txt");
        }

        [HttpGet]
        public IActionResult Splitter()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Splitter(SplitterViewModel Model)
        {
            IList<string> Sequences =  Helper.SequenceSpliter(Model.Sequence, Model.Divider).ToList();

            StringBuilder Sb = new StringBuilder();
            Sb.Append($"Your Sequences count:{Sequences.Count()}, Each sequence is {Model.Divider} length:");
            for (int i = 0; i < Sequences.Count; i++)
            {
                Sb.Append(Sequences[i]);
                Sb.Append(Environment.NewLine);
            }
            return File(Encoding.UTF8.GetBytes(Sb.ToString()), "plain/text", $"{Guid.NewGuid()}_SplitSequence.txt");
        }
    }
}
