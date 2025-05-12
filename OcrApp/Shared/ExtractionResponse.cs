using System.Text.Json.Serialization;

namespace OcrApp.Shared;

public class ExtractionResponse
{
    public MultiDocumentResponse? MultiDocumentResponse { get; set; } = new();
    public string? Summary { get; set; }
    [JsonPropertyName("extracted_values")]
    public List<ExtractedValue> ExtractedValues { get; set; } = [];
}
