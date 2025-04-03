using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitReason;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Common.Extensions;

namespace BinusSchool.Student.FnStudent.StudentExitForm
{
    public class GetListStudentExitReasonHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListStudentExitReasonHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetStudentExitReasonRequest>();

            var predicate = PredicateBuilder.Create<MsStudentExitReason>(x => true);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern()));

            if(!string.IsNullOrEmpty(param.IdStudentExitReason))
                predicate = predicate.And(x => x.Id == param.IdStudentExitReason);

            var query = _dbContext.Entity<MsStudentExitReason>()
                .Where(predicate)
                .OrderByDynamic(param)
                .Select(x => new GetStudentExitReasonResult
                {
                    Id = x.Id,
                    Description = x.Description
                });

            var res = await query.SetPagination(param).ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(res.Count)
            ? res.Count
          : await query.CountAsync(CancellationToken);

            return Request.CreateApiResult2(res as object, param.CreatePaginationProperty(count));
        }
    }
}
