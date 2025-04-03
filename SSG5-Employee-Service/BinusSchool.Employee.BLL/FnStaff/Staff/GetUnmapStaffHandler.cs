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
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Employee.FnStaff.Staff;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Employee.FnStaff.Staff
{
    public class GetUnmapStaffHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[] { "id", "fullName", "email" });
        private static readonly Lazy<IDictionary<string, string>> _aliasColumns = new Lazy<IDictionary<string, string>>(new Dictionary<string, string>
        {
            { _columns.Value[0], "idBinusian" },
            { _columns.Value[1], "firstName" },
            { _columns.Value[2], "binusianEmailAddress" }
        });

        private readonly IEmployeeDbContext _dbContext;

        public GetUnmapStaffHandler(IEmployeeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<CollectionSchoolRequest>();
            var predicate = PredicateBuilder.Create<MsStaff>(x => !string.IsNullOrEmpty(x.BinusianEmailAddress));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.IdBinusian, param.SearchPattern())
                    //|| EF.Functions.Like(x.FirstName, param.SearchPattern())
                    //|| EF.Functions.Like(x.LastName, param.SearchPattern())
                    || EF.Functions.Like(x.FirstName.Trim() +
                                        (string.IsNullOrWhiteSpace(x.LastName) ? "" : " " + x.LastName.Trim()),
                                        param.SearchPattern())
                    || EF.Functions.Like(x.BinusianEmailAddress, param.SearchPattern()));

            if (param.IdSchool != null && param.IdSchool.Any())
                predicate = predicate.And(x => param.IdSchool.Contains(x.IdSchool));

            // default order by first name
            param.OrderBy ??= _columns.Value[1];

            var query = _dbContext.Entity<MsStaff>()
                .Where(predicate)
                .OrderByDynamic(param, _aliasColumns.Value);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.IdBinusian, x.FirstName))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetUnmapStaffResult
                    {
                        Id = x.IdBinusian,
                        FullName = NameUtil.GenerateFullName(x.FirstName, x.LastName),
                        Email = x.BinusianEmailAddress
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdBinusian).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }
    }
}
