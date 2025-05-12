using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Refit;

namespace OcrApp.Shared;

public class ExtractionRequest
{
    [AliasAs("files")]
    public List<IFormFile> Files { get; set; } = [];
    
    [AliasAs("selectedSections")]
    public List<string> SelectedSections { get; set; } = [];
}