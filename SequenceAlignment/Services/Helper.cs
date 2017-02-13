using Microsoft.AspNetCore.Http;
using SequenceAlignment.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SequenceAlignment.Services
{
    public static class Helper
    {
        public static byte[] GetDocument(string AlignmentResult, float ScoreResult, string UniqeIdentifier, string Algorithm, string ScoringMatrix, int Gap, int GapOpenPenalty, int GapExtensionPenalty)
        {
            StringBuilder HtmlBuilder = new StringBuilder();
            HtmlBuilder.Append($"Date: {DateTime.Now}");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append($"Alignment ID: {UniqeIdentifier}");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append("Parameters Used:");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append($"   Algorithm: {Algorithm}");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append($"   Scoring Matrix: {ScoringMatrix}");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append($"   Gap: {Gap}");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append($"   Gap Open Penalty:: {GapOpenPenalty}");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append($"   Gap Extension Penalty: {GapExtensionPenalty}");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append($"Alignment Score: {ScoreResult}");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append("Alignment Result");
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append(AlignmentResult);
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append("MTI - DNA Alignment");
            return Encoding.UTF8.GetBytes(HtmlBuilder.ToString());
        }
        public static Sequence GetMatchedAlignment(IEnumerable<Sequence> Source, string FirstSequence, string SecondSequence, string UserId)
        {
            return Source.Where(MyUser => MyUser.UserFK == UserId)
                 .AsEnumerable()
                 .AsParallel()
                 .Where(Seq =>
                 {
                     if (Seq.FirstSequence == FirstSequence && Seq.SecondSequence == SecondSequence)
                         return true;
                     else if (Seq.SecondSequence == FirstSequence && Seq.FirstSequence == SecondSequence)
                         return true;
                     else
                         return false;
                 })
                 .SingleOrDefault();
        }
        public static async Task<string> ConvertFileByteToByteStringAsync(IFormFile UploadFile)
        {
            using (MemoryStream Stream = new MemoryStream())
            {
                // Open the image as a stream and copy it into Stream object
                await UploadFile.OpenReadStream().CopyToAsync(Stream);
                // Convert the stream to Byte array.
                return Encoding.UTF8.GetString(Stream.ToArray());
            }
        }

        public static char[] UnambiguousRNA = { 'G', 'A', 'U', 'C' };
        public static char[] UnambiguousDNA = { 'G', 'A', 'T', 'C' };
        public static char[] AmbiguousDNA = { 'G', 'A', 'T', 'C', 'R', 'Y', 'W', 'S', 'M', 'K', 'H', 'B', 'V', 'D', 'N' };
        public static char[] AmbiguousRNA = { 'G', 'A', 'U', 'C', 'R', 'Y', 'W', 'S', 'M', 'K', 'H', 'B', 'V', 'D', 'N' };
        public static char[] Protein = { 'A', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'Y' };

        public static string CleanUp(string Sequence, char[] AllowedCharacters)
        {
            string CleanedSequence = string.Empty;
            for (int i = 0; i < Sequence.Length; i++)
            {
                if (AllowedCharacters.Contains(Sequence[i]) == true)
                    CleanedSequence = string.Concat(CleanedSequence, Sequence[i]);
            }
            return CleanedSequence;
        }

        public static string GenerateSequence(int Length, char[] AllowedCharacters)
        {
            ThreadLocal<Random> appRandom = new ThreadLocal<Random>(() => new Random());
            string GeneratedSequence = string.Empty;
            for (int i = 0; i < Length; i++)
            {
                GeneratedSequence = string.Concat(GeneratedSequence, AllowedCharacters[appRandom.Value.Next(0, AllowedCharacters.Length)].ToString());
            }
            return GeneratedSequence;
        }

        public static IEnumerable<string> SequenceSpliter(string str, int BlockSize)
        {
            for (int i = 0; i < str.Length; i += BlockSize)
                yield return str.Substring(i, Math.Min(BlockSize, str.Length - i));
        }

    }
}
