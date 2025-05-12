using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference;
#pragma warning disable SKEXP0070

namespace OcrApp.Server;

public class MistralService
{
    private readonly ILogger<MistralService> _logger;
    private readonly Kernel _kernel;

    public MistralService(ILogger<MistralService> logger, IConfiguration configuration)
    {
        _logger = logger;

        var mistralApiKey = configuration["MISTRAL_API_KEY"] ??
                            throw new InvalidOperationException("MISTRAL_API_KEY is not configured");
        var mistralEndpoint = configuration["MISTRAL_ENDPOINT"] ??
                               "https://ais-doc-ocr-dev.services.ai.azure.com/models";
        var mistralModel = configuration["MISTRAL_MODEL"] ??
                            "mistral-small-2503";

        _kernel = Kernel.CreateBuilder()
            .AddAzureAIInferenceChatCompletion(
                modelId: mistralModel,
                endpoint: new Uri(mistralEndpoint),
                apiKey: mistralApiKey)
            .Build();

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-key", mistralApiKey);
    }

    public async Task<string> GenerateFromImage(string imagePath, string promptText)
    {
        try
        {
            _logger.LogInformation($"Generating text from image with prompt: {promptText}");
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(new ChatMessageContentItemCollection()
            {
                new TextContent(promptText),
                new ImageContent(imageBytes, "image/jpeg")
            });
            var executionSettings = new AzureAIInferencePromptExecutionSettings()
            {
                Temperature = 0.1f,
                MaxTokens = 4096
            };
            var result = await _kernel.GetRequiredService<IChatCompletionService>()
                .GetChatMessageContentAsync(chatHistory, executionSettings);
            return result.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating text from image: {ex.Message}");
            return $"{{\"error\": \"{ex.Message}\"}}";
        }
    }

    public async Task<string> ExecutePrompt(string promptText)
    {
        try
        {
            var result = await _kernel.InvokePromptAsync(promptText);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing prompt: {ex.Message}");
            return $"{{\"error\": \"{ex.Message}\"}}";
        }
    }
}