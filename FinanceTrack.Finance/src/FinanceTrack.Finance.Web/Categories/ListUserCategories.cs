using FinanceTrack.Finance.UseCases.Categories.List;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Categories;

public class ListUserCategoriesRequest
{
    [QueryParam]
    public string? Type { get; set; }
}

public class ListUserCategoriesResponse
{
    public List<CategoryRecord> Categories { get; set; } = [];
}

public class ListUserCategories(IMediator mediator)
    : Endpoint<ListUserCategoriesRequest, ListUserCategoriesResponse>
{
    public override void Configure()
    {
        Get("/Categories");
        Roles("user");
    }

    public override async Task HandleAsync(ListUserCategoriesRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var categories = await mediator.Send(new ListUserCategoriesQuery(userId, req.Type), ct);

        Response = new ListUserCategoriesResponse
        {
            Categories = categories
                .Select(c => new CategoryRecord(c.Id, c.Name, c.Type, c.Icon, c.Color))
                .ToList(),
        };
    }
}
