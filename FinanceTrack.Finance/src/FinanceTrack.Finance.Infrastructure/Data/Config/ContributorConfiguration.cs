using FinanceTrack.Finance.Core.ContributorAggregate;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class ContributorConfiguration : IEntityTypeConfiguration<Contributor>
{
    public void Configure(EntityTypeBuilder<Contributor> builder)
    {
        builder
            .Property(p => p.Name)
            .HasMaxLength(ContributorDataSchemaConstants.DEFAULT_NAME_LENGTH)
            .IsRequired();

        builder.OwnsOne(builder => builder.PhoneNumber);

        builder
            .Property(x => x.Status)
            .HasConversion(x => x.Value, x => ContributorStatus.FromValue(x));
    }
}
