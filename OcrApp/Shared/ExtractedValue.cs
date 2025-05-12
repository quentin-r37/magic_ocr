namespace OcrApp.Shared;

public class ExtractedValue
{
    public string Section { get; set; } = "";
    public object Value { get; set; } = "";
    public double Confidence { get; set; }
    public int Page { get; set; } = 1;
}