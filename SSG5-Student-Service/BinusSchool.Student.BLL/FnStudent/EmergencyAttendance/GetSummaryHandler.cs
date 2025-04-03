using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance
{
    public class GetSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetSummaryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetSummaryRequest>(nameof(GetSummaryRequest.Date),
                                                                  nameof(GetSummaryRequest.IdAcademicYear));

            var levels = await _dbContext.Entity<MsLevel>()
                                         .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                                         .Select(x => new CodeWithIdVm
                                         {
                                             Id = x.Id,
                                             Code = x.Code,
                                             Description = x.Description
                                         })
                                         .ToListAsync();

            var result = new List<SummaryResult>();
            foreach (var level in levels)
            {
                result.Add(new SummaryResult
                {
                    Level = level,
                    Unsubmitted = await _dbContext.Entity<MsStudent>()
                                                  .Include(x => x.TrStudentSafeReports)
                                                  .Include(x => x.MsHomeroomStudents).ThenInclude(x => x.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade)
                                                  .CountAsync(x => !x.TrStudentSafeReports.Any(y => y.Date.Date == param.Date.Date)
                                                                   && x.MsHomeroomStudents.Any(y => y.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.IdLevel == level.Id))
                });
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
