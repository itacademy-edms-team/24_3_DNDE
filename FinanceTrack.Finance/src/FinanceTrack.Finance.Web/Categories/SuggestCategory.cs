using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Categories;

public class SuggestCategoryRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    /// <summary>"Income" or "Expense"</summary>
    public string Type { get; set; } = null!;
}

public class SuggestCategoryResponse
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? CategoryIcon { get; set; }
}

public class SuggestCategoryValidator : Validator<SuggestCategoryRequest>
{
    public SuggestCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Type)
            .Must(t => t == "Income" || t == "Expense")
            .WithMessage("Type must be 'Income' or 'Expense'.");
    }
}

public class SuggestCategory(ICategoryAiService aiService)
    : Endpoint<SuggestCategoryRequest, SuggestCategoryResponse>
{
    public override void Configure()
    {
        Post("/Categories/Suggest");
        Roles("user");
    }

    public override async Task HandleAsync(SuggestCategoryRequest req, CancellationToken cancel)
    {
        if (!aiService.IsEnabled)
        {
            AddError("AI categorization is not enabled.");
            await SendErrorsAsync(503, cancel);
            return;
        }

        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var suggestion = await aiService.SuggestCategoryAsync(
            req.Name,
            req.Description,
            req.Type,
            userId,
            cancel
        );

        if (suggestion == null)
        {
            await SendNotFoundAsync(cancel);
            return;
        }

        await SendAsync(
            new SuggestCategoryResponse
            {
                CategoryId = suggestion.Id,
                CategoryName = suggestion.Name,
                CategoryIcon = suggestion.Icon,
            },
            200,
            cancel
        );
    }
}
