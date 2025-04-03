using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class GetCurrencyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetCurrencyHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCurrencyRequest>();

            var columns = new[] { "currency", "symbol", "currencyName", "countryName" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "Currency" },
                { columns[1], "Symbol" },
                { columns[2], "CurrencyName" },
                { columns[3], "CountryName" }
            };

            var predicate = PredicateBuilder.True<MsCurrency>();
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x =>
                            EF.Functions.Like(x.Country.CountryName, param.SearchPattern()) ||
                            EF.Functions.Like(x.Currency, param.SearchPattern()) ||
                            EF.Functions.Like(x.Symbol, param.SearchPattern()) ||
                            EF.Functions.Like(x.Name, param.SearchPattern()));

            //param.SearchBy ??= new List<string>(){ "Currency", "Symbol", "Name", "Country.CountryName" };

            var query = _dbContext.Entity<MsCurrency>()
                                .Include(c => c.Country)
                                .Where(x => (string.IsNullOrEmpty(param.IdCurrency) ? true : param.IdCurrency == x.Id))
                                .Where(predicate)
                                .Select(x => new GetCurrencyResult
                                {
                                    Id = x.Id,
                                    Currency = x.Currency,
                                    Symbol = x.Symbol,
                                    CurrencyName = x.Name,
                                    IdCountry = x.IdCountry,
                                    CountryName = x.Country.CountryName
                                })
                                .OrderBy(x => x.Currency)
                                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> resultItems;
            if (param.Return == CollectionType.Lov)
                resultItems = await query
                                    .Select(x => new ItemValueVm()
                                    {
                                        Id = x.Id,
                                        Description = string.Format("{0} - {1}{2}",x.CurrencyName, x.Currency, (x.Symbol.Contains("?") ? "" : " (" + x.Symbol + ")"))
                                    })
                                    .ToListAsync(CancellationToken);
            else
                resultItems = query
                                .Select(x => new GetCurrencyResult
                                {
                                    Id = x.Id,
                                    Currency = x.Currency,
                                    Symbol = (x.Symbol.Contains("?") ? "" : x.Symbol),
                                    CurrencyName = x.CurrencyName,
                                    IdCountry = x.IdCountry,
                                    CountryName = x.CountryName
                                })
                                .SetPagination(param)
                                .ToList();

            var count = param.CanCountWithoutFetchDb(query.Count())
                ? query.Count()
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(resultItems as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
