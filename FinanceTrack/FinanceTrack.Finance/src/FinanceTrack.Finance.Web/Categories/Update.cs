using FinanceTrack.Finance.UseCases.Categories.Update;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Categories;

public class UpdateCategoryRequest
{
    public const string Route = "/Categories/{CategoryId:guid}";
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

public class UpdateCategoryValidator : Validator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class UpdateCategory(IMediator mediator)
    : Endpoint<UpdateCategoryRequest, CategoryRecord>
{
    public override void Configure()
    {
        Put(UpdateCategoryRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(UpdateCategoryRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new UpdateCategoryCommand(req.CategoryId, userId, req.Name, req.Icon, req.Color);
        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        var c = result.Value;
        Response = new CategoryRecord(c.Id, c.Name, c.Type, c.Icon, c.Color);
    }
}
