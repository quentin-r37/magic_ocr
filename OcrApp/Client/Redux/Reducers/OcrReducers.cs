using Fluxor;
using OcrApp.Client.Redux.Actions;
using OcrApp.Client.Redux.State;
using OcrApp.Shared;

namespace OcrApp.Client.Redux.Reducers;

public static class OcrReducers
{
    // Scan Document Reducers

    [ReducerMethod]
    public static OcrState OnScanDocument(OcrState state, OcrActions.ResetSectionsAction action)
    {
        return state with { SectionChoices = [], SelectedSections = [] };
    }
    [ReducerMethod]
    public static OcrState OnScanDocument(OcrState state, OcrActions.ScanDocumentAction action)
    {
        return state with { IsProcessingScan = true };
    }

    [ReducerMethod]
    public static OcrState OnScanDocumentSuccess(OcrState state, OcrActions.ScanDocumentSuccessAction action)
    {
        return state with
        {
            IsProcessingScan = false,
            SectionChoices = action.SectionChoices,
            DetectedSections = action.DetectedSections
        };
    }

    [ReducerMethod]
    public static OcrState OnScanDocumentFailure(OcrState state, OcrActions.ScanDocumentFailureAction action)
    {
        return state with { IsProcessingScan = false };
    }

    // Extract Values Reducers
    [ReducerMethod]
    public static OcrState OnExtractValues(OcrState state, OcrActions.ExtractValuesAction action)
    {
        return state with { IsProcessingExtract = true };
    }

    [ReducerMethod]
    public static OcrState OnExtractValuesSuccess(OcrState state, OcrActions.ExtractValuesSuccessAction action)
    {
        return state with
        {
            IsProcessingExtract = false,
            ExtractResultSummary = action.Summary,
            ExtractionResponse = action.Response
        };
    }

    [ReducerMethod]
    public static OcrState OnExtractValuesFailure(OcrState state, OcrActions.ExtractValuesFailureAction action)
    {
        return state with
        {
            IsProcessingExtract = false,
            ExtractResultSummary = $"Error: {action.ErrorMessage}"
        };
    }

    // Section Management Reducers
    [ReducerMethod]
    public static OcrState OnAddManualSection(OcrState state, OcrActions.AddManualSectionAction action)
    {
        var formattedSection = $"{action.Title} (Niveau: {action.Level}, Type: {action.Type}, Page: {action.Page})";
        
        if (state.SectionChoices.Contains(formattedSection))
        {
            return state;
        }
        
        var newSectionChoices = new List<string>(state.SectionChoices) { formattedSection };
        var newDetectedSections = new List<Section>(state.DetectedSections)
        {
            new Section
            {
                Title = action.Title,
                Level = action.Level,
                Type = action.Type,
                Page = action.Page,
                DocIndex = -1 // Manual entry indicator
            }
        };
        
        return state with
        {
            SectionChoices = newSectionChoices,
            DetectedSections = newDetectedSections
        };
    }

    [ReducerMethod]
    public static OcrState OnToggleSectionSelection(OcrState state, OcrActions.ToggleSectionSelectionAction action)
    {
        var newSelectedSections = new List<string>(state.SelectedSections);
        
        if (action.IsSelected)
        {
            if (!newSelectedSections.Contains(action.Section))
            {
                newSelectedSections.Add(action.Section);
            }
        }
        else
        {
            newSelectedSections.Remove(action.Section);
        }
        
        return state with { SelectedSections = newSelectedSections };
    }

    [ReducerMethod]
    public static OcrState OnClearSelectedSections(OcrState state, OcrActions.ClearSelectedSectionsAction action)
    {
        return state with { SelectedSections = [] };
    }

    // Reset Reducers
    [ReducerMethod]
    public static OcrState OnResetScan(OcrState state, OcrActions.ResetScanAction action)
    {
        return state with
        {
            IsProcessingScan = false,
            SectionChoices = [],
            DetectedSections = []
        };
    }

    [ReducerMethod]
    public static OcrState OnResetExtract(OcrState state, OcrActions.ResetExtractAction action)
    {
        return state with
        {
            IsProcessingExtract = false,
            ExtractResultSummary = null,
            ExtractionResponse = null
        };
    }

    [ReducerMethod]
    public static OcrState OnResetAll(OcrState state, OcrActions.ResetAllAction action)
    {
        return OcrState.Initial;
    }
}