using FinanceTrack.Finance.Core.TransactionAggregate;

namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.Create;

public sealed class CreateIncomeTransactionHandler(IRepository<Transaction> transactionRepository)
    : ICommandHandler<CreateIncomeTransactionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateIncomeTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        var income = Transaction.CreateIncome(
            userId: request.UserId,
            name: request.Name,
            amount: request.Amount,
            operationDate: request.OperationDate,
            isMonthly: request.IsMonthly
        );

        await transactionRepository.AddAsync(income, cancellationToken);

        return income.Id;
    }
}
