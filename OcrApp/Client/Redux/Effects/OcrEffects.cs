using System.IO;
using Blazored.Toast.Services;
using Fluxor;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using OcrApp.Client.Redux.Actions;
using OcrApp.Client.Services;
using OcrApp.Shared;
using Refit;

namespace OcrApp.Client.Redux.Effects;

public class OcrEffects(IScanApiService scanApiService, IToastService toastService)
{
    [EffectMethod]
    public async Task HandleScanDocument(OcrActions.ScanDocumentAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await scanApiService.ScanAsync(new ByteArrayPart(action.FileBytes, action.FileName));

            dispatcher.Dispatch(new OcrActions.ScanDocumentSuccessAction(
                response.SectionChoices ?? [],
                response.Sections ?? []
            ));
            toastService.ShowSuccess("Document successfully scanned");
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new OcrActions.ScanDocumentFailureAction(ex.Message));
            toastService.ShowError($"Error scanning document: {ex.Message}");
        }
    }

    [EffectMethod]
    public async Task HandleExtractValues(OcrActions.ExtractValuesAction action, IDispatcher dispatcher)
    {
        try
        {
            var formFiles = action.Files.Select((t, i) => new ByteArrayPart(t, action.FileNames[i], action.ContentTypes[i])).ToList();

            var response = await scanApiService.ExtractAsync(
                files: formFiles,
                selectedSections: action.SelectedSections);

            dispatcher.Dispatch(new OcrActions.ExtractValuesSuccessAction(
                response,
                response.Summary ?? "Extraction completed successfully"
            ));

            toastService.ShowSuccess("Values successfully extracted");
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new OcrActions.ExtractValuesFailureAction(ex.Message));
            toastService.ShowError($"Error extracting values: {ex.Message}");
        }
    }

  
}