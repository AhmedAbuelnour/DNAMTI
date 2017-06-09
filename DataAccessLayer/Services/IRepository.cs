using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Services
{
    public interface IRepository
    {
        void DeleteAlignmentJob(string AlignmentJobID);
        Task DeleteAlignmentJobAsync(string AlignmentJobID);
        Task AddAlignmentJobAsync(AlignmentJob Job);
        void AddAlignmentJob(AlignmentJob Job);
        string GetEmailByAlingmnetJobId(string AlignmentJobId);
        Task AddAlignmentJobAsync(string SequenceA, string SequenceNameA, string SequenceB, string SequenceNameB, string UserId);
        void AddAlignmentJob(string SequenceA, string SequenceNameA, string SequenceB, string SequenceNameB, string UserId);
        IEnumerable<AlignmentJob> GetHistory(string UserID);
        AlignmentJob GetAlignmentJobById(string AlignmentJobID);
        Task<IEnumerable<AlignmentJob>> GetAlignmentJobsAsync();
        IEnumerable<AlignmentJob> GetAlignmentJobs();
        Task<IEnumerable<string>> GetPendingAlignmentJobsAsync();
        IEnumerable<string> GetPendingAlignmentJobs();
        void FinalizeJob(string AlignmentJobID, string AlignmentResult);
        Task FinalizeJobAsync(string AlignmentJobID, string AlignmentResult);
        bool IsFinished(string AlignmentJobID);
        Task<bool> IsFinishedAsync(string AlignmentJobID);
        AlignmentJob AreExist(string FirstSequence, string SecondSequence, string ScoreMatrix , int Gap);

        Tuple<string, string> GetSubmittedSequences(string AlignmentJobID);

    }
}
