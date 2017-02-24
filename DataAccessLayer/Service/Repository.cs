using DataAccessLayer.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BioEdge.MatricesHelper;
using BioEdge.Alignment;

namespace DataAccessLayer.Service
{
    public class Repository : IRepository
    {
        private readonly AlignmentDbContext db;
        public Repository(AlignmentDbContext _db)
        {
            db = _db;
            db.Database.EnsureCreated();
        }
        public void DeleteAlignmentJob(string AlignmentJobID)
        {
            if (string.IsNullOrWhiteSpace(AlignmentJobID))
                throw new Exception("ID Can't be Empty string");
            else
            {
                AlignmentJob LocalSequence = db.AlignmentJobs.Find(AlignmentJobID);
                if (LocalSequence == null)
                    throw new Exception("Can't Find A Record In The Database With The Specified ID");
                else
                {
                    db.AlignmentJobs.Remove(LocalSequence);
                    db.SaveChanges();
                }
            }
        }
        public async Task DeleteAlignmentJobAsync(string AlignmentJobID)
        {
            if (string.IsNullOrWhiteSpace(AlignmentJobID))
                throw new Exception("ID Can't be Empty string");
            else
            {
                AlignmentJob LocalSequence = await db.AlignmentJobs.FindAsync(AlignmentJobID);
                if (LocalSequence == null)
                    throw new Exception("Can't Find A Record In The Database With The Specified ID");
                else
                {
                    await Task.Run(() => { db.AlignmentJobs.Remove(LocalSequence); });
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task AddAlignmentJobAsync(AlignmentJob Job)
        {
            await db.AlignmentJobs.AddAsync(Job);
            await db.SaveChangesAsync();
        }
        public void AddAlignmentJob(AlignmentJob Job)
        {
            db.AlignmentJobs.Add(Job);
            db.SaveChanges();
        }
        public async Task AddAlignmentJobAsync(string SequenceA, string SequenceNameA, string SequenceB, string SequenceNameB, string UserId)
        {
            await db.AlignmentJobs.AddAsync(new AlignmentJob
            {
                FirstSequenceHash = SHA1HashStringForUTF8String(SequenceA),
                FirstSequenceName = SequenceNameA,
                SecondSequenceHash = SHA1HashStringForUTF8String(SequenceB),
                SecondSequenceName = SequenceNameB,
                UserFK = UserId
            });
            await db.SaveChangesAsync();
        }
        public void AddAlignmentJob(string SequenceA, string SequenceNameA, string SequenceB, string SequenceNameB, string UserId)
        {
            db.AlignmentJobs.Add(new AlignmentJob
            {
                FirstSequenceHash = SHA1HashStringForUTF8String(SequenceA),
                FirstSequenceName = SequenceNameA,
                SecondSequenceHash = SHA1HashStringForUTF8String(SequenceB),
                SecondSequenceName = SequenceNameB,
                UserFK = UserId
            });
            db.SaveChanges();
        }
        public IEnumerable<AlignmentJob> GetHistory(string UserID)
        {
            return db.AlignmentJobs.Where(Seq => Seq.UserFK == UserID).ToList();
        }
        /// <summary>
        /// Get a certian Alignment Job with specified ID
        /// </summary>
        /// <param name="AlignmentJobID">Alignment ID</param>
        /// <returns>Returns null if not found</returns>
        public AlignmentJob GetAlignmentJobById(string AlignmentJobID)
        {
            return db.AlignmentJobs.SingleOrDefault(Seq => Seq.AlignmentID == AlignmentJobID);

        }
        public async Task<IEnumerable<AlignmentJob>> GetAlignmentJobsAsync()
        {
            return await db.AlignmentJobs.ToListAsync();
        }
        public IEnumerable<AlignmentJob> GetAlignmentJobs()
        {
            return db.AlignmentJobs.ToList();
        }
        public async Task<IEnumerable<AlignmentJob>> GetPendingAlignmentJobsAsync()
        {
            return await db.AlignmentJobs.Where(Seq => Seq.ByteText == null).ToListAsync();
        }
        public IEnumerable<AlignmentJob> GetPendingAlignmentJobs()
        {
            return db.AlignmentJobs.Where(Seq => Seq.ByteText == null).ToList();
        }
        public void FinalizeJob(string AlignmentJobID, AlignedSequences AlignmentResult)
        {
            if(string.IsNullOrWhiteSpace(AlignmentJobID))
                throw new Exception("ID Can't be Empty string");
            if (AlignmentResult == null)
                throw new Exception("Alignment Result Can't be null");
            AlignmentJob Seq = db.AlignmentJobs.SingleOrDefault(Find => Find.AlignmentID == AlignmentJobID);
            if (Seq == null)
                throw new Exception("Can't Find A Record In The Database With The Specified ID");
            Seq.ByteText = GetText(AlignmentResult.StandardFormat(), AlignmentResult.AlignmentScore(DynamicInvoke.GetScoreMatrix(Seq.ScoringMatrix), Seq.GapOpenPenalty, Seq.GapExtensionPenalty), Seq.AlignmentID, Seq.Algorithm, Seq.ScoringMatrix, Seq.Gap, Seq.GapOpenPenalty, Seq.GapExtensionPenalty);
            db.AlignmentJobs.Update(Seq);
            db.SaveChanges();
        }
        public async Task FinalizeJobAsync(string AlignmentJobID, AlignedSequences AlignmentResult)
        {
            if (string.IsNullOrWhiteSpace(AlignmentJobID))
                throw new Exception("ID Can't be Empty string");
            if (AlignmentResult == null)
                throw new Exception("Alignment Result Can't be null");
            AlignmentJob Seq = await db.AlignmentJobs.SingleOrDefaultAsync(Find => Find.AlignmentID == AlignmentJobID);
            if (Seq == null)
                throw new Exception("Can't Find A Record In The Database With The Specified ID");
            Seq.ByteText = GetText(AlignmentResult.StandardFormat(), AlignmentResult.AlignmentScore(DynamicInvoke.GetScoreMatrix(Seq.ScoringMatrix), Seq.GapOpenPenalty, Seq.GapExtensionPenalty), Seq.AlignmentID, Seq.Algorithm, Seq.ScoringMatrix, Seq.Gap, Seq.GapOpenPenalty, Seq.GapExtensionPenalty);
            await Task.Run(() => db.AlignmentJobs.Update(Seq));
            await db.SaveChangesAsync();
        }
        public bool IsFinished(string AlignmentJobID)
        {
            if (string.IsNullOrWhiteSpace(AlignmentJobID))
                throw new Exception("Can't Send Empty string as ID");
            AlignmentJob LocalSequence = db.AlignmentJobs.SingleOrDefault(Seq => Seq.AlignmentID == AlignmentJobID);
            if (LocalSequence == null)
                throw new Exception("Can't Find A Record In The Database With The Specified ID");
            else if (LocalSequence.ByteText == null)
                return false;
            return true;
        }
        public async Task<bool> IsFinishedAsync(string AlignmentJobID)
        {
            if (string.IsNullOrWhiteSpace(AlignmentJobID))
                throw new Exception("Can't Send Empty string as ID");
            AlignmentJob LocalSequence = await db.AlignmentJobs.SingleOrDefaultAsync(Seq => Seq.AlignmentID == AlignmentJobID);
            if (LocalSequence == null)
                throw new Exception("Can't Find A Record In The Database With The Specified ID");
            else if (LocalSequence.ByteText == null)
                return false;
            return true;
        }
        /// <summary>
        /// Gets if the two sequences are submitted before or not.
        /// </summary>
        /// <param name="FirstSequence">First Sequences Submitted.</param>
        /// <param name="SecondSequence">Second Sequence Submitted.</param>
        /// <returns>Returns null if they are not got submitted before.</returns>
        public AlignmentJob AreExist(string FirstSequence, string SecondSequence)
        {
            if (string.IsNullOrWhiteSpace(FirstSequence) || string.IsNullOrWhiteSpace(SecondSequence))
                throw new Exception("Can't pass empty string as a sequence");
            string FirstSequenceHash = SHA1HashStringForUTF8String(FirstSequence);
            string SecondSequenceHash = SHA1HashStringForUTF8String(SecondSequence);
            AlignmentJob LocalSequence = db.AlignmentJobs.AsEnumerable().SingleOrDefault(Seq =>
            {
                if (Seq.FirstSequenceHash == FirstSequenceHash && Seq.SecondSequenceHash == SecondSequenceHash)
                    return true;
                else if (Seq.SecondSequenceHash == FirstSequenceHash && Seq.FirstSequenceHash == SecondSequenceHash)
                    return true;
                else
                    return false;
            });

            if (LocalSequence == null)
                return null;
            return LocalSequence;
        }
        public Tuple<string, string> GetSubmittedSequences(string AlignmentJobID)
        {
            AlignmentJob Seq = db.AlignmentJobs.SingleOrDefault(Find => Find.AlignmentID == AlignmentJobID);
            if (Seq == null)
                throw new Exception("Can't Find Alignment Job matches the Given ID");
            string File = Encoding.ASCII.GetString(Seq.ByteText);
            string FirstSequence = File.Substring(File.IndexOf("First Sequence:") + "First Sequence:".Length, File.IndexOf("Second Sequence:")).Trim();
            string SecondSequence = File.Substring(File.IndexOf("Second Sequence:") + "Second Sequence:".Length).Trim();
            return new Tuple<string, string>(FirstSequence, SecondSequence);
        }


        private string SHA1HashStringForUTF8String(string Input)
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
        private static byte[] GetText(string AlignmentResult, float ScoreResult, string UniqeIdentifier, string Algorithm, string ScoringMatrix, int Gap, int GapOpenPenalty, int GapExtensionPenalty)
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

    }
}
