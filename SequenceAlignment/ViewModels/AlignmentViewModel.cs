namespace SequenceAlignment.ViewModels
{
    public class AlignmentViewModel
    {
        public string FirstSequence { get; set; }
        public string SecondSequence { get; set; }
        public string Algorithm { get; set; }
        public string ScoreMatrix { get; set; }
        public int Gap { get; set; }
        public int GapOpenPenalty { get; set; }
        public int GapExtensionPenalty { get; set; }
    }
}
