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
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentStatus
{
    public class LtStudentStatusHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;

        public LtStudentStatusHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.GetParams<GetLtStudentStatusRequest>();

            var predicate = PredicateBuilder.True<LtStudentStatus>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.ShortDesc, param.SearchPattern())
                    || EF.Functions.Like(x.LongDesc, param.SearchPattern())
                    );

            var query = _dbContext.Entity<LtStudentStatus>()
                        .Where(x => (param.Ids == null || !param.Ids.Any()) ? true : param.Ids.Contains(x.IdStudentStatus.ToString()))
                        .SearchByDynamic(param)
                        .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.IdStudentStatus.ToString(), x.LongDesc))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetLtStudentStatusResult
                    {
                        IdStudentStatus = x.IdStudentStatus.ToString(),
                        ShortDesc = x.ShortDesc,
                        LongDesc = x.LongDesc
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdStudentStatus).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
