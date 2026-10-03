namespace FinanceTrack.Finance.UseCases.Users;

public sealed record EnableEmailNotificationsCommand(string UserId) : ICommand<Result>;
