namespace FinanceTrack.Finance.Infrastructure.Data;

// From template. Could be used in functional tests
public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext dbContext)
    {
        await PopulateTestDataAsync(dbContext);
    }

    public static async Task PopulateTestDataAsync(AppDbContext dbContext)
    {
        await Task.CompletedTask;
    }
}
