using System.ComponentModel.DataAnnotations;

namespace SequenceAlignment.ViewModels
{
    public class SequenceViewModel
    {
        [Required(ErrorMessage = "You have to enter the first sequence"),MaxLength(20000,ErrorMessage ="First Sequence can't be greater than 20,000")]
        public string FirstSequence { get; set; }
        [Required(ErrorMessage = "You have to enter the second sequence"), MaxLength(20000, ErrorMessage = "Second Sequence can't be greater than 20,000")]
        public string SecondSequence { get; set; }
        public int Gap { get; set; }
        public int GapOpenPenalty { get; set; }
        public int GapExtensionPenalty { get; set; }
        public string Algorithm { get; set; }
        public string ScoringMatrix { get; set; }
    }
}
