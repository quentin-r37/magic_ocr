using OcrApp.Shared;
using SkiaSharp;
using System.Runtime.Versioning;

namespace OcrApp.Server.Services;

public class DocumentProcessingService(ILogger<DocumentProcessingService> logger, MistralService mistralService, JsonExtractorService jsonExtractorService)
{
    private readonly string _tempDir = Path.GetTempPath();

    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("windows")]
    public async Task<List<Section>> ExtractSectionsFromDocument(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var extension = Path.GetExtension(filePath).ToLower();
        var allSections = new List<Section>();

        if (extension == ".pdf")
        {
            var images = await ConvertPdfToImages(filePath);

            for (var i = 0; i < images.Count; i++)
            {
                var pageSections = await ExtractSectionsFromImage(images[i]);
                foreach (var section in pageSections)
                {
                    section.Page = i + 1;
                    section.DocIndex = 0;
                    allSections.Add(section);
                }
            }
        }
        else
        {
            await using var fileStream = File.OpenRead(filePath);
            var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var sections = await ExtractSectionsFromImage(memoryStream);
            foreach (var section in sections)
            {
                section.Page = 1;
                section.DocIndex = 0;
                allSections.Add(section);
            }
        }

        var uniqueSections = allSections
            .GroupBy(s => s.Title)
            .Select(g => g.First())
            .ToList();

        return uniqueSections;
    }

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    public async Task<List<Stream>> ConvertPdfToImages(string pdfPath, int maxPages = 50)
    {
        var images = new List<Stream>();

        try
        {
            await using var pdfStream = File.OpenRead(pdfPath);

            var pageCount = 0;

            await foreach (var bitmap in PDFtoImage.Conversion.ToImagesAsync(pdfStream))
            {
                if (pageCount >= maxPages)
                    break;

                using var skImage = SKImage.FromBitmap(bitmap);
                using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);
                var memoryStream = new MemoryStream();
                data.SaveTo(memoryStream);
                memoryStream.Position = 0;
                images.Add(memoryStream);

                pageCount++;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error converting PDF to images: {ex.Message}");
            throw;
        }
        return images;
    }

    public async Task<List<Section>> ExtractSectionsFromImage(Stream imageStream)
    {
        try
        {
            // Save stream to temp file
            var tempImagePath = Path.Combine(_tempDir, $"temp_image_{Guid.NewGuid()}.png");
            await using (var fileStream = File.Create(tempImagePath))
            {
                imageStream.Position = 0;
                await imageStream.CopyToAsync(fileStream);
            }

            var prompt = @"
            Examine cette image de document et extrait tous les titres de sections, champs ou entités présentes.
            
            Retourne UNIQUEMENT une liste au format JSON sous cette forme précise sans renvoyer les entités (field):
            {
              ""sections"": [
                {
                  ""title"": ""Nom du titre ou champ 1"",
                  ""level"": 1,
                  ""type"": ""section|header""
                },
                {
                  ""title"": ""Nom du titre ou champ 2"",
                  ""level"": 2,
                  ""type"": ""section|header""
                }
              ]
            }
            
            Assure-toi que le JSON est parfaitement formaté et inclut absolument TOUTES les sections ou champs visibles.
            Ne renvoie aucune explication, juste le JSON.
            ";

            var result = await mistralService.GenerateFromImage(tempImagePath, prompt);

            // Clean up the temp file
            if (File.Exists(tempImagePath))
                File.Delete(tempImagePath);

            var sections = jsonExtractorService.ExtractJsonFromText<SectionsResponse>(result)?.Sections;
            return sections;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error extracting sections from image: {ex.Message}");
            return new List<Section>();
        }
    }

    public async Task<List<ExtractedValue>> ExtractSectionValues(string imagePath, List<string> selectedSections)
    {
        try
        {
            var sections = selectedSections.Select(s =>
            {
                if (s.Contains(" (Niveau:"))
                    return s.Split(" (Niveau:")[0].Trim();
                return s.Trim();
            }).ToList();

            var sectionsText = string.Join("\n", sections.Select(s => $"- {s}"));
            var prompt = $@"
            Examine cette image de document et extrait les valeurs correspondant exactement aux champs ou sections suivants:
            
            {sectionsText}
            
            Pour chaque section ou champ, trouve la valeur ou le contenu correspondant.
            Lorsque le contenu est une liste de valeur renvoi moi un tableau JSON avec les valeurs.
            Retourne UNIQUEMENT un objet JSON au format:
            
            {{
              ""extracted_values"": [
                {{
                  ""section"": ""Nom du champ 1"",
                  ""value"": ""Valeur extraite 1"",
                  ""confidence"": 0.95
                }},
                {{
                  ""section"": ""Nom du champ 2"",
                  ""value"": ""Valeur extraite 2"",
                  ""confidence"": 0.85
                }},
                {{
                  ""section"": ""Nom du champ 3"",
                  ""value"": [""Valeur extraite 3.1"", ""Valeur 3.2""],
                  ""confidence"": 0.85
                }}
              ]
            }}
            
            Si tu ne trouves pas de valeur pour un champ, indique une chaîne vide pour ""value"" et 0 pour ""confidence"".
            Assure-toi que le JSON est parfaitement formaté. Ne renvoie aucune explication, juste le JSON.
            ";

            var result = await mistralService.GenerateFromImage(imagePath, prompt);

            var input = @"
```json
{
  ""extracted_values"": [
    {
      ""section"": ""Name"",
      ""value"": ""ROGER"",
      ""confidence"": 1
    },
    {
      ""section"": ""FirstName"",
      ""value"": ""Christophe"",
      ""confidence"": 1
    }
  ]
}
```";

            var extractionResult = jsonExtractorService.ExtractJsonFromText<ExtractionResponse>(result);
            return extractionResult?.ExtractedValues ?? new List<ExtractedValue>();
        }
        catch (Exception ex)
        {
            logger.LogError($"Error extracting values from image: {ex.Message}");
            return new List<ExtractedValue>();
        }
    }
   
}