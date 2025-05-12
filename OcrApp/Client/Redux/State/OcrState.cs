using OcrApp.Shared;

namespace OcrApp.Client.Redux.State;

public record OcrState(
    bool IsProcessingScan,
    bool IsProcessingExtract,
    List<string> SectionChoices,
    List<Section> DetectedSections,
    List<string> SelectedSections,
    string? ExtractResultSummary,
    ExtractionResponse? ExtractionResponse
)
{
    public static OcrState Initial => new(
        IsProcessingScan: false,
        IsProcessingExtract: false,
        SectionChoices: [],
        DetectedSections: [],
        SelectedSections: [],
        ExtractResultSummary: null,
        ExtractionResponse: null
    );
}

public enum Tab
{
    Configure,
    Extract
}