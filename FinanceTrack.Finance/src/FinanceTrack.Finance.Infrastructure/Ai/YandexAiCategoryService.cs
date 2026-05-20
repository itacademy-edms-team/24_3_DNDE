using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using FinanceTrack.Finance.Core.CategoryAggregate;
using FinanceTrack.Finance.Core.CategoryAggregate.Specifications;
using FinanceTrack.Finance.Core.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace FinanceTrack.Finance.Infrastructure.Ai;

public sealed class YandexAiCategoryService(
    IHttpClientFactory httpClientFactory,
    IOptions<YandexAiOptions> options,
    IReadRepository<Category> categoryRepo,
    ILogger<YandexAiCategoryService> logger
) : ICategoryAiService
{
    public const string HttpClientName = "yandex-ai";
    private static readonly Uri YandexBaseUri = new("https://llm.api.cloud.yandex.net/v1/");
    private readonly YandexAiOptions _options = options.Value;

    public bool IsEnabled => _options.Enabled;

    public async Task<AiCategorySuggestion?> SuggestCategoryAsync(
        string name,
        string? description,
        string transactionType,
        string userId,
        CancellationToken cancel
    )
    {
        if (!_options.Enabled)
            return null;

        if (!CategoryType.TryFromName(transactionType, ignoreCase: true, out var categoryType))
        {
            logger.LogWarning(
                "AI category suggestion skipped: unknown transaction type '{Type}'",
                transactionType
            );
            return null;
        }

        var categories = await categoryRepo.ListAsync(
            new UserCategoriesByTypeSpec(userId, categoryType),
            cancel
        );

        if (categories.Count == 0)
            return null;

        var prompt = BuildPrompt(name, description, transactionType, categories);
        var responseText = await CallYandexGptAsync(prompt, cancel);

        if (responseText == null)
            return null;

        if (!Guid.TryParse(responseText.Trim(), out var categoryId) || categoryId == Guid.Empty)
        {
            logger.LogDebug(
                "AI returned non-UUID response '{Response}' — no suggestion",
                responseText.Trim()
            );
            return null;
        }

        var matched = categories.FirstOrDefault(c => c.Id == categoryId);
        if (matched == null)
        {
            logger.LogWarning(
                "AI returned category ID {Id} that does not belong to user {UserId}",
                categoryId,
                userId
            );
            return null;
        }

        return new AiCategorySuggestion(matched.Id, matched.Name, matched.Icon);
    }

    private async Task<string?> CallYandexGptAsync(string prompt, CancellationToken ct)
    {
        try
        {
            var http = httpClientFactory.CreateClient(HttpClientName);
            var transport = new HttpClientPipelineTransport(http);

            var clientOptions = new OpenAIClientOptions
            {
                Endpoint = YandexBaseUri,
                Transport = transport,
            };

            var client = new OpenAIClient(new ApiKeyCredential("placeholder"), clientOptions);
            var modelUri = $"gpt://{_options.FolderId}/{_options.Model}/latest";
            var chatClient = client.GetChatClient(modelUri);

            var response = await chatClient.CompleteChatAsync(
                [new UserChatMessage(prompt)],
                cancellationToken: ct
            );

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI category suggestion call failed");
            return null;
        }
    }

    private static string BuildPrompt(
        string name,
        string? description,
        string transactionType,
        IReadOnlyList<Category> categories
    )
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "You are a financial transaction categorizer. Based on the transaction details below, pick the most fitting category from the provided list."
        );
        sb.AppendLine();
        sb.AppendLine($"Transaction type: {transactionType}");
        sb.AppendLine($"Transaction name: {name}");
        sb.AppendLine(
            $"Transaction description: {(string.IsNullOrWhiteSpace(description) ? "not provided" : description)}"
        );
        sb.AppendLine();
        sb.AppendLine("Available categories (id: name):");
        foreach (var c in categories)
            sb.AppendLine($"{c.Id}: {c.Name}");
        sb.AppendLine();
        sb.AppendLine(
            "Reply with ONLY the UUID of the best matching category. If none fit, reply \"none\". Do not add any explanation."
        );
        return sb.ToString();
    }
}
