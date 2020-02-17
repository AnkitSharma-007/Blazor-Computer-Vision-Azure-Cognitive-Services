namespace BlazorComputerVision.Models
{
    public class RecognitionResult
    {
        public int Page { get; set; }
        public double ClockwiseOrientation { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Unit { get; set; }
        public Line[] Lines { get; set; }
    }
}
