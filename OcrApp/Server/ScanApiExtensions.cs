using System.Text.Json;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using OcrApp.Shared;
using OcrApp.Server.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Runtime.Versioning;

#pragma warning disable SKEXP0070

namespace OcrApp.Server;

public static class ScanApiExtensions
{
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    public static WebApplication AddScanApi(this WebApplication app)
    {
        app.MapPost("/api/scan",
                async (IFormFile file, string? option, DocumentProcessingService docService) =>
                {
                    try
                    {
                        var tempPath = Path.Combine(Path.GetTempPath(),
                            Path.GetRandomFileName() + Path.GetExtension(file.FileName));
                        await using (var stream = new FileStream(tempPath, FileMode.Create))
                        {
                            await file.OpenReadStream().CopyToAsync(stream);
                        }
                        var sections = await docService.ExtractSectionsFromDocument(tempPath);

                        if (File.Exists(tempPath))
                            File.Delete(tempPath);

                        var sectionChoices = sections.Select(section =>
                            $"{section.Title} (Niveau: {section.Level}, Type: {section.Type}, Page: {section.Page})"
                        ).ToList();

                        return Results.Ok(new ScanResponse()
                        {
                            Sections = sections,
                            SectionChoices = sectionChoices
                        });
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem($"Error during scan: {ex.Message}");
                    }
                })
            .WithName("ScanDocument")
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery()
            .WithTags("Documents")
            .RequireAuthorization("ApiOrJwt");

        app.MapPost("/api/extract",
                   async (HttpRequest request, DocumentProcessingService docService, [FromForm] ExtractionRequest req) =>
                   {
                       try
                       {
                           if (!request.HasFormContentType)
                               return Results.BadRequest("Form content type expected");

                           var form = await request.ReadFormAsync();
                           var files = form.Files;
                           var selectedSections = req.SelectedSections;

                           if (files == null || !files.Any())
                               return Results.BadRequest("No files uploaded");

                           if (selectedSections == null || !selectedSections.Any())
                               return Results.BadRequest("No sections selected");

                           var sectionTitles = selectedSections.Select(section =>
                           {
                               if (section.Contains(" (Niveau:"))
                                   return section.Split(" (Niveau:")[0].Trim();
                               return section.Trim();
                           }).ToList();

                           var allResults = new List<DocumentResult>();

                           foreach (var file in files)
                           {
                               var tempPath = Path.Combine(Path.GetTempPath(),
                                   Path.GetRandomFileName() + Path.GetExtension(file.FileName));

                               await using (var stream = new FileStream(tempPath, FileMode.Create))
                               {
                                   await file.CopyToAsync(stream);
                               }

                               var docResult = new DocumentResult
                               {
                                   Document = file.FileName,
                                   ExtractedValues = new List<ExtractedValue>()
                               };

                               var extension = Path.GetExtension(tempPath).ToLower();

                               if (extension == ".pdf")
                               {
                                   var images = await docService.ConvertPdfToImages(tempPath);

                                   for (int pageNum = 0; pageNum < images.Count; pageNum++)
                                   {
                                       var tempImagePath = Path.Combine(Path.GetTempPath(), $"temp_doc_page_{Guid.NewGuid()}.png");
                                       await using (var fileStream = File.Create(tempImagePath))
                                       {
                                           images[pageNum].Position = 0;
                                           await images[pageNum].CopyToAsync(fileStream);
                                       }

                                       var pageValues = await docService.ExtractSectionValues(tempImagePath, sectionTitles);

                                       foreach (var value in pageValues)
                                       {
                                           value.Page = pageNum + 1;
                                       }

                                       docResult.ExtractedValues.AddRange(pageValues);

                                       if (File.Exists(tempImagePath))
                                           File.Delete(tempImagePath);
                                   }
                               }
                               else
                               {
                                   var pageValues = await docService.ExtractSectionValues(tempPath, sectionTitles);
                                   foreach (var value in pageValues)
                                   {
                                       value.Page = 1;
                                   }

                                   docResult.ExtractedValues.AddRange(pageValues);
                               }

                               allResults.Add(docResult);

                               if (File.Exists(tempPath))
                                   File.Delete(tempPath);
                           }

                           var result = new MultiDocumentResponse
                           {
                               Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                               Mode = "api",
                               Documents = allResults
                           };

                           var summary = $"Extraction réussie pour {files.Count} documents (Mode: api):\n\n";

                           foreach (var docResult in allResults)
                           {
                               summary += $"Document: {docResult.Document} ({docResult.ExtractedValues.Count} valeurs extraites)\n";

                               foreach (var item in docResult.ExtractedValues)
                               {
                                   var confidence = item.Confidence * 100;
                                   summary += $"  • {item.Section} (p.{item.Page}): {item.Value} (confiance: {confidence:0}%)\n";
                               }

                               summary += "\n";
                           }

                           return Results.Ok(new ExtractionResponse()
                           {
                               Summary = summary,
                               MultiDocumentResponse = result
                           });
                       }
                       catch (Exception ex)
                       {
                           return Results.Problem($"Error during extraction: {ex.Message}");
                       }
                   })
                   .WithName("ExtractValues")
                   .Accepts<ExtractionRequest>("multipart/form-data")
                   .DisableAntiforgery()
                   .WithTags("Documents")
                   .RequireAuthorization("ApiOrJwt");

        app.MapPost("/api/extract/export",
                   async Task<Results<FileContentHttpResult, BadRequest<string>>>([FromBody] MultiDocumentResponse data) =>
                   {
                       if (data?.Documents == null || !data.Documents.Any())
                       {
                           return TypedResults.BadRequest("No data provided for export.");
                       }

                       try
                       {
                           using var workbook = new XLWorkbook();
                           var worksheet = workbook.Worksheets.Add("Extracted Data");

                           // Add headers
                           worksheet.Cell(1, 1).Value = "Document";
                           worksheet.Cell(1, 2).Value = "Section";
                           worksheet.Cell(1, 3).Value = "Value";
                           worksheet.Cell(1, 4).Value = "Confidence";
                           worksheet.Cell(1, 5).Value = "Page";
                           worksheet.Row(1).Style.Font.Bold = true;

                           var currentRow = 2;
                           foreach (var document in data.Documents)
                           {
                               foreach (var value in document.ExtractedValues)
                               {
                                   worksheet.Cell(currentRow, 1).Value = document.Document;
                                   worksheet.Cell(currentRow, 2).Value = value.Section;
                                   // Handle potential array values
                                   if (value.Value is JsonElement { ValueKind: JsonValueKind.Array } jsonElement)
                                   {
                                       worksheet.Cell(currentRow, 3).Value = string.Join("; ", jsonElement.EnumerateArray());
                                   }
                                   else
                                   {
                                       worksheet.Cell(currentRow, 3).Value = value.Value?.ToString();
                                   }
                                   worksheet.Cell(currentRow, 4).Value = value.Confidence;
                                   worksheet.Cell(currentRow, 5).Value = value.Page;
                                   currentRow++;
                               }
                           }

                           worksheet.Columns().AdjustToContents();

                           using var stream = new MemoryStream();
                           workbook.SaveAs(stream);
                           var content = stream.ToArray();
                           var fileName = $"ExtractedData_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                           return TypedResults.File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                       }
                       catch (Exception ex)
                       {
                           // Log the exception details server-side
                           Console.WriteLine($"Error generating Excel file: {ex}"); // Basic logging
                           return TypedResults.BadRequest($"Error generating Excel file: {ex.Message}");
                       }
                   })
                   .WithName("ExportExtractedData")
                   .DisableAntiforgery()
                   .WithTags("Documents")
                   .RequireAuthorization("ApiOrJwt");

        return app;
    }
}