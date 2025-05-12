namespace OcrApp.Shared;

public class MultiDocumentResponse
{
    public string Timestamp { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    public string Mode { get; set; } = "api";
    public List<DocumentResult> Documents { get; set; } = [];
}