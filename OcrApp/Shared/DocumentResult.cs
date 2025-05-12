namespace OcrApp.Shared;

public class DocumentResult
{
    public string Document { get; set; } = "";
    public List<ExtractedValue> ExtractedValues { get; set; } = [];
}