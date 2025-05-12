using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using System.Collections.Generic;
using OcrApp.Server.Services;

public class JsonExtractorServiceTests
{
    private readonly ILogger<JsonExtractorService> loggerSubstitute;
    private readonly JsonExtractorService _jsonExtractorService;

    public JsonExtractorServiceTests()
    {
        loggerSubstitute = Substitute.For<ILogger<JsonExtractorService>>();
        _jsonExtractorService = new JsonExtractorService(loggerSubstitute);
    }

    public class ExtractedValues
    {
        public List<ExtractedSection> extracted_values { get; set; }
    }

    public class ExtractedSection
    {
        public string section { get; set; }
        public string value { get; set; }
        public decimal confidence { get; set; }
    }

    [Fact]
    public void ExtractJsonFromText_NullOrEmptyInput_ReturnsNull()
    {
        // Arrange
        string nullText = null;
        string emptyText = "";
        string whitespaceText = "   ";

        // Act
        var resultNull = _jsonExtractorService.ExtractJsonFromText<ExtractedValues>(nullText);
        var resultEmpty = _jsonExtractorService.ExtractJsonFromText<ExtractedValues>(emptyText);
        var resultWhitespace = _jsonExtractorService.ExtractJsonFromText<ExtractedValues>(whitespaceText);

        // Assert
        Assert.Null(resultNull);
        Assert.Null(resultEmpty);
        Assert.Null(resultWhitespace);
    }

    [Fact]
    public void ExtractJsonFromText_WithJsonCodeBlock_ExtractsCorrectly()
    {
        // Arrange
        var input = @"Some text before
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
```
Some text after";

        // Act
        var result = _jsonExtractorService.ExtractJsonFromText<ExtractedValues>(input);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.extracted_values);
        Assert.Equal(2, result.extracted_values.Count);
        Assert.Equal("Name", result.extracted_values[0].section);
        Assert.Equal("ROGER", result.extracted_values[0].value);
        Assert.Equal(1, result.extracted_values[0].confidence);
        Assert.Equal("FirstName", result.extracted_values[1].section);
        Assert.Equal("Christophe", result.extracted_values[1].value);
    }

    [Fact]
    public void ExtractJsonFromText_WithGenericCodeBlock_ExtractsCorrectly()
    {
        // Arrange
        var input = @"Some text before
```
{
  ""extracted_values"": [
    {
      ""section"": ""Name"",
      ""value"": ""ROGER"",
      ""confidence"": 1
    }
  ]
}
```
Some text after";

        // Act
        var result = _jsonExtractorService.ExtractJsonFromText<ExtractedValues>(input);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.extracted_values);
        Assert.Single(result.extracted_values);
        Assert.Equal("Name", result.extracted_values[0].section);
        Assert.Equal("ROGER", result.extracted_values[0].value);
    }

    [Fact]
    public void ExtractJsonFromText_WithoutCodeBlock_ExtractsJsonByBraces()
    {
        // Arrange
        var input = @"Some text before
{
  ""extracted_values"": [
    {
      ""section"": ""Test"",
      ""value"": ""Value"",
      ""confidence"": 0.95
    }
  ]
}
Some text after";

        // Act
        var result = _jsonExtractorService.ExtractJsonFromText<ExtractedValues>(input);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.extracted_values);
        Assert.Single(result.extracted_values);
        Assert.Equal("Test", result.extracted_values[0].section);
        Assert.Equal("Value", result.extracted_values[0].value);
        Assert.Equal(0.95m, result.extracted_values[0].confidence);
    }

    [Fact]
    public void ExtractJsonFromText_InvalidJson_ReturnsNull()
    {
        // Arrange
        var input = @"```json
{
  ""extracted_values"": [
    {
      ""section"": ""Name"",
      ""value"": ""ROGER"",
      ""confidence"": 1,
    }
  ]
}
```";  // Note the extra comma after confidence: 1, which makes this invalid JSON

        // Act
        var result = _jsonExtractorService.ExtractJsonFromText<ExtractedValues>(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractJsonFromText_MismatchedType_ReturnsNull()
    {
        // Arrange
        var input = @"```json
{
  ""extracted_values"": ""not_an_array""
}
```";

        // Act
        var result = _jsonExtractorService.ExtractJsonFromText<ExtractedValues>(input);

        // Assert
        Assert.Null(result);
    }
}