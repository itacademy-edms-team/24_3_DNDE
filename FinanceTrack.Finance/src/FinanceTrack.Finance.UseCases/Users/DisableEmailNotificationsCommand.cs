namespace FinanceTrack.Finance.UseCases.Users;

public sealed record DisableEmailNotificationsCommand(string UserId) : ICommand<Result>;
