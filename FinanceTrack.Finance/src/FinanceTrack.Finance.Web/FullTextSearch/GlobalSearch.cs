using FinanceTrack.Finance.UseCases.FinancialTransactions.Expense;
using FinanceTrack.Finance.UseCases.FullTextSearch;
using FinanceTrack.Finance.Web.Extensions;
using FinanceTrack.Finance.Web.Transactions;
using FluentValidation;
using MediatR;

namespace FinanceTrack.Finance.Web.FullTextSearch;

public class GlobalFullTextSearchRequest
{
    public const string Route = "/GlobalSearch";

    [QueryParam]
    public string Query { get; set; } = null!;

    [QueryParam]
    public int LimitPerType { get; set; }
}

public class GlobalFullTextSearchValidator : Validator<GlobalFullTextSearchRequest>
{
    public GlobalFullTextSearchValidator()
    {
        RuleFor(r => r.Query).NotEmpty().WithMessage("Query is null or empty.");
        RuleFor(r => r.LimitPerType).NotEmpty().WithMessage("LimitPerType is null or empty.");
        RuleFor(r => r.LimitPerType).Must(r => r > 0).WithMessage("LimitPerType must be positive.");
    }
}

public class GlobalSearch(IMediator mediator)
    : Endpoint<GlobalFullTextSearchRequest, GlobalSearchResult>
{
    public override void Configure()
    {
        Get(GlobalFullTextSearchRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GlobalFullTextSearchRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var globalSearchQuery = new GlobalSearchQuery(userId, req.Query, req.LimitPerType);
        var result = await mediator.Send(globalSearchQuery, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendAsync(result.Value, 200, ct);
    }
}
