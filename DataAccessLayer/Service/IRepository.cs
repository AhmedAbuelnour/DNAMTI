using BioEdge.Alignment;
using DataAccessLayer.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Service
{
    public interface IRepository
    {
        void DeleteAlignmentJob(string AlignmentJobID);
        Task DeleteAlignmentJobAsync(string AlignmentJobID);
        Task AddAlignmentJobAsync(AlignmentJob Job);
        void AddAlignmentJob(AlignmentJob Job);
        Task AddAlignmentJobAsync(string SequenceA, string SequenceNameA, string SequenceB, string SequenceNameB, string UserId);
        void AddAlignmentJob(string SequenceA, string SequenceNameA, string SequenceB, string SequenceNameB, string UserId);
        Task<AlignmentJob> GetAlignmentJobByIdAsync(string AlignmentJobID);
        AlignmentJob GetAlignmentJobById(string AlignmentJobID);
        Task<IEnumerable<AlignmentJob>> GetAlignmentJobsAsync();
        IEnumerable<AlignmentJob> GetAlignmentJobs();
        Task<IEnumerable<AlignmentJob>> GetPendingAlignmentJobsAsync();
        IEnumerable<AlignmentJob> GetPendingAlignmentJobs();
        void FinalizeJob(string AlignmentJobID, AlignedSequences AlignmentResult);
        Task FinalizeJobAsync(string AlignmentJobID, AlignedSequences AlignmentResult);
        bool IsFinished(string AlignmentJobID);
        Task<bool> IsFinishedAsync(string AlignmentJobID);
        AlignmentJob AreExist(string FirstSequence, string SecondSequence);
        Tuple<string, string> GetSubmittedSequences(string AlignmentJobID);
    }
}
