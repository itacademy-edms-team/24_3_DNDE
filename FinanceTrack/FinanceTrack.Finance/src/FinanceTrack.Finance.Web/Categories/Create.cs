using FinanceTrack.Finance.UseCases.Categories.Create;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Categories;

public class CreateCategoryRequest
{
    public const string Route = "/Categories";
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!; // "Income" or "Expense"
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

public class CreateCategoryResponse(Guid id)
{
    public Guid Id { get; set; } = id;
}

public class CreateCategoryValidator : Validator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Type).NotEmpty().WithMessage("Type is required.");
    }
}

public class CreateCategory(IMediator mediator)
    : Endpoint<CreateCategoryRequest, CreateCategoryResponse>
{
    public override void Configure()
    {
        Post(CreateCategoryRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new CreateCategoryCommand(userId, req.Name, req.Type, req.Icon, req.Color);
        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendCreatedAtAsync<CreateCategory>(
            new { id = result.Value },
            new CreateCategoryResponse(result.Value),
            cancellation: ct
        );
    }
}
