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
using BinusSchool.Data.Model.Student.FnStudent.PreviousSchoolNew;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.PreviousSchoolNew
{
    public class PreviousSchoolNewHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public PreviousSchoolNewHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<CollectionRequest>(nameof(GetPreviousSchoolNewRequest.IdPreviousSchoolNew));

            var predicate = PredicateBuilder.Create<MsPreviousSchoolNew>(x => true);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Id, $"%{param.Search}%")
                    || EF.Functions.Like(x.SchoolName, $"%{param.Search}%"));

            var query = _dbContext.Entity<MsPreviousSchoolNew>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<object> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetPreviousSchoolNewResult
                    {
                        IdPreviousSchoolNew = x.Id,
                        SchoolName = x.SchoolName,
                        Province = x.Province,
                        Country = x.Country
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetPreviousSchoolNewResult
                    {
                        IdPreviousSchoolNew = x.Id,
                        SchoolName = x.SchoolName,
                        Province = x.Province,
                        Country = x.Country
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
