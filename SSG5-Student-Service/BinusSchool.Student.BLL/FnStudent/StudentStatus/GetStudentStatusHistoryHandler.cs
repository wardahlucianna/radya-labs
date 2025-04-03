using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentStatus
{
    public class GetStudentStatusHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentStatusHistoryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentStatusHistoryRequest>(
                                nameof(GetStudentStatusHistoryRequest.IdStudent)
                            );

            var historyStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                                        .Include(x => x.StudentStatus)
                                        .Include(x => x.AcademicYear)
                                        .Where(x => x.IdStudent == param.IdStudent)
                                        .OrderBy(x => x.StartDate)
                                        .ThenBy(x => x.DateIn)
                                        .Select(x => new GetStudentStatusHistoryResult
                                        {
                                            AcademicYear = new ItemValueVm
                                            {
                                                Id = x.IdAcademicYear,
                                                Description = x.AcademicYear.Description
                                            },
                                            StudentStatus = new ItemValueVm
                                            {
                                                Id = x.IdStudentStatus.ToString(),
                                                Description = x.StudentStatus.LongDesc
                                            },
                                            StartDate = x.StartDate,
                                            EndDate = x.EndDate,
                                            Remarks = x.Remarks
                                        })
                                        .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(historyStudentStatus as object);
        }
    }
}
