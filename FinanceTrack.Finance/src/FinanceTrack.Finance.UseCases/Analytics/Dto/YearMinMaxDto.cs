using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.Analytics.Dto;

public sealed record YearMinMaxDto(DateOnly MinYear, DateOnly MaxYear);
