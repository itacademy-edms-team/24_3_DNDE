using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.Wallets.GetForecastBalance;

public sealed record GetForecastBalanceQuery(string UserId, Guid WalletId)
    : IQuery<Result<WalletForecastBalanceDto>>;
