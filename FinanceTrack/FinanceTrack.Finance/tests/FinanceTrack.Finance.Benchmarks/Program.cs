using System.Reflection;
using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using FinanceTrack.Finance.Benchmarks.Benchmarks;
using FinanceTrack.Finance.Infrastructure;
using FinanceTrack.Finance.Infrastructure.Data;
using FinanceTrack.Finance.Infrastructure.Data.Queries;
using FinanceTrack.Finance.Web.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinanceTrack.Finance.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance;
        BenchmarkRunner.Run<GlobalFullTextSearchQueryServiceSequentialVsParallelBenchmarkTests>(
            config,
            args
        );

        // Use this to select benchmarks from the console:
        // var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}
