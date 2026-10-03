using FinanceTrack.Finance.UseCases.Categories.Delete;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Categories;

public class DeleteCategoryRequest
{
    public const string Route = "/Categories/{CategoryId:guid}";
    public Guid CategoryId { get; set; }
}

public class DeleteCategory(IMediator mediator) : Endpoint<DeleteCategoryRequest>
{
    public override void Configure()
    {
        Delete(DeleteCategoryRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(DeleteCategoryRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new DeleteCategoryCommand(req.CategoryId, userId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendNoContentAsync(ct);
    }
}
