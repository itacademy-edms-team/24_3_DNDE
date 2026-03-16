using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.FullTextSearch;

public interface IGlobalFullTextSearchQueryService
{
    Task<Result<GlobalSearchResult>> SearchAsync(
        string userId,
        string userQuery,
        int takeLimit,
        CancellationToken ct
    );
}
