using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.AspNetCore.Pagination;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class PaginationOptionsSetup : IConfigureOptions<PaginationOptions>
{
    public void Configure(PaginationOptions options)
    {
        options.FirstQueryParameterName = "first";
        options.BeforeQueryParameterName = "last";
        options.AfterQueryParameterName = "after";
        options.LastQueryParameterName = "last";
        options.PageQueryParameterName = "page";
        options.PageSizeQueryParameterName = "pageSize"; // Параметр запроса для размера страницы

        options.DefaultSize = 30; // Размер страницы по умолчанию
        options.MaxSize = 100; // Максимальный допустимый размер страницы через pageSize
        options.CanChangeSizeFromQuery = true; // Разрешать изменение размера страницы через параметры запроса
    }
}
