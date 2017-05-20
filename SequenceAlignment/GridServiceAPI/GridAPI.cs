namespace SequenceAlignment.GridServiceAPI
{
    public class GridAPI
    {
        public string FirstSequence { get; set; }
        public string SecondSequence { get; set; }
        public int Gap { get; set; }
        public int GapOpenPenalty { get; set; }
        public int GapExtensionPenalty { get; set; }
        public string ScoringMatrix { get; set; }
        public string Email { get; set; }
    }
}
