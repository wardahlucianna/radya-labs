using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitHistory;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Student.FnStudent.StudentExitForm
{
    public class GetListStudentExitHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListStudentExitHistoryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetStudentExitHistoryRequest>();

            var predicate = PredicateBuilder.Create<TrStudentExitStatus>(x => x.IdStudentExit == param.IdStudentExit);

            var query = _dbContext.Entity<TrStudentExitStatus>()
                .Where(predicate)
                .Select(x => new GetStudentExitHistoryResult
                {
                    Status = x.Status.GetDescription(),
                    DateLog = x.DateIn.Value
                });

            var res = await query.OrderByDescending(x => x.DateLog).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(res as object);
        }
    }
}
