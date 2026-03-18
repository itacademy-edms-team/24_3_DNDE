using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.Result;
using BenchmarkDotNet.Attributes;
using FinanceTrack.Finance.Infrastructure.Data.Queries;
using FinanceTrack.Finance.UseCases.FullTextSearch;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTrack.Finance.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class Fts_SequentialVsParallel
{
    private const string UserId = "d7a69a57-38b0-4031-9a27-185f4f460535";
    private const string UserQuery = "Красн";
    private const int LimitPerType = 10;

    private GlobalFullTextSearchQueryService _sequentialService = null!;
    private GlobalFullTextSearchParallelQueryService _parallelService = null!;

    [GlobalSetup]
    public void Setup()
    {
        var sp = GlobalSearchBenchContext.ServiceProvider;
        _sequentialService = sp.GetRequiredService<GlobalFullTextSearchQueryService>();
        _parallelService = sp.GetRequiredService<GlobalFullTextSearchParallelQueryService>();
    }

    [Benchmark(Baseline = true, Description = "Sequential search")]
    public async Task<Result<GlobalSearchResult>> Sequential()
    {
        return await _sequentialService.SearchAsync(
            UserId,
            UserQuery,
            LimitPerType,
            CancellationToken.None
        );
    }

    [Benchmark(Description = "Parallel search")]
    public async Task<Result<GlobalSearchResult>> Parallel()
    {
        return await _parallelService.SearchAsync(
            UserId,
            UserQuery,
            LimitPerType,
            CancellationToken.None
        );
    }
}
