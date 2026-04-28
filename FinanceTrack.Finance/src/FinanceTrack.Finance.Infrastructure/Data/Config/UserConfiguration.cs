using FinanceTrack.Finance.Core.UserAggregate;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);

        builder.Property(u => u.IsEmailNotificationsEnabled).IsRequired().HasDefaultValue(false);
    }
}
