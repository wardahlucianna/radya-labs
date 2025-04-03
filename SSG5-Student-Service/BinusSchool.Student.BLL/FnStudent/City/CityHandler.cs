using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.City;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.City
{
    public class CityHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public CityHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCityRequest>(nameof(GetCityRequest.IdCountry), nameof(GetCityRequest.IdProvince));

            var predicate = PredicateBuilder.Create<LtCity>(x => x.IdCountry == param.IdCountry && x.IdProvince == param.IdProvince);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Id, $"%{param.Search}%")
                    || EF.Functions.Like(x.CityName, $"%{param.Search}%"));

            var query = _dbContext.Entity<LtCity>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.CityName
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.CityName
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
