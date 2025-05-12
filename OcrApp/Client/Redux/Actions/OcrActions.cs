using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using OcrApp.Shared;

namespace OcrApp.Client.Redux.Actions;

public class OcrActions
{
    // Scan Document Actions
    public record ResetSectionsAction();
    public record ScanDocumentAction(byte[] FileBytes, string FileName, string ContentType);
    public record ScanDocumentSuccessAction(List<string> SectionChoices, List<Section> DetectedSections);
    public record ScanDocumentFailureAction(string ErrorMessage);

    // Extract Values Actions
    public record ExtractValuesAction(List<byte[]> Files, List<string> FileNames, List<string> ContentTypes, List<string> SelectedSections);
    public record ExtractValuesSuccessAction(ExtractionResponse Response, string Summary);
    public record ExtractValuesFailureAction(string ErrorMessage);

    // Section Management Actions
    public record AddManualSectionAction(string Title, int Level = 1, string Type = "manual", int Page = 1);
    public record ToggleSectionSelectionAction(string Section, bool IsSelected);
    public record ClearSelectedSectionsAction();

    // Reset Actions
    public record ResetScanAction();
    public record ResetExtractAction();
    public record ResetAllAction();
}