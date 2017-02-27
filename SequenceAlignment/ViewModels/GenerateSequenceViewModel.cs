namespace SequenceAlignment.ViewModels
{
    public class GenerateSequenceViewModel
    {
        public int SequenceLength { get; set; }
        public string Alphabet { get; set; }
        public int ConsecutiveMatch { get; set; }
        public char Position { get; set; } = 'M';
    }
}
