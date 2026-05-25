using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletInsightAggregate;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace FinanceTrack.Finance.Infrastructure.Ai;

public sealed class YandexAiInsightsService(
    IHttpClientFactory httpClientFactory,
    IOptions<YandexAiOptions> options,
    ILogger<YandexAiInsightsService> logger
) : IWalletInsightsAiService
{
    private static readonly Uri YandexBaseUri = new("https://llm.api.cloud.yandex.net/v1/");
    private readonly YandexAiOptions _options = options.Value;

    public bool IsEnabled => _options.Enabled;

    public async Task<WalletInsightsAiResult> GenerateInsightsAsync(
        WalletInsightsInput input,
        CancellationToken cancel
    )
    {
        var anomalies = input.PriorMonthsAverageExpenses.Count == 0
            ? "Недостаточно данных для выявления аномалий: за предыдущие 3 месяца расходов не было. Как только накопится история, здесь появится анализ отклонений."
            : await CallYandexGptAsync(BuildAnomaliesPrompt(input), cancel);

        var trends = input.MonthlyFlows.Count < 2
            ? "Недостаточно данных для анализа трендов: нужна история хотя бы за два месяца."
            : await CallYandexGptAsync(BuildTrendsPrompt(input), cancel);

        var expenseStructure = input.CurrentMonthExpenses.Count == 0
            ? "В текущем месяце расходов ещё нет — структура расходов не может быть проанализирована."
            : await CallYandexGptAsync(BuildExpenseStructurePrompt(input), cancel);

        string? recommendations = null;
        if (input.Goal is not null)
            recommendations = await CallYandexGptAsync(
                BuildRecommendationsPrompt(input.Goal),
                cancel
            );

        return new WalletInsightsAiResult(anomalies, trends, expenseStructure, recommendations);
    }

    private async Task<string?> CallYandexGptAsync(string prompt, CancellationToken cancel)
    {
        try
        {
            var http = httpClientFactory.CreateClient(YandexAiCategoryService.HttpClientName);
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
                cancellationToken: cancel
            );

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "AI insights call failed for prompt starting with: {Prefix}",
                prompt.Length > 80 ? prompt[..80] : prompt
            );
            return null;
        }
    }

    private static string BuildAnomaliesPrompt(WalletInsightsInput input)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "Ты - персональный финансовый аналитик. Проанализируй данные о расходах по категориям "
                + "и выяви аномалии - категории, где расходы текущего месяца значительно отличаются "
                + "от среднего за предыдущие 3 месяца. Напиши 2–4 предложения на русском языке. "
                + "Указывай конкретные цифры и названия категорий. "
                + "Если аномалий нет - напиши об этом кратко."
        );
        sb.AppendLine();
        sb.AppendLine("Расходы текущего месяца по категориям:");
        if (input.CurrentMonthExpenses.Count == 0)
        {
            sb.AppendLine("  (нет расходов)");
        }
        else
        {
            foreach (var c in input.CurrentMonthExpenses)
                sb.AppendLine($"  {c.CategoryName}: {c.Amount:F2} ({c.Percentage:F1}%)");
        }

        sb.AppendLine();
        sb.AppendLine("Среднемесячные расходы за предыдущие 3 месяца:");
        if (input.PriorMonthsAverageExpenses.Count == 0)
        {
            sb.AppendLine("  (нет данных)");
        }
        else
        {
            foreach (var c in input.PriorMonthsAverageExpenses)
                sb.AppendLine($"  {c.CategoryName}: {c.Amount:F2} ({c.Percentage:F1}%)");
        }

        return sb.ToString();
    }

    private static string BuildTrendsPrompt(WalletInsightsInput input)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "Ты - персональный финансовый аналитик. Проанализируй динамику доходов и расходов "
                + "за несколько месяцев и опиши устойчивые тенденции (тренды). Напиши 2–4 предложения "
                + "на русском языке. Указывай конкретные цифры. "
                + "Если явных трендов нет - напиши об этом кратко."
        );
        sb.AppendLine();
        sb.AppendLine("Данные по месяцам (доходы / расходы / нетто):");
        foreach (var m in input.MonthlyFlows.OrderBy(m => m.Year).ThenBy(m => m.Month))
        {
            sb.AppendLine(
                $"  {m.Year}-{m.Month:D2}: доходы {m.Income:F2}, расходы {m.Expense:F2}, нетто {m.NetFlow:F2}"
            );
        }

        return sb.ToString();
    }

    private static string BuildExpenseStructurePrompt(WalletInsightsInput input)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "Ты - персональный финансовый аналитик. Опиши структуру расходов текущего месяца: "
                + "какие категории занимают наибольшую долю и как выглядит распределение трат. "
                + "Напиши 2–4 предложения на русском языке с конкретными цифрами и процентами."
        );
        sb.AppendLine();
        sb.AppendLine("Расходы текущего месяца по категориям:");
        if (input.CurrentMonthExpenses.Count == 0)
        {
            sb.AppendLine("  (нет расходов)");
        }
        else
        {
            foreach (var c in input.CurrentMonthExpenses.OrderByDescending(c => c.Amount))
                sb.AppendLine($"  {c.CategoryName}: {c.Amount:F2} ({c.Percentage:F1}%)");
        }

        return sb.ToString();
    }

    private static string BuildRecommendationsPrompt(WalletGoalData goal)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "Ты - персональный финансовый советник. На основе приведённых ниже вычисленных данных "
                + "напиши краткую рекомендацию о прогрессе к финансовой цели. "
                + "Напиши 2–3 предложения на русском языке. Используй только предоставленные цифры - не придумывай."
        );
        sb.AppendLine();
        sb.AppendLine($"Текущий баланс: {goal.CurrentBalance:F2}");
        sb.AppendLine($"Цель накопления: {goal.TargetAmount:F2}");
        sb.AppendLine(
            goal.TargetDate.HasValue
                ? $"Желаемая дата достижения цели: {goal.TargetDate.Value:yyyy-MM-dd}"
                : "Желаемая дата: не задана"
        );
        sb.AppendLine(
            $"Среднемесячное нетто за последние 3 месяца: {goal.AverageMonthlyNetFlow:F2}"
        );

        if (goal.MonthsToGoal > 0)
            sb.AppendLine($"Расчётное время до цели при текущем темпе: {goal.MonthsToGoal} мес.");
        else if (goal.AverageMonthlyNetFlow <= 0)
            sb.AppendLine(
                "При текущем среднем нетто цель не достигается (расходы превышают доходы или равны им)."
            );
        else
            sb.AppendLine("Цель уже достигнута или осталось менее месяца.");

        return sb.ToString();
    }
}
