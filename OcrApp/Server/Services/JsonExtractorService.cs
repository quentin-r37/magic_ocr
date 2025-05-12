using System.Text.Json;

namespace OcrApp.Server.Services;

public class JsonExtractorService(ILogger<JsonExtractorService> logger)
{
    public T? ExtractJsonFromText<T>(string text) where T : class
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogWarning("Empty or null text provided for JSON extraction");
            return null;
        }

        try
        {
            var cleanedText = text.Trim();
            if (cleanedText.Contains("```json", StringComparison.OrdinalIgnoreCase))
            {
                var parts = cleanedText.Split(["```json"], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var jsonPart = parts[1].Split(["```"], StringSplitOptions.None)[0].Trim();
                    cleanedText = jsonPart;
                }
            }
            else if (cleanedText.Contains("```"))
            {
                var parts = cleanedText.Split(["```"], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    cleanedText = parts[1].Trim();
                }
            }
            if (cleanedText.Contains('{') && cleanedText.Contains('}'))
            {
                var start = cleanedText.IndexOf("{");
                var end = cleanedText.LastIndexOf("}") + 1;

                if (start >= 0 && end > start)
                {
                    cleanedText = cleanedText[start..end];
                }
            }
            return JsonSerializer.Deserialize<T>(cleanedText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            logger.LogError($"JSON deserialization error: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error extracting JSON from text: {ex.Message}");
            return null;
        }
    }
}