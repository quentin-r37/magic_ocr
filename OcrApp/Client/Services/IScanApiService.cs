using Microsoft.AspNetCore.Http;
using OcrApp.Shared;
using Refit;

namespace OcrApp.Client.Services
{
    public interface IScanApiService
    {
        [Multipart]
        [Post("/api/scan")]
        Task<ScanResponse> ScanAsync(
            [AliasAs("file")] ByteArrayPart file, 
            [AliasAs("option")] string? option = null);

        [Multipart]
        [Post("/api/extract")]
        Task<ExtractionResponse> ExtractAsync(
            [AliasAs("files")] IEnumerable<ByteArrayPart> files,
            [AliasAs("selectedSections")] IEnumerable<string> selectedSections);

        [Post("/api/extract/export")]
        Task<HttpResponseMessage> ExportResults([Body] MultiDocumentResponse data);
    }
}
