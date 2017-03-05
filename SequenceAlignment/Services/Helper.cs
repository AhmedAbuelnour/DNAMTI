using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace SequenceAlignment.Services
{
    public static class Helper
    {
        public static byte[] SetTextGrid(string FirstSequence , string SecondSequence)
        {
            StringBuilder HtmlBuilder = new StringBuilder();
            HtmlBuilder.Append("First Sequence:");
            HtmlBuilder.Append(FirstSequence);
            HtmlBuilder.Append(Environment.NewLine);
            HtmlBuilder.Append("Second Sequence:");
            HtmlBuilder.Append(SecondSequence);
            return Encoding.UTF8.GetBytes(HtmlBuilder.ToString());
        }
        public static byte[] GetText(string AlignmentResult, float ScoreResult, string UniqeIdentifier, string Algorithm, string ScoringMatrix, int Gap, int GapOpenPenalty, int GapExtensionPenalty)
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
            HtmlBuilder.Append($"   Gap Open Penalty: {GapOpenPenalty}");
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
        public static Tuple<string, string> GenerateSequences(int Length, char[] AllowedCharacters, int ConsecutiveMatch, char Position)
        {
            if (ConsecutiveMatch >= Length)
                throw new Exception("Consecutive Match Length can't be greater than or equal the acutal sequence length");
            Random appRandom = new Random();
            string GeneratedSequenceA = string.Empty;
            string CM;
            for (int i = 0; i < Length; i++)
                GeneratedSequenceA = string.Concat(GeneratedSequenceA, AllowedCharacters[appRandom.Next(0, AllowedCharacters.Length)].ToString());
            string GeneratedSequenceB = string.Empty;
            for (int i = 0; i < Length; i++)
                GeneratedSequenceB = string.Concat(GeneratedSequenceB, AllowedCharacters[appRandom.Next(0, AllowedCharacters.Length)].ToString());

            if (Position == 'L')
            {
                CM = GeneratedSequenceA.Substring(0, ConsecutiveMatch);
                GeneratedSequenceB = string.Copy(GeneratedSequenceB.Replace(GeneratedSequenceB.Substring(0, ConsecutiveMatch), CM));
            }
            else if (Position == 'R')
            {
                CM = GeneratedSequenceA.Substring(GeneratedSequenceA.Length - ConsecutiveMatch);
                GeneratedSequenceB = string.Copy(GeneratedSequenceB.Replace(GeneratedSequenceB.Substring(GeneratedSequenceB.Length - ConsecutiveMatch), CM));
            }
            else
            {
                if ((GeneratedSequenceA.Length / 2) / 2 > (GeneratedSequenceA.Length / 2))
                    throw new Exception("Middle Index can't be greater than or equal the sequence length");
                CM = GeneratedSequenceA.Substring((GeneratedSequenceA.Length / 2) - ConsecutiveMatch / 2, ConsecutiveMatch);
                GeneratedSequenceB = string.Copy(GeneratedSequenceB.Replace(GeneratedSequenceB.Substring((GeneratedSequenceA.Length / 2) - ConsecutiveMatch / 2, ConsecutiveMatch), CM));
            }
            return new Tuple<string, string>(GeneratedSequenceA, GeneratedSequenceB);
        }
        public static IEnumerable<string> SequenceSpliter(string str, int BlockSize)
        {
            for (int i = 0; i < str.Length; i += BlockSize)
                yield return str.Substring(i, Math.Min(BlockSize, str.Length - i));
        }
        public static string SHA1HashStringForUTF8String(string Input)
        {
            byte[] InputBytes = Encoding.UTF8.GetBytes(Input);
            SHA1 SHA = SHA1.Create();
            byte[] hashBytes = SHA.ComputeHash(InputBytes);
            StringBuilder Sb = new StringBuilder();
            hashBytes.ToList().ForEach((b) =>
            {
                Sb.Append(b.ToString("X2"));
            });
            return Sb.ToString();
        }
    }
}
