using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.Wallets.List;

public sealed record ListUserArchiveWalletsQuery(string userId) : IQuery<IReadOnlyList<WalletDto>>;
