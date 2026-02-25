using FinanceTrack.Finance.Core.ContributorAggregate;

namespace FinanceTrack.Finance.IntegrationTests.Data;

public class EfRepositoryAdd : BaseEfRepoTestFixture
{
    [Fact]
    public async Task AddAsync_NewContributor_SetsId()
    {
        var testContributorName = "testContributor";
        var testContributorStatus = ContributorStatus.NotSet;
        var repository = GetContributorRepository();
        var Contributor = new Contributor(testContributorName);

        await repository.AddAsync(Contributor);

        var newContributor = (await repository.ListAsync()).FirstOrDefault();

        newContributor.ShouldNotBeNull();
        testContributorName.ShouldBe(newContributor.Name);
        testContributorStatus.ShouldBe(newContributor.Status);
        newContributor.Id.ShouldBeGreaterThan(0);
    }
}
