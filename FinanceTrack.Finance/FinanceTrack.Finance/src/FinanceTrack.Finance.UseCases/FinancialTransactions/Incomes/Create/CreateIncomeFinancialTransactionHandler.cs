using FinanceTrack.Finance.Core.FinancialTransactionAggregate;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.Create;

public sealed class CreateIncomeFinancialTransactionHandler(
    IRepository<FinancialTransaction> transactionRepository
) : ICommandHandler<CreateIncomeFinancialTransactionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateIncomeFinancialTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        var income = FinancialTransaction.CreateIncome(
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
