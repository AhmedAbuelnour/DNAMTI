namespace SequenceAlignment.ViewModels
{
    public class GridViewModel
    {
        public int Gap { get; set; }
        public int GapOpenPenalty { get; set; }
        public int GapExtensionPenalty { get; set; }
        public string ScoringMatrix { get; set; }
    }
}
