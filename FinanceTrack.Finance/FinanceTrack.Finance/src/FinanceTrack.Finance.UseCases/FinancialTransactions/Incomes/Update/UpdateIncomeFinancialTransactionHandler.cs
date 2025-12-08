using FinanceTrack.Finance.Core.FinancialTransactionAggregate;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.Update;

public sealed class UpdateIncomeFinancialTransactionHandler(
    IRepository<FinancialTransaction> _repository
) : ICommandHandler<UpdateIncomeFinancialTransactionCommand, Result<FinancialTransactionDto>>
{
    public async Task<Result<FinancialTransactionDto>> Handle(
        UpdateIncomeFinancialTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        var transaction = await _repository.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null)
            return Result.NotFound();

        if (!string.Equals(transaction.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        if (transaction.TransactionType != FinancialTransactionType.Income)
            return Result.Error("Only income transactions can be updated with this operation.");

        transaction
            .UpdateName(request.Name)
            .UpdateAmount(request.Amount)
            .SetOperationDate(request.OperationDate)
            .SetMonthly(request.IsMonthly);
        await _repository.UpdateAsync(transaction, cancellationToken);

        var dto = new FinancialTransactionDto(
            transaction.Id,
            transaction.Name,
            transaction.Amount,
            transaction.OperationDate,
            transaction.IsMonthly,
            transaction.TransactionType.Name
        );

        return Result.Success(dto);
    }
}
